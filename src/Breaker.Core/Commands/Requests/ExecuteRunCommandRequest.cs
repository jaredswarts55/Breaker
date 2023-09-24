using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Breaker.Core.Listings.Requests
{
    public class ExecuteRunCommandRequest : IRequest<Unit>
    {
        public string CommandText { get; set; }
        public bool Hide { get; set; }
    }
}
