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
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Listings.Handlers
{
    public class ExecuteGoogleSearchRequestHandler : IRequestHandler<ExecuteGoogleSearchRequest>
    {
        public Task<Unit> Handle(ExecuteGoogleSearchRequest request, CancellationToken cancellationToken)
        {
            var feelingLuckyText = request.FeelingLucky ? "&btnI" : "";
            var url = $"https://www.google.com/search?q={HttpUtility.UrlEncode(request.SearchText)}{feelingLuckyText}";
            Process.Start(url);
            return Unit.Task;
        }
    }
}
