using System.ComponentModel.DataAnnotations;

namespace ClaimsProcessingSystem.Models
{
    public class Audit
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Action { get; set; } // e.g., "Added", "Updated", "Deleted"
        public string? TableName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; } // JSON string of old data
        public string? NewValues { get; set; } // JSON string of new data
        public string? PrimaryKey { get; set; }
    }
}