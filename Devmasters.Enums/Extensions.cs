using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Devmasters.Enums
{
    public static class Extensions
    {
        
        
        public static string FullEnumValue(this Enum value)
        {
            return String.Format("{0}_{1}", value.GetType().Name.ToString(), Enum.GetName(value.GetType(), value));

        }

        public static string ToNiceDisplayName(this Enum value)
        {
            string result = value.ToString();
            //try NiceDisplayName
            if (value.GetType().GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in value.GetType().GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(ShowNiceDisplayNameAttribute))
                    {
                        //look for NiceDisplayAttribute
                        object[] attributes = value.GetType().GetField(result).GetCustomAttributes(false);
                        if (attributes.Length > 0)
                        {
                            foreach (object oa in attributes)
                            {
                                if (oa.GetType() == typeof(NiceDisplayNameAttribute))
                                {
                                    NiceDisplayNameAttribute dnn = (NiceDisplayNameAttribute)oa;
                                    return dnn.DisplayName;
                                }
                            }
                        }


                    }
                }
            }
            return result;

        }

        public static string[] GroupValues(this Enum value)
        {
            return EnumTools.GetGroupValues(value.GetType().GetField(value.ToString()));
        }

        public static bool IsDefault(this Enum value)
        {
            object[] attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(false);
            foreach (object oa in attributes)
                if (oa.GetType() == typeof(DefaultValueAttribute))
                    return true;

            return false;
        }

        public static TEnum ConvertToEnum<TEnum>(this int input) where TEnum : Enum
        {
            return (TEnum)Enum.Parse(typeof(TEnum), input.ToString());
        }

        public static IEnumerable<T> GetUniqueFlags<T>(this Enum flags) where T : Enum
        {
            foreach (Enum value in Enum.GetValues(flags.GetType()))
                if (flags.HasFlag(value))
                    yield return (T)value;
        }
    }
}
