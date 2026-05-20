using System.Linq.Expressions;

namespace AudioBookStudio.Common.Shared;
public static class QuerableExtensions
{
    public static IQueryable<T> OrderByPropertyOrField<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return source;
        }

        var parameter = Expression.Parameter(typeof(T), "p");
        var property = Expression.Property(parameter, propertyName);
        var orderByExpression = Expression.Lambda(property, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.Type },
                                                source.Expression, Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<T>(resultExpression);
    }

    public static IQueryable<T> ThenByPropertyOrField<T>(this IQueryable<T> source, string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return source;
        }

        var parameter = Expression.Parameter(typeof(T), "p");
        var property = Expression.Property(parameter, propertyName);
        var orderByExpression = Expression.Lambda(property, parameter);

        var resultExpression = Expression.Call(typeof(Queryable), "ThenBy", new Type[] { typeof(T), property.Type },
                                                source.Expression, Expression.Quote(orderByExpression));

        return source.Provider.CreateQuery<T>(resultExpression);
    }
}
