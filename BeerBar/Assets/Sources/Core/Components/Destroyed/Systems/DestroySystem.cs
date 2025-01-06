using Unity.Entities;

namespace Core.Components.Destroyed.Systems
{
    public partial class DestroySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Destroyed>().ForEach((Entity entity, in Destroyed _) =>
            {
                EntityManager.DestroyEntity(entity);
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}