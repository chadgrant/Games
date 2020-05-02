using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GameStudio.Repository;
using GameStudio.Repository.Document;
using Polly;
using Polly.CircuitBreaker;

namespace GameStudio.Games.Repository
{
	public class FaultTolerantGameRepository : IGameRepository
	{
		readonly IGameRepository _repository;
		readonly IAsyncPolicy _policy;

		public FaultTolerantGameRepository(IGameRepository repository, GameRepositoryMetricsRegistry metrics, IOptions<GameRepositoryOptions> options)
		{
			_repository = repository;

			// Retry Policy
			// We don't retry if the inner circuit-breaker
			// judges the underlying system is out of commission.
			//
			// Exponential Back off  1, 2, 4, 8, 16 etc...
			var waitAndRetry = Policy.Handle<Exception>(e => !(e is BrokenCircuitException))
				.WaitAndRetryAsync(options.Value.Retries,
				attempt => TimeSpan.FromMilliseconds(options.Value.TimeoutStepMilliseconds * Math.Pow(2, attempt)),
				(exception, waitDuration) =>
				{
					//TODO Log Errors
					metrics.Counters.Errors.Retry.Increment();
				});

			var circuitBreaker = Policy.Handle<Exception>(e => !(e is ConcurrencyException))
				.CircuitBreakerAsync(
					exceptionsAllowedBeforeBreaking: options.Value.ExceptionsBeforeBreaking,
					durationOfBreak: TimeSpan.FromMilliseconds(options.Value.DurationOfBreakMilliseconds),
					onBreak: (ex, breakDelay) =>
					{
						//TODO Log Errors

						metrics.Gauges.Open.Increment();
					},
					onReset: () =>
					{
						metrics.Gauges.Open.Decrement();
						metrics.Gauges.HalfOpen.Decrement();
					},
					onHalfOpen: () =>
					{
						metrics.Gauges.HalfOpen.Increment();
					}
				);

			_policy = Policy.WrapAsync(waitAndRetry, circuitBreaker);
		}
		
		public Task<IGetPagedResults<Game>> GetPagedAsync(string ns, PagedQuery query, CancellationToken cancellationToken = default(CancellationToken))
		{
			return _policy.ExecuteAsync(() => _repository.GetPagedAsync(ns, query, cancellationToken));
		}

		public Task<Game> GetAsync(string ns, string urlSafeName, CancellationToken token = default(CancellationToken))
		{
			return _policy.ExecuteAsync(() =>_repository.GetAsync(ns, urlSafeName, token));
		}

		public Task AddAsync(string ns, Game game, CancellationToken token = default(CancellationToken))
		{
			return _policy.ExecuteAsync(() => _repository.AddAsync(ns, game,token));
		}

		public Task UpdateAsync(string ns, string urlSafeName, Game game, CancellationToken token = default(CancellationToken))
		{
			return _policy.ExecuteAsync(() => _repository.UpdateAsync(ns, urlSafeName, game, token));
		}

		public Task DeleteAsync(string ns, string urlSafeName, CancellationToken token = default(CancellationToken))
		{
			return _policy.ExecuteAsync(() => _repository.DeleteAsync(ns, urlSafeName, token));
		}
	}
}