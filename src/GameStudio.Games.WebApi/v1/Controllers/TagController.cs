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
	public class TagController : TagControllerBase
	{
		public TagController(IReferenceRepository repository) : base(repository)
		{
		}
	}

	[Route("v{version:apiVersion}/[controller]"),Produces("application/json"),ApiController]
	public abstract class TagControllerBase : ControllerBase
	{
		readonly IReferenceRepository _repository;

		protected TagControllerBase(IReferenceRepository repository)
		{
			_repository = repository;
		}

		/// <summary>
		/// Gets tags document which contains Items of all the tags
		/// </summary>
		/// <response code="404">Tags not found</response>
		[HttpGet()]
		[ProducesResponse(HttpStatus.OK)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult<Reference>> GetAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var tags = await _repository.GetAsync("tags", cancellationToken);
			if (tags == null)
				return NotFound($"Tags not found");

			return Ok(tags);
		}

		/// <summary>
		/// Updates the tags document in whole
		/// </summary>
		/// <response code="202">The tags were updated</response>
		/// <response code="404">The tags do not exist</response>
		[HttpPut(), Consumes("application/json")]
		[ProducesResponse(HttpStatus.Accepted)]
		[ProducesResponse(HttpStatus.NotFound)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult> UpdateAsync([FromBody,Required] UpdateTagsRequest updateTags, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			Reference existing;
			if ((existing = await _repository.GetAsync("tags", cancellationToken)) == null)
				return NotFound();

			var tags = Mapper.Map(updateTags, existing);
			tags.Updated = DateTime.UtcNow;
		
			await _repository.UpsertAsync("tags", tags, cancellationToken);

			return Accepted();
		}

		/// <summary>
		/// Creates a new tag
		/// </summary>
		/// <response code="201">The tag was added</response>
		[HttpPost, Consumes("application/json")]
		[ProducesResponse(HttpStatus.Created)]
		[ProducesResponse(HttpStatus.BadRequest)]
		public virtual async Task<ActionResult<Reference>> AddAsync([FromBody,Required,StringLength(2)] AddTagRequest addTag, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var tags = await _repository.GetAsync("tags", cancellationToken);
			if (tags == null)
				return NotFound($"Tags not found");

			var tag = Mapper.Map<ReferenceItem>(addTag);
			tag.Name = addTag.Name;
			tag.Created = DateTime.UtcNow;
			tags.Items.Add(tag);
			tags.Updated = DateTime.UtcNow;
			tags.UpdatedBy = addTag.CreatedBy;

			await _repository.UpsertAsync("tags", tags, cancellationToken);

			return CreatedAtAction(nameof(GetAsync), new {}, tags);
		}

		/// <summary>
		/// Deletes a tag
		/// </summary>
		/// <response code="204">The tag was deleted</response>
		/// <response code="404">The tag was not found</response>
		[HttpDelete("{urlSafeName:regex([[A-Za-z0-9_\\-]]+)}")]
		[ProducesResponse(HttpStatus.NoContent)]
		[ProducesResponse(HttpStatus.NotFound)]
		public virtual async Task<ActionResult> DeleteAsync([FromRoute,Required,StringLength(2)] string urlSafeName, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tags = await _repository.GetAsync("tags", cancellationToken);

			var tagToDelete = tags.Items.SingleOrDefault(item => item.Name == urlSafeName);

			if (tagToDelete == null)
				return NotFound();

			tags.Items.Remove(tagToDelete);
			tags.Updated = DateTime.UtcNow;
			await _repository.UpsertAsync("tags", tags, cancellationToken);

			return NoContent();
		}
	}
}