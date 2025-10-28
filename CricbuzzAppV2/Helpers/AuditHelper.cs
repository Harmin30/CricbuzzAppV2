using CricbuzzAppV2.Data;
using CricbuzzAppV2.Models;
using Microsoft.AspNetCore.Http;

namespace CricbuzzAppV2.Helpers
{
    public static class AuditHelper
    {
        public static async Task LogAction(
  ApplicationDbContext context,
  HttpContext httpContext,
 string action,
     string entityName,
            string entityId,
   string details)
   {
            var username = httpContext.Session.GetString("Username") ?? "System";

          var audit = new Audit
    {
        Action = action,
      EntityName = entityName,
         EntityId = entityId,
   UserName = username,
          Details = details,
      Timestamp = DateTime.Now
            };

   context.Audits.Add(audit);
            await context.SaveChangesAsync();
 }

    // Helper method for Create actions
public static async Task LogCreate(
            ApplicationDbContext context,
   HttpContext httpContext,
            string entityName,
     string entityId,
         string details = null)
    {
      await LogAction(context, httpContext, "Created", entityName, entityId, details);
     }

        // Helper method for Update actions
     public static async Task LogUpdate(
            ApplicationDbContext context,
       HttpContext httpContext,
            string entityName,
          string entityId,
            string details = null)
        {
            await LogAction(context, httpContext, "Updated", entityName, entityId, details);
        }

        // Helper method for Delete actions
        public static async Task LogDelete(
       ApplicationDbContext context,
            HttpContext httpContext,
 string entityName,
        string entityId,
        string details = null)
      {
   await LogAction(context, httpContext, "Deleted", entityName, entityId, details);
        }
    }
}