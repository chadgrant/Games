using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using GameStudio.Games.Repository.Mongo;
using GameStudio.References.Repository.Mongo;
using GameStudio.Repository.Document.Mongo;
using Xunit;

namespace GameStudio.Games.Tests.Repository
{
	[CollectionDefinition("Repository collection")]
	public abstract class ReferenceRepositoryTests : IClassFixture<RepositoryFixture>
	{
		protected IOptions<MongoOptions> _options;
		protected abstract IReferenceRepository TagsRepository { get; }

		protected ReferenceRepositoryTests(RepositoryFixture fixture)
		{
			_options = fixture.GetService<IOptions<MongoOptions>>();
		}

		[Fact]
		public async Task Get_Tags_Should_Not_Throw()
		{
			var t = CreateTestModel();

			await TagsRepository.GetAsync("tags");
		}

		[Fact]
		public async Task Retrieve_Added_Tags()
		{
			var t = CreateTestModel();

			var tagsDocument = await TagsRepository.GetAsync("tags");
			tagsDocument.Items.Add(t);
			await TagsRepository.UpsertAsync("tags", tagsDocument);

			var n = await TagsRepository.GetAsync("tags");

			Assert.Contains(n.Items, tag => tag.Name == t.Name);
		}

		[Fact]
		public async Task Can_Add_Multiple_Tags()
		{

			var tagsDocument = await TagsRepository.GetAsync("tags");

			var tags = new HashSet<ReferenceItem>();
			for (var i = 0; i < 5; i++)
			{
				tags.Add(CreateTestModel());
			}

			tagsDocument.Items.UnionWith(tags);

			await TagsRepository.UpsertAsync("tags", tagsDocument);

			var n = await TagsRepository.GetAsync("tags");

			foreach (var t in tags)
				Assert.Contains(n.Items, tag => tag.Name == t.Name);
		}

		[Fact]
		public async Task Can_Update_One_Tag()
		{
			var tagsDocument = await TagsRepository.GetAsync("tags");

			var tags = new List<ReferenceItem>();
			for (var i = 0; i < 5; i++)
			{
				tags.Add(CreateTestModel());
			}

			tagsDocument.Items.UnionWith(tags);

			var updated = await TagsRepository.UpsertAsync("tags", tagsDocument);

			var tagToUpdate = updated.Items.SingleOrDefault(t => t.Name == tags[0].Name);
			tagToUpdate.Name = "singleUpdate-" + Guid.NewGuid();

			var newestCollection = await TagsRepository.UpsertAsync("tags", updated);
			var newest = newestCollection.Items.SingleOrDefault(t => t.Name == tagToUpdate.Name);

			Assert.Equal(newest.Name, tagToUpdate.Name);
			Assert.Equal(newest.CreatedBy, tagToUpdate.CreatedBy);
			Assert.Equal(newest.UpdatedBy, tagToUpdate.UpdatedBy);
		}

		[Fact]
		public async Task Can_Delete_Tag()
		{
			var tag = CreateTestModel();
			tag.Name = "deleteMe-" + Guid.NewGuid();

			var tagsDocument = await TagsRepository.GetAsync("tags");
			tagsDocument.Items.Add(tag);
			await TagsRepository.UpsertAsync("tags", tagsDocument);

			var updated = await TagsRepository.GetAsync("tags"); 
			var single = updated.Items.SingleOrDefault(t => t.Name == tag.Name);

			updated.Items.Remove(single);
			var n = await TagsRepository.UpsertAsync("tags", updated);

			Assert.DoesNotContain(n.Items, t => t.Name == tag.Name);
		}

		[Fact]
		public async Task Can_Delete_Document()
		{
			var document = CreateReferenceModel();
			await TagsRepository.UpsertAsync("delete", document);

			await TagsRepository.DeleteAsync("delete");
			var n = await TagsRepository.GetAsync("delete");

			Assert.Null(n);
		}
		Reference CreateReferenceModel()
		{
			return new Reference
			{
				UrlSafeName = "reference-" + Guid.NewGuid(),
				Created = DateTime.UtcNow,
				CreatedBy = "Unit Tests",
			};
		}

		ReferenceItem CreateTestModel()
		{
			return new ReferenceItem
			{
				Name = "tag-" + Guid.NewGuid(),
				Created = DateTime.UtcNow,
				CreatedBy = "Unit Tests",
			};
		}
	}

	public class MongoReferenceRepositoryTests : ReferenceRepositoryTests
	{
		public MongoReferenceRepositoryTests(RepositoryFixture fixture) : base(fixture)
		{
		}

		protected override IReferenceRepository TagsRepository =>
			new MongoReferenceRepository(_options, new ReferenceMapper(_options));
	}
}