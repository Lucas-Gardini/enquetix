using MongoDB.Bson;

namespace enquetix.Modules.AuditLog.Repository
{
    public enum LogOperation { Insert, Update, Delete };

    public class LogModel
    {
        public ObjectId Id { get; set; }
        public required string EntityName { get; set; }
        public required string EntityId { get; set; }
        public required LogOperation Operation { get; set; }
        public DateTime Timestamp { get; set; }
        public required string User { get; set; }
        public required Dictionary<string, object?> OldValues { get; set; }
        public required Dictionary<string, object?> NewValues { get; set; }
    }
}
