using System;

namespace Devmasters.Net.HttpClient
{
    public abstract class ContentResultBase
    {
        public UrlContentContext Context { get; internal set; }
        public Exception Exception { get; internal set; }


    }

    public class TextContentResult : ContentResultBase
    {
        public string Text { get; internal set; }

    }
    public class BinaryContentResult : ContentResultBase
    {
        public byte[] Binary { get; internal set; }

    }
}
