using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using GameStudio.Repository;

namespace GameStudio.Games.Repository
{
	public class CachedGameRepository : IGameRepository
	{
		static readonly ConcurrentDictionary<string, object> CacheKeys =
			new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		readonly IMemoryCache _cache;
		readonly IOptions<GameRepositoryOptions> _options;
		readonly IGameRepository _repository;

		public CachedGameRepository(IGameRepository repository, IMemoryCache cache, IOptions<GameRepositoryOptions> options)
		{
			_repository = repository;
			_cache = cache;
			_options = options;
		}

		public Task<IGetPagedResults<Game>> GetPagedAsync(string ns, PagedQuery query, CancellationToken cancellationToken = default(CancellationToken))
		{
			return CacheGetAsync(ns + query, _repository.GetPagedAsync(ns, query, cancellationToken));
		}

		public Task<Game> GetAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			return CacheGetAsync(string.Join(':', ns, urlSafeName), _repository.GetAsync(ns, urlSafeName, cancellationToken));
		}

		public async Task AddAsync(string ns, Game game, CancellationToken cancellationToken = default(CancellationToken))
		{
			await _repository.AddAsync(ns, game, cancellationToken);

			CacheSet(CacheKey(string.Join(':', ns, game.UrlSafeName)), game);
		}

		public async Task UpdateAsync(string ns, string urlSafeName, Game game, CancellationToken cancellationToken = default(CancellationToken))
		{
			await _repository.UpdateAsync(ns, urlSafeName, game, cancellationToken);

			CacheSet(CacheKey(string.Join(':', ns, game.UrlSafeName)), game);
		}

		public async Task DeleteAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			await _repository.DeleteAsync(ns, urlSafeName, cancellationToken);

			_cache.Remove(CacheKey(string.Join(':', ns, urlSafeName)));
		}

		async Task<T> CacheGetAsync<T>(string cacheKey, Task<T> getter)
		{
			var locker = CacheKey(cacheKey);

			if (_cache.TryGetValue(locker, out T val))
				return val;

			await locker.WaitAsync();
			try
			{
				if (_cache.TryGetValue(locker, out T val2))
					return val2;

				return CacheSet(locker, await getter);
			}
			finally
			{
				locker.Release();
			}
		}

		T CacheSet<T>(object key, T value)
		{
			var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.Value.MemoryCacheDurationMinutes));

			return _cache.Set(key, value, options);
		}

		SemaphoreSlim CacheKey(string name)
		{
			return (SemaphoreSlim) CacheKeys.GetOrAdd("game_" + name.ToLower(), new SemaphoreSlim(1, 1));
		}
	}
}