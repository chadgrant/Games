using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using GameStudio.Repository;
using GameStudio.Repository.Document;
using GameStudio.Repository.Document.Mongo;

namespace GameStudio.Games.Repository.Mongo
{
	public class MongoGameRepository : IGameRepository
	{
		static readonly ConcurrentDictionary<string, GameBsonMapper> DocumentRepos = new ConcurrentDictionary<string, GameBsonMapper>(StringComparer.OrdinalIgnoreCase);

		readonly IOptions<MongoOptions> _options;
		readonly GameMapper _mapper;

		public MongoGameRepository(IOptions<MongoOptions> options, GameMapper mapper)
		{
			_options = options;
			_mapper = mapper;
		}
		
		public async Task<IGetPagedResults<Game>> GetPagedAsync(string ns, PagedQuery query, CancellationToken cancellationToken = default(CancellationToken))
		{
			var documents = await GetDocumentRepo(ns).GetPagedAsync(query, cancellationToken);
			
			return GetPagedResults<Game>.FromQueryResults(documents.Results.Select(d=> d?.Item).ToList(), documents.Page, documents.Size);
		}

		public async Task<Game> GetAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			var document = await GetDocumentRepo(ns).GetAsync(urlSafeName, cancellationToken);
            
            return document?.Item;
        }

		public Task AddAsync(string ns, Game game, CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = Document.Create(game.UrlSafeName, game);

			return GetDocumentRepo(ns).AddAsync(game.UrlSafeName, document, cancellationToken);
		}

		public Task UpdateAsync(string ns, string urlSafeName, Game game, CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = Document.Create(game.UrlSafeName, game);

			return GetDocumentRepo(ns).UpdateAsync(game.UrlSafeName, document, cancellationToken);
		}

		public Task DeleteAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			return GetDocumentRepo(ns).DeleteAsync(urlSafeName, cancellationToken);
		}

		public Task<long> GetRecordCountAsync(string ns)
		{
			return GetDocumentRepo(ns).GetRecordCountAsync();
		}

		GameBsonMapper GetDocumentRepo(string ns)
		{
			return DocumentRepos.GetOrAdd(ns, k => new GameBsonMapper(_options, ns));
		}
	}
}