using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patience.Models;

// https://blog.jcoglan.com/2017/09/28/implementing-patience-diff/
// ReSharper disable InconsistentNaming

namespace Patience.Core
{
    class Slice
    {
        public int a_low;
        public int a_high;
        public int b_low;
        public int b_high;

        public IEnumerable<int> a_range => Enumerable.Range(a_low, a_high - a_low);
        public IEnumerable<int> b_range => Enumerable.Range(b_low, b_high - b_low);

        public bool not_empty() => a_low < a_high && b_low < b_high;

        public Slice(int aLow, int aHigh, int bLow, int bHigh)
        {
            a_low = aLow;
            a_high = aHigh;
            b_low = bLow;
            b_high = bHigh;
        }
    }

    class Match
    {
        public int a_line;
        public int b_line;
        public Match prev;
        public Match next;

        public Match(int aLine, int bLine)
        {
            a_line = aLine;
            b_line = bLine;
        }
    }

    class LineOccurrence
    {
        public int a_count;
        public int b_count;
        public int? a_first;
        public int? b_first;
    }

    class Patience
    {
        private readonly Func<Slice, List<LineDiff>> _fallback;
        private readonly List<string> _a;
        private readonly List<string> _b;
        
        public Patience(string a, string b)
        {
            _fallback = Myers;
            _a = a.GetLines().ToList();
            _b = b.GetLines().ToList();
        }

        public List<LineDiff> Diff()
        {
            var slice = new Slice(0, _a.Count, 0, _b.Count);
            return Diff(slice);
        }

        private List<LineDiff> Myers(Slice slice)
        {
            if (slice.a_high == slice.a_low && slice.b_high == slice.b_low)
            {
                return new List<LineDiff>(0);
            }

            var a_lines = _a.GetRange(slice.a_low, slice.a_high - slice.a_low);
            var b_lines = _b.GetRange(slice.b_low, slice.b_high - slice.b_low);

            var hash = new Dictionary<string, char>(a_lines.Count + b_lines.Count);
            var a_chars = new StringBuilder(a_lines.Count);
            var b_chars = new StringBuilder(b_lines.Count);
            foreach (var line in a_lines)
            {
                if (!hash.ContainsKey(line))
                {
                    var index = (char) (hash.Count + 1); // 1-based index
                    hash.Add(line, index);
                }
                a_chars.Append(hash[line]);
            }
            foreach (var line in b_lines)
            {
                if (!hash.ContainsKey(line))
                {
                    var index = (char) (hash.Count + 1); // 1-based index
                    hash.Add(line, index);
                }
                b_chars.Append(hash[line]);
            }

            var lineText1 = a_chars.ToString();
            var lineText2 = b_chars.ToString();
            var lineArray = hash.OrderBy(x => x.Value).Select(x => x.Key).ToList();
            lineArray.Insert(0, null);

            var dmp = new Myers();
            var diffs = dmp.diff_main(lineText1, lineText2);
            var result = new List<LineDiff>();
            foreach (var diff in diffs)
            {
                foreach (var ch in diff.Text)
                {
                    result.Add(new LineDiff(diff.Operation, lineArray[ch]));
                }
            }

            if (true) // TODO: here should be option for interline diff
            {
                // TODO: can i move it before main diff?
                var commonPrefix = new List<LineDiff>();
                var commonSuffix = new List<LineDiff>();
                var commonPrefixComplete = false;
                var commonSuffixComplete = false;
                for (var index = 0; index < result.Count; index++)
                {
                    var diffStart = result[index];
                    if (diffStart.Operation == Operation.Equal && !commonPrefixComplete)
                    {
                        commonPrefix.Add(diffStart);
                    }
                    else
                    {
                        commonPrefixComplete = true;
                    }

                    var diffEnd = result[result.Count - index - 1];
                    if (diffEnd.Operation == Operation.Equal && !commonSuffixComplete)
                    {
                        commonSuffix.Add(diffEnd);
                    }
                    else
                    {
                        commonSuffixComplete = true;
                    }
                }

                var deletes = result.Where(x => x.Operation == Operation.Delete).ToList();
                var inserts = result.Where(x => x.Operation == Operation.Insert).ToList();
                if (inserts.Count == deletes.Count && inserts.Count + deletes.Count == result.Count - commonPrefix.Count - commonSuffix.Count)
                {
                    var interlineResults = new List<LineDiff>();
                    foreach (var diff in commonPrefix)
                    {
                        interlineResults.Add(diff);
                    }
                    for (var i = 0; i < deletes.Count; i++)
                    {
                        var del = deletes[i];
                        var ins = inserts[i];
                        var interlineDiffs = dmp.diff_main(del.Diffs[0].Text, ins.Diffs[0].Text);
                        var diff = new LineDiff(Operation.Modify);
                        foreach (var interlineDiff in interlineDiffs)
                        {
                            diff.Add(interlineDiff);
                        }
                        interlineResults.Add(diff);
                    }
                    foreach (var diff in commonSuffix)
                    {
                        interlineResults.Add(diff);
                    }
                    return interlineResults;
                }
            }

            return result;
        }

        private List<LineDiff> Diff(Slice slice)
        {
            var match = PatienceSort(UniqueMatchingLines(slice));
            if (match == null)
            {
                return _fallback(slice);
            }

            var lines = new List<LineDiff>();
            var (a_line, b_line) = (slice.a_low, slice.b_low);
            while (true)
            {
                var (a_next, b_next) = match != null
                    ? (match.a_line, match.b_line) 
                    : (slice.a_high, slice.b_high);

                var subslice = new Slice(a_line, a_next, b_line, b_next);
                lines.AddRange(Diff(subslice));
                if (match == null)
                {
                    return lines;
                }

                var change = _a[a_line];
                lines.Add(new LineDiff(Operation.Equal, change));

                (a_line, b_line) = (match.a_line + 1, match.b_line + 1);
                match = match.next;
            }
            return null;
        }

        private IEnumerable<Match> UniqueMatchingLines(Slice slice)
        {
            var counts = new Dictionary<string, LineOccurrence>();
            foreach (var n in slice.a_range)
            {
                var text = _a[n];
                var occurrence = counts.GetOrCreate(text, key => new LineOccurrence());
                occurrence.a_count++;
                occurrence.a_first = occurrence.a_first ?? n;
                counts[text] = occurrence;
            }

            foreach (var n in slice.b_range)
            {
                var text = _b[n];
                var occurrence = counts.GetOrCreate(text, key => new LineOccurrence());
                occurrence.b_count++;
                occurrence.b_first = occurrence.b_first ?? n;
                counts[text] = occurrence;
            }

            return counts
                .Select(kvp => kvp.Value)
                .Where(occ => occ.a_count == 1 && occ.b_count == 1)
                .Select(occ => new Match(occ.a_first.Value, occ.b_first.Value))
                .OrderBy(mch => mch.a_line);
        }

        private Match PatienceSort(IEnumerable<Match> matches)
        {
            var stacks = new Dictionary<int, Match>();
            foreach (var m in matches)
            {
                var i = BinarySearch(stacks, m);
                if (i >= 0)
                {
                    m.prev = stacks[i];
                }
                stacks[i + 1] = m;
            }

            var match = stacks.OrderByDescending(x => x.Key).Select(x => x.Value).FirstOrDefault();
            if (match == null)
            {
                return null;
            }

            while (match.prev != null)
            {
                match.prev.next = match;
                match = match.prev;
            }

            return match;
        }

        private int BinarySearch(Dictionary<int, Match> stacks, Match m)
        {
            var (low, high) = (-1, stacks.Count);
            while (low + 1 < high)
            {
                var mid = (low + high) / 2;
                if (stacks[mid].b_line < m.b_line)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
            return low;
        }
    }

}
