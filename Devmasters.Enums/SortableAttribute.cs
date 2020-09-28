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
    [AttributeUsage(AttributeTargets.Enum)]
    public class SortableAttribute : Attribute
    {
        public enum SortAlgorithm
        {
            AlphabeticallyOnly,
            BySortValueAndThenAlphabetically,
            BySortValue,
            None
        }
        public SortAlgorithm Algorithm { get; private set; }
        public SortableAttribute(SortAlgorithm algorithm)
        {
            this.Algorithm = algorithm;
        }

    }



}
