using System;
using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace GameStudio.Games.LoadTests
{
	public class RequestBuilder
	{
		WebTestRequest _request;

		private RequestBuilder(WebTestRequest request)
		{
			_request = request;
		}

		public static RequestBuilder Get(Uri uri, string contentType = "application/json")
		{
			return Request(uri,HttpMethod.Get,contentType);
		}

		public static RequestBuilder Post(Uri uri, string contentType = "application/json")
		{
			return Request(uri, HttpMethod.Post, contentType);
		}

		public static RequestBuilder Put(Uri uri, string contentType = "application/json")
		{
			return Request(uri, HttpMethod.Put, contentType);
		}

		public static RequestBuilder Delete(Uri uri, string contentType = "application/json")
		{
			return Request(uri, HttpMethod.Delete);
		}

		public static RequestBuilder Request(Uri uri, HttpMethod method, string contentType = "application/json")
		{
			return new RequestBuilder(new WebTestRequest(uri) { Method = method.ToString() })
				.WithHeader("Content-Type", contentType);
		}

		public RequestBuilder WithHeader( string name, string value)
		{
			if (_request.Headers.Contains(name))
				_request.Headers.First(h => h.Name == name).Value = value;
			else
				_request.Headers.Add(new WebTestRequestHeader(name, value));

			return this;
		}

		public RequestBuilder WithBody(byte[] bytes, string contentType = "application/json")
		{
			_request.Body = new BinaryHttpBody {Data = bytes, ContentType = contentType};
			return this;
		}

		public RequestBuilder WithBody(string body, string contentType = "application/json")
		{
			_request.Body = new StringHttpBody { BodyString = body, ContentType = contentType};
			return this;
		}

		public RequestBuilder WithRequest(WebTestRequest req)
		{
			_request.DependentRequests.Add(req);
			return this;
		}

		public WebTestRequest Build()
		{
			return _request;
		}
	}
}
