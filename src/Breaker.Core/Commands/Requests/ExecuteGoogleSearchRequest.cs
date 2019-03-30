using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Breaker.Core.Listings.Requests
{
    public class ExecuteGoogleSearchRequest : IRequest
    {
        public string SearchText { get; set; }
        public bool FeelingLucky { get; set; }
    }
}
