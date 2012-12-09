using System.Collections.Generic;
using System.Reflection;

namespace SmartConf
{
    /// <summary>
    ///     Extension class for merging objects.
    /// </summary>
    public static class MergeObjectExtensions
    {
        /// <summary>
        ///     Merges all properties in the secondary object
        ///     with those in the primary.
        ///     A property value will be set only if the
        ///     value in <paramref name="secondary" /> is
        ///     different from the value in <paramref name="primary" />
        ///     AND different from the default value (after construction).
        ///     Note: Dynamic default values (like DateTime.Now) may not
        ///     return the same default value for multiple invocations,
        ///     thus may not merge accurately.
        /// </summary>
        /// <typeparam name="T">
        ///     Type to merge. Must have a default constructor that
        ///     sets default property values.
        /// </typeparam>
        /// <param name="primary">Object to overwrite values for.</param>
        /// <param name="secondary">
        ///     Object to merge into <paramref name="primary" />.
        /// </param>
        public static void MergeWith<T>(this T primary, T secondary) where T : new()
        {
            // We need to know whether or not the value is new or from the constructor.
            // Doesn't work on objects that don't implement IEquatable.
            var defaultObject = new T();
            if (Equals(secondary, null)) return;
            if (Equals(primary, null)) primary = defaultObject;

            foreach (var pi in typeof (T).GetProperties())
            {
                var priValue = pi.GetGetMethod().Invoke(primary, null);
                var secValue = pi.GetGetMethod().Invoke(secondary, null);
                var defaultValue = typeof (T).GetProperty(pi.Name).GetGetMethod().Invoke(defaultObject, null);

                if (priValue == null && secValue != null ||
                    (priValue != null && !priValue.Equals(secValue) &&
                     (secValue != defaultValue ||
                      secValue != null && !secValue.Equals(defaultValue))))
                {
                    pi.GetSetMethod().Invoke(primary, new[] {secValue});
                }
            }
        }

        /// <summary>
        /// Merge the properties of a list of objects
        /// into a new object, in order.
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="objects">Objects to compress.</param>
        /// <returns>A single object containing the merged results.</returns>
        public static T Merge<T>(this IEnumerable<T> objects) where T : new()
        {
            return objects.Merge(new T());
        }

        /// <summary>
        /// Merge the properties of a list of objects
        /// into a new object, in order.
        /// </summary>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <param name="objects">Objects to compress.</param>
        /// <param name="seed">Seed value used as the base of the merge.</param>
        /// <returns>A single object containing the merged results.</returns>
        public static T Merge<T>(this IEnumerable<T> objects, T seed) where T : new()
        {
            foreach (var o in objects)
            {
                seed.MergeWith(o);
            }
            return seed;
        }
    }
}