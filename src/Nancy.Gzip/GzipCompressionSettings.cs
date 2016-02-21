namespace Nancy.Gzip
{
    using System.Collections.Generic;

    public class GzipCompressionSettings
    {
        public int MinimumBytes { get; set; } = 4096;

        public IList<string> MimeTypes { get; set; } = new List<string>
        {
            "text/plain",
            "text/html",
            "text/xml",
            "text/css",
            "application/json",
            "application/x-javascript",
            "application/atom+xml",
        };
    }
}
