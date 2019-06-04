﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Core.Models.Settings
{
    public class QuickRunSetting
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string WorkingDirectory { get; set; }
        public string Arguments { get; set; }
        public uint? Priority { get; set; }
    }
}
