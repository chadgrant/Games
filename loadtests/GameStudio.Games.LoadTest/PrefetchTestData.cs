using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Newtonsoft.Json.Linq;

namespace GameStudio.Games.LoadTests
{
	public class PrefetchTestData : ILoadTestPlugin
	{
		public void Initialize(LoadTest loadTest)
		{
			loadTest.LoadTestStarting += (s, e) =>
			{
				var test = (LoadTest) s;

				test.Context["games"] = GetGames((string)test.Context["host"]); 
			};
		}

		static List<string> GetGames(string host)
		{
			using (var client = new HttpClient())
			{
				var json = client.GetStringAsync(new Uri(new Uri(host), "/v1/test/game")).Result;

				var top = JObject.Parse(json);

				var results = top.GetValue("Results") as JArray;
				return results.Select(g =>
					((JObject) g).GetValue("urlsafename", StringComparison.OrdinalIgnoreCase).ToString()).ToList();
			}
		}
	}
}