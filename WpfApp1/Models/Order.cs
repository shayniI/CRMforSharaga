using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Text.Json.Serialization;

namespace WpfApp1.Models
{
    [Table("orders")]
    public class Order : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = null!;

        [Column("client_id")]
        public string? ClientId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "new";

        [Column("total")]
        public decimal Total { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Опционально: при запросе с join поле items может заполняться
        [Column("items")]
        public List<OrderItem>? Items { get; set; }
    }
}