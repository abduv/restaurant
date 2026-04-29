using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly RestaurantService _service;

    public ObservableCollection<MenuItem> FilteredItems { get; } = new();
    public Array Categories => Enum.GetValues(typeof(Category));

    [ObservableProperty] private MenuItem? _selectedItem;
    [ObservableProperty] private string _activeFilter = "Все";

    [ObservableProperty] private string _newName = "";
    [ObservableProperty] private string _newDescription = "";
    [ObservableProperty] private string _newPrice = "";
    [ObservableProperty] private Category _newCategory;

    public MenuViewModel(RestaurantService service)
    {
        _service = service;
        ApplyFilter();
        _service.Menu.CollectionChanged += (_, _) => ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredItems.Clear();
        var items = ActiveFilter == "Все"
            ? _service.Menu.AsEnumerable()
            : _service.Menu.Where(m => m.Category.ToString() == ActiveFilter);

        foreach (var item in items)
            FilteredItems.Add(item);
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        ActiveFilter = filter;
        ApplyFilter();
    }

    [RelayCommand]
    private void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewName) || !decimal.TryParse(NewPrice, out decimal price) || price <= 0)
            return;

        _service.AddMenuItem(NewName.Trim(), NewDescription.Trim(), price, NewCategory);
        NewName = "";
        NewDescription = "";
        NewPrice = "";
        ApplyFilter();
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem == null) return;
        _service.RemoveMenuItem(SelectedItem);
        SelectedItem = null;
        ApplyFilter();
    }

    [RelayCommand]
    private void ToggleAvailability()
    {
        if (SelectedItem == null) return;
        SelectedItem.IsAvailable = !SelectedItem.IsAvailable;
        ApplyFilter();
    }
}
