using System;
using System.Collections.Generic;
using System.Text;

namespace Devmasters.Cache.Elastic
{
    internal class Bag<T>
              where T : class
    {
        [Nest.Keyword]
        public string Id { get; set; }

        [Nest.Date]
        public DateTime Updated { get; set; }
        [Nest.Date]
        public DateTime ExpirationTime { get; set; }
        [Nest.Keyword]
        public string ProviderId { get; set; }

        string _rawVal = null;

        [Nest.Object(Enabled = false)]
        public string RawValue
        {
            get
            {
                return _rawVal;
            }
            set
            {
                _rawVal = value;
                _val = null;
            }
        }


        T _val = null;
        [Nest.Object(Ignore = true)]
        public T Value
        {
            get
            {
                if (_val == null && !string.IsNullOrWhiteSpace(this.RawValue))
                    _val = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(this.RawValue);
                return _val;
            }

            set
            {
                _val = value;
                _rawVal = Newtonsoft.Json.JsonConvert.SerializeObject(_val);
            }
        }
    }
}
