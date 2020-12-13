using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devmasters;

namespace Devmasters
{
    public class Args
        : IEnumerable<string>
    {
        public string[] Arguments { get; }
        public string[] Mandatory { get; }
        public string ParametrPrefix { get; }
        public char ParamValueDelimiter { get; }

        private static Dictionary<string, string> args = new Dictionary<string, string>();


        public Args(string[] commandLineArguments, 
            string[] mandatory = null,
            string parametrPrefix = "/", 
            char paramValueDelimiter = '=')
        {
            Arguments = commandLineArguments;
            ParametrPrefix = parametrPrefix;
            ParamValueDelimiter = paramValueDelimiter;

            Init();
        }

        protected virtual void Init()
        {
            args = this.Arguments
                .Select(m => m.Split('='))
                .ToDictionary(m => m[0].ToLower(), v => v.Length == 1 ? "" : v[1]);
        }
        protected virtual string Fix(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            else
                return s.Trim().ToLower();
        }

        public bool MandatoryPresent()
        {
            if (this.Mandatory == null)
                return true;
            if (this.Mandatory.Count() == 0)
                return true;

            return this.Mandatory.All(m => Exists(m));
        }

        public string Get(string parameter, string valueIfMissing = "")
        {
            return args.TryGetValue(Fix(parameter), out var value) ? value : valueIfMissing;
        }
        public int? GetNumber(string parameter, int? valueIfMissing = null)
        {
            if (int.TryParse(Get(parameter), out var res))
                return res;
            else
                return valueIfMissing;
        }
        public decimal? GetPrice(string parameter, decimal? valueIfMissing = null)
        {
            decimal? num = Get(parameter)?.ToDecimal(valueIfMissing);
            return num;
        }
        public string[] GetArray(string parameter, char valuedelimiter=',')
        {
            var val = Get(parameter);
            return val.Split(new char[] { valuedelimiter }, StringSplitOptions.RemoveEmptyEntries);
        }
        public DateTime? GetDate(string parameter, string format = null, DateTime? valueIfMissing = null)
        {
            var val = Get(parameter);
            if (string.IsNullOrEmpty(format))
                return Devmasters.DT.Util.ToDate(val) ?? valueIfMissing;
            else
                return DT.Util.ToDateTime(val, format) ?? valueIfMissing;
        }
        public DateTime? GetDateTime(string parameter, string format = null, DateTime? valueIfMissing = null)
        {
            var val = Get(parameter);
            if (string.IsNullOrEmpty(format))
                return Devmasters.DT.Util.ToDateTime(val) ?? valueIfMissing;
            else
                return DT.Util.ToDateTime(val, format) ?? valueIfMissing;
        }

        public bool Exists(string parameter)
        {
            return args.ContainsKey(Fix(parameter));
        }
        public bool HasValue(string parameter)
        {
            return !string.IsNullOrEmpty(Get(parameter));
        }

        public string this[string parameter]
        {
            get
            {
                return Get(parameter);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return args.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
