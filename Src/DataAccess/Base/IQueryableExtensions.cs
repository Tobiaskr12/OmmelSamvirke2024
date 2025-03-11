using DomainModules.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess.Base;

public static class IQueryableExtensions
{
    public static IQueryable<T> IncludeNavigationProperties<T>(this IQueryable<T> query, DbContext context, int maxDepth) where T : BaseEntity
    {
        IEntityType? entityType = context.Model.FindEntityType(typeof(T));
        if (entityType == null)
            return query;

        IEnumerable<string> includePaths = GetNavigationPaths(entityType, maxDepth, new HashSet<IEntityType>())
            .Distinct();

        foreach (string path in includePaths)
        {
            query = query.Include(path);
        }
        return query;
    }

    private static IEnumerable<string> GetNavigationPaths(IEntityType entityType, int depth, HashSet<IEntityType> visited)
    {
        if (depth == 0)
            yield break;

        visited.Add(entityType);

        // Process regular navigations.
        foreach (INavigation navigation in entityType.GetNavigations())
        {
            // Always yield the navigation's name.
            yield return navigation.Name;

            // If this is a collection, don't recuse further
            if (!navigation.IsCollection)
            {
                IEntityType targetType = navigation.TargetEntityType;
                var newVisited = new HashSet<IEntityType>(visited);
                if (!newVisited.Contains(targetType))
                {
                    foreach (string subPath in GetNavigationPaths(targetType, depth - 1, newVisited))
                    {
                        yield return $"{navigation.Name}.{subPath}";
                    }
                }
            }
        }

        // Process skip navigations (for many-to-many relationships).
        foreach (ISkipNavigation skipNav in entityType.GetSkipNavigations())
        {
            yield return skipNav.Name;
        }
    }
}
