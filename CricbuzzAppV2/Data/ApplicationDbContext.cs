using CricbuzzAppV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CricbuzzAppV2.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // DbSets
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Scorecard> Scorecards { get; set; }
        public DbSet<PlayerPersonalInfo> PlayerPersonalInfos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Audit> Audits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relationships ---
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany()
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany()
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.WinnerTeam)
                .WithMany()
                .HasForeignKey(m => m.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Scorecard>()
                .HasOne(s => s.Player)
                .WithMany()
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Scorecard>()
                .HasOne(s => s.Match)
                .WithMany(m => m.Scorecards)
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerStats>()
                .HasOne(ps => ps.Player)
                .WithMany(p => p.PlayerStats)
                .HasForeignKey(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Intercept SaveChanges to record logs automatically
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await AddAuditLogsAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AddAuditLogsAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        private async Task AddAuditLogsAsync()
        {
            // ✅ Ensure EF Core has detected all pending changes
            ChangeTracker.DetectChanges();

            var httpContext = _httpContextAccessor.HttpContext;
            var userName = httpContext?.Session.GetString("Username") ?? "System";

            // ✅ Get all entity entries that are Added / Modified / Deleted (except Audits)
            var entries = ChangeTracker.Entries()
                .Where(e =>
                    e.Entity is not Audit &&
                    e.Entity.GetType().Namespace?.Contains("CricbuzzAppV2.Models") == true &&
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (!entries.Any()) return;

            foreach (var entry in entries)
            {
                var entityName = entry.Entity.GetType().Name;
                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                string entityId = primaryKey?.CurrentValue?.ToString() ?? "N/A";

                // ✅ Determine the action type
                string action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => "Unknown"
                };

                // ✅ Build readable JSON details
                string details;

                if (entry.State == EntityState.Modified)
                {
                    var changes = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name,
                                      p => new
                                      {
                                          OldValue = p.OriginalValue,
                                          NewValue = p.CurrentValue
                                      });

                    details = changes.Any()
                        ? JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = true })
                        : "No fields changed.";
                }
                else if (entry.State == EntityState.Added)
                {
                    details = JsonSerializer.Serialize(entry.CurrentValues.ToObject(), new JsonSerializerOptions { WriteIndented = true });
                }
                else // Deleted
                {
                    details = JsonSerializer.Serialize(entry.OriginalValues.ToObject(), new JsonSerializerOptions { WriteIndented = true });
                }

                // ✅ Create the audit entry
                var audit = new Audit
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    UserName = userName,
                    Details = details,
                    Timestamp = DateTime.Now
                };

                await Audits.AddAsync(audit);
            }
        }

    }
}
