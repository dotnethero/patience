using System.Collections.Generic;
using DiffMatchPatch;

namespace Patience.Core
{
    internal class LineDiff
    {
        public Operation Operation { get; set; }
        public List<Diff> Diffs { get; }

        public LineDiff()
        {
            Diffs = new List<Diff>();
        }

        public LineDiff(Operation operation, string text) : this()
        {
            Operation = operation;
            if (text != string.Empty) // empty text does not change operation
            {
                Diffs.Add(new Diff(operation, text));
            }
        }

        public void AddDiff(Diff diff)
        {
            if (Operation != diff.operation && Diffs.Count > 0)
            {
                Operation = Operation.MODIFIED;
            }
            Diffs.Add(diff);
        }
    }
}