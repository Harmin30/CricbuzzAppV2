using CricbuzzAppV2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CricbuzzAppV2.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --------------------
        // DbSets
        // --------------------
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<Match> Matches { get; set; }

        // OLD (keep for now)
        public DbSet<Scorecard> Scorecards { get; set; }

        // NEW SCORECARD SYSTEM
        public DbSet<MatchInnings> MatchInnings { get; set; }
        public DbSet<BattingInningsScorecard> BattingScorecards { get; set; }

        public DbSet<BowlingScorecard> BowlingScorecards { get; set; }

        public DbSet<PlayerPersonalInfo> PlayerPersonalInfos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Audit> Audits { get; set; }

        // --------------------
        // Relationships
        // --------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Player → Team
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Match → Team A
            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany()
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match → Team B
            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany()
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            // Match → Winner
            modelBuilder.Entity<Match>()
                .HasOne(m => m.WinnerTeam)
                .WithMany()
                .HasForeignKey(m => m.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------
            // OLD Scorecard (keep)
            // --------------------
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

            // --------------------
            // NEW MatchInnings (FIXED)
            // --------------------
            modelBuilder.Entity<MatchInnings>()
                .HasOne(mi => mi.Match)
                .WithMany(m => m.MatchInnings)
                .HasForeignKey(mi => mi.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchInnings>()
                .HasOne(mi => mi.BattingTeam)
                .WithMany()
                .HasForeignKey(mi => mi.BattingTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchInnings>()
                .HasOne(mi => mi.BowlingTeam)
                .WithMany()
                .HasForeignKey(mi => mi.BowlingTeamId)
                .OnDelete(DeleteBehavior.Restrict);


            // --------------------
            // BattingScorecard
            // --------------------
            modelBuilder.Entity<BattingInningsScorecard>()
                .HasOne(bs => bs.MatchInnings)
                .WithMany(mi => mi.BattingScorecards)
                .HasForeignKey(bs => bs.MatchInningsId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BattingInningsScorecard>()
                .HasOne(bs => bs.Player)
                .WithMany()
                .HasForeignKey(bs => bs.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // --------------------
            // BowlingScorecard
            // --------------------
            modelBuilder.Entity<BowlingScorecard>()
                .HasOne(bs => bs.MatchInnings)
                .WithMany(mi => mi.BowlingScorecards)
                .HasForeignKey(bs => bs.MatchInningsId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BowlingScorecard>()
                .HasOne(bs => bs.Player)
                .WithMany()
                .HasForeignKey(bs => bs.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlayerStats
            modelBuilder.Entity<PlayerStats>()
                .HasOne(ps => ps.Player)
                .WithMany(p => p.PlayerStats)
                .HasForeignKey(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // --------------------
        // Audit logic (UNCHANGED)
        // --------------------
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
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
            ChangeTracker.DetectChanges();

            var httpContext = _httpContextAccessor.HttpContext;
            var userName = httpContext?.Session.GetString("Username") ?? "System";

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

                string action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => "Unknown"
                };

                string details;

                if (entry.State == EntityState.Modified)
                {
                    var changes = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(
                            p => p.Metadata.Name,
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
                    details = JsonSerializer.Serialize(
                        entry.CurrentValues.ToObject(),
                        new JsonSerializerOptions { WriteIndented = true });
                }
                else
                {
                    details = JsonSerializer.Serialize(
                        entry.OriginalValues.ToObject(),
                        new JsonSerializerOptions { WriteIndented = true });
                }

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
