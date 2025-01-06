using Unity.Entities;

namespace Core.Extensions
{
    public static class EntityManagerExtensions
    { 
        public static bool HasSingleton<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
        {
            return entityManager.CreateEntityQuery(typeof(T)).TryGetSingleton<T>(out _);
        }
    }
}