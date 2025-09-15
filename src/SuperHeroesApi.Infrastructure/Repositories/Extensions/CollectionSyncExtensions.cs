namespace SuperHeroesApi.Infrastructure.Repositories.Extensions
{
  public static class CollectionSyncExtensions
  {
    /// <summary>
    /// Sincroniza uma coleção de relacionamento many-to-many.
    /// Remove itens que não estão mais presentes e adiciona novos.
    /// </summary>
    public static void SyncWith<TEntity, TKey>(
        this ICollection<TEntity> existingCollection,
        IEnumerable<TKey> updatedKeys,
        Func<TEntity, TKey> keySelector,
        Func<TKey, TEntity> entityFactory)
        where TEntity : class
    {
      var currentKeys = existingCollection.Select(keySelector).ToList();

      var toRemove = existingCollection
          .Where(e => !updatedKeys.Contains(keySelector(e)))
          .ToList();

      foreach (var entity in toRemove)
        existingCollection.Remove(entity);

      var toAdd = updatedKeys
          .Where(k => !currentKeys.Contains(k))
          .Select(entityFactory)
          .ToList();

      foreach (var entity in toAdd)
        existingCollection.Add(entity);
    }
  }
}
