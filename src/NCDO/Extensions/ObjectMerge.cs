using System;
using System.Reflection;
using NCDO.Interfaces;

namespace NCDO.Extensions
{
    public static class ObjectMerge
    {
        /// <summary>
        /// Conventional merge object into the target object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Merge<T>(this T target, T source) where T : ICloudDataRecord
        {
            if (source != null)
            {
                foreach (var propertyInfo in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                )
                {
                    if (propertyInfo.CanRead)
                    {
                        var targetValue = propertyInfo.GetValue(target);
                        if (propertyInfo.CanWrite && propertyInfo.PropertyType.GetDefaultValue() == targetValue)
                        {
                            propertyInfo.SetValue(target, propertyInfo.GetValue(source));
                        }
                    }
                }
            }

            return target;
        }

        public static object GetDefaultValue(this Type target)
        {
            return target.GetType().IsValueType ? Activator.CreateInstance(target.GetType()) : null;
        }
    }
}
