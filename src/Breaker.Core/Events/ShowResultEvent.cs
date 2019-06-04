using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Events
{
    public class ShowResultEvent
    {
        public string Result { get; set; }
        public string Header { get; set; }
        public string Type { get; set; }
        public JavascriptMessageType MessageType {get;set; }
    }

    public enum JavascriptMessageType
    {
        Information,
        Warning,
        Error
    }
}
