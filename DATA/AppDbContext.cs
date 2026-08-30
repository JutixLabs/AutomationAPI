using AutomationAPI.MODEL.Entity;
using Microsoft.EntityFrameworkCore;

namespace AutomationAPI.DATA
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AutomationRule> AutomationRules { get; set; }
        public DbSet<ExecutionLog> ExecutionLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WorkFlowStep> WorkFlowSteps { get; set; }
        public DbSet<WorkSpace> WorkSpaces { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<ConnectedApp> ConnectedApps { get; set; }
        public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
        public DbSet<FormWatch> FormWatches { get; set; }
        public DbSet<RuleCondition> RuleConditions { get; set; }
        public DbSet<WorkflowExecutionLog> WorkflowExecutionLogs { get; set; }
        public DbSet<WorkflowDeadLetter> WorkflowDeadLetters { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<IntegrationCredential> IntegrationCredentials { get; set; }
        public DbSet<TrelloWatchedBoard> TrelloWatchedBoards { get; set; }
        public DbSet<DiscordWatchedChannel> DiscordWatchedChannels { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkFlowStep>()
                .HasOne(ws => ws.AutomationRule)
                .WithMany(r => r.Steps)
                .HasForeignKey(ws => ws.AutomationRuleId);
        }
    }
}
