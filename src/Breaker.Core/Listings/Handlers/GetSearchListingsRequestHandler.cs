using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Breaker.Core.Listings.Requests;
using Breaker.Core.Models;
using Breaker.Utilities;
using Breaker.ViewModels.SubModels;
using MediatR;
using NullFight;

using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Listings.Handlers
{
    public class GetSearchListingsRequestHandler : IRequestHandler<GetSearchListingsRequest, Result<SearchEntry[]>>
    {
        public Task<Result<SearchEntry[]>> Handle(GetSearchListingsRequest request, CancellationToken cancellationToken)
        {
            var s = request.SearchText?.ToLower();
            var foundItems = SearchEntries.AllEntries.AsParallel()
                                      .Select(x => new { Item = x, Distance = x.Terms.Min(y => FindSmallestDistanceInText(y, s)) })
                                      .Where(x => s != null && x.Distance < (int)(s.Length * .85 + .5))
                                      .OrderByDescending(x => x.Item.Priority)
                                      .ThenBy(x => x.Distance)
                                      .Select(x => x.Item)
                                      .Take(10)
                                      .ToArray();
            return Task.FromResult(ResultValue(foundItems));
        }

        private int FindSmallestDistanceInText(string inputText, string search)
        {
            var input = inputText?.ToLower() ?? string.Empty;
            if (input.Length <= search.Length)
                return LevenshteinDistance.Compute(search, input);
            if (input.StartsWith(search))
                return 0;
            var smallest = int.MaxValue;
            if (inputText?.Contains(search) ?? false)
                return 0;
            for (var i = 0; i < (input.Length - search.Length + 1); i++)
            {
                if (input[i] != search[0])
                    break;
                smallest = Math.Min(smallest, LevenshteinDistance.Compute(input.Substring(i, search.Length), search));
                if (smallest == 0)
                    break;
            }
            return smallest;
        }
    }
}
