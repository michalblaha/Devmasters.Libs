using System;

namespace DatabaseUpgrader
{
    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DatabaseUpgradeMethodAttribute : Attribute
    {
        readonly Version _version;

        public DatabaseUpgradeMethodAttribute(string version)
        {
            this._version = new Version(version);
        }

        public DatabaseUpgradeMethodAttribute(int major, int minor, int build, int revision)
        {
            this._version = new Version(major, minor, build, revision);
        }

        public Version Version
        {
            get { return this._version; }
        }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class BeforeUpgradeMethodAttribute : Attribute
    {
        public BeforeUpgradeMethodAttribute()
        { }
    }

    [global::System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AfterUpgradeMethodAttribute : Attribute
    {
        public AfterUpgradeMethodAttribute()
        { }
    }
}
