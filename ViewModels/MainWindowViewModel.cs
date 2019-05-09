using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffMatchPatch;

namespace Patience.ViewModels
{
    internal class MainWindowViewModel
    {
        public List<Diff> Diff { get; set; }

        public MainWindowViewModel()
        {
            var file1 = File.ReadAllText("Data/413158_source.txt");
            var file2 = File.ReadAllText("Data/413158_recalc.txt");
            var dmp = new diff_match_patch();
            var diff = dmp.diff_main(file1, file2, true);
            dmp.diff_cleanupSemantic(diff);
            Diff = diff;
        }
    }
}
