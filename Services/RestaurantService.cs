using System;
using System.Collections.ObjectModel;
using System.Linq;
using Restaurant.Models;

namespace Restaurant.Services;

public class RestaurantService
{
    public ObservableCollection<MenuItem> Menu { get; } = new();
    public ObservableCollection<Order> Orders { get; } = new();

    public RestaurantService()
    {
        SeedMenu();
    }

    private void SeedMenu()
    {
        var items = new[]
        {
            new MenuItem("Самса с мясом", "Слоёная выпечка с бараниной", 15000, Category.Закуски),
            new MenuItem("Манты", "Паровые пельмени с мясом и луком", 25000, Category.Закуски),
            new MenuItem("Чучвара", "Маленькие пельмени в бульоне", 22000, Category.Закуски),
            new MenuItem("Салат «Цезарь»", "Курица, салат, пармезан, сухарики", 35000, Category.Салаты),
            new MenuItem("Салат «Ташкент»", "Редька, мясо, зелень, майонез", 28000, Category.Салаты),
            new MenuItem("Ачик-чучук", "Салат из помидоров и лука", 15000, Category.Салаты),
            new MenuItem("Шурпа", "Наваристый суп из баранины с овощами", 30000, Category.Супы),
            new MenuItem("Мастава", "Рисовый суп с мясом и овощами", 28000, Category.Супы),
            new MenuItem("Плов", "Узбекский плов из баранины", 40000, Category.Горячее),
            new MenuItem("Шашлык из баранины", "Маринованная баранина на углях", 45000, Category.Горячее),
            new MenuItem("Лагман", "Домашняя лапша с мясом и овощами", 35000, Category.Горячее),
            new MenuItem("Казан-кабоб", "Мясо с картофелем в казане", 42000, Category.Горячее),
            new MenuItem("Чак-чак", "Медовая восточная сладость", 18000, Category.Десерты),
            new MenuItem("Пахлава", "Слоёная выпечка с орехами и мёдом", 20000, Category.Десерты),
            new MenuItem("Чай зелёный", "Узбекский зелёный чай (чайник)", 10000, Category.Напитки),
            new MenuItem("Компот", "Домашний компот из сухофруктов", 12000, Category.Напитки),
            new MenuItem("Лимонад", "Свежий домашний лимонад", 15000, Category.Напитки),
        };

        foreach (var item in items)
            Menu.Add(item);
    }

    public void AddMenuItem(string name, string description, decimal price, Category category)
    {
        Menu.Add(new MenuItem(name, description, price, category));
    }

    public void RemoveMenuItem(MenuItem item) => Menu.Remove(item);

    public Order CreateOrder(int tableNumber, string waiterName)
    {
        var order = new Order(tableNumber, waiterName);
        Orders.Add(order);
        return order;
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
