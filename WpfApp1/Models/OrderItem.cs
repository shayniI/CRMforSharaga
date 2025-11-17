using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Text.Json.Serialization;
namespace WpfApp1.Models
{
    [Table("order_items")]
    public class OrderItem : BaseModel
    {
        [PrimaryKey("id")]
        public string? Id { get; set; }

        [Column("order_id")]
        public string OrderId { get; set; } = null!;

        [Column("product_id")]
        public string? ProductId { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        [System.Runtime.Serialization.IgnoreDataMember]
        [JsonIgnore]
        public decimal Total { get; set; }
    }
}