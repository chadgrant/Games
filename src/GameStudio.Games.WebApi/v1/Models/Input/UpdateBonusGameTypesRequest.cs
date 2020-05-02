using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameStudio.Games.WebApi.v1
{
	public class UpdateBonusGameTypesRequest
	{
		[Required, RegularExpression("[A-Za-z0-9_\\-]+")]
		public string UrlSafeName { get; set; }
		[Required]
		public string UpdatedBy { get; set; }
		public HashSet<ReferenceItem> Items { get; set; } = new HashSet<ReferenceItem>();
	}
}
