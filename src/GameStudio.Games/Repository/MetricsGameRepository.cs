using System;
using System.Threading;
using System.Threading.Tasks;
using GameStudio.Metrics;
using GameStudio.Repository;

namespace GameStudio.Games.Repository
{
	public class MetricsGameRepository : IGameRepository
	{
		readonly IGameRepository _repo;
		readonly Counters _counters;
		readonly ErrorCounters _errors;
		readonly Histograms _histograms;

		public MetricsGameRepository(IGameRepository repo, GameRepositoryMetricsRegistry metrics)
		{
			_repo = repo;

			_counters = metrics.Counters;
			_errors = metrics.Counters.Errors;
			_histograms = metrics.Histograms;
		}

		public Task<IGetPagedResults<Game>> GetPagedAsync(string ns, PagedQuery query, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecuteAsync(_counters.GetPaged, _errors.GetPaged, _histograms.GetPaged, () => _repo.GetPagedAsync(ns, query, cancellationToken));
		}

		public Task<Game> GetAsync(string ns, string name, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecuteAsync(_counters.Get, _errors.Get, _histograms.Get, () => _repo.GetAsync(ns, name, cancellationToken));
		}

		public Task AddAsync(string ns, Game game, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecuteAsync(_counters.Add, _errors.Add, _histograms.Add, () => _repo.AddAsync(ns, game, cancellationToken));
		}

		public Task UpdateAsync(string ns, string urlSafeName, Game game, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecuteAsync(_counters.Update, _errors.Update, _histograms.Update, () => _repo.UpdateAsync(ns, urlSafeName, game, cancellationToken));
		}

		public Task DeleteAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecuteAsync(_counters.Delete, _errors.Delete, _histograms.Delete, () => _repo.DeleteAsync(ns, urlSafeName, cancellationToken));
		}

		T ExecuteAsync<T>(ICounter total, ICounter error, IHistogram histogram, Func<T> fn)
		{
			try
			{
				using (histogram.Time())
					return fn();
			}
			catch (Exception)
			{
				error.Increment();
				throw;
			}
			finally
			{
				total.Increment();
			}
		}
	}
}