using System.Threading.Tasks;

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

        private class TestModule : NancyModule
        {
            public TestModule()
            {
                Get("/ok", _ =>
                {
                    var response = new Response
                    {
                        ContentType = "application/json; charset=utf-8",
                        StatusCode = HttpStatusCode.OK,
                    };

                    return response;
                });

                Get("/small", _ =>
                {
                    var response = new Response();
                    response.Headers.Add("Content-Length", "0");
                    return response;
                });


                Get("/novalidcontenttype", _ =>
                {
                    var response = new Response();
                    response.Headers.Add("Content-Type", "application/x-not-valid");
                    return response;
                });

                Get("/nocontent", _ => HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task should_return_content_encoding_when_accept()
        {
            var response = await _app.Get("/ok", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });

            response.Headers["Content-Encoding"].ShouldBeEquivalentTo("gzip");
        }

        [Fact]
        public async Task should_return_no_content_encoding_when_too_small()
        {
            var response = await _app.Get("/small", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });

            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public async Task should_return_no_content_encoding_when_not_valid_content_type()
        {
            var response = await _app.Get("/novalidcontenttype", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });

            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public async Task should_return_no_content_encoding_when_not_valid_accept()
        {
            var response = await _app.Get("/ok", context =>
            {
                context.HttpRequest();
            });

            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }

        [Fact]
        public async Task should_return_no_content_encoding_when_not_valid_return_status_code()
        {
            var response = await _app.Get("/nocontent", context =>
            {
                context.Header("Accept-Encoding", "gzip");
                context.HttpRequest();
            });

            response.Headers.ContainsKey("Content-Encoding").Should().BeFalse();
        }
    }
}
