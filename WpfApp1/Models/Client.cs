using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Text.Json.Serialization;

namespace WpfApp1.Models
{
    [Table("clients")]
    public class Client : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = null!;

        [Column("full_name")]
        public string FullName { get; set; } = null!;

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}