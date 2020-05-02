using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json;

namespace GameStudio.Games.LoadTests
{
	public abstract class ApiWebTest : WebTest
	{
		protected string _baseUrl = "/v1/test/game/";

		protected Uri GetUri(string path = "")
		{
			return new Uri(new Uri((string) Context["host"]), _baseUrl + path);
		}

		protected List<string> Games => GetFromContext("games");

		List<string> GetFromContext(string name)
		{
			if (Context.TryGetValue(name, out object g))
				return g as List<string>;

			var games = new List<string>();
			Context[name] = games;
			return games;
		}
	}
	
	public class UpdateGameTest : ApiWebTest
	{
		readonly Random _rand = new Random(DateTime.UtcNow.Millisecond);

		public override IEnumerator<WebTestRequest> GetRequestEnumerator()
		{
			var urlSafeName = Games[_rand.Next(Games.Count)];

			using (var ms = new MemoryStream())
			{
				using (var json = new JsonTextWriter(new StreamWriter(ms)))
				{
					json.WriteStartObject();
					json.WritePropertyName("Name");
					json.WriteValue("A load test updated game");
					json.WritePropertyName("UrlSafeName");
					json.WriteValue(urlSafeName);
					json.WritePropertyName("UpdatedBy");
					json.WriteValue("Load Test");
					json.WriteEndObject();
					json.Flush();

					//var str = Encoding.UTF8.GetString(ms.ToArray());

					yield return RequestBuilder.Put(GetUri(urlSafeName)).WithBody(ms.ToArray()).Build();
				}
			}
		}
	}

	public class AddGameTest : ApiWebTest
	{
		public override IEnumerator<WebTestRequest> GetRequestEnumerator()
		{
			using (var ms = new MemoryStream())
			{
				using (var json = new JsonTextWriter(new StreamWriter(ms)))
				{
					var urlSafeName = "loadtest-" + Guid.NewGuid();
					json.WriteStartObject();
					json.WritePropertyName("Name");
					json.WriteValue("A load test game");
					json.WritePropertyName("UrlSafeName");
					json.WriteValue(urlSafeName);
					json.WritePropertyName("StartDate");
					json.WriteValue(DateTime.UtcNow.Subtract(TimeSpan.FromDays(90)));
					json.WritePropertyName("EndDate");
					json.WriteValue(DateTime.UtcNow.Add(TimeSpan.FromDays(90)));
					json.WritePropertyName("CreatedBy");
					json.WriteValue("Load Test");
					json.WriteEndObject();
					json.Flush();

					yield return RequestBuilder.Post(GetUri())
						.WithBody(ms.ToArray())
						.WithRequest(RequestBuilder.Delete(GetUri(urlSafeName)).Build())
						.Build();
				}
			}
		}
	}

	public class GetAllGamesTests : ApiWebTest
	{
		public override IEnumerator<WebTestRequest> GetRequestEnumerator()
		{
			yield return new WebTestRequest(GetUri());
		}
	}

	public class GetSingleGameTests : ApiWebTest
	{
		readonly Random _rand = new Random(DateTime.UtcNow.Millisecond);

		public override IEnumerator<WebTestRequest> GetRequestEnumerator()
		{
			yield return new WebTestRequest(GetUri(Games[_rand.Next(Games.Count)]));
		}
	}
}