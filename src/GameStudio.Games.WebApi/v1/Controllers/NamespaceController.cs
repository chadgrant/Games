using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using GameStudio.WebApi;

namespace GameStudio.Games.WebApi.v1
{
	[ApiVersion("1.0")]
	public class NamespaceController : NamespaceControllerBase
	{
		public NamespaceController(IConfiguration config) : base(config)
		{
		}
	}

	[Route("v{version:apiVersion}/[controller]"), Produces("application/json"), ApiController]
	public abstract class NamespaceControllerBase : ControllerBase
	{
		readonly string[] _namespaces;

		protected NamespaceControllerBase(IConfiguration config)
		{
			_namespaces = config.GetSection("namespaces").Get<string[]>();
		}

		/// <summary>
		/// Gets all the namespaces
		/// </summary>
		[HttpGet]
		[ProducesResponse(HttpStatus.OK)]
		public ActionResult<string[]> Get()
		{
			return Ok(_namespaces);
		}
	}
}
