using System;

namespace DatabaseUpgrader
{
    [global::System.Serializable]
    public class DatabaseUpgradeException : Exception
    {
        public DatabaseUpgradeException() { }
        public DatabaseUpgradeException(string message) : base(message) { }
        public DatabaseUpgradeException(string message, Exception inner) : base(message, inner) { }
        protected DatabaseUpgradeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
