using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patience.Core
{
    internal static class Utils
    {
        internal static string[] GetLines(this string text)
        {
            return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        internal static bool IsCompletedLine(this string text)
        {
            return text.EndsWith(Environment.NewLine);
        }
    }
}
