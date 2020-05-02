using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace GameStudio.Games.WebApi.v2
{
	[ApiVersion("2.0")]
	public class GameController : v1.GameControllerBase
	{
		public GameController(IGameRepository repository) : base(repository)
		{
		}
	}
}
