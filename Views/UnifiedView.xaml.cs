using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Patience.Models;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class UnifiedView : UserControl
    {
        private readonly UserControlBrushes _brushes;

        public event ScrollChangedEventHandler ScrollChanged;
        
        private class UserControlBrushes
        {
            public Brush Deleted { get; set; }
            public Brush Inserted { get; set; }
            public Brush InlineDeleted { get; set; }
            public Brush InlineInserted { get; set; }
        }

        public UnifiedView()
        {
            InitializeComponent();

            _brushes = new UserControlBrushes
            {
                Deleted = new SolidColorBrush(Color.FromRgb(255, 204, 204)), 
                Inserted = new SolidColorBrush(Color.FromRgb(235, 241, 221)), 
                InlineDeleted = new SolidColorBrush(Color.FromRgb(255, 153, 153)),
                InlineInserted = new SolidColorBrush(Color.FromRgb(215, 227, 188))
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is List<LineDiff> diffs)
            {
                var document = new FlowDocument { PageWidth = 3000 }; // fast resize tweak
                var leftNumbers = new StringBuilder();
                var rightNumbers = new StringBuilder();
                var unifiedLeftNumber = 1;
                var unifiedRightNumber = 1;
                foreach (var line in diffs)
                {
                    var paragraphs = CreateParagraphs(line);
                    foreach (var paragraph in paragraphs)
                    {
                        document.Blocks.Add(paragraph);
                    }
                    if (line.Operation == Operation.Modify)
                    {
                        leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                        leftNumbers.AppendLine("");
                        rightNumbers.AppendLine();
                        rightNumbers.AppendLine($"{unifiedRightNumber++}");
                    }
                    if (line.Operation == Operation.Insert)
                    {
                        leftNumbers.AppendLine();
                        rightNumbers.AppendLine($"{unifiedRightNumber++}");
                    }
                    if (line.Operation == Operation.Delete)
                    {
                        leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                        rightNumbers.AppendLine();
                    }
                    if (line.Operation == Operation.Equal)
                    {
                        leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                        rightNumbers.AppendLine($"{unifiedRightNumber++}");
                    }
                }
                textBox.Document = document;
                leftNumbersBox.Text = leftNumbers.ToString();
                rightNumbersBox.Text = rightNumbers.ToString();
            }
        }

        private IEnumerable<Paragraph> CreateParagraphs(LineDiff line)
        {
            var paragraph = new Paragraph();

            if (line.Operation == Operation.Modify)
            {
                var p1 = new Paragraph();
                var p2 = new Paragraph();

                p1.Background = _brushes.Deleted;
                p2.Background = _brushes.Inserted;

                foreach (var diff in line.Diffs)
                {
                    var inline1 = new Run();
                    var inline2 = new Run();
                    if (diff.Operation == Operation.Insert)
                    {
                        inline2.Text = diff.Text;
                        inline2.Background = _brushes.InlineInserted;
                    }
                    if (diff.Operation == Operation.Delete)
                    {
                        inline1.Text = diff.Text;
                        inline1.Background = _brushes.InlineDeleted;
                    }
                    if (diff.Operation == Operation.Equal)
                    {
                        inline1.Text = diff.Text;
                        inline2.Text = diff.Text;
                    }
                    p1.Inlines.Add(inline1);
                    p2.Inlines.Add(inline2);
                }

                yield return p1;
                yield return p2;
                yield break;
            }

            if (line.Operation == Operation.Insert)
            {
                paragraph.Background = _brushes.Inserted;
            }
            if (line.Operation == Operation.Delete)
            {
                paragraph.Background = _brushes.Deleted;
            }
            
            foreach (var diff in line.Diffs)
            {
                var inline = new Run();
                inline.Text = diff.Text;
                paragraph.Inlines.Add(inline);
            }

            yield return paragraph;
        }

        public void ScrollToVerticalOffset(double offset)
        {
            textBox.ScrollToVerticalOffset(offset);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            leftNumbersBox.ScrollToVerticalOffset(e.VerticalOffset);
            rightNumbersBox.ScrollToVerticalOffset(e.VerticalOffset);
            ScrollChanged?.Invoke(this, e);
        }
    }
}
