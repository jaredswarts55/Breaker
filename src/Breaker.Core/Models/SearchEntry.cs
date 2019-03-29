using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using Breaker.Core.Annotations;
using PropertyChanged;

namespace Breaker.Core.Models
{
    public class SearchEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly object _padlock = new object();
        private readonly object _displayLock = new object();
        private string[] _terms = null;
        private string _display = null;
        public string Name { get; set; }
        public string Path { get; set; }
        public string Display
        {
            get
            {
                if (_display == null)
                {
                    lock (_displayLock)
                    {
                        if (_display == null)
                        {
                            _display = $"{Path} {Arguments}";
                        }
                    }
                }
                return _display;
            }
            set => _display = value;
        }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public uint Priority = 0;
        public string[] Keywords = new string[0];
        public string[] Terms
        {
            get
            {
                if (_terms == null)
                {
                    lock (_padlock)
                    {
                        if (_terms == null)
                        {
                            _terms = new[] { Name?.ToLower() }.Union(Keywords?.Select(x => x?.ToLower()) ?? new string[0]).ToArray();
                        }
                    }
                }
                return _terms;
            }
            set => _terms = value?.Select(x => x.ToLower()).ToArray();
        }

    }
}
