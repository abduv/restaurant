using CommunityToolkit.Mvvm.ComponentModel;

namespace Restaurant.Models;

public enum Category
{
    Закуски,
    Салаты,
    Супы,
    Горячее,
    Десерты,
    Напитки
}

public partial class MenuItem : ObservableObject
{
    private static int _nextId = 1;

    public int Id { get; }

    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private Category _category;
    [ObservableProperty] private bool _isAvailable = true;

    public MenuItem(string name, string description, decimal price, Category category)
    {
        Id = _nextId++;
        _name = name;
        _description = description;
        _price = price;
        _category = category;
    }
}
