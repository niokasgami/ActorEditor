// Views/MainWindow.axaml.cs
using Trickster.ViewModels;
using Avalonia.Controls;

namespace Trickster.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainViewModel();
  }
}
