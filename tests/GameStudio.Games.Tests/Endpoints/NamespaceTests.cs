using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace GameStudio.Games.Tests.Endpoints
{
	public class NamespaceTests : IClassFixture<TestServerFixture>
	{
		readonly HttpClient _client;
		readonly string _baseUri = "v1/namespace/";

		public NamespaceTests(TestServerFixture testServer)
		{
			_client = testServer.CreateClient();
		}

		[Fact]
		public async Task GetNamespaces_Returns_Array_Of_Namespaces()
		{
			var response = await _client.GetAsync(_baseUri);
			Assert.True(response.IsSuccessStatusCode);

			var content = await response.Content.ReadAsStringAsync();
			Assert.NotEmpty(content);

			var result = JsonConvert.DeserializeObject<string[]>(content);
			Assert.NotEmpty(result);
		}
	}
}
