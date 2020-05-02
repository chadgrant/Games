using System;
using System.Collections.Generic;
using GameStudio.Repository;

namespace GameStudio.Games
{
	public class Game : IAudit, IAuditor
	{
		public string UrlSafeName { get; set; }
		public string Name { get; set; }
		public string Thumbnail { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public Stats<ulong> Sessions { get; set; } = new Stats<ulong>();
		public Stats<ulong> CoinIn { get; set; } = new Stats<ulong>();

		public DateTime Created { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? Updated { get; set; }
		public string UpdatedBy { get; set; }

		public Dictionary<string, string[]> SymbolMapping { get; set; }
		public string[] Tags { get; set; }
		public string[] BonusGameTypes { get; set; }
    }
	
	public class Stats<T>
	{
		public Stats()
		{
		}

		public Stats(T hour, T day, T week, T month, T year)
		{
			Hour = hour;
			Day = day;
			Week = week;
			Month = month;
			Year = year;
		}

		public T Hour { get; set; }
		public T Day { get; set; }
		public T Week { get; set; }
		public T Month { get; set; }
		public T Year { get; set; }
	}
}