using Unity.Entities;

namespace Core.Components.Destroyed.Systems
{
    [UpdateBefore(typeof(DestroySystem))]
    public partial class DestroyEntityBehaviourSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Destroyed>().WithAll<EntityBehaviourView>().ForEach((Entity entity, in EntityBehaviourView view) =>
            {
                view.Value.EntityDestroyed();
                EntityManager.RemoveComponent<EntityBehaviourView>(entity);
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}