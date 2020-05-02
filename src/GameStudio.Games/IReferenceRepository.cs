using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameStudio.Repository.Document;

namespace GameStudio.Games
{
	// This repository is for reference when creating subcollections like Tags, Symbols etc.
	public interface IReferenceRepository
	{
		Task<Reference> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken));

		Task<Reference> UpsertAsync(string key, Reference entity, CancellationToken cancellationToken = default(CancellationToken));

		Task<Reference> DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
	}
}