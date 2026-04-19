using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FerPROJ.Libraries.DBHelper.Extensions {
    public static class QueryableExtensions {

        #region Search By Date
        public static IQueryable<T> SearchDateRange<T>(this IQueryable<T> queryable, DateTime? dateFrom, DateTime? dateTo, string dateProperty = "") {
            if (!dateFrom.HasValue && !dateTo.HasValue) {
                return queryable; // Return original collection if both dates are null
            }

            ParameterExpression param = Expression.Parameter(typeof(T), "x");

            Expression combinedExpression = null;

            // If specific property is provided
            if (!string.IsNullOrEmpty(dateProperty)) {
                combinedExpression = BuildDateExpression<T>(param, dateFrom, dateTo, dateProperty);
            }
            else {
                // Search all DateTime properties
                var dateProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                                              .ToList();

                foreach (var prop in dateProperties) {
                    var expr = BuildDateExpression<T>(param, dateFrom, dateTo, prop.Name);

                    // Combine multiple expressions using OR
                    combinedExpression = combinedExpression == null
                        ? expr
                        : Expression.OrElse(combinedExpression, expr);
                }
            }

            // If no DateTime properties found, return original query
            if (combinedExpression == null)
                return queryable;

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, param);
            return queryable.Where(lambda);
        }

        private static Expression BuildDateExpression<T>(ParameterExpression param, DateTime? dateFrom, DateTime? dateTo, string propertyName) {
            
            var property = Expression.Property(param, propertyName);
            
            Expression greaterThanOrEqual = null;
            Expression lessThanOrEqual = null;

            dateFrom = dateFrom.HasValue && dateFrom.Value.Date == DateTime.Today ? null : dateFrom;

            // Handle non-nullable DateTime properties
            if (property.Type == typeof(DateTime)) {
                if (dateFrom.HasValue) {
                    greaterThanOrEqual = Expression.GreaterThanOrEqual(property, Expression.Constant(dateFrom.Value));
                }

                if (dateTo.HasValue) {
                    lessThanOrEqual = Expression.LessThanOrEqual(property, Expression.Constant(dateTo.Value));
                }
            }
            // Handle nullable DateTime? properties
            else if (property.Type == typeof(DateTime?)) {
                var hasValue = Expression.Property(property, "HasValue");
                var value = Expression.Property(property, "Value");

                if (dateFrom.HasValue) {
                    var fromCondition = Expression.GreaterThanOrEqual(value, Expression.Constant(dateFrom.Value));
                    greaterThanOrEqual = Expression.AndAlso(hasValue, fromCondition);
                }

                if (dateTo.HasValue) {
                    var toCondition = Expression.LessThanOrEqual(value, Expression.Constant(dateTo.Value));
                    lessThanOrEqual = Expression.AndAlso(hasValue, toCondition);
                }
            }

            // Combine conditions based on provided date range
            if (greaterThanOrEqual != null && lessThanOrEqual != null) {
                return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
            }
            else if (greaterThanOrEqual != null) {
                return greaterThanOrEqual;
            }
            else {
                return lessThanOrEqual;
            }
        }
        #endregion

        #region Search by Text Property
        public static IQueryable<T> SearchTextByProperty<T>(this IQueryable<T> queryable, string searchText, string propertyName) {
            // Get the property from the type or its base classes
            PropertyInfo property = null;
            Type type = typeof(T);

            // Traverse up the inheritance chain to find the property in the class or its base classes
            while (type != null) {
                property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null) {
                    break;
                }
                type = type.BaseType;
            }

            // If the property was not found, return the original collection as there's nothing to search by
            if (property == null) {
                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}' or its base classes.");
            }

            // Check if the found property is of type string
            if (property.PropertyType != typeof(string)) {
                throw new ArgumentException($"Property '{propertyName}' must be of type 'string'.");
            }

            // Create parameter for the entity
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            // Create the property expression
            MemberExpression propertyExpression = Expression.Property(parameter, property);

            // Create the search text comparison
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodCallExpression containsExpression = Expression.Call(propertyExpression, containsMethod, Expression.Constant(searchText));

            // Create the predicate expression
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

            // Apply the predicate to filter the IQueryable collection
            return queryable.Where(predicate);
        }

        #endregion

        #region Search by Text
        public static IQueryable<T> SearchText<T>(this IQueryable<T> queryable, string searchText) {
            if (string.IsNullOrEmpty(searchText)) {
                return queryable; // Return original query if searchText is empty.
            }

            // Get the properties of the type and all its base classes
            var properties = new List<PropertyInfo>();
            Type type = typeof(T);

            // Traverse up the inheritance hierarchy to get all string properties
            while (type != null) {
                properties.AddRange(
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(p => p.PropertyType == typeof(string))
                );
                type = type.BaseType;
            }

            if (!properties.Any()) {
                return queryable; // Return original query if no string properties are found.
            }

            // Create parameter for the entity
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            // Initialize the expression for the OR condition
            Expression combinedExpression = null;

            foreach (var property in properties) {
                // Create the property expression
                MemberExpression propertyExpression = Expression.Property(parameter, property);

                // Check if the property is of type string and build the "Contains" check
                if (property.PropertyType == typeof(string)) {
                    // Create the method call expression for the Contains method
                    MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    MethodCallExpression containsExpression = Expression.Call(propertyExpression, containsMethod, Expression.Constant(searchText));

                    // Combine the expression with OR if it's the first one or use OR for subsequent properties
                    if (combinedExpression == null) {
                        combinedExpression = containsExpression;
                    }
                    else {
                        combinedExpression = Expression.OrElse(combinedExpression, containsExpression);
                    }

                }
            }

            // If no matching properties found, return the original query
            if (combinedExpression == null) {
                return queryable;
            }

            // Create the lambda expression and apply the predicate to filter the IQueryable collection
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return queryable.Where(predicate);
        }

        #endregion

    }
}
