﻿using System;

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
    public class GroupValueAttribute : Attribute
    {
        public string[] GroupValues { get; private set; }
        public GroupValueAttribute(params string[] value)
        {
            GroupValues = value;
        }


    }



}
