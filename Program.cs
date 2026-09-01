using AutomationAPI.DATA;
using AutomationAPI.DATA.Congiguration;
using AutomationAPI.MODEL;
using AutomationAPI.MODEL.ActionExecutor;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES;
using AutomationAPI.SERVICES.Email_Service;
using AutomationAPI.SERVICES.Persistence;
using AutomationAPI.SERVICES.Providers;
using AutomationAPI.SERVICES.Resources;
using AutomationAPI.SERVICES.Secrets;
using Google;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the bearer scheme. Enter Bearer [space] add your token in the text input. Example: Bearer KnU78^tUyuf65VFT56r%^&*7tGdf54cfJHBUtyFTt76T",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            },
                            Scheme = "oauth2",

                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("JWT:Secret"));
string audience = builder.Configuration.GetValue<string>("JWT:Audience");
string issuer = builder.Configuration.GetValue<string>("JWT:Issuer");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateIssuer = true,
        ValidIssuer = issuer
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IExecutionLogService, ExecutionLogService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtGenerator, JwtService>();
builder.Services.AddScoped<IUserService,  UserService>();
builder.Services.AddScoped<ICustomIdGenerator, CustomIdGenerator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IWorkSpaceService, WorkSpaceService>();
builder.Services.AddScoped<IConnectAppsService, ConnectAppsService>();
builder.Services.AddScoped<IIntegrationCredentialService, IntegrationCredentialService>();
builder.Services.AddHttpClient<GmailIntegrationService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHttpClient<SlackService>();
//builder.Services.AddHttpClient();
builder.Services.AddHttpClient<GitHubService>();
//builder.Services.AddScoped<IGoogleSheetsService, GoogleSheetsService>();
//builder.Services.AddHostedService<FormWatcherService>();
builder.Services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
builder.Services.AddScoped<IActionExecutor, GmailActionExecutor>();
builder.Services.AddScoped<IActionExecutor, SlackActionExecutor>();
builder.Services.AddScoped<IActionExecutor, GitHubActionExecutor>();
builder.Services.AddScoped<IActionExecutorFactory, ActionExecutorFactory>();
builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
builder.Services.AddScoped<IVariableResolver, VariableResolver >();
builder.Services.AddScoped<IActionExecutor, DelayActionExecutor>();
builder.Services.AddScoped<IWorkflowLogger, WorkflowLogger>();
builder.Services.AddScoped<IFailureAlertService, FailureAlertService>();
builder.Services.AddScoped<IDeadLetterService, DeadLetterService>();
builder.Services.AddScoped<IActionExecutor, LoopActionExecutor>();
builder.Services.AddScoped<IWorkflowInstanceService, WorkflowInstanceService>();
builder.Services.AddScoped<ISlackProvider, SlackProvider>();
builder.Services.AddScoped<TriggerDefinitionRegistry>();
builder.Services.AddScoped<ITriggerEngineService, TriggerEngineService>();
builder.Services.AddScoped<GmailPollingService>();
builder.Services.AddScoped<DiscordPollingService>();
builder.Services.AddScoped<ScheduleTriggerService>();
//builder.Services.AddHttpClient<GoogleFormsService>();
//builder.Services.AddScoped<GoogleFormsPollingService>();
builder.Services.AddScoped<IResourceProviderResolver, ResourceProviderResolver>();
builder.Services.AddScoped<IResourceProvider, GitHubResourceProvider>();
builder.Services.AddScoped<IGitHubProvider, GitHubProvider>();
builder.Services.AddScoped<IMetadataService, MetadataService>();
builder.Services.AddScoped<IResourceProvider, SlackResourceProvider>();
builder.Services.AddHttpClient<IDiscordProvider, DiscordProvider>();
builder.Services.AddScoped<IResourceProvider, DiscordResourceProvider>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ISecretProtector, SecretProtector>();
builder.Services.AddScoped<IActionExecutor, DiscordActionExecutor>();
builder.Services.AddHttpClient<IResourceProvider, NotionResourceProvider>();
builder.Services.AddHttpClient<IActionExecutor, NotionActionExecutor>();
builder.Services.AddHttpClient<ITrelloProvider, TrelloProvider>();
builder.Services.AddScoped<IResourceProvider, TrelloResourceProvider>();
builder.Services.AddScoped<IActionExecutor, TrelloActionExecutor>();
builder.Services.AddHttpClient<IStripeProvider, StripeProvider>();
builder.Services.AddScoped<IResourceProvider, StripeResourceProvider>();
builder.Services.AddScoped<IActionExecutor, StripeActionExecutor>();




builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
                        // AllowAnyOrigin() + WithOrigins() together silently drops AllowAnyOrigin() —
                        // only the origins listed below are actually allowed. Add preview-deploy
                        // origins here as you get them.
                        .WithOrigins(
                            "https://jutixlabs-nexus.vercel.app/",
                            "http://localhost:3000",
                            "http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});



builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

// 🔥 AUTO APPLY MIGRATIONS ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseCors("AllowAll");

//Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<ScheduleTriggerService>("daily-trigger", x => x.FireDailyTrigger(), Cron.Daily);
recurringJobManager.AddOrUpdate<ScheduleTriggerService>("hourly-trigger", x => x.FireHourlyTrigger(), Cron.Hourly);
recurringJobManager.AddOrUpdate<GmailPollingService>("gmail-polling", x => x.PollAsync(), "*/5 * * * *"); // every 5 minutes
//recurringJobManager.AddOrUpdate<GoogleFormsPollingService>("google-forms-polling", x => x.PollAsync(), "*/5 * * * *");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
