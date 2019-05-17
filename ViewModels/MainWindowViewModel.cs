using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Patience.Annotations;
using Patience.Models;
using Patience.Utils;

namespace Patience.ViewModels
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        public List<LineDiff> Diff { get; private set; }
        public MainWindowMode Mode { get; private set; }

        public ParameterCommand SetModeCommand { get; }

        public MainWindowViewModel()
        {
            SetModeCommand = new ParameterCommand(SetMode);
        }

        public MainWindowViewModel(string path1, string path2): this()
        {
            var file1 = File.ReadAllText(path1);
            var file2 = File.ReadAllText(path2);
            var diffs = new Core.Patience().Diff(file1, file2);
            Diff = diffs;
            Mode = MainWindowMode.Split;
        }

        private void SetMode(object mode)
        {
            Mode = (MainWindowMode) Enum.Parse(typeof(MainWindowMode), mode.ToString());
            OnPropertyChanged(nameof(Mode));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
