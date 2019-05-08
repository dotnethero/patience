using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class BindableRichTextBox : UserControl
    {
        public BindableRichTextBox()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string text)
            {
                var doc = new FlowDocument();
                doc.PageWidth = 1000; // fast resize tweak
                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    var para = new Paragraph();
                    para.Inlines.Add(line);
                    doc.Blocks.Add(para);
                }
                textBox.Document = doc;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
