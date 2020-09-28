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
    public class ThisIsMaxValueAttribute : Attribute
    {
        protected int value = int.MinValue;
        // The constructor is called when the attribute is set.
        public ThisIsMaxValueAttribute()
        { }

        public ThisIsMaxValueAttribute(int value)
        {
            this.value = value;
        }

        public int MaxValue
        {
            get { return this.value; }
        }
    }



}
