using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffMatchPatch;

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
        private readonly Func<Slice, List<Diff>> _fallback;
        private readonly List<string> _a;
        private readonly List<string> _b;
        
        public Patience(string a, string b)
        {
            _fallback = Myers;
            _a = a.GetLines().ToList();
            _b = b.GetLines().ToList();
        }

        public List<Diff> Diff()
        {
            var slice = new Slice(0, _a.Count, 0, _b.Count);
            return Diff(slice);
        }

        private List<Diff> Myers(Slice slice)
        {
            var a_lines = _a.GetRange(slice.a_low, slice.a_high - slice.a_low);
            var b_lines = _b.GetRange(slice.b_low, slice.b_high - slice.b_low);
            var file1 = string.Join(Environment.NewLine, a_lines);
            var file2 = string.Join(Environment.NewLine, b_lines);

            var dmp = new diff_match_patch();
            var a = dmp.diff_linesToChars(file1, file2);
            var lineText1 = (string)a[0];
            var lineText2 = (string)a[1];
            var lineArray = (List<string>)a[2];
            var diffs = dmp.diff_main(lineText1, lineText2, false);
            dmp.diff_charsToLines(diffs, lineArray);
            return diffs;
        }

        private List<Diff> Diff(Slice slice)
        {
            var match = PatienceSort(UniqueMatchingLines(slice));
            if (match == null)
            {
                return _fallback(slice);
            }

            var lines = new List<Diff>();
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

                var change = _a[a_line] + Environment.NewLine;
                lines.Add(new Diff(Operation.EQUAL, change));

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
