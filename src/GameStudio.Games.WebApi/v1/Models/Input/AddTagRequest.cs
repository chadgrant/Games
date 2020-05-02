using System;
using System.ComponentModel.DataAnnotations;

namespace GameStudio.Games.WebApi.v1
{
	public class AddTagRequest
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public string CreatedBy { get; set; }
	}
}
