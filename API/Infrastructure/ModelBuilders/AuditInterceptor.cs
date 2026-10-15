using API.Domain.Abstractions;
using API.Domain.Entities;
using API.Domain.Enums;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace API.Infrastructure.ModelBuilders;

public class AuditInterceptor(IUserContext userContext) : ISaveChangesInterceptor
{

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
            return result;

        context.ChangeTracker.DetectChanges();

        var fixedTime = DateTime.UtcNow;
        var userId = userContext.UserId ?? Guid.Empty;

        var modifiedEntities = context.ChangeTracker.Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified 
            )
            .ToList();

        if (modifiedEntities.Count == 0)
            return result;

        foreach (var change in modifiedEntities)
        {
            if (change.Entity is IBaseEntity entity)
            {
                switch (change.State)
                {
                    case EntityState.Added:
                        entity.CreatedOn = fixedTime;
                        entity.CreatedBy = userId;
                        entity.UpdatedOn = fixedTime;
                        entity.UpdatedBy = userId;
                        entity.StatusCode = RecordStatus.Active;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedOn = fixedTime;
                        entity.UpdatedBy = userId;
                        if (change.Property(nameof(entity.StatusCode)).IsModified
                            && entity.StatusCode != RecordStatus.Active)
                        {
                            entity.DeletedOn = fixedTime;
                            entity.DeletedBy = userId;
                        }
                        break;
                }
            }
        }

        var changedParams = modifiedEntities
            .Where(e => e.Entity is ReportParameter)
            .Select(e => ((ReportParameter)e.Entity).ReportId)
            .Distinct()
            .ToList();

        if (changedParams.Count > 0)
        {
            var reports = context.Set<Report>()
                .Where(r => changedParams.Contains(r.Id))
                .ToList();

            foreach (var report in reports)
            {
                var entry = context.Entry(report);

                if (entry.State != EntityState.Added)
                {
                    report.UpdatedOn = fixedTime;
                    report.UpdatedBy = userId;
                }
            }
        }

        return result;
    }
}