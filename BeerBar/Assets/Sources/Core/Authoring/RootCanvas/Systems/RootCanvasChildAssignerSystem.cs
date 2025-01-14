using Core.Utilities;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.RootCanvas.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class RootCanvasChildAssignerSystem : SystemBase
    {
        private EntityQuery _rootCanvasQuery;

        protected override void OnCreate()
        {
            using var rootCanvasBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _rootCanvasQuery = rootCanvasBuilder.WithAll<RootCanvas, RootCanvasView>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (_rootCanvasQuery.IsEmpty)
            {
                return;
            }

            Entities.WithAll<SpawnRootCanvasChild>().ForEach((Entity entity, in SpawnRootCanvasChild rootCanvasChild) =>
            {
                AssignChildToRootCanvas(entity, rootCanvasChild);

                var rootCanvasEntity = _rootCanvasQuery.GetSingletonEntity();

                if (!EntityManager.HasComponent<ReorderRootCanvas>(rootCanvasEntity))
                {
                    EntityUtilities.AddOneFrameComponent<ReorderRootCanvas>(rootCanvasEntity);
                }
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void AssignChildToRootCanvas(Entity entity, in SpawnRootCanvasChild spawnRootCanvasChild)
        {
            var rootCanvasView = _rootCanvasQuery.GetSingleton<RootCanvasView>();

            spawnRootCanvasChild.Transform.SetParent(rootCanvasView.RootCanvasAuthoring.Transform, false);

            EntityManager.AddComponentObject(entity, new RootCanvasChild
            {
                Transform = spawnRootCanvasChild.Transform,
                SortingOrder = spawnRootCanvasChild.SortingOrder
            });

            EntityManager.RemoveComponent<SpawnRootCanvasChild>(entity);
        }
    }
}