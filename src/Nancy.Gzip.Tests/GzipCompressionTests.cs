using System;
using System.Linq;

namespace Nancy.Gzip.Tests
{
    using FluentAssertions;
    using Testing;
    using Xunit;

    public class GzipCompressionTests
    {
        private readonly Browser _app = new Browser(with =>
        {
            with.Module<TestModule>();
            with.ApplicationStartup((container, pipelines) =>
            {
                pipelines.EnableGzipCompression();
            });
        });

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestModule : NancyModule
        {
            private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVXYZ";

#pragma warning disable S1144 // Unused private types or members should be removed
            public TestModule()
            {
                Get["/ok"] = _ =>
                {
                    Response response = String.Join(",", Enumerable.Repeat(Alphabet, 1000));
                    response.ContentType = "application/json; charset=utf-8";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                };

                Get["/small"] = _ =>
                {
                    var response = new Response();
                    response.Headers.Add("Content-Length", "0");
                    return response;
                };


                Get["/novalidcontenttype"] = _ =>
                {
                    Response response = Alphabet;
                    response.Headers.Add("Content-Type", "application/x-not-valid");
                    return response;
                };

                Get["/nocontent"] = _ => HttpStatusCode.NoContent;
            }
#pragma warning restore S1144 // Unused private types or members should be removed
        }

        [Fact]
        public void should_return_content_encoding_when_accept()
        {
            BrowserResponse response = _app.Get("/ok", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });

            response.Headers.ContainsKey("Content-Encoding").Should().BeTrue();
            response.Headers["Content-Encoding"].Should().Be("gzip");
        }

        [Fact]
        public void should_return_no_content_encoding_when_too_small()
        {
            BrowserResponse response = _app.Get("/small", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });
            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public void should_return_no_content_encoding_when_not_valid_content_type()
        {
            BrowserResponse response = _app.Get("/novalidcontenttype", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });
            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public void should_return_no_content_encoding_when_not_valid_accept()
        {
            BrowserResponse response = _app.Get("/ok", context =>
            {
                context.HttpRequest();
            });
            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public void should_return_no_content_encoding_when_not_valid_return_status_code()
        {
            BrowserResponse response = _app.Get("/nocontent", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });
            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }
    }
}
