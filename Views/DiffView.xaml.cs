using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Patience.Core;
using Patience.Models;
using Patience.ViewModels;

namespace Patience.Views
{
    /// <summary>
    /// Interaction logic for BindableRichTextBox.xaml
    /// </summary>
    public partial class DiffView : UserControl
    {
        private readonly UserControlBrushes _brushes;

        public event ScrollChangedEventHandler ScrollChanged;

        #region Dependency properties

        public static readonly DependencyProperty ScrollBarVisibilityProperty = DependencyProperty.Register(
            "ScrollBarVisibility", typeof(ScrollBarVisibility), typeof(DiffView), new PropertyMetadata(ScrollBarVisibility.Visible));

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility) GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        public static readonly DependencyProperty ShowModeProperty = DependencyProperty.Register(
            "ShowMode", typeof(DiffShowMode), typeof(DiffView), new PropertyMetadata(default(DiffShowMode)));


        public DiffShowMode ShowMode
        {
            get => (DiffShowMode) GetValue(ShowModeProperty);
            set => SetValue(ShowModeProperty, value);
        }

        #endregion
        
        private class UserControlBrushes
        {
            public Brush ModifiedDeleted { get; set; }
            public Brush ModifiedInserted { get; set; }
            public Brush Deleted { get; set; }
            public Brush Inserted { get; set; }
            public Brush AbsentArea { get; set; }
            public Brush AbsentAreaForeground { get; set; }
        }

        public DiffView()
        {
            InitializeComponent();

            _brushes = new UserControlBrushes
            {
                ModifiedDeleted = new SolidColorBrush(Color.FromRgb(255, 204, 204)), 
                ModifiedInserted = new SolidColorBrush(Color.FromRgb(235, 241, 221)), 
                Deleted = new SolidColorBrush(Color.FromRgb(255, 153, 153)),
                Inserted = new SolidColorBrush(Color.FromRgb(215, 227, 188)),
                AbsentArea = (Brush) TryFindResource("AbsentArea"),
                AbsentAreaForeground = Brushes.DarkGray
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is List<LineDiff> diffs)
            {
                var document = new FlowDocument { PageWidth = 3000 }; // fast resize tweak
                var lineNumbers = new StringBuilder();
                var currentLineNumber = 1;
                foreach (var line in diffs)
                {
                    var paragraph = CreateParagraph(line);
                    document.Blocks.Add(paragraph);
                    lineNumbers.AppendLine($"{currentLineNumber++}");
                }
                textBox.Document = document;
                lineBox.Text = lineNumbers.ToString();
            }
        }

        private Paragraph CreateParagraph(LineDiff line)
        {
            var paragraph = new Paragraph();

            if (ShowMode == DiffShowMode.File1)
            {
                if (line.Operation == Operation.Insert)
                {
                    paragraph.Foreground = _brushes.AbsentAreaForeground;
                    paragraph.Background = _brushes.AbsentArea;
                }
                if (line.Operation == Operation.Modify)
                {
                    paragraph.Background = _brushes.ModifiedDeleted;
                }
                if (line.Operation == Operation.Delete)
                {
                    paragraph.Background = _brushes.Deleted;
                }
            }

            if (ShowMode == DiffShowMode.File2)
            {
                if (line.Operation == Operation.Insert)
                {
                    paragraph.Background = _brushes.Inserted;
                }
                if (line.Operation == Operation.Modify)
                {
                    paragraph.Background = _brushes.ModifiedInserted;
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
                if (line.Operation == Operation.Insert && ShowMode == DiffShowMode.File1)
                {
                    inline.Text = "";
                }
                else if (line.Operation == Operation.Delete && ShowMode == DiffShowMode.File2)
                {
                    inline.Text = "";
                }
                else if (line.Operation != Operation.Modify)
                {
                    inline.Text = diff.Text;
                }
                else
                {
                    if (diff.Operation == Operation.Delete && ShowMode == DiffShowMode.File1)
                    {
                        inline.Text = diff.Text;
                        inline.Background = _brushes.Deleted;
                    }
                    if (diff.Operation == Operation.Insert && ShowMode == DiffShowMode.File2)
                    {
                        inline.Text = diff.Text;
                        inline.Background = _brushes.Inserted;
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

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            lineBox.ScrollToVerticalOffset(e.VerticalOffset);
            ScrollChanged?.Invoke(this, e);
        }
    }
}
