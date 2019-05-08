using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patience.ViewModels
{
    internal class MainWindowViewModel
    {
        public string File1 { get; set; }
        public string File2 { get; set; }

        public MainWindowViewModel()
        {
            File1 = File.ReadAllText("Data/413158_source.txt");
            File2 = File.ReadAllText("Data/413158_recalc.txt");
        }
    }
}
