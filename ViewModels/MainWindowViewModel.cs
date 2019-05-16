using System.Collections.Generic;
using System.IO;
using Patience.Models;

namespace Patience.ViewModels
{
    internal class MainWindowViewModel
    {
        public List<LineDiff> Diff { get; set; }

        public MainWindowViewModel()
        {
            var file1 = File.ReadAllText("Data/413158_source.txt");
            var file2 = File.ReadAllText("Data/413158_recalc.txt");

            var diffs = new Core.Patience(file1, file2).Diff();
            Diff = diffs;
        }
    }
}
