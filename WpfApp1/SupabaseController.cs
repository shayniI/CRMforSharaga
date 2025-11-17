using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Supabase;
using WpfApp1.Models;

namespace WpfApp1
{
    public static class SupabaseController
    {
        public static async Task<Profile?> GetProfileAsync(string userId)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            var response = await App._Supabase.From<Profile>().Where(p => p.Id == userId).Get();
            return response.Models.FirstOrDefault();
        }

        public static async Task<Profile?> UpsertProfileAsync(Profile profile)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");
            var ins = await App._Supabase.From<Profile>().Insert(profile);
            return ins.Models.FirstOrDefault();
        }

        public static async Task<bool> SetUserRoleAsync(string userId, string role)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            role = role == "admin" ? "admin" : "user";
            var res = await App._Supabase.From<Profile>().Where(p => p.Id == userId).Update(new Profile { Role = role });
            return res.Models != null;
        }

        public static async Task<WpfApp1.Models.Client[]?> GetClientsAsync()
        {
            var res = await App._Supabase!.From<WpfApp1.Models.Client>().Get();
            return res.Models.ToArray();
        }

        public static async Task<WpfApp1.Models.Client?> CreateClientAsync(WpfApp1.Models.Client client)
        {
            var res = await App._Supabase!.From<WpfApp1.Models.Client>().Insert(client);
            return res.Models.FirstOrDefault();
        }

        public static async Task<WpfApp1.Models.Client?> UpdateClientAsync(WpfApp1.Models.Client client)
        {
            var res = await App._Supabase!.From<WpfApp1.Models.Client>().Where(c => c.Id == client.Id).Update(client);
            return res.Models.FirstOrDefault();
        }

        public static async Task<bool> DeleteClientAsync(string clientId)
        {
            await App._Supabase!.From<WpfApp1.Models.Client>().Where(c => c.Id == clientId).Delete();
            return true;
        }

        public static async Task<WpfApp1.Models.Client?> GetClientByIdAsync(string clientId)
        {
            var res = await App._Supabase!.From<WpfApp1.Models.Client>().Where(c => c.Id == clientId).Get();
            return res.Models.FirstOrDefault();
        }

        public static async Task<Product[]?> GetProductsAsync()
        {
            var res = await App._Supabase!.From<Product>().Get();
            return res.Models.ToArray();
        }

        public static async Task<Product?> CreateProductAsync(Product product)
        {
            var res = await App._Supabase!.From<Product>().Insert(product);
            return res.Models.FirstOrDefault();
        }

        public static async Task<bool> UpdateProductStockAsync(string productId, int newStock)
        {
            var res = await App._Supabase!.From<Product>().Where(p => p.Id == productId).Update(new Product { Stock = newStock });
            return res.Models != null;
        }

        public static async Task<Product?> UpdateProductAsync(Product product)
        {
            var res = await App._Supabase!.From<Product>().Where(p => p.Id == product.Id).Update(product);
            return res.Models.FirstOrDefault();
        }

        public static async Task<bool> DeleteProductAsync(string productId)
        {
            await App._Supabase!.From<Product>().Where(p => p.Id == productId).Delete();
            return true;
        }

        public static async Task<Product?> GetProductByIdAsync(string productId)
        {
            var res = await App._Supabase!.From<Product>().Where(p => p.Id == productId).Get();
            return res.Models.FirstOrDefault();
        }

        public static async Task<Order[]?> GetOrdersAsync()
        {
            var res = await App._Supabase!.From<Order>().Get();
            return res.Models.ToArray();
        }

        public static async Task<Order?> CreateOrderAsync(Order order)
        {
            if (App._Supabase == null) throw new InvalidOperationException();
            order.Id = null;
            var r = await App._Supabase.From<Order>().Insert(order);
            return r.Models.FirstOrDefault();
        }

        public static async Task<OrderItem[]?> GetOrderItemsAsync()
        {
            var res = await App._Supabase!.From<OrderItem>().Get();
            return res.Models.ToArray();
        }

        public static async Task<OrderItem[]?> GetOrderItemsByOrderIdAsync(string orderId)
        {
            var all = await GetOrderItemsAsync();
            return all?.Where(i => i.OrderId == orderId).ToArray();
        }

        public static async Task<OrderItem?> CreateOrderItemAsync(OrderItem item)
        {
            item.Id = null;
            var res = await App._Supabase!.From<OrderItem>().Insert(item);
            return res.Models.FirstOrDefault();
        }

        public static async Task<bool> DeleteOrderItemAsync(string itemId)
        {
            await App._Supabase!.From<OrderItem>().Where(i => i.Id == itemId).Delete();
            return true;
        }

        public static async Task<Order?> UpdateOrderAsync(Order order)
        {
            var res = await App._Supabase!.From<Order>().Where(o => o.Id == order.Id).Update(order);
            return res.Models.FirstOrDefault();
        }

        public static async Task<bool> DeleteOrderAsync(string orderId)
        {
            var items = await GetOrderItemsByOrderIdAsync(orderId);
            if (items != null)
            {
                foreach (var item in items)
                {
                    await DeleteOrderItemAsync(item.Id);
                }
            }
            await App._Supabase!.From<Order>().Where(o => o.Id == orderId).Delete();
            return true;
        }

        public static async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            var res = await App._Supabase!.From<Order>().Where(o => o.Id == orderId).Get();
            return res.Models.FirstOrDefault();
        }

        public static async Task<Invoice[]?> GetInvoicesAsync()
        {
            try
            {
                var res = await App._Supabase!.From<Invoice>().Get();
                return res.Models.OrderByDescending(i => i.CreatedAt).ToArray();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<decimal> GetDailyRevenueAsync(DateTime day)
        {
            var invoices = await GetInvoicesAsync();
            if (invoices == null) return 0;
            var start = day.Date;
            var end = start.AddDays(1);
            return invoices.Where(i => i.CreatedAt >= start && i.CreatedAt < end).Sum(i => i.Amount);
        }

        public static async Task<decimal> GetMonthlyProfitAsync(int year, int month)
        {
            var invoices = await GetInvoicesAsync();
            var orders = await GetOrdersAsync();
            var items = await GetOrderItemsAsync();
            var products = await GetProductsAsync();

            if (invoices == null || orders == null || items == null || products == null) return 0;

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var monthInvoices = invoices.Where(inv => inv.CreatedAt >= start && inv.CreatedAt < end).ToArray();
            var revenue = monthInvoices.Sum(inv => inv.Amount);

            var orderIds = orders.Where(o => o.CreatedAt >= start && o.CreatedAt < end).Select(o => o.Id).ToHashSet();
            var monthItems = items.Where(it => orderIds.Contains(it.OrderId)).ToArray();

            decimal cogs = 0m;
            foreach (var it in monthItems)
            {
                var prod = products.FirstOrDefault(p => p.Id == it.ProductId);
                if (prod != null) cogs += prod.Cost * it.Quantity;
            }

            return revenue - cogs;
        }

        public static async Task<(string productId, int count)[]> GetPopularProductsAsync(int top = 5)
        {
            var items = await GetOrderItemsAsync();
            if (items == null) return Array.Empty<(string, int)>();
            var grouped = items.Where(i => !string.IsNullOrEmpty(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => (productId: g.Key!, count: g.Sum(i => i.Quantity)))
                .OrderByDescending(x => x.count)
                .Take(top)
                .ToArray();
            return grouped;
        }

        public static async Task<(string productId, string productName, int count)[]> GetPopularProductsWithNamesAsync(int top = 5)
        {
            var popular = await GetPopularProductsAsync(top);
            var products = await GetProductsAsync();
            if (products == null) return Array.Empty<(string, string, int)>();

            return popular.Select(p =>
            {
                var product = products.FirstOrDefault(pr => pr.Id == p.productId);
                return (p.productId, product?.Name ?? "Неизвестный товар", p.count);
            }).ToArray();
        }

        public static async Task<(bool Ok, string? Error)> CreateUserAsync(string email, string password, string? fullName = null, string role = "user")
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            try
            {
                var session = await App._Supabase.Auth.SignUp(email, password);
                var user = session?.User;
                if (user == null)
                    return (false, "Регистрация вернула пустую сессию/пользователя.");

                var profile = new Profile
                {
                    Id = user.Id!,
                    FullName = fullName,
                    Role = role == "admin" ? "admin" : "user",
                    CreatedAt = DateTime.UtcNow
                };
                await UpsertProfileAsync(profile);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : ""));
            }
        }
    }

    public class Invoice : Supabase.Postgrest.Models.BaseModel
    {
        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? IssuedTo { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}