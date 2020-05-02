using GameStudio.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameStudio.Games.WebApi.v1;
using Xunit;

namespace GameStudio.Games.Tests.Endpoints
{
	public class BonusGameTypeTests : EndpointTests
	{
		public BonusGameTypeTests(TestServerFixture testServer) : base(testServer, "v1/bonusgametype/")
		{
		}

		[Fact]
		public async Task Get_BonusGameTypes_Returns_BonusGameTypes()
		{
			var reference = await Get("")
				.StatusCode(OK)
				.As<Reference>(AssertRequiredFields);

			reference
					.Items
					.ForEach(AssertRequiredFieldsItems);
		}

		[Fact]
		public async Task Add_BonusGameType_Succeeds()
		{
			var bonusGameType = TestReferenceItem();

			await Post("", bonusGameType)
					.StatusCode(Created)
					.HeaderContains("Location", $"bonusGameType/")
					.As<Reference>(AssertRequiredFields);

			(await Get("")
					.StatusCode(OK)
					.As<Reference>())
					.Items
					.ForEach(AssertRequiredFieldsItems);
		}

		[Fact]
		public async Task Update_BonusGameTypes_Succeeds()
		{
			var existing = (await Get("")
				.As<Reference>());

			var newItem = TestReferenceItem();

			existing.Items.Add(newItem);

			var request = new UpdateBonusGameTypesRequest
			{
				UpdatedBy = "Unit Tests",
				UrlSafeName = existing.UrlSafeName,
				Items = existing.Items,
			};

			await Put("", request)
				.StatusCode(Accepted);

			await Get("")
				.StatusCode(OK)
				.As<Reference>(t =>
				{
					Assert.Equal(existing.UrlSafeName, t.UrlSafeName);
					Assert.Equal(request.UpdatedBy, t.UpdatedBy);
					Assert.Equal(existing.CreatedBy, t.CreatedBy);
					Assert.Contains(newItem, t.Items);
				});
		}

		[Fact]
		public async Task Delete_BonusGameType_Succeeds()
		{
			var bonusGameType = TestReferenceItem();

			await Post("", bonusGameType)
					.StatusCode(Created)
					.HeaderContains("Location", $"bonusGameType/")
					.As<Reference>(AssertRequiredFields);

			await Delete(bonusGameType.Name)
				.StatusCode(Accepted);

			var referenceItems = (await Get("")
					.StatusCode(OK)
					.As<Reference>())
					.Items;

			Assert.DoesNotContain(bonusGameType, referenceItems);
		}

		Reference TestReference()
		{
			return new Reference
			{
				UrlSafeName = "endpointTest-" + Guid.NewGuid(),
				CreatedBy = "Endpoint Tests",
				Items = RandomReferenceItems(),
			};
		}

		ReferenceItem TestReferenceItem()
		{
			return new ReferenceItem
			{
				Name = Guid.NewGuid().ToString(),
				CreatedBy = $"EndpointTestAuthor",
				Created = RandomDateTime(),
			};
		}

		HashSet<ReferenceItem> RandomReferenceItems()
		{
			Random gen = new Random();
			int numberOfItems = gen.Next(1, 12);

			var referenceItems = new HashSet<ReferenceItem>();

			for (var i = 0; i < numberOfItems; i++)
			{
				referenceItems.Add(TestReferenceItem());
			}

			return referenceItems;
		}

		DateTime RandomDateTime()
		{
			DateTime start = new DateTime(1995, 1, 1);
			Random gen = new Random();
			int range = (DateTime.Today - start).Days;
			return start.AddDays(gen.Next(range));
		}

		void AssertRequiredFields(Reference t)
		{
			Assert.NotEmpty(t.UrlSafeName);
			Assert.NotEmpty(t.Items);
		}
		void AssertRequiredFieldsItems(ReferenceItem t)
		{
			Assert.NotEmpty(t.Name);
			Assert.NotEmpty(t.CreatedBy);
			Assert.NotEqual(default(DateTime), t.Created);
		}

		void AssertEqual(Reference expected, Reference actual)
		{
			Assert.Equal(expected.UrlSafeName, actual.UrlSafeName);
			Assert.Equal(expected.CreatedBy, actual.CreatedBy);
			Assert.Equal(expected.Items, actual.Items);
		}
	}
} 