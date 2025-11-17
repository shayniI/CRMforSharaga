using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;
using System.Text.Json.Serialization;
namespace WpfApp1.Models
{
    [Table("products")]
    public class Product : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = null!;

        [Column("sku")]
        public string? Sku { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("stock")]
        public int Stock { get; set; }

        [Column("cost")]
        public decimal Cost { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}