using Unity.Collections;
using Unity.Entities;

namespace Core.Components.Wait.Systems
{
    public partial class WaitSystem : SystemBase
    {
        private EntityQuery _waitQuery;
        private EntityQuery _waitTimerQuery;
        
        protected override void OnCreate()
        {
            using var waitQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitQuery = waitQueryBuilder.WithAll<StartWaitTime>().WithAllRW<WaitTime>().Build(this);
            
            using var waitTimerQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitTimerQuery = waitTimerQueryBuilder.WithAll<WaitTimer>().WithAllRW<WaitTime>().Build(this);
        }
        
        protected override void OnUpdate()
        {
            Entities.WithAll<WaitTime>().WithNone<StartWaitTime>().ForEach((Entity entity, in WaitTime time) =>
            {
                EntityManager.AddComponentData(entity, new StartWaitTime { Start = time.Current });

            }).WithoutBurst().WithStructuralChanges().Run();
            
            /*var waitTimerArray = _waitTimerQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var entity in waitTimerArray)
            {
                var wait = EntityManager.GetComponentData<WaitTime>(entity);
                
                wait.Current += World.Time.DeltaTime;
                
                // if (wait.Current <= 0)
                // {
                //     EntityManager.RemoveComponent<WaitTime>(entity);
                //     EntityManager.RemoveComponent<StartWaitTime>(entity);
                //     return;
                // }
                
                EntityManager.SetComponentData(entity, wait);
            }*/
            
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