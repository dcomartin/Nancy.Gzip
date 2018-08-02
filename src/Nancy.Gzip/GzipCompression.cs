namespace Nancy.Gzip
{
    using System.IO.Compression;
    using System.Linq;
    using Bootstrapper;

    public static class GzipCompression
    {
        private static GzipCompressionSettings _settings;

        public static void EnableGzipCompression(this IPipelines pipelines, GzipCompressionSettings settings)
        {
            _settings = settings;
            pipelines.AfterRequest += CheckForCompression;
        }

        public static void EnableGzipCompression(this IPipelines pipelines)
        {
            EnableGzipCompression(pipelines, new GzipCompressionSettings());
        }

        private static void CheckForCompression(NancyContext context)
        {
            if (!RequestIsGzipCompatible(context.Request))
            {
                return;
            }

            if (context.Response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            if (!ResponseIsCompatibleMimeType(context.Response))
            {
                return;
            }

            if (ContentLengthIsTooSmall(context.Response))
            {
                return;
            }

            CompressResponse(context);
        }

        private static void CompressResponse(NancyContext context)
        {
            if (context.Request.Headers.AcceptEncoding.Any(x => x.Contains("deflate")))
            {
                context.Response.Headers["Content-Encoding"] = "deflate";

                var contents = context.Response.Contents;
                context.Response.Contents = responseStream =>
                {
                    using (var compression = new DeflateStream(responseStream, CompressionLevel.Optimal, true))
                    {
                        contents(compression);
                    }
                };
            }
            else
            {
                context.Response.Headers["Content-Encoding"] = "gzip";

                var contents = context.Response.Contents;
                context.Response.Contents = responseStream =>
                {
                    using (var compression = new GZipStream(responseStream, CompressionMode.Compress))
                    {
                        contents(compression);
                    }
                };
            }

            context.Response.Headers.Remove("Content-Length");
        }

        private static bool ContentLengthIsTooSmall(Response response)
        {
            var ret = true;
            string contentLength;
            if (!response.Headers.TryGetValue("Content-Length", out contentLength)) contentLength ="0";
            var length = long.Parse(contentLength);
            if (length > _settings.MinimumBytes)
            {
                ret = false;
            }
            return ret;
        }

        private static bool ResponseIsCompatibleMimeType(Response response)
        {
            return _settings.MimeTypes.Any(x => x == response.ContentType || response.ContentType.StartsWith($"{x};"));
        }

        private static bool RequestIsGzipCompatible(Request request)
        {
            return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip") || x.Contains("deflate"));
        }
    }
}
