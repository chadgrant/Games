using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using GameStudio.Repository.Document.Mongo;

namespace GameStudio.Games.Repository.Mongo
{
	public class GameBsonMapper : MongoRepository<string,Game>
	{
		public GameBsonMapper(IOptions<MongoOptions> options, string collectionName) 
            : base(options, new GameMapper(options), collectionName)
		{
		}

		public Task<long> GetRecordCountAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return GetCollection().CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken);
		}
	}
}