using Breaker.Core.Models;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

namespace Breaker.Core.Listings.Requests
{
    public class GetSearchListingsRequest : IRequest<Result<SearchEntry[]>>
    {
        public string SearchText { get; set; }
    }
}
