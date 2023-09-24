using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breaker.Core.Events;
using Breaker.Events;
using Newtonsoft.Json;

namespace Breaker.Core.Services
{
    public class JavascriptConsole
    {
        private readonly IEventAggregator _eventAggregator;

        public JavascriptConsole(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        // ReSharper disable once InconsistentNaming
        public void log(params object[] outputs)
        {
            InternalLogMessage(outputs, JavascriptMessageType.Information);
        }
        // ReSharper disable once InconsistentNaming
        public void error(params object[] outputs)
        {
            InternalLogMessage(outputs, JavascriptMessageType.Error);
        }

        private void InternalLogMessage(object[] outputs, JavascriptMessageType messageType)
        {
            var builder = new StringBuilder();
            foreach (var output in outputs)
            {
                if (output is string s)
                    builder.Append(s);
                else if (output.GetType().IsPrimitive)
                    builder.Append(output);
                else
                    builder.Append(JsonConvert.SerializeObject(output));
            }

            _eventAggregator.PublishOnUIThread(
                new ShowResultEvent {Result = builder.ToString(), Header = "Javascript Command", Type = "js", MessageType = JavascriptMessageType.Information}
            );
        }
    }
}
