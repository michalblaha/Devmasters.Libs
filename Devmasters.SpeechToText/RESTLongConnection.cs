﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Devmasters.SpeechToText
{
    public class RESTLongConnection
        : System.Net.WebClient
    {

        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        public RESTLongConnection() : this(18 * 60 * 60 * 1000) { } //18 hours

        public RESTLongConnection(int timeout)
        {
            this.Timeout = timeout;
            this.Encoding = System.Text.Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {

            var request = base.GetWebRequest(address);
            if (request != null)
            {
                ((HttpWebRequest)request).KeepAlive = false;
                ((HttpWebRequest)request).ReadWriteTimeout = Timeout;
                request.Timeout = this.Timeout;
            }
            return request;
        }
    }
}

