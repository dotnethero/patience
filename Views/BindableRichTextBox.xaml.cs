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
using DiffMatchPatch;
using Patience.Core;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class BindableRichTextBox : UserControl
    {
        public event ScrollChangedEventHandler ScrollChanged;

        #region Dependency properties

        public static readonly DependencyProperty ScrollBarVisibilityProperty = DependencyProperty.Register(
            "ScrollBarVisibility", typeof(ScrollBarVisibility), typeof(BindableRichTextBox), new PropertyMetadata(ScrollBarVisibility.Visible));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility) GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty ShowModeProperty = DependencyProperty.Register(
            "ShowMode", typeof(DiffShowMode), typeof(BindableRichTextBox), new PropertyMetadata(default(DiffShowMode)));

        public DiffShowMode ShowMode
        {
            get => (DiffShowMode) GetValue(ShowModeProperty);
            set => SetValue(ShowModeProperty, value);
        }

        #endregion

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

            if (e.NewValue is List<Diff> diffs)
            {
                var doc = new FlowDocument { PageWidth = 1000 }; // fast resize tweak
                var lightRed = new SolidColorBrush(Color.FromRgb(255, 204, 204));
                var hardRed = new SolidColorBrush(Color.FromRgb(255, 153, 153));
                var hardGreen = new SolidColorBrush(Color.FromRgb(215, 227, 188));
                var lightGreen = new SolidColorBrush(Color.FromRgb(235, 241, 221));
                var gray = Brushes.LightGray;
                var para = new Paragraph();
                foreach (var diff in diffs)
                {
                    var inline = new Run();
                    if (diff.operation == Operation.INSERT && ShowMode == DiffShowMode.File1)
                    {
                        inline.Foreground = gray;
                    }
                    if (diff.operation == Operation.INSERT && ShowMode == DiffShowMode.File2)
                    {
                        inline.Background = hardGreen;
                    }
                    if (diff.operation == Operation.DELETE && ShowMode == DiffShowMode.File1)
                    {
                        inline.Background = hardRed;
                    }
                    if (diff.operation == Operation.DELETE && ShowMode == DiffShowMode.File2)
                    {
                        inline.Foreground = gray;
                    }
                    inline.Text = diff.text;
                    para.Inlines.Add(inline);
                }
                doc.Blocks.Add(para);
                textBox.Document = doc;
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
