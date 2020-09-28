using System;

namespace Devmasters.Net.HttpClient
{
    public class UrlContentException
        : ApplicationException
    {
        public byte[] DownloadedContent { get; set; }
        public UrlContentException()
            : base()
        { }

        public UrlContentException(string message)
            : base(message)
        { }

        public UrlContentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
