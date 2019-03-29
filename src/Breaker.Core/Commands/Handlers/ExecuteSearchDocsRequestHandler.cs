using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Core.Utilities;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Listings.Handlers
{
    public class ExecuteSearchDocsRequestHandler : IRequestHandler<ExecuteSearchDocsRequest>
    {
        public Task<Unit> Handle(ExecuteSearchDocsRequest request, CancellationToken cancellationToken)
        {
            var cmdArgs = request.Hide ? "/c " : "/k ";
            var process = new Process
                          {
                              StartInfo = new ProcessStartInfo
                                          {
                                              Arguments = $"{cmdArgs}{request.CommandText}",
                                              FileName = "cmd.exe",
                                              CreateNoWindow = request.Hide,
                                              UseShellExecute = !request.Hide
                                          }
                          };

            process.Start();
            var windowHandle = WindowsUtilities.GetWindowHandleByProcessName("Velocity");
            if(windowHandle.HasValue)
                WindowsUtilities.RestoreWindow(windowHandle.Value);
            return Unit.Task;
        }
    }
}
