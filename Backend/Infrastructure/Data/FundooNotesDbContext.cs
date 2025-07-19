using Microsoft.EntityFrameworkCore;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Infrastructure.Data.Configurations;

namespace fundoo_notes.Infrastructure.Data
{
    /// <summary>
    /// Database context for Fundoo Notes application
    /// </summary>
    public class FundooNotesDbContext : DbContext
    {
        public FundooNotesDbContext(DbContextOptions<FundooNotesDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<NoteLabel> NoteLabels { get; set; }
        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<NoteAttachment> NoteAttachments { get; set; }
        public DbSet<NoteReminder> NoteReminders { get; set; }
        public DbSet<NoteListItem> NoteListItems { get; set; }
        public DbSet<NoteTemplate> NoteTemplates { get; set; }
        public DbSet<NoteHistory> NoteHistory { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new NoteConfiguration());
            modelBuilder.ApplyConfiguration(new LabelConfiguration());
            modelBuilder.ApplyConfiguration(new NoteLabelConfiguration());
            modelBuilder.ApplyConfiguration(new CollaboratorConfiguration());
            modelBuilder.ApplyConfiguration(new NoteAttachmentConfiguration());
            modelBuilder.ApplyConfiguration(new NoteReminderConfiguration());
            modelBuilder.ApplyConfiguration(new NoteListItemConfiguration());
            modelBuilder.ApplyConfiguration(new NoteTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new NoteHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new LoginHistoryConfiguration());

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Note>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<Label>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<NoteLabel>().HasQueryFilter(nl => !nl.IsDeleted);
            modelBuilder.Entity<Collaborator>().HasQueryFilter(c => !c.IsDeleted);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Only configure if not already configured (for testing scenarios)
            if (!optionsBuilder.IsConfigured)
            {
                // This will be overridden by DI configuration in production
                optionsBuilder.UseSqlServer(options =>
                {
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    options.CommandTimeout(30); // 30 seconds timeout
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update timestamps before saving
            var entries = ChangeTracker.Entries<BaseEntity>();
            
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
