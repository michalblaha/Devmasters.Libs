using System;
using System.Linq;
using System.Collections.Generic;
using Devmasters.SpeechToText;

namespace Newton.SpeechToText.Cloud.FileAPI
{
    public class VoiceToTerms

    {
        private AccessToken accessToken = null;
        private NtxToken ntxToken = null;
        private string mp3file = null;

        private bool converted = false;

        public decimal MinDelayToCreateParagraph = 1.0m;


        public VoiceToTerms(string fullPathToMP3File,
            string username, string password,
            string taskId, string taskLabel, string audience = "https://newton.nanogrid.cloud/")
        {
            accessToken = AccessToken.Login(username, password, audience);
            ntxToken = NtxToken.GetToken(accessToken, taskId, taskLabel);
            mp3file = fullPathToMP3File;
        }

        public VoiceToTerms(string fullPathToMP3File, AccessToken accessToken, NtxToken ntxToken)
        {
            this.accessToken = accessToken;
            this.ntxToken = ntxToken;
            mp3file = fullPathToMP3File;
        }
        public VoiceToTerms(string rawData)
        {
            converted = true;
            this.Raw = rawData;
            this.Chunks = RawToChunks(this.Raw);
            this.Terms = ChunksToTerms(this.Chunks);
        }


        public bool Convert()
        {
            if (mp3file == null)
                throw new ArgumentNullException("mp3file", "is null");
            if (System.IO.File.Exists(mp3file) == false)
                throw new ArgumentNullException("mp3file", "no file on path " + mp3file);

            using ( RESTLongConnection wc = new RESTLongConnection())
            {
                string url = this.accessToken.Audience + "api/v1/file/v2t";
                wc.Headers.Add("ntx-token", ntxToken.ntxToken);
                var resbyte = wc.UploadFile(url, "POST", this.mp3file);
                var res = System.Text.Encoding.UTF8.GetString(resbyte);
                converted = true;
                this.Raw = res;
                this.Chunks = RawToChunks(this.Raw);
                this.Terms = ChunksToTerms(this.Chunks);
            }
            return true;
        }

        private string _raw;
        public string Raw
        {
            get
            {
                if (converted == false)
                    throw new ApplicationException("Start Conversion with Convert() method first.");
                else return _raw;
            }
            private set => _raw = value;
        }

        private IEnumerable<ChunkLine> _chunks;
        public IEnumerable<ChunkLine> Chunks
        {
            get
            {
                if (converted == false)
                    throw new ApplicationException("Start Conversion with Convert() method first.");
                else return _chunks;
            }
            private set => _chunks = value;
        }

        private IEnumerable<Term> _terms;
        public IEnumerable<Term> Terms
        {
            get
            {
                if (converted == false)
                    throw new ApplicationException("Start Conversion with Convert() method first.");
                else return _terms;
            }

            private set => _terms = value;
        }

        public static IEnumerable<Term> ChunksToTerms(IEnumerable<ChunkLine> chunks)
        {
            List<Term> terms = new List<Term>();
            Term prev = null;
            foreach (var ch in chunks)
            {
                var chTerms = ch?.push?.events?.ToTerms(prev);
                if (chTerms != null && chTerms.Count > 0)
                {
                    terms.AddRange(chTerms);
                    prev = chTerms.Last();
                }

            }

            return terms;
        }

        public static IEnumerable<ChunkLine> RawToChunks(string rawApiResult)
        {
            List<ChunkLine> lines = new List<ChunkLine>();
            foreach (var line in rawApiResult.Split('\r', '\n'))
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var ev = Newtonsoft.Json.JsonConvert.DeserializeObject<ChunkLine>(line);
                    if (ev?.push != null)
                        lines.Add(ev);
                }
            }

            return lines;

        }
    }
}

