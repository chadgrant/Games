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
	public class SymbolController : SymbolControllerBase
	{
		public SymbolController(IReferenceRepository repository) : base(repository)
		{
		}
	}

	[Route("v{version:apiVersion}/[controller]"),Produces("application/json"),ApiController]
	public abstract class SymbolControllerBase : ControllerBase
	{
		readonly IReferenceRepository _repository;

		protected SymbolControllerBase(IReferenceRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Gets symbols collection.
		/// </summary>
		/// <response code="404">Symbols collection was not found.</response>
		[HttpGet()]
		[ProducesResponse(HttpStatus.OK)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult<Reference>> GetAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var symbols = await _repository.GetAsync("symbols", cancellationToken);
			if (symbols == null)
				return NotFound($"Symbols not found");

			return Ok(symbols);
		}

		/// <summary>
		/// Updates comprehensive list of all symbols.
		/// </summary>
		/// <response code="202">The symbols were updated</response>
		/// <response code="404">The symbols collection does not exist</response>
		[HttpPut(), Consumes("application/json")]
		[ProducesResponse(HttpStatus.Accepted)]
		[ProducesResponse(HttpStatus.NotFound)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult> UpdateAsync([FromBody,Required] UpdateSymbolsRequest updateSymbols, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			Reference existing;
			if ((existing = await _repository.GetAsync("symbols", cancellationToken)) == null)
				return NotFound();

			var symbols = Mapper.Map(updateSymbols, existing);
			symbols.Updated = DateTime.UtcNow;
		
			await _repository.UpsertAsync("symbols", symbols, cancellationToken);

			return Accepted();
		}

		/// <summary>
		/// Creates a new symbol.
		/// </summary>
		/// <response code="201">The symbol was added</response>
		[HttpPost, Consumes("application/json")]
		[ProducesResponse(HttpStatus.Created)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult<Reference>> AddAsync([FromBody,Required,StringLength(2)] AddSymbolRequest addSymbol, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var symbols = await _repository.GetAsync("symbols", cancellationToken);
			if (symbols == null)
				return NotFound("Symbols not found");

			var symbol = Mapper.Map<ReferenceItem>(addSymbol);
			symbol.Created = DateTime.UtcNow;
			symbols.Items.Add(symbol);
			symbols.Updated = DateTime.UtcNow;
			symbols.UpdatedBy = symbol.CreatedBy;

			await _repository.UpsertAsync("symbols", symbols, cancellationToken);

			return CreatedAtAction(nameof(GetAsync), new {}, symbols);
		}

		/// <summary>
		/// Deletes a Symbol.
		/// </summary>
		/// <response code="204">The symbol was deleted</response>
		/// <response code="404">The symbol was not found</response>
		[HttpDelete("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}")]
		[ProducesResponse(HttpStatus.NoContent)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult> DeleteAsync([FromRoute,Required,StringLength(2)] string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			var symbols = await _repository.GetAsync("symbols", cancellationToken);

			var symbolToDelete = symbols.Items.SingleOrDefault(item => item.Name == urlSafeName);

			if (symbolToDelete == null)
				return NotFound();

			symbols.Items.Remove(symbolToDelete);
			symbols.Updated = DateTime.UtcNow;
			await _repository.UpsertAsync("symbols", symbols, cancellationToken);

			return NoContent();
		}
	}
}