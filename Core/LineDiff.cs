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
            if (text != string.Empty)
            {
                AddDiff(new Diff(operation, text));
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

        public bool IsUncompleted()
        {
            return !Diffs[Diffs.Count - 1].text.IsCompletedLine();
        }

        //public LineDiff(Diff diff)
        //{
        //    Diffs = new List<Diff> { diff };
        //    Operation = diff.operation;
        //}

        //public LineDiff(List<Diff> diffs, Operation operation)
        //{
        //    Diffs = diffs;
        //    Operation = operation;
        //}
    }
}