using System;
using System.Collections.Generic;
using System.Linq;
using DiffMatchPatch;

namespace Patience.Core
{
    internal enum LineOperation
    {
        Unmodified,
        Inserted,
        Deleted,
        Modified
    }

    internal class LineDiff
    {
        public LineOperation Operation { get; set; }
        public List<Diff> Diffs { get; }

        public LineDiff()
        {
            Operation = LineOperation.Unmodified;
            Diffs = new List<Diff>();
        }

        public LineDiff(Operation operation, string text) : this()
        {
            if (string.IsNullOrEmpty(text))
            {
                Operation = GetLineOperation(operation);
                Diffs = new List<Diff>();
            }
            else
            {
                Operation = GetLineOperation(operation);
                Diffs = new List<Diff> { new Diff(operation, text) };
            }
        }

        public void AddDiff(Diff diff)
        {
            var hasAny = Diffs.Any();
            if (!hasAny)
            {
                Operation = GetLineOperation(diff.operation);
            }
            else
            {
                var hasAnyOther = Diffs.Any(x => x.operation != diff.operation);
                if (hasAnyOther)
                {
                    Operation = LineOperation.Modified;
                }
            }
            Diffs.Add(diff);
        }

        private static LineOperation GetLineOperation(Operation operation)
        {
            switch (operation)
            {
                case DiffMatchPatch.Operation.DELETE: return LineOperation.Deleted;
                case DiffMatchPatch.Operation.INSERT: return LineOperation.Inserted;
                case DiffMatchPatch.Operation.EQUAL:  return LineOperation.Unmodified;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        public override string ToString()
        {
            return $"{Operation}: {string.Join(", ", Diffs.Select(x => x.ToString()).ToArray())}";
        }
    }
}