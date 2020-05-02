using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GameStudio.Games.WebApi.v2
{
	[ApiVersion("2.0")]
	public class NamespaceController : v1.NamespaceControllerBase
	{
		public NamespaceController(IConfiguration config) : base(config)
		{
		}
	}
}
