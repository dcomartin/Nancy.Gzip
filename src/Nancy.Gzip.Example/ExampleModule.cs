﻿namespace Nancy.Gzip.Example
{
    public class ExampleModule : NancyModule
    {
        public ExampleModule()
        {
            Get("/example", _ => "Hello World");
        }
    }
}