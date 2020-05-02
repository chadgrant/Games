using System;
using System.ComponentModel.DataAnnotations;

namespace GameStudio.Games.WebApi.v1
{
	public class AddBonusGameTypeRequest
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public string CreatedBy { get; set; }
	}
}
