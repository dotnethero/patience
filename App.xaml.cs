using System.IO;
using System.Windows;
using Patience.ViewModels;

namespace Patience
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var model = e.Args.Length == 2 && File.Exists(e.Args[0]) && File.Exists(e.Args[1])
                ? new MainWindowViewModel(e.Args[0], e.Args[1])
                : new MainWindowViewModel("Data/before.txt", "Data/after.txt");

            var window = new MainWindow(model);
            window.Show();
            MainWindow = window;
        }
    }
}
