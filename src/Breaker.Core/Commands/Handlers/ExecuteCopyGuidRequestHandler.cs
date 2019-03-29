using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Breaker.Core.Commands.Requests;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Listings.Handlers
{
    public class ExecuteCopyGuidRequestHandler : IRequestHandler<ExecuteCopyGuidRequest>
    {
        public Task<Unit> Handle(ExecuteCopyGuidRequest request, CancellationToken cancellationToken)
        {
            Clipboard.SetText(Guid.NewGuid().ToString());
            return Unit.Task;
        }
    }
}
