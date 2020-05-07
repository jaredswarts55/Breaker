using System;
using System.Threading.Tasks;
using Breaker.Core.Models.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NullFight;
using static NullFight.FunctionalExtensions;

namespace Breaker.Core.Models
{
    public class SlashCommand : SearchEntry
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions(), new NullLoggerFactory());
        private string _autoCompleteTemplate;
        private readonly Guid _cacheKeyPrefix;

        public SlashCommand()
        {
            _cacheKeyPrefix = Guid.NewGuid();
        }

        public string SubstituteCommandText { get; set; }
        public string DisplayTemplate { get; set; }
        public Func<string, UserSettings, IRequest> CreateRequest { get; set; }
        public bool RunOnType { get; set; }
        public string CurrentResult { get; set; }
        public Func<string, string> ProcessResultForClipboard { get; set; }
        public UserSettings UserSettings { get; set; }
        public bool SupportsAutocomplete => AutoComplete != null;
        public Func<string, SlashCommand, Task<string[]>> AutoComplete = (s, slash) => Task.FromResult(new string[0]);

        public string AutoCompleteTemplate
        {
            get => _autoCompleteTemplate ?? DisplayTemplate;
            set => _autoCompleteTemplate = value;
        }

        public async Task<T> GetOrCreateCache<T>(string key, Func<Task<T>> getValue, double expirationSeconds = 60)
        {
            var cached = GetCache<T>(key);
            if (cached.HasValue)
                return cached.Value;
            var result = await getValue();
            return SetCache(key, result, expirationSeconds);
        }

        public T SetCache<T>(string key, T obj, double expirationSeconds = 60)
        {
            _cache.Set(CreateCacheKey(key), obj, new TimeSpan(0, 0, 0, (int)(expirationSeconds * 1000)));
            return obj;
        }

        public Option<T> GetCache<T>(string key)
        {
            if (_cache.TryGetValue(CreateCacheKey(key), out var test))
                return Some((T)test);
            return None();
        }

        public SlashCommand CloneForSearchEntry(string name, string commandArguments, bool forAutoComplete = false)
        {
            return new SlashCommand
            {
                Name = name,
                DisplayTemplate = DisplayTemplate,
                SubstituteCommandText = commandArguments,
                Display = forAutoComplete ? string.Format(AutoCompleteTemplate, commandArguments) : string.Format(DisplayTemplate, commandArguments),
                CreateRequest = CreateRequest,
                RunOnType = RunOnType,
                CurrentResult = CurrentResult,
                ProcessResultForClipboard = ProcessResultForClipboard,
                UserSettings = UserSettings,
                AutoComplete = AutoComplete
            };
        }

        private string CreateCacheKey(string key)
        {
            return $"{_cacheKeyPrefix}|{key}";
        }
    }
}