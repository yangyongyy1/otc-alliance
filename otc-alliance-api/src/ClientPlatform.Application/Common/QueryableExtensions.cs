using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ClientPlatform.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyCustomFilters<T>(this IQueryable<T> query, List<CustomSearchFilter> filters)
        {
            if (filters == null || !filters.Any())
            {
                return query;
            }

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.FieldName) || string.IsNullOrWhiteSpace(filter.Value))
                {
                    continue;
                }

                try 
                {
                    query = query.ApplyFilter(filter);
                }
                catch (Exception)
                {
                    // Ignore invalid filters for now
                }
            }

            return query;
        }

        private static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, CustomSearchFilter filter)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression propertyAccess = parameter;
            
            // Handle nested properties
            foreach (var propertyName in filter.FieldName.Split('.'))
            {
                // Find property case-insensitively
                var property = propertyAccess.Type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    // Property not found, ignore this filter
                    return query;
                }
                propertyAccess = Expression.Property(propertyAccess, property);
            }

            var targetType = propertyAccess.Type;
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            object safeValue = null;
            try
            {
                if (underlyingType.IsEnum)
                {
                    safeValue = Enum.Parse(underlyingType, filter.Value);
                }
                else if (underlyingType == typeof(Guid))
                {
                    safeValue = Guid.Parse(filter.Value);
                }
                else
                {
                    safeValue = Convert.ChangeType(filter.Value, underlyingType);
                }
            }
            catch
            {
                // If conversion fails, return original query
                return query;
            }

            // Create constant expression with the correct type (including nullable)
            var constant = Expression.Constant(safeValue, targetType);
            
            Expression expression = null;
            var op = filter.Operator?.ToLower() ?? "eq";

            // For string operations, we need to handle nulls if we want to be safe, but EF Core usually handles it.
            // However, for Contains/StartsWith, we should ensure the property is string.

            switch (op)
            {
                case "eq":
                    expression = Expression.Equal(propertyAccess, constant);
                    break;
                case "neq":
                    expression = Expression.NotEqual(propertyAccess, constant);
                    break;
                case "contains":
                    if (underlyingType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        expression = Expression.Call(propertyAccess, method, constant);
                    }
                    break;
                case "startswith":
                    if (underlyingType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                        expression = Expression.Call(propertyAccess, method, constant);
                    }
                    break;
                case "endswith":
                    if (underlyingType == typeof(string))
                    {
                        var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                        expression = Expression.Call(propertyAccess, method, constant);
                    }
                    break;
                case "gt":
                    expression = Expression.GreaterThan(propertyAccess, constant);
                    break;
                case "lt":
                    expression = Expression.LessThan(propertyAccess, constant);
                    break;
                case "gte":
                    expression = Expression.GreaterThanOrEqual(propertyAccess, constant);
                    break;
                case "lte":
                    expression = Expression.LessThanOrEqual(propertyAccess, constant);
                    break;
            }

            if (expression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
                return query.Where(lambda);
            }

            return query;
        }
    }
}
