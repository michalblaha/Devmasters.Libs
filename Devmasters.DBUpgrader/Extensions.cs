using System.Data;
using System
/* Unmerged change from project 'Devmasters.DBUpgrader (net472)'
Before:
using System.Linq;
using System.Text;
After:
using System.Data;
using System.Linq;
*/
.Text;

namespace DatabaseUpgrader
{
    internal static class Extensions
    {
        public static void SmartOpen(this IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
    }
}
