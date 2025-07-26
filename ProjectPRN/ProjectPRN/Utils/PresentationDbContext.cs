using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ProjectPRN.Utils
{
    public class PresentationDbContext : ApplicationDbContext
    {
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            LogChangesToDatabase();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            LogChangesToDatabase();
            return base.SaveChanges();
        }

        private void LogChangesToDatabase()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .Where(e => e.Entity.GetType() != typeof(ActivityLog))
                .ToList();

            foreach (var entry in entries)
            {
                var entityName = entry.Entity.GetType().Name;
                var operation = entry.State.ToString();
                var logMessage = $"Database Operation: {operation} on {entityName}";

                // Get user ID from session
                int userId = SessionManager.GetCurrentUserId();

                // Add operation details
                if (entry.State == EntityState.Modified)
                {
                    var changedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => $"{p.Metadata.Name}: {p.OriginalValue} → {p.CurrentValue}")
                        .ToList();

                    if (changedProperties.Any())
                    {
                        logMessage += $" - Changes: {string.Join(", ", changedProperties)}";
                    }
                }

                var activityLog = new ActivityLog
                {
                    UserId = userId,
                    Action = logMessage,
                    Timestamp = DateTime.Now
                };

                ActivityLogs.Add(activityLog);
            }
        }

        // Enhanced helper method with session context
        public void LogActivity(string action)
        {
            int? userId = SessionManager.IsLoggedIn ? SessionManager.GetCurrentUserId() : null;
            LogActivity(userId, action);
        }

        public void LogActivity(int? userId, string action)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Timestamp = DateTime.Now
                };

                ActivityLogs.Add(log);
                SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }
    }
}
