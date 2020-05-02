using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using GameStudio.Repository;
using GameStudio.WebApi;

namespace GameStudio.Games.WebApi.v1
{
	[ApiVersion("1.0")]
	public class BonusGameTypeController : BonusGameTypeControllerBase
	{
		public BonusGameTypeController(IReferenceRepository repository) : base(repository)
		{
		}
	}

	[Route("v{version:apiVersion}/[controller]"),Produces("application/json"),ApiController]
	public abstract class BonusGameTypeControllerBase : ControllerBase
	{
		readonly IReferenceRepository _repository;

		protected BonusGameTypeControllerBase(IReferenceRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Gets bonus game types collection.
		/// </summary>
		/// <response code="404">Bonus game types collection was not found.</response>
		[HttpGet()]
		[ProducesResponse(HttpStatus.OK)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult<Reference>> GetAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var bonusGameTypes = await _repository.GetAsync("bonusGameTypes", cancellationToken);
			if (bonusGameTypes == null)
				return NotFound($"BonusGameTypes not found");

			return Ok(bonusGameTypes);
		}

		/// <summary>
		/// Updates comprehensive list of all bonus game types.
		/// </summary>
		/// <response code="202">The bonus game types were updated</response>
		/// <response code="404">The bonus game types collection does not exist</response>
		[HttpPut(), Consumes("application/json")]
		[ProducesResponse(HttpStatus.Accepted)]
		[ProducesResponse(HttpStatus.NotFound)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult> UpdateAsync([FromBody,Required] UpdateBonusGameTypesRequest updateBonusGameTypes, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			Reference existing;
			if ((existing = await _repository.GetAsync("bonusGameTypes", cancellationToken)) == null)
				return NotFound();

			var bonusGameTypes = Mapper.Map(updateBonusGameTypes, existing);
			bonusGameTypes.Updated = DateTime.UtcNow;
		
			await _repository.UpsertAsync("bonusGameTypes", bonusGameTypes, cancellationToken);

			return Accepted();
		}

		/// <summary>
		/// Creates a new bonus game type.
		/// </summary>
		/// <response code="201">The bonus game type was added</response>
		[HttpPost, Consumes("application/json")]
		[ProducesResponse(HttpStatus.Created)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult<Reference>> AddAsync([FromBody,Required,StringLength(2)] AddBonusGameTypeRequest addBonusGameType, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var bonusGameTypes = await _repository.GetAsync("bonusGameTypes", cancellationToken);
			if (bonusGameTypes == null)
				return NotFound("BonusGameTypes not found");

			var bonusGameType = Mapper.Map<ReferenceItem>(addBonusGameType);
			bonusGameType.Created = DateTime.UtcNow;
			bonusGameTypes.Items.Add(bonusGameType);
			bonusGameTypes.Updated = DateTime.UtcNow;
			bonusGameTypes.UpdatedBy = bonusGameType.CreatedBy;

			await _repository.UpsertAsync("bonusGameTypes", bonusGameTypes, cancellationToken);

			return CreatedAtAction(nameof(GetAsync), new {}, bonusGameTypes);
		}

		/// <summary>
		/// Deletes a BonusGameType.
		/// </summary>
		/// <response code="204">The bonus game type was deleted</response>
		/// <response code="404">The bonus game type was not found</response>
		[HttpDelete("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}")]
		[ProducesResponse(HttpStatus.NoContent)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult> DeleteAsync([FromRoute,Required,StringLength(2)] string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			var bonusGameTypes = await _repository.GetAsync("bonusGameTypes", cancellationToken);

			var bonusGameTypeToDelete = bonusGameTypes.Items.SingleOrDefault(item => item.Name == urlSafeName);

			if (bonusGameTypeToDelete == null)
				return NotFound();

			bonusGameTypes.Items.Remove(bonusGameTypeToDelete);
			bonusGameTypes.Updated = DateTime.UtcNow;
			await _repository.UpsertAsync("bonusGameTypes", bonusGameTypes, cancellationToken);

			return NoContent();
		}
	}
}