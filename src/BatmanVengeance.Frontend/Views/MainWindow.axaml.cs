using Avalonia.Controls;
using BatmanVengeance.Frontend.ViewModels;

namespace BatmanVengeance.Frontend.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var viewModel = (MainWindowViewModel)DataContext;
        if (viewModel != null)
        {
            var scrollViewer = sender as ScrollViewer;
            viewModel.OnScrollChanged(ref  scrollViewer, scrollViewer?.Offset.Y, scrollViewer.Extent.Height - scrollViewer.Bounds.Height);
        }
    }
}