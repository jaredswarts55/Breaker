﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class SearchEntryMatch
    {
        public SearchEntry Item { get; set; }
        public int Distance { get; set; }
    }

    public class GetSearchListingsRequestHandler : IRequestHandler<GetSearchListingsRequest, Result<SearchEntry[]>>
    {
        private static object _padlock = new object();
        public Task<Result<SearchEntry[]>> Handle(GetSearchListingsRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var searchTerms = request.SearchText?.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray() ?? new string[0];
            var foundItems = new List<SearchEntry>();
            foreach (var s in searchTerms)
            {
                if (SearchEntries.IndexedLookupTable.TryGetValue(s, out var results))
                {
                    foreach (var result in results)
                        foundItems.Add(result);
                }
                for (var i = 0; i < s.Length - 1; i++)
                {
                    var key = s.Remove(i, 1);
                    if (SearchEntries.IndexedLookupTable.TryGetValue(key, out var partialTermResults))
                    {
                        foreach (var result in partialTermResults)
                            foundItems.Add(result);
                    }
                }
            }
            var toReturn = Task.FromResult(ResultValue(foundItems.GroupBy(x => x.Name).OrderByDescending(x => x.First().Priority).ThenBy(x => x.Count()).Select(x => x.First()).Take(10).ToArray()));
            stopwatch.Stop();
            return toReturn;
            //SearchEntry[] foundItems = SearchEntries.AllEntries.AsParallel()
            //                          .Select(x => new SearchEntryMatch {Item = x, Distance = x.Terms.Min(y => FindSmallestDistanceInText(y, s))})
            //                          .Where(x => s != null && x.Distance < (int)(s.Length * .85 + .5))
            //                          .OrderByDescending(x => x.Item.Priority)
            //                          .ThenBy(x => x.Distance)
            //                          .Select(x => x.Item)
            //                          .Take(10)
            //                          .ToArray();
            //var foundItems = new List<SearchEntryMatch>();

            //Parallel.ForEach(SearchEntries.AllEntries.Where(x => x != null), x =>
            //{
            //    if (cancellationToken.IsCancellationRequested)
            //        return;
            //    var smallestMatch = new SearchEntryMatch { Item = x, Distance = x.Terms.Min(y => FindSmallestDistanceInText(y, s, cancellationToken)) };
            //    if (cancellationToken.IsCancellationRequested)
            //        return;
            //    if (s != null && smallestMatch?.Distance < (int)(s.Length * .85 + .5))
            //    {
            //        foundItems.Add(smallestMatch);
            //    }
            //});
            //if (cancellationToken.IsCancellationRequested)
            //    return Task.FromResult(ResultValue(new SearchEntry[0]));
            //var ordered = foundItems
            //                  .OrderBy(x => x.Distance - x.Item.Priority)
            //                  .Select(x => x.Item)
            //                  .Take(10)
            //                  .ToArray();
            //return Task.FromResult(ResultValue(ordered));
        }

        private int FindSmallestDistanceInText(string inputText, string search, CancellationToken cancellationToken)
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
                if (cancellationToken.IsCancellationRequested)
                    break;
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
