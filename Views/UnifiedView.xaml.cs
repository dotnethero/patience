using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Patience.Core;
using Patience.Models;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class UnifiedView : UserControl
    {
        private readonly UserControlBrushes _brushes;

        private Paragraph _previousSelected;
        private Brush _previousSelectedBackground;

        public event ScrollChangedEventHandler ScrollChanged;
        
        private class UserControlBrushes
        {
            public Brush Deleted { get; set; }
            public Brush Inserted { get; set; }
            public Brush InlineDeleted { get; set; }
            public Brush InlineInserted { get; set; }
            public Brush Focus { get; set; }
        }

        public UnifiedView()
        {
            InitializeComponent();

            _brushes = new UserControlBrushes
            {
                Deleted = new SolidColorBrush(Color.FromRgb(255, 204, 204)), 
                Inserted = new SolidColorBrush(Color.FromRgb(235, 241, 221)), 
                InlineDeleted = new SolidColorBrush(Color.FromRgb(255, 153, 153)),
                InlineInserted = new SolidColorBrush(Color.FromRgb(215, 227, 188)),
                Focus = new SolidColorBrush(Color.FromRgb(255, 235, 180)),
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
                var first = true;
                var packs = PackOperations(diffs);
                foreach (var pack in packs)
                {
                    var paragraphs = CreateParagraphs(pack);
                    foreach (var paragraph in paragraphs)
                    {
                        if (first)
                        {
                            _previousSelected = paragraph;
                            _previousSelectedBackground = paragraph.Background;
                            paragraph.Background = _brushes.Focus;
                            first = false;
                        }
                        document.Blocks.Add(paragraph);
                    }

                    var line1 = pack[0];
                    if (line1.Operation == Operation.Modify)
                    {
                        for (var i = 0; i < pack.Count; i++)
                        {
                            leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                        }
                        for (var i = 0; i < pack.Count; i++)
                        {
                            leftNumbers.AppendLine("");
                            rightNumbers.AppendLine();
                        }
                        for (var i = 0; i < pack.Count; i++)
                        {
                            rightNumbers.AppendLine($"{unifiedRightNumber++}");
                        }
                    }
                    if (line1.Operation == Operation.Insert)
                    {
                        for (var i = 0; i < pack.Count; i++)
                        {
                            leftNumbers.AppendLine();
                        }
                        for (var i = 0; i < pack.Count; i++)
                        {
                            rightNumbers.AppendLine($"{unifiedRightNumber++}");
                        }
                    }
                    if (line1.Operation == Operation.Delete)
                    {
                        for (var i = 0; i < pack.Count; i++)
                        {
                            leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                        }
                        for (var i = 0; i < pack.Count; i++)
                        {
                            rightNumbers.AppendLine();
                        }
                    }
                    if (line1.Operation == Operation.Equal)
                    {
                        for (var i = 0; i < pack.Count; i++)
                        {
                            leftNumbers.AppendLine($"{unifiedLeftNumber++}");
                            rightNumbers.AppendLine($"{unifiedRightNumber++}");
                        }
                    }
                }
                textBox.Document = document;
                leftNumbersBox.Text = leftNumbers.ToString();
                rightNumbersBox.Text = rightNumbers.ToString();
            }
        }

        private List<List<LineDiff>> PackOperations(List<LineDiff> lines)
        {
            var pack = new List<List<LineDiff>>(lines.Count);
            var current = new List<LineDiff>();
            for (var i = 0; i < lines.Count; i++)
            {
                if (i > 0 && lines[i].Operation == lines[i - 1].Operation)
                {
                    current.Add(lines[i]);
                }
                else
                {
                    if (current.Count > 0)
                    {
                        pack.Add(current);
                    }
                    current = new List<LineDiff> { lines[i] };
                }
            }
            if (current.Count > 0)
            {
                pack.Add(current);
            }
            return pack;
        }

        private IEnumerable<Paragraph> CreateParagraphs(List<LineDiff> pack)
        {
            var line1 = pack[0];
            if (line1.Operation == Operation.Modify)
            {
                foreach (var line in pack)
                {
                    var paragraph = new Paragraph { Background = _brushes.Deleted };
                    foreach (var diff in line.Diffs)
                    {
                        var inline1 = new Run();
                        if (diff.Operation == Operation.Delete)
                        {
                            inline1.Text = diff.Text;
                            inline1.Background = _brushes.InlineDeleted;
                        }
                        if (diff.Operation == Operation.Equal)
                        {
                            inline1.Text = diff.Text;
                        }
                        paragraph.Inlines.Add(inline1);
                    }
                    yield return paragraph;
                }
                
                foreach (var line in pack)
                {
                    var paragraph = new Paragraph { Background = _brushes.Inserted };
                    foreach (var diff in line.Diffs)
                    {
                        var inline2 = new Run();
                        if (diff.Operation == Operation.Insert)
                        {
                            inline2.Text = diff.Text;
                            inline2.Background = _brushes.InlineInserted;
                        }
                        if (diff.Operation == Operation.Equal)
                        {
                            inline2.Text = diff.Text;
                        }
                        paragraph.Inlines.Add(inline2);
                    }
                    yield return paragraph;
                }
            }

            if (line1.Operation == Operation.Insert)
            {
                foreach (var line in pack)
                {
                    var paragraph = new Paragraph { Background = _brushes.Inserted };
                    foreach (var diff in line.Diffs)
                    {
                        var inline = new Run { Text = diff.Text };
                        paragraph.Inlines.Add(inline);
                    }
                    yield return paragraph;
                }
               
            }

            if (line1.Operation == Operation.Delete)
            {
                foreach (var line in pack)
                {
                    var paragraph = new Paragraph { Background = _brushes.Deleted };
                    foreach (var diff in line.Diffs)
                    {
                        var inline = new Run { Text = diff.Text };
                        paragraph.Inlines.Add(inline);
                    }
                    yield return paragraph;
                }
            }

            if (line1.Operation == Operation.Equal)
            {
                foreach (var line in pack)
                {
                    var paragraph = new Paragraph();
                    foreach (var diff in line.Diffs)
                    {
                        var inline = new Run { Text = diff.Text };
                        paragraph.Inlines.Add(inline);
                    }
                    yield return paragraph;
                }
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

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (e.Source is RichTextBox rtb)
            {
                if (_previousSelected != null)
                {
                    _previousSelected.Background = _previousSelectedBackground;
                    _previousSelected = null;
                }
                if (rtb.CaretPosition.Paragraph != null)
                {
                    _previousSelectedBackground = rtb.CaretPosition.Paragraph.Background;
                    _previousSelected = rtb.CaretPosition.Paragraph;
                    rtb.CaretPosition.Paragraph.Background = _brushes.Focus;
                }
            }
        }
    }
}
