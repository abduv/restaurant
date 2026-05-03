using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Services;

public class RestaurantService
{
    public ObservableCollection<MenuItem> Menu { get; } = new();
    public ObservableCollection<Order> Orders { get; } = new();

    public RestaurantService()
    {
        InitializeDatabase();
        LoadData();
    }

    private void InitializeDatabase()
    {
        using var db = new RestaurantDbContext();
        db.Database.EnsureCreated();

        if (!db.MenuItems.Any())
        {
            db.MenuItems.AddRange(
                new MenuItem("Самса с мясом", "Слоёная выпечка с бараниной", 1500, Category.Закуски),
                new MenuItem("Манты", "Паровые пельмени с мясом и луком", 2500, Category.Закуски),
                new MenuItem("Чучвара", "Маленькие пельмени в бульоне", 2200, Category.Закуски),
                new MenuItem("Салат «Цезарь»", "Курица, салат, пармезан, сухарики", 3500, Category.Салаты),
                new MenuItem("Салат «Ташкент»", "Редька, мясо, зелень, майонез", 2800, Category.Салаты),
                new MenuItem("Ачик-чучук", "Салат из помидоров и лука", 1500, Category.Салаты),
                new MenuItem("Шурпа", "Наваристый суп из баранины с овощами", 3000, Category.Супы),
                new MenuItem("Мастава", "Рисовый суп с мясом и овощами", 2800, Category.Супы),
                new MenuItem("Плов", "Узбекский плов из баранины", 4000, Category.Горячее),
                new MenuItem("Шашлык из баранины", "Маринованная баранина на углях", 4500, Category.Горячее),
                new MenuItem("Лагман", "Домашняя лапша с мясом и овощами", 3500, Category.Горячее),
                new MenuItem("Казан-кабоб", "Мясо с картофелем в казане", 4200, Category.Горячее),
                new MenuItem("Чак-чак", "Медовая восточная сладость", 1800, Category.Десерты),
                new MenuItem("Пахлава", "Слоёная выпечка с орехами и мёдом", 2000, Category.Десерты),
                new MenuItem("Чай зелёный", "Узбекский зелёный чай (чайник)", 1000, Category.Напитки),
                new MenuItem("Компот", "Домашний компот из сухофруктов", 1200, Category.Напитки),
                new MenuItem("Лимонад", "Свежий домашний лимонад", 1500, Category.Напитки)
            );
            db.SaveChanges();
        }
    }

    private void LoadData()
    {
        using var db = new RestaurantDbContext();

        foreach (var item in db.MenuItems.AsNoTracking().ToList())
            Menu.Add(item);

        foreach (var order in db.Orders.Include(o => o.Items).ThenInclude(oi => oi.MenuItem).AsNoTracking().ToList())
        {
            var loaded = new Order
            {
                Id = order.Id,
                TableNumber = order.TableNumber,
                WaiterName = order.WaiterName,
                CreatedAt = order.CreatedAt,
                Status = order.Status
            };
            foreach (var oi in order.Items)
            {
                var menuItem = Menu.First(m => m.Id == oi.MenuItemId);
                loaded.Items.Add(new OrderItem(menuItem, oi.Quantity) { Id = oi.Id, OrderId = oi.OrderId, MenuItemId = oi.MenuItemId });
            }
            Orders.Add(loaded);
        }
    }

    public void AddMenuItem(string name, string description, decimal price, Category category)
    {
        var item = new MenuItem(name, description, price, category);
        using var db = new RestaurantDbContext();
        db.MenuItems.Add(item);
        db.SaveChanges();
        Menu.Add(item);
    }

    public void RemoveMenuItem(MenuItem item)
    {
        using var db = new RestaurantDbContext();
        var entity = db.MenuItems.Find(item.Id);
        if (entity != null)
        {
            db.MenuItems.Remove(entity);
            db.SaveChanges();
        }
        Menu.Remove(item);
    }

    public Order CreateOrder(int tableNumber, string waiterName)
    {
        var order = new Order(tableNumber, waiterName);
        using var db = new RestaurantDbContext();
        db.Orders.Add(order);
        db.SaveChanges();
        Orders.Add(order);
        return order;
    }

    public void SaveOrder(Order order)
    {
        using var db = new RestaurantDbContext();
        var entity = db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == order.Id);
        if (entity == null) return;

        entity.Status = order.Status;

        // Sync items
        var existingIds = entity.Items.Select(i => i.Id).ToHashSet();
        var currentIds = order.Items.Where(i => i.Id > 0).Select(i => i.Id).ToHashSet();

        // Remove deleted items
        foreach (var removed in entity.Items.Where(i => !currentIds.Contains(i.Id)).ToList())
            db.OrderItems.Remove(removed);

        // Update or add items
        foreach (var item in order.Items)
        {
            if (item.Id > 0 && existingIds.Contains(item.Id))
            {
                var existing = entity.Items.First(i => i.Id == item.Id);
                existing.Quantity = item.Quantity;
            }
            else
            {
                entity.Items.Add(new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity
                });
            }
        }

        db.SaveChanges();

        // Update IDs for newly added items
        var savedOrder = db.Orders.Include(o => o.Items).AsNoTracking().First(o => o.Id == order.Id);
        foreach (var item in order.Items.Where(i => i.Id == 0))
        {
            var match = savedOrder.Items.FirstOrDefault(si =>
                si.MenuItemId == item.MenuItemId && !order.Items.Any(oi => oi.Id == si.Id && oi.Id != 0));
            if (match != null) item.Id = match.Id;
        }
    }

    public void SaveMenuItem(MenuItem item)
    {
        using var db = new RestaurantDbContext();
        var entity = db.MenuItems.Find(item.Id);
        if (entity == null) return;
        entity.Name = item.Name;
        entity.Description = item.Description;
        entity.Price = item.Price;
        entity.Category = item.Category;
        entity.IsAvailable = item.IsAvailable;
        db.SaveChanges();
    }

    public ObservableCollection<Order> GetActiveOrders()
    {
        var active = Orders.Where(o => o.Status != OrderStatus.Оплачен && o.Status != OrderStatus.Отменён);
        return new ObservableCollection<Order>(active);
    }

    public decimal GetTotalRevenue() => Orders.Where(o => o.Status == OrderStatus.Оплачен).Sum(o => o.Total);
    public int GetPaidCount() => Orders.Count(o => o.Status == OrderStatus.Оплачен);
    public int GetCancelledCount() => Orders.Count(o => o.Status == OrderStatus.Отменён);
    public int GetActiveCount() => Orders.Count(o => o.Status != OrderStatus.Оплачен && o.Status != OrderStatus.Отменён);
}
