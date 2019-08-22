﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Core.Services;
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
            var log = new Action<object>(s => _eventAggregator.PublishOnUIThread(new ShowResultEvent { Result = s?.ToString(), Header = JavascriptCommandHeader, Type = "js", MessageType = JavascriptMessageType.Information }));
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

            var bytes2Hex = new Func<object, string>(s =>
            {
                if (s is object[] numbers)
                {
                    return string.Concat(numbers.Select(x => (Convert.ToByte((double)x)).ToString("X2")));
                }
                return string.Empty;
            });

            var hex2Bytes = new Func<string, object>(hex =>
            {
                if (hex.Length % 2 == 1)
                    throw new Exception("The binary key cannot have an odd number of digits");

                byte[] arr = new byte[hex.Length >> 1];

                for (int i = 0; i < hex.Length >> 1; ++i)
                {
                    arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
                }

                return arr;
            });

            var engine = new Engine()
                    .SetValue("console", new JavascriptConsole(_eventAggregator))
                    .SetValue(nameof(guid2Bytes), guid2Bytes)
                    .SetValue(nameof(bytes2Hex), bytes2Hex)
                    .SetValue(nameof(bytes2Guid), bytes2Guid)
                    .SetValue(nameof(hex2Bytes), hex2Bytes);
            try
            {
                engine.Execute($"console.log(JSON.stringify(({request.Javascript})))");
            }
            catch (Exception ex)
            {
                _eventAggregator.PublishOnUIThread(new ShowResultEvent { Result = ex.Message, Header = JavascriptCommandHeader, Type = "js", MessageType = JavascriptMessageType.Error });
            }
            return Unit.Task;
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
