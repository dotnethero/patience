using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Patience.Core;
using Patience.Models;
using Patience.Utils;
using Patience.ViewModels;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class DiffView : UserControl
    {
        private readonly UserControlBrushes _brushes;
        private readonly ParagraphStyles _styles;

        private Paragraph _previousSelected;
        private int _currentLine;

        public event ScrollChangedEventHandler ScrollChanged;
        public event SelectedLineChangedEventHandler SelectedLineChanged;

        #region Dependency properties

        public static readonly DependencyProperty ScrollBarVisibilityProperty = DependencyProperty.Register(
            "ScrollBarVisibility", typeof(ScrollBarVisibility), typeof(DiffView), new PropertyMetadata(ScrollBarVisibility.Visible));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility) GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode", typeof(DiffViewMode), typeof(DiffView), new PropertyMetadata(default(DiffViewMode)));

        public DiffViewMode Mode
        {
            get => (DiffViewMode) GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        #endregion
        
        private class ParagraphStyles
        {
            public Style Default { get; set; }
            public Style Focus { get; set; }
        }

        private class UserControlBrushes
        {
            public Brush Deleted { get; set; }
            public Brush Inserted { get; set; }
            public Brush InlineDeleted { get; set; }
            public Brush InlineInserted { get; set; }
            public Brush AbsentArea { get; set; }
            public Brush AbsentAreaForeground { get; set; }
        }

        public DiffView()
        {
            InitializeComponent();

            _brushes = new UserControlBrushes
            {
                Deleted = new SolidColorBrush(Color.FromRgb(255, 204, 204)), 
                Inserted = new SolidColorBrush(Color.FromRgb(235, 241, 221)), 
                InlineDeleted = new SolidColorBrush(Color.FromRgb(255, 153, 153)),
                InlineInserted = new SolidColorBrush(Color.FromRgb(215, 227, 188)),
                AbsentArea = (Brush) TryFindResource("AbsentArea"),
                AbsentAreaForeground = Brushes.DarkGray
            };

            _styles = new ParagraphStyles
            {
                Default = (Style) TryFindResource("DefaultParagraph"),
                Focus = (Style) TryFindResource("FocusedParagraph")
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is List<LineDiff> diffs)
            {
                var document = new FlowDocument { PageWidth = 3000 }; // fast resize tweak
                var lineNumbers = new StringBuilder();
                var currentLineNumber = 1;
                var first = true;
                foreach (var line in diffs)
                {
                    var paragraph = CreateParagraph(line);
                    if (first)
                    {
                        _currentLine = 0;
                        _previousSelected = paragraph;
                        paragraph.Style = _styles.Focus;
                        first = false;
                    }
                    document.Blocks.Add(paragraph);
                    if (line.Operation == Operation.Delete && Mode == DiffViewMode.File2 ||
                        line.Operation == Operation.Insert && Mode == DiffViewMode.File1)
                    {
                        lineNumbers.AppendLine();
                    }
                    else
                    {
                        lineNumbers.AppendLine($"{currentLineNumber++}");
                    }
                }
                textBox.Document = document;
                lineBox.Text = lineNumbers.ToString();
            }
        }

        private Paragraph CreateParagraph(LineDiff line)
        {
            var paragraph = new Paragraph();

            if (Mode == DiffViewMode.File1)
            {
                if (line.Operation == Operation.Insert)
                {
                    paragraph.Foreground = _brushes.AbsentAreaForeground;
                    paragraph.Background = _brushes.AbsentArea;
                }
                if (line.Operation == Operation.Modify)
                {
                    paragraph.Background = _brushes.Deleted;
                }
                if (line.Operation == Operation.Delete)
                {
                    paragraph.Background = _brushes.Deleted;
                }
            }

            if (Mode == DiffViewMode.File2)
            {
                if (line.Operation == Operation.Insert)
                {
                    paragraph.Background = _brushes.Inserted;
                }
                if (line.Operation == Operation.Modify)
                {
                    paragraph.Background = _brushes.Inserted;
                }
                if (line.Operation == Operation.Delete)
                {
                    paragraph.Foreground = _brushes.AbsentAreaForeground;
                    paragraph.Background = _brushes.AbsentArea;
                }
            }

            foreach (var diff in line.Diffs)
            {
                var inline = new Run();
                if (line.Operation == Operation.Insert && Mode == DiffViewMode.File1)
                {
                    inline.Text = "";
                }
                else if (line.Operation == Operation.Delete && Mode == DiffViewMode.File2)
                {
                    inline.Text = "";
                }
                else if (line.Operation != Operation.Modify)
                {
                    inline.Text = diff.Text;
                }
                else
                {
                    if (diff.Operation == Operation.Delete && Mode == DiffViewMode.File1)
                    {
                        inline.Text = diff.Text;
                        inline.Background = _brushes.InlineDeleted;
                    }
                    if (diff.Operation == Operation.Insert && Mode == DiffViewMode.File2)
                    {
                        inline.Text = diff.Text;
                        inline.Background = _brushes.InlineInserted;
                    }
                    if (diff.Operation == Operation.Equal)
                    {
                        inline.Text = diff.Text;
                    }
                }
                paragraph.Inlines.Add(inline);
            }

            return paragraph;
        }

        public void ScrollToVerticalOffset(double offset)
        {
            textBox.ScrollToVerticalOffset(offset);
        }
        
        public void SelectLineByIndex(int index)
        {
            if (index != _currentLine)
            {
                var paragraph = ((IList) textBox.Document.Blocks)[index];
                if (paragraph is Paragraph p)
                {
                    textBox.CaretPosition = p.ContentStart;
                }
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            lineBox.ScrollToVerticalOffset(e.VerticalOffset);
            ScrollChanged?.Invoke(this, e);
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (e.Source is RichTextBox rtb)
            {
                if (_previousSelected != null)
                {
                    _previousSelected.Style = _styles.Default;
                    _previousSelected = null;
                }
                if (rtb.CaretPosition.Paragraph != null)
                {
                    _previousSelected = rtb.CaretPosition.Paragraph;
                    rtb.CaretPosition.Paragraph.Style = _styles.Focus;
                    var index = ((IList) rtb.Document.Blocks).IndexOf(rtb.CaretPosition.Paragraph);
                    if (index != _currentLine)
                    {
                        _currentLine = index;
                        SelectedLineChanged?.Invoke(this, new SelectedLineChangedEventArgs { LineIndex = index });
                    }
                }
            }
        }
    }
}
