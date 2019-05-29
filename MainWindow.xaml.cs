using System.Windows;
using System.Windows.Controls;
using Patience.Utils;
using Patience.ViewModels;

namespace Patience
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        public MainWindow(MainWindowViewModel model)
        {
            InitializeComponent();
            DataContext = model;
        }

        private void File1_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            file2.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void File1_OnSelectedLineChanged(object sender, SelectedLineChangedEventArgs args)
        {
            file2.SelectLineByIndex(args.LineIndex);
        }

        private void File2_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            file1.ScrollToVerticalOffset(e.VerticalOffset);
        }
        
        private void File2_OnSelectedLineChanged(object sender, SelectedLineChangedEventArgs args)
        {
            file1.SelectLineByIndex(args.LineIndex);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}
