using System.Threading;
using System.Threading.Tasks;
using GameStudio.Repository;

namespace GameStudio.Games
{
	public interface IGameRepository
	{
		Task<IGetPagedResults<Game>> GetPagedAsync(string ns, PagedQuery query, CancellationToken cancellationToken = default(CancellationToken));

		Task<Game> GetAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken));

		Task AddAsync(string ns, Game game, CancellationToken cancellationToken = default(CancellationToken));

		Task UpdateAsync(string ns, string urlSafeName, Game game, CancellationToken cancellationToken = default(CancellationToken));

		Task DeleteAsync(string ns, string urlSafeName, CancellationToken cancellationToken = default(CancellationToken));
	}
}