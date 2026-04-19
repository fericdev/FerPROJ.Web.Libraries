using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FerPROJ.Web.Libraries.BaseDataExtensions {
    public static class PropertiesExtensions {

        #region Convert
        public static T To<T>(this object value) {

            if (value == null) {
                return (T)default;
            }

            if (value is T) {
                return (T)value;
            }

            Type targetType = typeof(T);

            // Check if the target type has a public static Parse method
            MethodInfo parseMethod = targetType.GetMethod("Parse", new[] { typeof(string) });
            if (parseMethod != null && parseMethod.IsStatic && parseMethod.ReturnType == targetType) {
                return (T)parseMethod.Invoke(null, new[] { value.ToString() });
            }

            // Additional conversion logic
            if (targetType == typeof(int)) {
                return (T)(object)int.Parse(value.ToString(), CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(long)) {
                return (T)(object)long.Parse(value.ToString(), CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(float)) {
                return (T)(object)float.Parse(value.ToString(), CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(double)) {
                return (T)(object)double.Parse(value.ToString(), CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(char)) {
                if (value.ToString().Length == 1) {
                    return (T)(object)value.ToString()[0];
                }
            }

            if (targetType == typeof(Guid)) {
                return (T)(object)Guid.Parse(value.ToString());
            }

            if (targetType == typeof(bool)) {
                return (T)(object)bool.Parse(value.ToString());
            }

            if (targetType == typeof(string)) {
                return (T)(object)value.ToString();
            }

            throw new InvalidCastException($"Cannot convert {value.GetType().Name} to {typeof(T).Name}");
        }
        #endregion

        #region Properties
        public static TType GetPropertyValue<TType>(this object obj, string propertyName) {
            try {
                if (obj == null)
                    return default;

                var propertyInfo = obj.GetPropertyInfo(propertyName);
                if (propertyInfo == null)
                    return default;

                var value = propertyInfo.GetValue(obj);
                if (value == null)
                    return default;

                if (value is TType variable)
                    return variable;

                return value.To<TType>();
            }
            catch {
                return default;
            }
        }
        public static PropertyInfo GetPropertyInfo<T>(this T obj, string propertyName) {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            var type = obj.GetType();

            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static PropertyInfo GetPropertyInfo(this Type obj, string propertyName) {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            return obj.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static PropertyInfo GetPropertyInfo<T>(this T obj, Expression<Func<T, object>> propertyExpression) {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var memberExpression = propertyExpression.Body as MemberExpression;

            // Handle value types (boxing)
            if (memberExpression == null && propertyExpression.Body is UnaryExpression unaryExpression)
                memberExpression = unaryExpression.Operand as MemberExpression;

            if (memberExpression.Member is PropertyInfo propertyInfo)
                return propertyInfo;

            return null;
        }
        #endregion

        #region Null Check
        public static bool IsNullOrEmpty(this Guid? value) {
            try {
                return value == null || value.Value == Guid.Empty;
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty(this Guid value) {
            try {
                return value == null || value == Guid.Empty;
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty(this string value) {
            try {
                return string.IsNullOrEmpty(value);
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty(this DateTime? value) {
            try {
                return value == null || value.Value == default || value.Value == DateTime.MinValue;
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty(this DateTime value) {
            try {
                return value == null || value == default || value == DateTime.MinValue;
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty<T>(this T value) where T : class {
            try {
                return value == null;
            }
            catch {
                return false;
            }
        }
        public static bool IsNullOrEmpty<T>(this List<T> value) where T : class {
            try {
                return value == null || value.Count == 0;
            }
            catch {
                return false;
            }
        }
        #endregion

        #region Search Date
        public static bool SearchForDate(this DateTime source, DateTime? dateFrom, DateTime? dateTo) {

            if (dateFrom.IsNullOrEmpty() && dateTo.IsNullOrEmpty())
                return true;

            bool afterStart = dateFrom.IsNullOrEmpty() || source >= dateFrom.Value.Date;

            bool beforeEnd = dateTo.IsNullOrEmpty() || source <= dateTo.Value.Date.AddDays(1).AddTicks(-1);

            return afterStart && beforeEnd;
        }
        public static bool SearchForDate<TSource>(this TSource source, DateTime? dateFrom, DateTime? dateTo, Expression<Func<TSource, DateTime>> dateFilter) {

            if (source == null)
                return false;

            // If no filter range → always valid
            if (dateFrom.IsNullOrEmpty() && dateTo.IsNullOrEmpty())
                return true;

            // Compile expression and get value
            var getter = dateFilter.Compile();

            var value = getter(source);

            return value.SearchForDate(dateFrom, dateTo);
        }
        #endregion

        #region Search String
        public static bool SearchContains(this string source, string searchText) {

            if (source == null || searchText == null)
                return false;

            source = source.ToLower();

            searchText = searchText.ToLower();

            return source.Contains(searchText);
        }
        public static bool SearchFor(this string source, string searchText) {
            if (source == null || searchText == null)
                return false;

            return source.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        public static bool SearchFor<TSource>(this TSource source, string searchText, DateTime? dateFrom, DateTime? dateTo, Expression<Func<TSource, DateTime>> dateFilter) {
            return source.SearchForText(searchText) && source.SearchForDate(dateFrom, dateTo, dateFilter);
        }
        public static bool SearchForText(this object source, string searchText) {
            if (source == null)
                return false;

            if (string.IsNullOrEmpty(searchText)) {
                return true;
            }

            // Get all public instance properties of the object
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties) {
                // Skip properties with index parameters
                if (property.GetIndexParameters().Length > 0)
                    continue;

                // Get the property value
                var value = property.GetValue(source);

                // If the value is null, skip this property
                if (value == null)
                    continue;

                if (value.ToString().SearchContains(searchText))
                    return true;
            }

            return false; // No match found
        }
        #endregion

    }
}
