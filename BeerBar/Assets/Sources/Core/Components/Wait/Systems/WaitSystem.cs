using Unity.Collections;
using Unity.Entities;

namespace Core.Components.Wait.Systems
{
    public partial class WaitSystem : SystemBase
    {
        private EntityQuery _waitQuery;

        protected override void OnCreate()
        {
            using var waitQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitQuery = waitQueryBuilder.WithAll<StartWaitTime>().WithAllRW<WaitTime>().Build(this);
        }
        
        protected override void OnUpdate()
        {
            Entities.WithAll<WaitTime>().WithNone<StartWaitTime>().ForEach((Entity entity, in WaitTime time) =>
            {
                EntityManager.AddComponentData(entity, new StartWaitTime { Start = time.Current });

            }).WithoutBurst().WithStructuralChanges().Run();
            
            var waitArray = _waitQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var entity in waitArray)
            {
                var wait = EntityManager.GetComponentData<WaitTime>(entity);
                
                wait.Current -= World.Time.DeltaTime;
                
                if (wait.Current <= 0)
                {
                    EntityManager.RemoveComponent<WaitTime>(entity);
                    EntityManager.RemoveComponent<StartWaitTime>(entity);
                    return;
                }
                
                EntityManager.SetComponentData(entity, wait);
            }
        }
    }
}