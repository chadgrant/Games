using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameStudio.Games.WebApi.v1
{
	public class UpdateGameRequest
	{
		[Required, RegularExpression("[A-Za-z0-9_\\-]+")]
		public string UrlSafeName { get; set; }
		[Required]
		public string Name { get; set; }
		public string Thumbnail { get; set; }
		[Required]
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		[Required]
		public string UpdatedBy { get; set; }
		public Dictionary<string, string[]> SymbolMapping { get; set; }
		public string[] Tags { get; set; }
		public string[] BonusGameTypes { get; set; }
	}
}
