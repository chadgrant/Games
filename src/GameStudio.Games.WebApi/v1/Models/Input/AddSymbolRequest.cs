using System;
using System.ComponentModel.DataAnnotations;

namespace GameStudio.Games.WebApi.v1
{
	public class AddSymbolRequest
	{
		[Required,RegularExpression("[A-Za-z0-9_\\-]+")]
		public string UrlSafeName { get; set; }
		[Required]
		public string Name { get; set; }
		public string ServiceUrl { get; set; }
		public string Thumbnail { get; set; }
		[Required]
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		[Required]
		public string CreatedBy { get; set; }
	}
}
