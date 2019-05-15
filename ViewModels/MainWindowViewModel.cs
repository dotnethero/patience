using System.Collections.Generic;
using System.IO;
using DiffMatchPatch;
using Patience.Core;

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

        private static List<Diff> FindEqualSequences(List<Diff> diffs)
        {
            var dmp = new diff_match_patch();
            var result = new List<Diff>();
            for (var i = 0; i < diffs.Count; i++)
            {
                if (diffs[i].operation == Operation.DELETE &&
                    diffs[i + 1].operation == Operation.INSERT &&
                    diffs[i].text.GetLinesCount() == diffs[i + 1].text.GetLinesCount())
                {
                    var partialDiffs = dmp.diff_main(diffs[i].text, diffs[i + 1].text);
                    foreach (var partialDiff in partialDiffs)
                    {
                        result.Add(partialDiff);
                    }

                    i++;
                }
                else
                {
                    result.Add(diffs[i]);
                }
            }

            return result;
        }
    }
}
