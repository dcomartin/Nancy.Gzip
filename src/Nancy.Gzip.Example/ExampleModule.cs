namespace Nancy.Gzip.Example
{
    public sealed class ExampleModule : NancyModule
    {
        public ExampleModule()
        {
            Get("/example", _ => "Hello World");
        }
    }
}