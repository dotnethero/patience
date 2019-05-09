using System;
using System.Collections.Generic;
using System.Linq;
using DiffMatchPatch;

namespace Patience.Core
{
    internal static class Utils
    {
        internal static string[] GetLines(this string text)
        {
            return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        internal static List<LineDiff> ToLineDiffs(this IEnumerable<Diff> diffs)
        {
            List<LineDiff> all = new List<LineDiff>();
            LineDiff last = null;
            foreach (var diff in diffs)
            {
                var lines = diff.text.GetLines();
                var skip = 0;
                if (last != null)
                {
                    var first = lines[0];
                    last.AddDiff(new Diff(diff.operation, first));
                    skip = 1;
                }
                var lineDiffs = lines.Skip(skip).Select(line => new LineDiff(diff.operation, line)).ToList();
                if (lineDiffs.Count > 0)
                {
                    all.AddRange(lineDiffs);
                    last = lineDiffs[lineDiffs.Count - 1];
                }
            }
            return all;
        }
    }
}
