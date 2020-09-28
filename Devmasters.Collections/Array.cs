using System;
using System.Collections.Generic;
using System.Linq;

namespace Devmasters.Collections
{
    public static class ArrayExt
    {

        
        public static decimal WeightedAverage<T>(this IEnumerable<T> records, Func<T, decimal> value, Func<T, decimal> weight)
        {
            if (records == null)
                throw new ArgumentNullException();
            if (records.Count() == 0)
                return 0m;

            decimal weightedValueSum = records.Sum(x => value(x) * weight(x));
            decimal weightSum = records.Sum(x => weight(x));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                throw new DivideByZeroException();
        }

        public static double WeightedAverage<T>(this IEnumerable<T> records, Func<T, double> value, Func<T, double> weight)
        {
            if (records == null)
                throw new ArgumentNullException();
            if (records.Count() == 0)
                return 0;

            double weightedValueSum = records.Sum(x => value(x) * weight(x));
            double weightSum = records.Sum(x => weight(x));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                throw new DivideByZeroException();
        }


        public static T[,] TrimArray<T>(int? rowToRemove, int? columnToRemove, T[,] originalArray)            
        {
            int resultRows = rowToRemove.HasValue ? originalArray.GetLength(0) - 1 : originalArray.GetLength(0);
            int resultColumns = columnToRemove.HasValue ? originalArray.GetLength(1) - 1 : originalArray.GetLength(1);

            T[,] result = new T[resultRows, resultColumns];

            for (int i = 0, j = 0; i < originalArray.GetLength(0); i++)
            {
                if (i == rowToRemove)
                    continue;

                for (int k = 0, u = 0; k < originalArray.GetLength(1); k++)
                {
                    if (k == columnToRemove)
                        continue;

                    result[j, u] = originalArray[i, k];
                    u++;
                }
                j++;
            }

            return result;
        }
 
        public static T[] Add<T>(this T[] target, params T[] items)
        {
            // Validate the parameters
            if (target == null)
            {
                throw new ArgumentNullException();
            }
            if (items == null)
            {
                items = new T[] { };
            }

            // Join the arrays
            T[] result = new T[target.Length + items.Length];
            target.CopyTo(result, 0);
            items.CopyTo(result, target.Length);
            return result;
        }


        public static T[] AddOrUpdate<T>(this T[] target, params T[] items)
        {
            // Validate the parameters
            if (target == null)
            {
                throw new ArgumentNullException();
            }
            if (items == null)
            {
                items = new T[] { };
            }

            List<T> newItems = new List<T>();
            foreach (var item in items)
            {
                if (item != null)
                {
                    var existingIdx = Array.FindIndex(target, e => e.Equals(item));

                    if (existingIdx > -1)
                    {
                        target[existingIdx] = item;
                    }
                    else
                        newItems.Add(item);
                }
                else
                    newItems.Add(item);
            }
            T[] newItemsArr = newItems.ToArray();

            // Join the arrays
            T[] result = new T[target.Length + newItemsArr.Length];
            target.CopyTo(result, 0);
            newItemsArr.CopyTo(result, target.Length);
            return result;
        }
        public static T[] AddOrIgnore<T>(this T[] target, params T[] items)
        {
            // Validate the parameters
            if (target == null)
            {
                throw new ArgumentNullException();
            }
            if (items == null)
            {
                items = new T[] { };
            }

            List<T> newItems = new List<T>();
            foreach (var item in items)
            {
                if (item != null)
                {
                    var existingIdx = Array.FindIndex(target, e => e.Equals(item));
                    if (existingIdx == -1)
                        newItems.Add(item);
                }
                else
                    newItems.Add(item);
            }
            T[] newItemsArr = newItems.ToArray();

            // Join the arrays
            T[] result = new T[target.Length + newItemsArr.Length];
            target.CopyTo(result, 0);
            newItemsArr.CopyTo(result, target.Length);
            return result;
        }


    }
}
