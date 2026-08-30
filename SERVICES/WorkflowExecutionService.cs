using AutomationAPI.DATA;
using AutomationAPI.MODEL.ActionExecutor;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace AutomationAPI.SERVICES
{
    public class WorkflowExecutionService
        : IWorkflowExecutionService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<WorkflowExecutionService> _logger;
        private readonly IActionExecutorFactory _executorFactory;
        private readonly IConditionEvaluator _conditionEvaluator;
        private readonly IWorkflowLogger _workflowLogger;
        private readonly IDeadLetterService _deadLetterService;
        private readonly IFailureAlertService _failureAlertService;
        private readonly IWorkflowInstanceService _workflowInstanceService;
        //private readonly IntegrationProviderResolver _providerResolver;
        private readonly IVariableResolver _variableResolver;
        public WorkflowExecutionService(AppDbContext dbContext,
            ILogger<WorkflowExecutionService> logger, IActionExecutorFactory executorFactory,
            IConditionEvaluator conditionEvaluator, IWorkflowLogger workflowLogger, IDeadLetterService deadLetterService,
            IFailureAlertService failureAlertService, IWorkflowInstanceService workflowInstanceService,
            /*IntegrationProviderResolver providerResolver*/ IVariableResolver variableResolver)
        {
            _dbContext = dbContext;
            _logger = logger;
            _executorFactory = executorFactory;
            _conditionEvaluator = conditionEvaluator;
            _workflowLogger = workflowLogger;
            _deadLetterService = deadLetterService;
            _failureAlertService = failureAlertService;
            _workflowInstanceService = workflowInstanceService;
            //_providerResolver = providerResolver;
            _variableResolver = variableResolver;
        }

        public async Task ExecuteRuleAsync(int ruleId, Dictionary<string, object> payload)
        {
            try
            {
                var rule =
                    await _dbContext.AutomationRules
                        .Include(x => x.Steps)
                            .ThenInclude(x => x.Conditions)
                        .FirstOrDefaultAsync(
                            x => x.ID == ruleId);

                if (rule == null)
                    return;

                var firstStep =
                    rule.Steps
                        .OrderBy(x => x.Order)
                        .FirstOrDefault();

                if (firstStep == null)
                    return;

                var instance =
                    await _workflowInstanceService
                        .CreateAsync(
                            rule.ID,
                            firstStep.Id,
                            payload);

                await ExecuteStepRecursive(
                    instance.Id,
                    rule,
                    firstStep,
                    payload,
                    new HashSet<int>());
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[ERROR] Workflow execution failed");
            }
        }

        private async Task ExecuteStepRecursive(int instanceId, AutomationRule rule, WorkFlowStep step, Dictionary<string, object> payload, HashSet<int>? visitedSteps = null, CancellationToken cancellationToken = default)
        {
            visitedSteps ??=
                new HashSet<int>();

            // =========================
            // PREVENT INFINITE LOOPS
            // =========================

            if (visitedSteps.Contains(step.Id))
            {
                throw new Exception(
                    $"Workflow loop detected at step {step.Id}");
            }

            visitedSteps.Add(step.Id);

            cancellationToken
                .ThrowIfCancellationRequested();

            // =========================
            // UPDATE INSTANCE STATE
            // =========================

            await _workflowInstanceService
                .UpdateStepAsync(
                    instanceId,
                    step.Id,
                    "RUNNING");

            // =========================
            // CONDITION EVALUATION
            // =========================

            var conditionPassed = true;

            if (step.Conditions != null && step.Conditions.Any())
            {
                foreach (var condition
                    in step.Conditions)
                {
                    var passed =
                        _conditionEvaluator
                            .Evaluate(
                                condition,
                                payload);

                    if (!passed)
                    {
                        conditionPassed = false;
                        break;
                    }
                }
            }

            // =========================
            // HANDLE FALSE BRANCH
            // =========================

            if (!conditionPassed)
            {
                if (step.IsBranchStep &&
                    step.FalseStepId.HasValue)
                {
                    var falseStep =
                        rule.Steps
                            .FirstOrDefault(
                                x =>
                                    x.Id ==
                                    step.FalseStepId.Value);

                    if (falseStep != null)
                    {
                        await ExecuteStepRecursive(
                            instanceId,
                            rule,
                            falseStep,
                            payload,
                            visitedSteps,
                            cancellationToken);
                    }
                }

                return;
            }

            // =========================
            // DELAY STEP
            // =========================

            if (step.Action == "delay.wait")
            {
                await ScheduleDelayedExecution(
                    instanceId,
                    rule,
                    step,
                    payload);

                return;
            }

            // =========================
            // LOOP STEP
            // =========================

            if (step.IsLoopStep)
            {
                await ExecuteLoopStep(
                    instanceId,
                    rule,
                    step,
                    payload);

                return;
            }

            // =========================
            // RETRY EXECUTION
            // =========================

            var executionSucceeded = false;

            var maxRetries =
                step.RetryCount <= 0
                    ? 1
                    : step.RetryCount;

            var attempt = 0;

            while (!executionSucceeded &&
                   attempt < maxRetries)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                attempt++;

                try
                {
                    var executor = _executorFactory.GetExecutor(step.Action);

                    var resolvedPayload = ResolvePayloadVariables(payload);

                    await executor.ExecuteAsync(
                        rule.UserID,
                        step,
                        resolvedPayload);

                    executionSucceeded = true;

                    await _workflowLogger
                        .LogAsync(
                            rule.ID,
                            step.Id,
                            "SUCCESS",
                            "Step executed successfully",
                            attempt);

                    _dbContext.ExecutionLogs.Add(new ExecutionLog
                    {
                        UserId = rule.UserID,
                        Trigger = rule.Trigger,
                        Action = step.Action,
                        Target = step.Target,
                        Status = "Success",
                        RetryCount = attempt,
                    });
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await _workflowLogger
                        .LogAsync(
                            rule.ID,
                            step.Id,
                            "FAILED",
                            ex.Message,
                            attempt);

                    _logger.LogError(
                        ex,
                        "[ERROR] Workflow step failed");

                    if (attempt >= maxRetries)
                    {
                        _logger.LogError(
                            ex,
                            $"[ERROR] Workflow permanently failed at step {step.Id}");

                        await _workflowInstanceService
                            .FailAsync(instanceId);

                        await _deadLetterService
                            .SaveAsync(
                                rule.ID,
                                step.Id,
                                payload,
                                ex.Message,
                                attempt);

                        await _failureAlertService
                            .SendFailureAlertAsync(
                                rule.UserID,
                                $"Workflow step failed permanently: {step.Action}");

                        _dbContext.ExecutionLogs.Add(new ExecutionLog
                        {
                            UserId = rule.UserID,
                            Trigger = rule.Trigger,
                            Action = step.Action,
                            Target = step.Target,
                            Status = "Failed",
                            RetryCount = attempt,
                            ErrorMessage = ex.Message,
                        });
                        await _dbContext.SaveChangesAsync();

                        return;
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(
                            step.RetryDelaySeconds <= 0
                                ? 5
                                : step.RetryDelaySeconds),
                        cancellationToken);
                }
            }

            // =========================
            // BRANCHING
            // =========================

            if (step.IsBranchStep)
            {
                int? nextStepId =
                    executionSucceeded
                        ? step.TrueStepId
                        : step.FalseStepId;

                if (nextStepId.HasValue)
                {
                    var nextBranchStep =
                        rule.Steps
                            .FirstOrDefault(
                                x =>
                                    x.Id ==
                                    nextStepId.Value);

                    if (nextBranchStep != null)
                    {
                        await ExecuteStepRecursive(
                            instanceId,
                            rule,
                            nextBranchStep,
                            payload,
                            visitedSteps,
                            cancellationToken);
                    }
                }

                return;
            }

            // =========================
            // NORMAL FLOW
            // =========================

            var nextStep =
                rule.Steps
                    .Where(x =>
                        x.Order > step.Order)
                    .OrderBy(x => x.Order)
                    .FirstOrDefault();

            if (nextStep != null)
            {
                await ExecuteStepRecursive(
                    instanceId,
                    rule,
                    nextStep,
                    payload,
                    visitedSteps,
                    cancellationToken);
            }
            else
            {
                await _workflowInstanceService
                    .CompleteAsync(instanceId);
            }
        }

        private Dictionary<string, object> ResolvePayloadVariables(Dictionary<string, object> payload)
        {
            var resolved =
                new Dictionary<string, object>();

            foreach (var item in payload)
            {
                if (item.Value is string value)
                {
                    resolved[item.Key] =
                        _variableResolver.Resolve(
                            value,
                            payload);
                }
                else
                {
                    resolved[item.Key] =
                        item.Value;
                }
            }

            return resolved;
        }
        private async Task ScheduleDelayedExecution(int instanceId, AutomationRule rule, WorkFlowStep step, Dictionary<string, object> payload)
        {
            var delayExecutor =
                _executorFactory
                    .GetExecutor(step.Action)
                    as DelayActionExecutor;

            if (delayExecutor == null)
                return;

            var delay =
                delayExecutor.GetDelay(step);

            var nextStep =
                rule.Steps
                    .Where(x =>
                        x.Order > step.Order)
                    .OrderBy(x => x.Order)
                    .FirstOrDefault();

            if (nextStep == null)
                return;

            await _workflowInstanceService
                .UpdateStepAsync(
                    instanceId,
                    step.Id,
                    "WAITING");

            BackgroundJob.Schedule<
                WorkflowExecutionService>(
                x => x.ResumeWorkflowAsync(
                    instanceId,
                    rule.ID,
                    nextStep.Id,
                    payload),
                delay);
        }

        public async Task ResumeWorkflowAsync(int instanceId, int ruleId, int stepId, Dictionary<string, object> payload)
        {
            var rule =
                await _dbContext.AutomationRules
                    .Include(x => x.Steps)
                        .ThenInclude(x => x.Conditions)
                    .FirstOrDefaultAsync(
                        x => x.ID == ruleId);

            if (rule == null)
                return;

            var step =
                rule.Steps
                    .FirstOrDefault(
                        x => x.Id == stepId);

            if (step == null)
                return;

            await ExecuteStepRecursive(
                instanceId,
                rule,
                step,
                payload,
                new HashSet<int>());
        }

        private async Task ExecuteLoopStep(int instanceId, AutomationRule rule, WorkFlowStep loopStep, Dictionary<string, object> payload)
        {
            if (string.IsNullOrWhiteSpace(
                loopStep.LoopField))
            {
                _logger.LogError(
                    "[ERROR] Loop field is empty");

                return;
            }

            if (!payload.ContainsKey(
                loopStep.LoopField))
            {
                _logger.LogError(
                    $"[ERROR] Payload does not contain loop field {loopStep.LoopField}");

                return;
            }

            var rawCollection =
                payload[loopStep.LoopField];

            if (rawCollection is not IEnumerable enumerable)
            {
                _logger.LogError(
                    $"[ERROR] Loop field {loopStep.LoopField} is not iterable");

                return;
            }

            var collection =
                enumerable.Cast<object>();

            var nextStep =
                rule.Steps
                    .Where(x =>
                        x.Order > loopStep.Order)
                    .OrderBy(x => x.Order)
                    .FirstOrDefault();

            if (nextStep == null)
                return;

            foreach (var item in collection)
            {
                var loopPayload =
                    new Dictionary<string, object>(
                        payload);

                loopPayload["CurrentItem"] =
                    item;

                await ExecuteStepRecursive(
                    instanceId,
                    rule,
                    nextStep,
                    loopPayload,
                    new HashSet<int>());
            }

            _logger.LogInformation(
                $"[INFO] Completed loop step {loopStep.Id}");
        }
    }
}