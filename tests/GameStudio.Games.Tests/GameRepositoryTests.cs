using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GameStudio.Extensions;
using GameStudio.Games.Repository;
using GameStudio.Games.Repository.Mongo;
using GameStudio.Metrics.Null;
using GameStudio.Metrics.Prometheus;
using GameStudio.Repository;
using GameStudio.Repository.Document.Mongo;
using Xunit;

namespace GameStudio.Games.Tests.Repository
{
	[CollectionDefinition("Repository collection")]
	public abstract class GameRepositoryTests : IClassFixture<RepositoryFixture>
	{
		protected IOptions<MongoOptions> _options;
		const string TestNamespace = "test";

		protected GameRepositoryTests(RepositoryFixture fixture)
		{
			_options = fixture.GetService<IOptions<MongoOptions>>();
		}

		protected abstract IGameRepository Repository { get; }

		public IGameRepository GetRepository()
		{
			return new FaultTolerantGameRepository(Repository, 
				new GameRepositoryMetricsRegistry(
					new NullMetricsFactory()), 
					Options.Create(new GameRepositoryOptions()
				));
		}

		[Fact]
		public async Task Add_Should_Not_Throw()
		{
			var g = CreateTestModel();

			await GetRepository().AddAsync(TestNamespace, g);
		}

		[Fact]
		public async Task Retrieve_Added()
		{
			var g = CreateTestModel();

			var repo = GetRepository();

			await repo.AddAsync(TestNamespace, g);

			var n = await repo.GetAsync(TestNamespace, g.UrlSafeName);

			Assert.Equal(g.UrlSafeName, n.UrlSafeName);
		}

		[Fact]
		public async Task Can_Get_Paged()
		{
			var repo = GetRepository();
			var pagedResults = await repo.GetPagedAsync(TestNamespace, PagedQuery.Create());
			Assert.NotEmpty(pagedResults.Results);

			foreach (var g in pagedResults.Results)
			{
				var game = await repo.GetAsync(TestNamespace, g.UrlSafeName);
				Assert.NotNull(game);
				Assert.Equal(g.UrlSafeName, game.UrlSafeName);
			}
		}

		[Fact]
		public async Task Can_Get_Game()
		{
			var repo = GetRepository();
			var pagedResults = await repo.GetPagedAsync(TestNamespace, PagedQuery.Create());

			Assert.NotEmpty(pagedResults.Results);

			var game = await repo.GetAsync(TestNamespace, pagedResults.Results.First().UrlSafeName);

			Assert.NotNull(game);
		}

		[Fact]
		public async Task Can_Update_Game()
		{
			var repo = GetRepository();
			var game = (await repo.GetPagedAsync(TestNamespace, PagedQuery.Create())).Results.Random();

			game.Name = "Updated " + Guid.NewGuid();
			game.Updated = DateTime.UtcNow;
			game.UpdatedBy = "Unit Tests";

			await repo.UpdateAsync(TestNamespace, game.UrlSafeName, game);

			var updated = await repo.GetAsync(TestNamespace, game.UrlSafeName);
			Assert.Equal(game.UrlSafeName, updated.UrlSafeName);
			Assert.Equal(game.Name, updated.Name);
			Assert.Equal(game.Thumbnail, updated.Thumbnail);
			Assert.Equal(game.UpdatedBy, updated.UpdatedBy);
		}

		[Fact]
		public async Task Can_Add_Symbol_Mappings()
		{
			var repo = GetRepository();
			var game = (await repo.GetPagedAsync(TestNamespace, PagedQuery.Create())).Results.Random();

			game.SymbolMapping = new Dictionary<string, string[]>
			{
				["TestSymbol1"] = new string[] {"TestValue1", "TestValue2"},
				["TestSymbol2"] = new string[] {"TestValue3"},
				["TestSymbol3"] = new string[] {"TestValue4"},
				["TestSymbol4"] = new string[] {"TestValue5"},
				["TestSymbol5"] = new string[] {"TestValue6"},
				["TestSymbol6"] = new string[] {"TestValue7"},
				["TestSymbol7"] = new string[] {"TestValue8"},
				["TestSymbol8"] = new string[] {"TestValue9"},
			};

			await repo.UpdateAsync(TestNamespace, game.UrlSafeName, game);

			var updated = await repo.GetAsync(TestNamespace, game.UrlSafeName);
			Assert.Equal(game.SymbolMapping, updated.SymbolMapping);
		}
		
		[Fact]
		public async Task Can_Add_Tags()
		{
			var repo = GetRepository();
			var game = (await repo.GetPagedAsync(TestNamespace, PagedQuery.Create())).Results.Random();

			game.Tags = new[] { "TestCategory1", "TestCategory2" };

			await repo.UpdateAsync(TestNamespace, game.UrlSafeName, game);

			var updated = await repo.GetAsync(TestNamespace, game.UrlSafeName);
			Assert.Equal(game.Tags, updated.Tags);
		}

		[Fact]
		public async Task Can_Add_BonusGameTypes()
		{
			var repo = GetRepository();
			var game = (await repo.GetPagedAsync(TestNamespace, PagedQuery.Create())).Results.Random();

			game.BonusGameTypes = new[] { "TestBonusGameType1", "TestBonusGameType2" };

			await repo.UpdateAsync(TestNamespace, game.UrlSafeName, game);

			var updated = await repo.GetAsync(TestNamespace, game.UrlSafeName);
			Assert.Equal(game.BonusGameTypes, updated.BonusGameTypes);
		}

		[Fact(Skip = "Mongo does not allow you to change the unique id")]
		public async Task Can_Update_Game_PrimaryKey()
		{
			var repo = GetRepository();

			var game = (await repo.GetPagedAsync(TestNamespace, PagedQuery.Create())).Results.Random();

			var oldName = game.UrlSafeName;
			game.UrlSafeName = "updated-" + Guid.NewGuid();
			game.Name = "Updated " + Guid.NewGuid();
			game.Updated = DateTime.UtcNow;
			game.UpdatedBy = "Unit Tests";

			await repo.UpdateAsync(TestNamespace, oldName, game);

			var updated = await repo.GetAsync(TestNamespace, game.UrlSafeName);
			Assert.Equal(game.UrlSafeName, updated.UrlSafeName);
			Assert.Equal(game.Name, updated.Name);
			Assert.Equal(game.Thumbnail, updated.Thumbnail);
			Assert.Equal(game.UpdatedBy, updated.UpdatedBy);
		}

		Game CreateTestModel()
		{
			return new Game
			{
				Name = "An awesome game of unit tests",
				UrlSafeName = "game-" + Guid.NewGuid(),
				StartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)),
				EndDate = DateTime.UtcNow.Add(TimeSpan.FromDays(90)),

				Sessions = new Stats<ulong>(100, 4000, 12312432324, 314425654674, 234464664347536),
				CoinIn = new Stats<ulong>(10035234, 2342344000, 23423412312432324, 23423314425654674, 234234464664347536),
				Thumbnail = "http://thumbnails.com/image.png",
				Created = DateTime.UtcNow,
				CreatedBy = "Unit Tests",
			};
		}
	}

	public class MongoGameRepositoryTests : GameRepositoryTests
	{
		public MongoGameRepositoryTests(RepositoryFixture fixture) : base(fixture)
		{
		}
		
		protected override IGameRepository Repository => new MongoGameRepository(_options, new GameMapper(_options));
	}

	public class MetricsGameRepositoryTests : GameRepositoryTests
	{
		public MetricsGameRepositoryTests(RepositoryFixture fixture) : base(fixture)
		{
		}

		protected override IGameRepository Repository => new MetricsGameRepository(new MongoGameRepository(_options,new GameMapper(_options)), new GameRepositoryMetricsRegistry(new PrometheusMetricsFactory()));
	}

	public class CachedGameRepositoryTests : GameRepositoryTests
	{
		public CachedGameRepositoryTests(RepositoryFixture fixture) : base(fixture)
		{
		}

		protected override IGameRepository Repository => new CachedGameRepository(new MongoGameRepository(_options,new GameMapper(_options)), new MemoryCache(new MemoryCacheOptions()), Options.Create(new GameRepositoryOptions { MemoryCacheDurationMinutes = 1}));
	}


	[CollectionDefinition("Repository collection")]
	public class RepositoryCollection : ICollectionFixture<RepositoryFixture>
	{
		// This class has no code, and is never created. Its purpose is simply
		// to be the place to apply [CollectionDefinition] and all the
		// ICollectionFixture<> interfaces.
	}

	public class RepositoryFixture : IDisposable
	{
		readonly ServiceProvider _serviceProvider;

		public RepositoryFixture()
		{
			TestConfig.Initialize();

			_serviceProvider = TestConfig.ServiceProvider;

			//Docker may not be available for some time
			var repo = new MongoGameRepository(_serviceProvider.GetService<IOptions<MongoOptions>>(), new GameMapper(_serviceProvider.GetService<IOptions<MongoOptions>>()));
			for (var i = 0; i < 5; i++)
			{
				try
				{
					repo.GetRecordCountAsync("games");
					return;
				}
				catch (Exception)
				{
					Console.WriteLine("Waiting for mongo db...");
					Thread.Sleep(2000);
				}
			}
		}

		public T GetService<T>()
		{
			return _serviceProvider.GetService<T>();
		}

		public void Dispose()
		{
			// ... clean up test data from the database ...
		}
	}
}