using GameStudio.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pathoschild.Http.Client;
using GameStudio.Games.WebApi.v1;
using Xunit;
using Xunit.Sdk;

namespace GameStudio.Games.Tests.Endpoints
{
	public class GameResults
	{
		public Game[] Results { get; set; }
	}

	public class GameTests : EndpointTests
	{
		public GameTests(TestServerFixture testServer) : base(testServer, "v1/test/game/")
		{
		}

		[Fact]
		public async Task Get_Games_Returns_Array_Of_Games()
		{
			(await Get("")
					.StatusCode(OK)
					.As<GameResults>())
					.Results
					.ForEach(AssertRequiredFields);
		}

		[Fact]
		public async Task Add_Game_Succeeds()
		{
			var game = TestGame();

			await Post("", game)
					.StatusCode(Created)
					.HeaderContains("Location", $"game/{game.UrlSafeName}")
					.As<Game>(AssertRequiredFields);

			await Get(game.UrlSafeName)
					.StatusCode(OK)
					.As<Game>(g =>
					{
						AssertRequiredFields(g);
						AssertEqual(game, g);
					});
		}

		[Fact]
		public async Task Update_Game_Succeeds()
		{
			var game = TestGame();

			var existing = await Post("", game)
					.StatusCode(Created)
					.HeaderContains("Location", $"game/{game.UrlSafeName}")
					.As<Game>(AssertRequiredFields);
			
			var request = new UpdateGameRequest
			{
				Name = $"Updated {Guid.NewGuid()}",
				UpdatedBy = "Unit Tests",
				StartDate = existing.StartDate,
				EndDate = existing.EndDate,
				Thumbnail = existing.Thumbnail,
				UrlSafeName = existing.UrlSafeName,
				SymbolMapping = existing.SymbolMapping,
				Tags = existing.Tags,
				BonusGameTypes = new string[] { "NewBonusGameType1" },
			};

			await Put(existing.UrlSafeName, request)
				.StatusCode(Accepted);

			await Get(existing.UrlSafeName)
				.StatusCode(OK)
				.As<Game>(g =>
				{
					Assert.Equal(request.Name, g.Name);
					Assert.Equal(request.UpdatedBy, g.UpdatedBy);
					Assert.Equal(request.BonusGameTypes, g.BonusGameTypes);

					Assert.Equal(existing.UrlSafeName, g.UrlSafeName);
					Assert.Equal(existing.CreatedBy, g.CreatedBy);
					Assert.Equal(existing.StartDate.Year, g.StartDate.Year);
					Assert.Equal(existing.StartDate.Hour, g.StartDate.Hour);
					Assert.Equal(existing.StartDate.Minute, g.StartDate.Minute);
					Assert.Equal(existing.Thumbnail, g.Thumbnail);
				});
		}

		[Fact]
		public async Task Modified_UrlSafeName_Returns_BadRequest()
		{
			var existing = (await Get("")
				.As<GameResults>())
				.Results
				.First();
			
			var request = new UpdateGameRequest
			{
				Name = $"Updated {Guid.NewGuid()}",
				UpdatedBy = "Unit Tests",
				StartDate = existing.StartDate,
				EndDate = existing.EndDate,
				Thumbnail = existing.Thumbnail,
				UrlSafeName = "mutated-urlSafeName",
				SymbolMapping = existing.SymbolMapping,
				Tags = existing.Tags,
				BonusGameTypes = new string[] { "NewBonusGameType1" },
			};

			await Assert.ThrowsAsync<ApiException>(async () => await Put(existing.UrlSafeName, request).StatusCode(HttpStatusCode.BadRequest));
		}

		[Fact]
		public async Task Delete_Game_Succeeds()
		{
			var game = TestGame();

			await Post("", game)
					.StatusCode(Created)
					.HeaderContains("Location", $"game/{game.UrlSafeName}")
					.As<Game>(AssertRequiredFields);
			

			await Delete(game.UrlSafeName)
				.StatusCode(Accepted);

			var updatedGames = (await Get("")
					.StatusCode(OK)
					.As<GameResults>())
				.Results;

			Assert.DoesNotContain(game, updatedGames);
		}

		Game TestGame()
		{
			return new Game
			{
				UrlSafeName = Guid.NewGuid().ToString(),
				Name = "Endpoint Test",
				CreatedBy = "Endpoint Tests",
				StartDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
				Thumbnail = "http://thumbnails.com/image.png",
				SymbolMapping = new Dictionary<string, string[]>
				{
					["TestSymbol1"] = new string[] {"TestValue1", "TestValue2"},
					["TestSymbol2"] = new string[] {"TestValue3"},
					["TestSymbol3"] = new string[] {"TestValue4"},
					["TestSymbol4"] = new string[] {"TestValue5"},
					["TestSymbol5"] = new string[] {"TestValue6"},
					["TestSymbol6"] = new string[] {"TestValue7"},
					["TestSymbol7"] = new string[] {"TestValue8"},
					["TestSymbol8"] = new string[] {"TestValue9"},
				},
				Tags = new[] { "TestCategory1", "TestCategory2" },
				BonusGameTypes = new[] { "TestBonusGameType1", "TestBonusGameType2" },
			};
		}

		void AssertRequiredFields(Game g)
		{
			Assert.NotEmpty(g.UrlSafeName);
			Assert.NotEmpty(g.Name);
			Assert.NotEmpty(g.CreatedBy);
			Assert.NotEqual(default(DateTime), g.Created);
			Assert.NotNull(g.Thumbnail);
		}

		void AssertEqual(Game expected, Game actual)
		{
			Assert.Equal(expected.UrlSafeName, actual.UrlSafeName);
			Assert.Equal(expected.Name, actual.Name);
			Assert.Equal(expected.CreatedBy, actual.CreatedBy);
			Assert.Equal(expected.StartDate.Year, actual.StartDate.Year);
			Assert.Equal(expected.StartDate.Hour, actual.StartDate.Hour);
			Assert.Equal(expected.StartDate.Minute, actual.StartDate.Minute);
			Assert.Equal(expected.SymbolMapping, actual.SymbolMapping);
			Assert.Equal(expected.Tags, actual.Tags);
			Assert.Equal(expected.BonusGameTypes, actual.BonusGameTypes);
		}
	}
} 