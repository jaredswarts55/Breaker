using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breaker.Core.Models.Settings;
using MediatR;

namespace Breaker.Core.Models
{
    public class SlashCommand : SearchEntry
    {
        public string DisplayTemplate { get; set; }
        public Func<string, UserSettings, IRequest> CreateRequest { get; set; }
        public bool RunOnType { get; set; }
        public string CurrentResult { get; set; }
        public Func<string, string> ProcessResultForClipboard { get; set; }

        public UserSettings UserSettings { get; set; }
    }
}
