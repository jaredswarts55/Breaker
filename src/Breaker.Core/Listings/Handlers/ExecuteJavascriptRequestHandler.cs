using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Events;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using Caliburn.Micro;
using Jint;
using MediatR;
using NullFight;

using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Listings.Handlers
{
    public class ExecuteJavascriptRequestHandler : IRequestHandler<ExecuteJavascriptRequest>
    {
        private readonly IEventAggregator _eventAggregator;
        const string JavascriptCommandHeader = "Javascript Command";

        public ExecuteJavascriptRequestHandler(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public Task<Unit> Handle(ExecuteJavascriptRequest request, CancellationToken cancellationToken)
        {
            var log = new Action<object>(s => _eventAggregator.PublishOnUIThread(new ShowResultEvent { Result = s?.ToString(), Header = JavascriptCommandHeader, Type = "js" }));
            var guid2Bytes = new Func<object, byte[]>(s =>
            {
                if (Guid.TryParse(s?.ToString(), out var g))
                    return g.ToByteArray();
                return new byte[0];
            });
            var bytes2Guid = new Func<object, string>(s =>
            {
                if (s is object[] numbers)
                {
                    try
                    {
                        return new Guid(numbers.Select(x => Convert.ToByte((double)x)).ToArray()).ToString();
                    }
                    catch (Exception ex)
                    {
                        return string.Empty;
                    }
                }
                return string.Empty;
            });

            var engine = new Engine()
                    .SetValue(nameof(log), log)
                    .SetValue(nameof(guid2Bytes), guid2Bytes)
                    .SetValue(nameof(bytes2Guid), bytes2Guid);
            try
            {
                engine.Execute($"log(JSON.stringify(({request.Javascript})))");
            }
            catch (Exception ex)
            {
                _eventAggregator.PublishOnUIThread(new ShowResultEvent { Result = ex.Message, Header = JavascriptCommandHeader, Type = "js" });
            }
            return Unit.Task;
        }
    }
}
