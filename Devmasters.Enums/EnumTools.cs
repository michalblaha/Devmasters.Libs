using System;

/* Unmerged change from project 'Devmasters.Core (net472)'
Before:
using System.Linq;
After:
using System.Collections;
*/
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
/* Unmerged change from project 'Devmasters.Core (net472)'
Before:
using System.ComponentModel;
After:
using System.Text;
*/


namespace Devmasters.Enums
{

    public class EnumTools
    {
        public struct KeyValue
        {
            public string Key;
            public string Value;
            public bool Default;
            internal int? Sortvalue;
            public string[] Groups;
        }

        public struct KeyValue<T>
        {
            public string Key;
            public T Value;
            public bool Default;
            internal int? Sortvalue;
            public string[] Groups;
        }

        public static bool IsSortable<T>(out SortableAttribute.SortAlgorithm algorithm)
            where T : System.Enum
        {
            return IsSortable(typeof(T), out algorithm);
        }

        public static bool IsSortable(Type enumType, out SortableAttribute.SortAlgorithm algorithm)
        {
            algorithm = SortableAttribute.SortAlgorithm.None;
            if (enumType.GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in enumType.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(SortableAttribute))
                    {
                        algorithm = ((SortableAttribute)o).Algorithm;
                        return true;
                    }
                }
            }

            return false;
        }
        public static IEnumerable<T> InGroup<T>(string groupName, bool ignoreDisabled = false)
            where T : System.Enum
        {
            if (!typeof(T).IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            return InGroup(typeof(T), groupName, ignoreDisabled).Cast<T>();
        }
        public static IEnumerable<System.Enum> InGroup(Type enumType, string groupName, bool ignoreDisabled = false)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            List<System.Enum> groups = new List<System.Enum>();
            if (enumType.GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in enumType.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(GroupableAttribute))
                    {

                        foreach (string name in Enum.GetNames(enumType))
                        {
                            FieldInfo f = enumType.GetField(name);
                            var gn = GetGroupValues(f);
                            if (IsDisabled(f) == false || ignoreDisabled == true)
                            {
                                if (gn.Contains(groupName))
                                    groups.Add((System.Enum)Enum.Parse(enumType, name, true));
                            }
                        }

                        return groups;
                    }
                }
            }
            return groups;
        }


        public static IEnumerable<string> Groups<T>()
            where T : System.Enum
        {
            return Groups(typeof(T));
        }

        public static IEnumerable<string> Groups(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            List<string> groups = new List<string>();
            if (enumType.GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in enumType.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(GroupableAttribute))
                    {

                        foreach (string name in Enum.GetNames(enumType))
                        {
                            FieldInfo f = enumType.GetField(name);
                            var groupName = GetGroupValues(f);
                            if (groupName != null)
                                groups.AddRange(groupName);
                        }

                        return groups.Distinct();
                    }
                }
            }
            return new string[] { };
        }
        public static bool IsGroupable<T>()
            where T : System.Enum
        {
            return IsGroupable(typeof(T));
        }
        public static bool IsGroupable(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            List<string> groups = new List<string>();
            if (enumType.GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in enumType.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(GroupableAttribute))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasNiceNames<T>()
            where T : System.Enum
        {
            return HasNiceNames(typeof(T));
        }

        public static bool HasNiceNames(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            if (enumType.GetCustomAttributes(false).Length > 0)
            {
                foreach (object o in enumType.GetCustomAttributes(false))
                {
                    if (o.GetType() == typeof(ShowNiceDisplayNameAttribute))
                        return true;
                }
            }
            return false;
        }

        public static int? GetMaxValue(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            foreach (string name in Enum.GetNames(enumType))
            {
                FieldInfo f = enumType.GetField(name);
                if (f != null)
                {
                    object[] attributes = f.GetCustomAttributes(false);
                    if (attributes.Length > 0)
                    {
                        foreach (object o in attributes)
                        {
                            if (o.GetType() == typeof(ThisIsMaxValueAttribute))
                            {
                                ThisIsMaxValueAttribute attr = o as ThisIsMaxValueAttribute;
                                if (attr != null)
                                {
                                    if (attr.MaxValue == int.MinValue)
                                    {
                                        try
                                        {
                                            return Convert.ToInt32(Enum.Parse(enumType, name));
                                        }
                                        catch
                                        {
                                            return null;
                                        }
                                    }

                                    else
                                        return attr.MaxValue;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }


        public static System.Enum GetValueOrDefaultValue(string inputValue, Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            if (!string.IsNullOrEmpty(inputValue))
                if (Enum.IsDefined(enumType, inputValue))
                    //return Enum.Parse(enumType, inputValue, true);
                    return (System.Enum)Enum.Parse(enumType, inputValue, true);



            //return defaultValue
            foreach (string name in Enum.GetNames(enumType))
            {
                FieldInfo f = enumType.GetField(name);
                if (f != null)
                {
                    object[] attributes = f.GetCustomAttributes(false);
                    if (attributes.Length > 0)
                    {
                        foreach (object o in attributes)
                        {
                            if (o.GetType() == typeof(DefaultValueAttribute))
                            {
                                return (System.Enum)Enum.Parse(enumType, name);
                            }
                        }
                    }
                }
            }
            throw new ArgumentOutOfRangeException("inputValue doesn't exists in this enumeration and no default value is set");
        }



        public static bool IsDisabled(FieldInfo field)
        {
            if (field != null)
            {
                object[] attributes = field.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    foreach (object o in attributes)
                    {
                        if (o.GetType() == typeof(DisabledAttribute))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static string GetNiceDisplayName(FieldInfo field)
        {
            if (field != null)
            {
                object[] attributes = field.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    foreach (object o in attributes)
                    {
                        if (o.GetType() == typeof(NiceDisplayNameAttribute))
                        {
                            NiceDisplayNameAttribute dnn = (NiceDisplayNameAttribute)o;
                            return dnn.DisplayName;
                        }
                    }
                }
            }
            return null;
        }

        public static int? GetSortableValue(FieldInfo field)
        {
            if (field != null)
            {
                object[] attributes = field.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    foreach (object o in attributes)
                    {
                        if (o.GetType() == typeof(SortValueAttribute))
                        {
                            SortValueAttribute dnn = (SortValueAttribute)o;
                            return dnn.SortValue;
                        }
                    }
                }
            }
            return null;
        }


        public static string[] GetGroupValues(FieldInfo field)
        {
            if (field != null)
            {
                object[] attributes = field.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    foreach (object o in attributes)
                    {
                        if (o.GetType() == typeof(GroupValueAttribute))
                        {
                            GroupValueAttribute dnn = (GroupValueAttribute)o;
                            return dnn.GroupValues;
                        }
                    }
                }
                return new string[] { };
            }
            else
                return null;
        }

        public static List<KeyValue<T>> EnumToEnumerable<T>(bool ignoreDisabled = false)
        {
            return EnumToEnumerable<T>(System.Globalization.CultureInfo.CurrentUICulture, ignoreDisabled);
        }
        public static List<KeyValue<T>> EnumToEnumerable<T>(System.Globalization.CultureInfo culture, bool ignoreDisabled = false)
        {
            if (typeof(T).IsEnum == false)
                throw new ArgumentException(typeof(T).Name + " is not enum");

            var kv = EnumToEnumerable(typeof(T), culture, ignoreDisabled);
            return kv.Select(m => new KeyValue<T>()
            {
                Default = m.Default,
                Groups = m.Groups,
                Key = m.Key,
                Sortvalue = m.Sortvalue,
                Value = (T)Enum.Parse(typeof(T), m.Value)
            })
                .ToList();
        }

        public static List<KeyValue> EnumToEnumerable(Type enumType, bool ignoreDisabled = false)
        {
            return EnumToEnumerable(enumType, System.Globalization.CultureInfo.CurrentUICulture, ignoreDisabled);
        }
        public static List<KeyValue> EnumToEnumerable(Type enumType, System.Globalization.CultureInfo culture, bool ignoreDisabled = false)
        {
            if (!enumType.IsEnum)
                throw new InvalidCastException("Parameters must be enumeration");

            bool useNiceNames = HasNiceNames(enumType);

            SortableAttribute.SortAlgorithm sortAlg;
            bool sortable = IsSortable(enumType, out sortAlg);

            bool groupable = IsGroupable(enumType);

            List<KeyValue> coll = new List<KeyValue>();

            foreach (string name in Enum.GetNames(enumType))
            {
                //is disabled?
                if (ignoreDisabled == false)
                    if (IsDisabled(enumType.GetField(name)))
                        continue; //is disabled, skip this value


                KeyValue kv = default(KeyValue);
                kv.Key = null;
                kv.Default = ((Enum)Enum.Parse(enumType, name)).IsDefault();


                if (useNiceNames && kv.Key == null)
                {
                    kv.Key = GetNiceDisplayName(enumType.GetField(name));
                }

                //default. value for all other cases
                if (kv.Key == null)
                    kv.Key = name;


                if (Enum.GetUnderlyingType(enumType).UnderlyingSystemType == typeof(byte))
                    kv.Value = ((int)Enum.Parse(enumType, name)).ToString();
                else if (Enum.GetUnderlyingType(enumType).UnderlyingSystemType == typeof(Int16))
                    kv.Value = ((int)Enum.Parse(enumType, name)).ToString();
                else if (Enum.GetUnderlyingType(enumType).UnderlyingSystemType == typeof(Int64))
                    kv.Value = Convert.ToInt32((string)Enum.Parse(enumType, name)).ToString();
                else
                    kv.Value = ((Int32)Enum.Parse(enumType, name)).ToString();


                //kv.Value = Enum.Parse(enumType, name).ToString();

                if (sortable)
                {
                    kv.Sortvalue = GetSortableValue(enumType.GetField(name));
                }
                if (groupable)
                    kv.Groups = GetGroupValues(enumType.GetField(name));

                coll.Add(kv);
            }
            if (sortable)
            {

                coll = Sort(coll, sortAlg).ToList();
            }

            return coll;
        }

        private static IEnumerable<KeyValue> Sort(IEnumerable<KeyValue> collection, SortableAttribute.SortAlgorithm algoritm)
        {
            switch (algoritm)
            {
                case SortableAttribute.SortAlgorithm.AlphabeticallyOnly:
                    return collection.OrderBy(m => m.Key);
                    break;
                case SortableAttribute.SortAlgorithm.BySortValueAndThenAlphabetically:
                    return collection
                            .OrderBy(n => n.Sortvalue ?? Convert.ToInt32(n.Value))
                            .ThenBy(n => n.Key);
                    break;
                case SortableAttribute.SortAlgorithm.BySortValue:
                    return collection.Select(m => new KeyValue()
                    {
                        Default = m.Default,
                        Key = m.Key,
                        Value = m.Value,
                        Sortvalue = m.Sortvalue ?? Convert.ToInt32(m.Value),
                        Groups = m.Groups
                    })
                                    .OrderBy(n => n.Sortvalue);

                    ;
                    break;
                default: //SortableAttribute.SortAlgorithm.None
                    return collection;
                    break;
            }
        }


    }



}
