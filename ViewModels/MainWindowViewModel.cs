using System.Collections.Generic;
using System.IO;
using Patience.Models;

namespace Patience.ViewModels
{
    public class MainWindowViewModel
    {
        public List<LineDiff> Diff { get; set; }

        public MainWindowViewModel()
        {
        }

        public MainWindowViewModel(string path1, string path2)
        {
            var file1 = File.ReadAllText(path1);
            var file2 = File.ReadAllText(path2);
            var diffs = new Core.Patience().Diff(file1, file2);
            Diff = diffs;
        }
    }
}
