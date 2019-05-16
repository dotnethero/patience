using System.Collections.Generic;
using System.Linq;

namespace Patience.Models
{
    public class LineDiff
    {
        public Operation Operation { get; }
        public List<Diff> Diffs { get; }

        public LineDiff(Operation operation)
        {
            Operation = operation;
            Diffs = new List<Diff>();
        }

        public LineDiff(Operation operation, string text)
        {
            Operation = operation;
            Diffs = new List<Diff> { new Diff(operation, text) };
        }
        
        public void Add(Diff diff)
        {
            Diffs.Add(diff);
        }

        public override string ToString()
        {
            return $"{Operation}: {string.Join(",", Diffs.Select(x => $"[{x}]").ToArray())}";
        }
    }
}