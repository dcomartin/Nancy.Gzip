using System.IO;

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

            if (ResponseIsCompressed(context.Response))
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

        private static bool ResponseIsCompressed(Response response)
        {
            var ret = response.Headers.Keys.Any(x => x.Contains("Content-Encoding"));

            return ret;
        }

        private static void CompressResponse(NancyContext context)
        {
            _settings.Logger?.Debug("GZip compress response");

            if (context.Request.Headers.AcceptEncoding.Any(x => x.Contains("deflate")))
            {
                _settings.Logger?.Debug("GZip compress response with deflate");
                context.Response.Headers["Content-Encoding"] = "deflate";

                var contents = context.Response.Contents;
                context.Response.Contents = responseStream =>
                {
                    using (DeflateStream compression = new DeflateStream(responseStream, CompressionLevel.Optimal, true))
                    {
                        contents(compression);
                    }
                };
            }
            else
            {
                _settings.Logger?.Debug("GZip compress response with gzip");
                context.Response.Headers["Content-Encoding"] = "gzip";

                var contents = context.Response.Contents;
                context.Response.Contents = responseStream =>
                {
                    using (GZipStream compression = new GZipStream(responseStream, CompressionMode.Compress, true))
                    {
                        contents(compression);
                    }
                };
            }

            using (MemoryStream mm = new MemoryStream())
            {
                context.Response.Contents.Invoke(mm);
                mm.Flush();

                string contentLength = mm.Length.ToString();
                context.Response.Headers["Content-Length"] = contentLength;
                _settings.Logger?.Debug($"GZip compress response content-length: {contentLength}");
            }
        }

        private static bool ContentLengthIsTooSmall(Response response)
        {
            var ret = true;
            string contentLength;
            if (!response.Headers.TryGetValue("Content-Length", out contentLength))
            {
                using (MemoryStream mm = new MemoryStream())
                {
                    response.Contents.Invoke(mm);
                    mm.Flush();
                    contentLength = mm.Length.ToString();
                }
            }
            _settings.Logger?.Debug($"GZip Content-Length of response is {contentLength}");

            var length = long.Parse(contentLength);
            if (length > _settings.MinimumBytes)
            {
                ret = false;
                //response.Headers.Remove("Content-Length");
            }

            _settings.Logger?.Debug($"GZip Content-Length is too small {ret}");

            return ret;
        }

        private static bool ResponseIsCompatibleMimeType(Response response)
        {
            var ret = _settings.MimeTypes.Any(x => x == response.ContentType || response.ContentType.StartsWith($"{x};"));
            _settings.Logger?.Debug($"GZip Content-Type is Mime compatible {ret}");

            return ret;
        }

        private static bool RequestIsGzipCompatible(Request request)
        {
            var ret = request.Headers.AcceptEncoding.Any(x => x.Contains("gzip") || x.Contains("deflate"));
            _settings.Logger?.Debug($"GZip Accept-Encoding is GZip or Deflate compatible {ret}");

            return ret;
        }
    }
}
