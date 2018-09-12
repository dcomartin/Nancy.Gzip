using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Nancy.Gzip.Example
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
#pragma warning disable S125 // Sections of code should not be commented out
            // Enable Compression with Settings
            //var settings = new GzipCompressionSettings();
            //settings.MinimumBytes = 1024;
            //settings.MimeTypes.Add("application/vnd.myexample");
            //pipelines.EnableGzipCompression(settings);
#pragma warning restore S125 // Sections of code should not be commented out

            // Enable Compression with Default Settings
            pipelines.EnableGzipCompression();

            base.ApplicationStartup(container, pipelines);
        }
    }
}