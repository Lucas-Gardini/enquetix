using MongoDB.Bson;

namespace enquetix.Modules.AuditLog.Repository
{
    public enum AccessLogOperation { Login, Logout }

    public class AccessLogModel
    {
        public ObjectId Id { get; set; }
        public required AccessLogOperation Operation { get; set; }
        public DateTime Timestamp { get; set; }
        public required string User { get; set; }
    }
}
