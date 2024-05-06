using Avalonia.Controls;
using Avalonia.Input;
using BatmanVengeance.Frontend.ViewModels;

namespace BatmanVengeance.Frontend.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NumericUpDown_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return && e.Key != Key.Enter)
            return;
        var textBox = (NumericUpDown)sender;
        ((MainWindowViewModel)DataContext).ShowItensController(int.Parse(textBox.Text));
        textBox.Text = ((MainWindowViewModel)DataContext).Page.ToString();
    }

}