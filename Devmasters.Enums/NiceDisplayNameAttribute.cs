using System;

/* Unmerged change from project 'Devmasters.Core (net472)'
Before:
using System.Linq;
After:
using System.Collections;
*/
/* Unmerged change from project 'Devmasters.Core (net472)'
Before:
using System.ComponentModel;
After:
using System.Text;
*/


namespace Devmasters.Enums
{
    [AttributeUsage(AttributeTargets.All)]
    public class NiceDisplayNameAttribute : Attribute
    {
        protected string displayedValue = "";
        // The constructor is called when the attribute is set.
        public NiceDisplayNameAttribute(string value)
        {
            displayedValue = value;
        }

        public string DisplayName
        {
            get { return displayedValue; }
        }
    }



}
