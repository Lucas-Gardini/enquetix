using enquetix.Modules.Application;
using enquetix.Modules.AuditLog.Repository;

namespace enquetix.Modules.AuditLog.Services
{
    public class AuditLogService(IMongoDBService mongoDBService) : IAuditLogService
    {
        public async Task SaveLog(LogModel log)
        {
            ArgumentNullException.ThrowIfNull(log);
            await mongoDBService.InsertAsync(log);
        }

        public async Task SaveAccessLog(AccessLogModel log)
        {
            ArgumentNullException.ThrowIfNull(log);
            await mongoDBService.InsertAsync(log);
        }
    }

    public interface IAuditLogService
    {
        Task SaveLog(LogModel log);
        Task SaveAccessLog(AccessLogModel log);
    }
}
