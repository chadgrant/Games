using GameStudio.Metrics;

namespace GameStudio.Games.Repository
{
	static class Ops
	{
		internal const string GetPaged = "get_paged";
		internal const string Get = "get";
		internal const string Add = "add";
		internal const string Update = "update";
		internal const string Delete = "delete";
		internal const string Retry = "retry";
	}

	public class GameRepositoryMetricsRegistry
	{
		public GameRepositoryMetricsRegistry(IMetricsFactory factory)
		{
			Counters = new Counters(factory);
			Histograms = new Histograms(factory);
			Gauges = new Gauges(factory);
		}

		public Counters Counters { get; }
		public Histograms Histograms { get; }
		public Gauges Gauges { get; }
	}

	public class Counters
	{
		public Counters(IMetricsFactory factory)
		{
			All = factory.Counter("game_repository", "counts all repository calls", "operation");

			GetPaged = factory.Counter(All, "counts get paged calls", Ops.GetPaged);
			Get = factory.Counter(All, "counts get calls", Ops.Get);
			Add = factory.Counter(All, "counts add operations", Ops.Add);
			Update = factory.Counter(All, "counts update operations", Ops.Update);
			Delete = factory.Counter(All, "counts delete operations", Ops.Delete);

			Errors = new ErrorCounters(factory);
		}

		public ICounter All { get; }
		public ICounter GetPaged { get; }
		public ICounter Get { get; }
		public ICounter Add { get; }
		public ICounter Update { get; }
		public ICounter Delete { get; }
		public ErrorCounters Errors { get; }
	}

	public class ErrorCounters
	{
		public ErrorCounters(IMetricsFactory factory)
		{
			All = factory.Counter("game_repository_err", "counts all errors to the game repository", "operation");

			GetPaged = factory.Counter(All, "counts errors to get paged", Ops.GetPaged);
			Get = factory.Counter(All, "counts errors to get", Ops.Get);
			Add = factory.Counter(All, "counts errors to add", Ops.Add);
			Update = factory.Counter(All, "counts errors update", Ops.Update);
			Delete = factory.Counter(All, "counts errors to delete", Ops.Delete);
			Retry = factory.Counter(All, "counts repo retries", Ops.Retry);
		}

		public ICounter All { get; }
		public ICounter GetPaged { get; }
		public ICounter Get { get; }
		public ICounter Add { get; }
		public ICounter Update { get; }
		public ICounter Delete { get; }
		public ICounter Retry { get; }
	}

	public class Histograms
	{
		static readonly double[] Buckets = {
			0,
			0.1,
			0.2,
			0.4,
			0.6,
			0.8,
			1,
			2,
			3,
			5
		};

		public Histograms(IMetricsFactory factory)
		{
			All = factory.Histogram("game_repository_call_duration", "measures the duration of game repository calls", Buckets, "operation");

			GetPaged = factory.Histogram(All, "times calls to get paged", Ops.GetPaged);
			Get = factory.Histogram(All, "times calls to get", Ops.Get);
			Add = factory.Histogram(All, "times calls to Add", Ops.Add);
			Update = factory.Histogram(All, "times calls to update", Ops.Update);
			Delete = factory.Histogram(All, "times calls to delete", Ops.Delete);
		}

		public IHistogram All { get; }
		public IHistogram GetPaged { get; }
		public IHistogram Get { get; }
		public IHistogram Add { get; }
		public IHistogram Update { get; }
		public IHistogram Delete { get; }
	}

	public class Gauges
	{
		public Gauges(IMetricsFactory factory)
		{
			var open = factory.Gauge("circuit_breaker_open", "tracks open circuit breakers", "service");
			Open = factory.Gauge(open, "","game_repository");

			var halfOpen = factory.Gauge("circuit_breaker_half_open", "tracks half open circuit breakers", "service");
			HalfOpen = factory.Gauge(halfOpen, "", "game_repository");
		}

		public IGauge Open { get; }
		public IGauge HalfOpen { get; }
	}
}