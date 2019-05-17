using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Slice(int aLow, int aHigh, int bLow, int bHigh)
        {
            a_low = aLow;
            a_high = aHigh;
            b_low = bLow;
            b_high = bHigh;
        }

        public override string ToString()
        {
            return $"A:{a_low}-{a_high}, B:{b_low}-{b_high}";
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

        public override string ToString()
        {
            return $"A:{a_line}, B:{b_line}";
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
        public List<LineDiff> Diff(string a, string b)
        {
            var a_lines = a.GetLines().ToList();
            var b_lines = b.GetLines().ToList();
            var slice = new Slice(0, a_lines.Count, 0, b_lines.Count);
            return Diff(a_lines, b_lines, slice);
        }
        
        private List<LineDiff> Myers(List<string> a, List<string> b, Slice slice)
        {
            if (slice.a_high == slice.a_low && slice.b_high == slice.b_low) return new List<LineDiff>(0);

            var myers = new Myers();
            var a_lines = a.GetRange(slice.a_low, slice.a_high - slice.a_low);
            var b_lines = b.GetRange(slice.b_low, slice.b_high - slice.b_low);
            var linesDiff = myers.LinesDiff(a_lines, b_lines);
            var linesDiffWithEdits = myers.MergeLineModifications(linesDiff);
            return linesDiffWithEdits;
        }

        private List<LineDiff> Diff(List<string> a, List<string> b, Slice slice)
        {
            var unique = UniqueMatchingLines(a, b, slice);
            var match = PatienceSort(unique);
            if (match == null)
            {
                return Myers(a, b, slice);
            }

            var lines = new List<LineDiff>();
            var (a_line, b_line) = (slice.a_low, slice.b_low);
            while (true)
            {
                var (a_next, b_next) = match != null 
                    ? (match.a_line, match.b_line) 
                    : (slice.a_high, slice.b_high);

                var subslice = new Slice(a_line, a_next, b_line, b_next);
                lines.AddRange(Diff(a, b, subslice));
                if (match == null)
                {
                    return lines;
                }

                var change = a[match.a_line];
                lines.Add(new LineDiff(Operation.Equal, change));

                (a_line, b_line) = (match.a_line + 1, match.b_line + 1);
                match = match.next;
            }
        }

        private IEnumerable<Match> UniqueMatchingLines(List<string> a, List<string> b, Slice slice)
        {
            var counts = new Dictionary<string, LineOccurrence>();
            foreach (var n in slice.a_range)
            {
                var text = a[n];
                var occurrence = counts.GetOrCreate(text, key => new LineOccurrence());
                occurrence.a_count++;
                occurrence.a_first = occurrence.a_first ?? n;
                counts[text] = occurrence;
            }

            foreach (var n in slice.b_range)
            {
                var text = b[n];
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
