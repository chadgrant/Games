using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using GameStudio.Repository;
using GameStudio.WebApi;

namespace GameStudio.Games.WebApi.v1
{
	[ApiVersion("1.0")]
	public class GameController : GameControllerBase
	{
		public GameController(IGameRepository repository) : base(repository)
		{
		}
	}

	[Route("v{version:apiVersion}/{namespace}/[controller]"),Produces("application/json"),ApiController]
	public abstract class GameControllerBase : ControllerBase
	{
		readonly IGameRepository _repository;

		protected GameControllerBase(IGameRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Gets all the games (paging needed)
		/// </summary>
		[HttpGet]
		[ProducesResponse(HttpStatus.OK)]
		public virtual async Task<ActionResult<IReadOnlyList<Game>>> GetPagedAsync([FromRoute,Required,StringLength(2)]string @namespace, int? page, int? size, string sort, string sortField, CancellationToken cancellationToken = default(CancellationToken))
		{
			var opts = new GetPagedOptions {
				Sort = new SortOptions
				{
					Ascending = string.Equals("asc", sort,StringComparison.OrdinalIgnoreCase),
					Descending = string.Equals("desc", sort, StringComparison.OrdinalIgnoreCase),
					Field = sortField
				}
			};
			var query = PagedQuery.Create(page.GetValueOrDefault(1), size.GetValueOrDefault(50), opts);
			return Ok(await _repository.GetPagedAsync(@namespace, query, cancellationToken));
		}

		/// <summary>
		/// Gets name by key (urlSafeName)
		/// </summary>
		/// <response code="404">The game does not exist</response>
		[HttpGet("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}")]
		[ProducesResponse(HttpStatus.OK)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult<Game>> GetAsync([FromRoute,Required,StringLength(2)]string @namespace, [FromRoute,Required,StringLength(2)]string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			var game = await _repository.GetAsync(@namespace, urlSafeName, cancellationToken);
			if (game == null)
				return NotFound($"Game {urlSafeName} in namespace {@namespace} not found");

			return Ok(game);
		}

		/// <summary>
		/// Updates an existing game
		/// </summary>
		/// <response code="202">The game was updated</response>
		/// <response code="404">The game does not exist</response>
		[HttpPut("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}"), Consumes("application/json")]
		[ProducesResponse(HttpStatus.Accepted)]
		[ProducesResponse(HttpStatus.NotFound)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult> UpdateAsync([FromRoute,Required,StringLength(2)]string @namespace, [FromRoute,Required,StringLength(2)] string urlSafeName, [FromBody,Required] UpdateGameRequest updateGame, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			Game existing;
			if ((existing = await _repository.GetAsync(@namespace, urlSafeName, cancellationToken)) == null)
				return NotFound();

			var game = Mapper.Map(updateGame, existing);
			game.Updated = DateTime.UtcNow;

			if (game.UrlSafeName != urlSafeName)
				return BadRequest($"Game JSON has modified urlSafeName which is currently not allowed.");
		
			await _repository.UpdateAsync(@namespace, urlSafeName, game, cancellationToken);

			return Accepted();
		}

		/// <summary>
		/// Creates a new game
		/// </summary>
		/// <response code="201">The game was created</response>
		[HttpPost, Consumes("application/json")]
		[ProducesResponse(HttpStatus.Created)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult<Game>> AddAsync([FromRoute,Required,StringLength(2)]string @namespace, [FromBody,Required,StringLength(2)] AddGameRequest addGame, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var game = Mapper.Map<Game>(addGame);
				game.Created = DateTime.UtcNow;

			await _repository.AddAsync(@namespace, game, cancellationToken);

			return CreatedAtAction(nameof(GetAsync), new {@namespace, game.UrlSafeName}, game);
		}

		/// <summary>
		/// Deletes a game
		/// </summary>
		/// <response code="204">The game was deleted</response>
		/// <response code="404">The game was not found</response>
		[HttpDelete("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}")]
		[ProducesResponse(HttpStatus.NoContent)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult> DeleteAsync([FromRoute,Required,StringLength(2)]string @namespace, [FromRoute,Required,StringLength(2)] string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (await _repository.GetAsync(@namespace, urlSafeName, cancellationToken) == null)
				return NotFound();

			await _repository.DeleteAsync(@namespace, urlSafeName, cancellationToken);

			return NoContent();
		}
	}
}
