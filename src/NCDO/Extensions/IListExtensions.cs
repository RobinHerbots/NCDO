using System;
using System.Collections.Generic;
using System.Text;

namespace NCDO.Extensions
{
    public static class IListExtensions
    {
        public static void AddNew<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (!list.Contains(item))
                        list.Add(item);
                }
            }
        }
    }
}
