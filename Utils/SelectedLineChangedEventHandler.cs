using System;

namespace Patience.Utils
{
    public delegate void SelectedLineChangedEventHandler(object sender, SelectedLineChangedEventArgs args);

    public class SelectedLineChangedEventArgs : EventArgs
    {
        public int LineIndex { get; set; }
    }
}