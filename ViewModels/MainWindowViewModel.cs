using System.Collections.Generic;
using System.IO;
using DiffMatchPatch;
using Patience.Core;

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
            var a = dmp.diff_linesToChars(file1, file2);
            var lineText1 = (string)a[0];
            var lineText2 = (string)a[1];
            var lineArray = (List<string>)a[2];
            var diffs = dmp.diff_main(lineText1, lineText2, false);
            dmp.diff_charsToLines(diffs, lineArray);

            // greedy sequence finder

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


            Diff = result;
        }
    }
}
