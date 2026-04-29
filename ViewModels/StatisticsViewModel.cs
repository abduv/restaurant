using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly RestaurantService _service;

    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private int _activeOrders;
    [ObservableProperty] private int _paidOrders;
    [ObservableProperty] private int _cancelledOrders;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private string _topDish = "—";
    [ObservableProperty] private decimal _averageCheck;

    public StatisticsViewModel(RestaurantService service)
    {
        _service = service;
        Refresh();
    }

    [RelayCommand]
    public void Refresh()
    {
        TotalOrders = _service.Orders.Count;
        ActiveOrders = _service.GetActiveCount();
        PaidOrders = _service.GetPaidCount();
        CancelledOrders = _service.GetCancelledCount();
        TotalRevenue = _service.GetTotalRevenue();

        var allItems = _service.Orders
            .Where(o => o.Status == OrderStatus.Оплачен)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.MenuItem.Name)
            .OrderByDescending(g => g.Sum(i => i.Quantity))
            .FirstOrDefault();

        TopDish = allItems != null ? $"{allItems.Key} ({allItems.Sum(i => i.Quantity)} шт.)" : "—";
        AverageCheck = PaidOrders > 0 ? TotalRevenue / PaidOrders : 0;
    }
}
