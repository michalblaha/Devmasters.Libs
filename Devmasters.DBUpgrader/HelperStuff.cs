using System;
using System.Collections.Generic;
using System.
/* Unmerged change from project 'Devmasters.DBUpgrader (net472)'
Before:
using System.Text;
using System.Reflection;
After:
using System.Reflection;
using System.Text;
*/
Reflection;

namespace DatabaseUpgrader
{
    internal class HelperStuff
    {
        internal static Version GetVersionOfLibrary(Type member)
        {
            // this takes AssemblyVersionAttribute from assembly
            return member.Assembly.GetName().Version;
        }

        internal static IDictionary<Version, MemberInfo> GetAvailableDatabaseUpgradeMethods(Type member)
        {
            IDictionary<Version, MemberInfo> result = new Dictionary<Version, MemberInfo>();

            foreach (MemberInfo mi in member.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                foreach (object attribute in mi.GetCustomAttributes(true))
                {
                    if (attribute is DatabaseUpgradeMethodAttribute)
                    {
                        DatabaseUpgradeMethodAttribute a = attribute as DatabaseUpgradeMethodAttribute;
                        result.Add(a.Version, mi);
                    }
                }
            }

            return result;
        }

        internal static IList<MemberInfo> GetBeforeUpgradeMethods(Type member)
        {
            IList<MemberInfo> result = new List<MemberInfo>();

            foreach (MemberInfo mi in member.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (mi.IsDefined(typeof(BeforeUpgradeMethodAttribute), true))
                {
                    result.Add(mi);
                }
            }

            return result;
        }

        internal static IList<MemberInfo> GetAfterUpgradeMethods(Type member)
        {
            IList<MemberInfo> result = new List<MemberInfo>();

            foreach (MemberInfo mi in member.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (mi.IsDefined(typeof(AfterUpgradeMethodAttribute), true))
                {
                    result.Add(mi);
                }
            }

            return result;
        }

        internal static void ExecuteDatabaseUpgraderReflectionMethodWithParameters(MemberInfo method, params object[] parameters)
        {
            method.ReflectedType.InvokeMember(
                method.Name,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                null,
                null,
                parameters);
        }
    }
}
