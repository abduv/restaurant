using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels;

public partial class OrdersViewModel : ViewModelBase
{
    private readonly RestaurantService _service;

    public ObservableCollection<Order> Orders => _service.Orders;
    public ObservableCollection<MenuItem> AvailableMenu { get; } = new();

    [ObservableProperty] private Order? _selectedOrder;
    [ObservableProperty] private MenuItem? _selectedMenuItem;
    [ObservableProperty] private OrderItem? _selectedOrderItem;
    [ObservableProperty] private string _tableNumber = "";
    [ObservableProperty] private string _waiterName = "";
    [ObservableProperty] private int _addQuantity = 1;
    [ObservableProperty] private bool _showOnlyActive = true;
    [ObservableProperty] private string _receiptText = "";
    [ObservableProperty] private bool _showReceipt;

    public ObservableCollection<Order> FilteredOrders { get; } = new();

    public OrdersViewModel(RestaurantService service)
    {
        _service = service;
        RefreshAvailableMenu();
        RefreshFilteredOrders();
        _service.Orders.CollectionChanged += (_, _) => RefreshFilteredOrders();
    }

    partial void OnShowOnlyActiveChanged(bool value) => RefreshFilteredOrders();

    private void RefreshAvailableMenu()
    {
        AvailableMenu.Clear();
        foreach (var item in _service.Menu.Where(m => m.IsAvailable))
            AvailableMenu.Add(item);
    }

    private void RefreshFilteredOrders()
    {
        FilteredOrders.Clear();
        var orders = ShowOnlyActive
            ? _service.Orders.Where(o => o.Status != OrderStatus.Оплачен && o.Status != OrderStatus.Отменён)
            : _service.Orders.AsEnumerable();

        foreach (var order in orders)
            FilteredOrders.Add(order);
    }

    [RelayCommand]
    private void CreateOrder()
    {
        if (!int.TryParse(TableNumber, out int table) || table <= 0 || string.IsNullOrWhiteSpace(WaiterName))
            return;

        var order = _service.CreateOrder(table, WaiterName.Trim());
        TableNumber = "";
        WaiterName = "";
        RefreshFilteredOrders();
        SelectedOrder = order;
    }

    [RelayCommand]
    private void AddItemToOrder()
    {
        if (SelectedOrder == null || SelectedMenuItem == null || AddQuantity <= 0) return;
        if (SelectedOrder.Status == OrderStatus.Оплачен || SelectedOrder.Status == OrderStatus.Отменён) return;

        SelectedOrder.AddItem(SelectedMenuItem, AddQuantity);
        AddQuantity = 1;
        OnPropertyChanged(nameof(SelectedOrder));
    }

    [RelayCommand]
    private void RemoveItemFromOrder()
    {
        if (SelectedOrder == null || SelectedOrderItem == null) return;
        SelectedOrder.RemoveItem(SelectedOrderItem);
        SelectedOrderItem = null;
        OnPropertyChanged(nameof(SelectedOrder));
    }

    [RelayCommand]
    private void SetCooking()
    {
        if (SelectedOrder == null) return;
        SelectedOrder.Status = OrderStatus.Готовится;
        RefreshFilteredOrders();
    }

    [RelayCommand]
    private void SetReady()
    {
        if (SelectedOrder == null) return;
        SelectedOrder.Status = OrderStatus.Готов;
        RefreshFilteredOrders();
    }

    [RelayCommand]
    private void PayOrder()
    {
        if (SelectedOrder == null || SelectedOrder.Items.Count == 0) return;
        SelectedOrder.Status = OrderStatus.Оплачен;
        GenerateReceipt(SelectedOrder);
        RefreshFilteredOrders();
    }

    [RelayCommand]
    private void CancelOrder()
    {
        if (SelectedOrder == null) return;
        SelectedOrder.Status = OrderStatus.Отменён;
        RefreshFilteredOrders();
    }

    [RelayCommand]
    private void CloseReceipt()
    {
        ShowReceipt = false;
        ReceiptText = "";
    }

    [RelayCommand]
    public void RefreshMenu()
    {
        RefreshAvailableMenu();
    }

    private void GenerateReceipt(Order order)
    {
        var lines = new System.Collections.Generic.List<string>
        {
            "══════════════════════════════════",
            "        РЕСТОРАН «ВОСТОК»",
            "══════════════════════════════════",
            $"  Заказ №{order.Id}     Стол №{order.TableNumber}",
            $"  Официант: {order.WaiterName}",
            $"  Дата: {order.CreatedAt:dd.MM.yyyy HH:mm}",
            "──────────────────────────────────"
        };

        foreach (var item in order.Items)
        {
            lines.Add($"  {item.MenuItem.Name}");
            lines.Add($"    {item.Quantity} x {item.MenuItem.Price:N0} = {item.Subtotal:N0} сум");
        }

        lines.Add("──────────────────────────────────");
        lines.Add($"  ИТОГО: {order.Total:N0} сум");
        lines.Add("══════════════════════════════════");
        lines.Add("      Спасибо за визит!");

        ReceiptText = string.Join("\n", lines);
        ShowReceipt = true;
    }
}
