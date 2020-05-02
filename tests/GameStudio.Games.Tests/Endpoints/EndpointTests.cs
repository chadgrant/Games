using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Pathoschild.Http.Client;
using Xunit;

namespace GameStudio.Games.Tests.Endpoints
{

	public abstract class EndpointTests : IClassFixture<TestServerFixture>
	{
		protected static HttpStatusCode OK = HttpStatusCode.OK;
		protected static HttpStatusCode Created = HttpStatusCode.Created;
		protected static HttpStatusCode Accepted = HttpStatusCode.Accepted;

		readonly TestServerFixture _server;
		readonly string _baseUrl;

		//IClient _client;
		//IRequest _request;
		//HttpStatusCode _expectStatusCode;

		protected EndpointTests(TestServerFixture server, string baseUrl)
		{
			_server = server;
			_baseUrl = baseUrl;
		}

		protected RequestWrapper Get(string resource)
		{
			var client = _server.CreateClient();
			var fc = new FluentClient(client.BaseAddress + _baseUrl, client);
			var request = fc.GetAsync(resource);
			return new RequestWrapper(fc, request);
		}

		protected RequestWrapper Post<T>(string resource, T body = default(T))
		{
			var client = _server.CreateClient();
			var fc = new FluentClient(client.BaseAddress + _baseUrl, _server.CreateClient());
			var request = fc.PostAsync(resource, body);
			return new RequestWrapper(fc, request);
		}

		protected RequestWrapper Put<T>(string resource, T body = default(T))
		{
			var client = _server.CreateClient();
			var fc = new FluentClient(client.BaseAddress + _baseUrl, _server.CreateClient());
			var request = fc.PutAsync(resource, body);
			return new RequestWrapper(fc, request);
		}
		protected RequestWrapper Delete(string resource)
		{
			var client = _server.CreateClient();
			var fc = new FluentClient(client.BaseAddress + _baseUrl, _server.CreateClient());
			var request = fc.DeleteAsync(resource);
			return new RequestWrapper(fc, request);
		}
	}

	public class RequestWrapper
	{
		FluentClient _client;
		IRequest _request;
		HttpStatusCode _expectStatusCode;
		Dictionary<string,string> _headerContains = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public RequestWrapper(FluentClient client, IRequest request)
		{
			_client = client;
			_request = request;
		}

		public RequestWrapper StatusCode(HttpStatusCode status)
		{
			_expectStatusCode = status;
			return this;
		}

		public async Task<T> As<T>(Action<T> action = null)
		{
			var response = await _request.AsResponse();
				//			var txt = await _request.AsString();

			if (_expectStatusCode > 0)
				Assert.Equal((int) _expectStatusCode, (int) response.Status);

			var item = await response.As<T>();

			action?.Invoke(item);

			return item;
		}

		public RequestWrapper HeaderContains(string header, string expect)
		{
			_headerContains.Add(header,expect);
			return this;
		}

		public TaskAwaiter<IResponse> GetAwaiter()
		{
			return _request.GetAwaiter();
		}
	}
}
