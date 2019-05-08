using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public event ScrollChangedEventHandler ScrollChanged;
        
        public static readonly DependencyProperty ScrollBarVisibilityProperty = DependencyProperty.Register(
            "ScrollBarVisibility", typeof(ScrollBarVisibility), typeof(BindableRichTextBox), new PropertyMetadata(ScrollBarVisibility.Visible));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility) GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public BindableRichTextBox()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string text)
            {
                var doc = new FlowDocument { PageWidth = 1000 }; // fast resize tweak
                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lineNumbers = new StringBuilder();
                var currentLineNumber = 1;
                foreach (var line in lines)
                {
                    var para = new Paragraph();
                    para.Inlines.Add(line);
                    doc.Blocks.Add(para);
                    lineNumbers.AppendLine($"{currentLineNumber++}");
                }
                textBox.Document = doc;
                lineBox.Text = lineNumbers.ToString();
            }
        }

        public void ScrollToVerticalOffset(double offset)
        {
            textBox.ScrollToVerticalOffset(offset);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            lineBox.ScrollToVerticalOffset(e.VerticalOffset);
            ScrollChanged?.Invoke(this, e);
        }
    }
}
