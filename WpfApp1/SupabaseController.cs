using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Supabase;
using WpfApp1.Models;
using System.Windows;

namespace WpfApp1
{
    public static class SupabaseController
    {
        // Получить профиль по user id
        public static async Task<Profile?> GetProfileAsync(string userId)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            var response = await App._Supabase.From<Profile>().Where(p => p.Id == userId).Get();
            return response.Models.FirstOrDefault();
        }

        // Создать или обновить профиль
        public static async Task<Profile?> UpsertProfileAsync(Profile profile)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");
            MessageBox.Show(profile.Id);
                var ins = await App._Supabase.From<Profile>().Insert(profile);
                return ins.Models.FirstOrDefault();
        }

        // Назначить роль (admin/user)
        public static async Task<bool> SetUserRoleAsync(string userId, string role)
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            role = role == "admin" ? "admin" : "user";
            var res = await App._Supabase.From<Profile>().Where(p => p.Id == userId).Update(new Profile { Role = role });
            return res.Models != null;
        }

        // Клиенты
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

        // Продукты
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

        // Заказы и позиции
        public static async Task<Order[]?> GetOrdersAsync()
        {
            var res = await App._Supabase!.From<Order>().Get();
            return res.Models.ToArray();
        }

        public static async Task<Order?> CreateOrderAsync(Order order)
        {
            if (App._Supabase == null) throw new InvalidOperationException();
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

        // Invoices
        public static async Task<Invoice[]?> GetInvoicesAsync()
        {
            try
            {
                // Удаляем несуществующий параметр OrderBy и сортируем вручную после получения данных
                var res = await App._Supabase!.From<Invoice>().Get();
                return res.Models.OrderByDescending(i => i.CreatedAt).ToArray();
            }
            catch
            {
                return null;
            }
        }

        // Отчёты: простая реализация на стороне клиента

        // Ежедневная выручка (сумма invoices за сегодня)
        public static async Task<decimal> GetDailyRevenueAsync(DateTime day)
        {
            var invoices = await GetInvoicesAsync();
            if (invoices == null) return 0;
            var start = day.Date;
            var end = start.AddDays(1);
            return invoices.Where(i => i.CreatedAt >= start && i.CreatedAt < end).Sum(i => i.Amount);
        }

        // Месячная прибыль (sum(invoice.amount) - sum(product.cost * qty) за месяц)
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

            // Найдём связанные order_items за те же заказы
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

        // Популярные товары (по количеству в order_items)
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

        // Создать пользователя через auth и вернуть подробный результат
        public static async Task<(bool Ok, string? Error)> CreateUserAsync(string email, string password, string? fullName = null, string role = "user")
        {
            if (App._Supabase == null)
                throw new InvalidOperationException("Supabase client not initialized.");

            try
            {
                var session = await App._Supabase.Auth.SignUp(email, password);
                var user = session?.User;
                MessageBox.Show("Debug: User created with ID: " + user?.Id);
                if (user == null)
                    return (false, "Регистрация вернула пустую сессию/пользователя.");

                var profile = new Profile
                {
                    Id = user.Id!,
                    FullName = fullName,
                    Role = role == "admin" ? "admin" : "user",
                    CreatedAt = DateTime.UtcNow
                };
                MessageBox.Show(profile.Id);
                await UpsertProfileAsync(profile);
                return (true, null);
            }
            catch (Exception ex)
            {
                // Возвращаем детальный текст ошибки для отображения в UI
                return (false, ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : ""));
            }
        }
    }

    // Доп. модель Invoice для работы отчётов
    public class Invoice : Supabase.Postgrest.Models.BaseModel
    {
        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? IssuedTo { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}