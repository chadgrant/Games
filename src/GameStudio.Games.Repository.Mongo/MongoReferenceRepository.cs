using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GameStudio.References.Repository.Mongo;
using GameStudio.Repository.Document;
using GameStudio.Repository.Document.Mongo;

namespace GameStudio.Games.Repository.Mongo
{
	public class MongoReferenceRepository : MongoRepository<string, Reference>, IReferenceRepository
	{
		public MongoReferenceRepository(IOptions<MongoOptions> options, ReferenceMapper mapper) :
			base(options, mapper, "reference")
		{
		}

		public new async Task<Reference> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
		{
			var results = await base.GetAsync(key, cancellationToken);
			return results?.Item;
		}

		public async Task<Reference> UpsertAsync(string key, Reference entity, CancellationToken cancellationToken = default(CancellationToken))
		{
			var document = Document.Create(key, entity);
			var results = await base.UpsertAsync(key, document, cancellationToken);

			return results?.Item;
		}

		public new async Task<Reference> DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
		{
			return (await base.DeleteAsync(key, cancellationToken))?.Item;
		}
	}
}