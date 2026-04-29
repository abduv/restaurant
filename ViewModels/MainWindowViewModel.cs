using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Restaurant.Services;

namespace Restaurant.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly RestaurantService _service = new();

    [ObservableProperty] private ViewModelBase _currentView;
    [ObservableProperty] private int _selectedNavIndex;

    public MenuViewModel MenuVm { get; }
    public OrdersViewModel OrdersVm { get; }
    public StatisticsViewModel StatisticsVm { get; }

    public MainWindowViewModel()
    {
        MenuVm = new MenuViewModel(_service);
        OrdersVm = new OrdersViewModel(_service);
        StatisticsVm = new StatisticsViewModel(_service);
        _currentView = MenuVm;
    }

    [RelayCommand]
    private void NavigateToMenu()
    {
        CurrentView = MenuVm;
        SelectedNavIndex = 0;
    }

    [RelayCommand]
    private void NavigateToOrders()
    {
        OrdersVm.RefreshMenuCommand.Execute(null);
        CurrentView = OrdersVm;
        SelectedNavIndex = 1;
    }

    [RelayCommand]
    private void NavigateToStatistics()
    {
        StatisticsVm.Refresh();
        CurrentView = StatisticsVm;
        SelectedNavIndex = 2;
    }
}
