using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Text.Json.Serialization;

namespace WpfApp1.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("role")]
        public string Role { get; set; } = "user";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}