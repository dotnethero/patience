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
            var file1 = File.ReadAllText("Data/before.txt");
            var file2 = File.ReadAllText("Data/after.txt");

            var diffs = new Core.Patience().Diff(file1, file2);
            Diff = diffs;
        }
    }
}
