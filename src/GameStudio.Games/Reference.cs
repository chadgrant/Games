using System;
using System.Collections.Generic;
using GameStudio.Repository;
using GameStudio.Repository.Document;

namespace GameStudio.Games
{
	public class Reference: IAudit, IAuditor
	{
		public string UrlSafeName { get; set; }
		public HashSet<ReferenceItem> Items { get; set; } = new HashSet<ReferenceItem>();

		public DateTime Created { get; set; }
		public DateTime? Updated { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
	}
}
