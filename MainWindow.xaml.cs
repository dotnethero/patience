using System.Windows;
using System.Windows.Controls;
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
        
        private void File1_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            file2.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void File2_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            file1.ScrollToVerticalOffset(e.VerticalOffset);
        }
    }
}
