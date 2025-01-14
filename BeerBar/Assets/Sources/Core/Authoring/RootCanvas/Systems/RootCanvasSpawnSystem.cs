using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.RootCanvas.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class RootCanvasSpawnSystem : SystemBase
    {
        private EntityQuery _rootCanvasQuery;
        protected override void OnCreate()
        {
            using var rootCanvasBuilder = new EntityQueryBuilder(Allocator.Temp);
            _rootCanvasQuery = rootCanvasBuilder.WithAll<RootCanvas>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnRootCanvas>().ForEach((Entity entity, in SpawnRootCanvas spawnRootCanvas) =>
            {
                if (!_rootCanvasQuery.IsEmpty)
                {
                    EntityManager.DestroyEntity(entity);
                    return;
                }
                
                SpawnRootCanvas(entity, spawnRootCanvas);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnRootCanvas(Entity entity, in SpawnRootCanvas spawnRootCanvas)
        {
            var rootCanvas = EntityManager.CreateSingleton<RootCanvas>();
            EntityManager.SetName(rootCanvas, EntityConstants.RootCanvasName);
            var rootCanvasView = Object.Instantiate(spawnRootCanvas.RootCanvasPrefab);
            Object.DontDestroyOnLoad(rootCanvasView.gameObject);
            rootCanvasView.Initialize(EntityManager, rootCanvas);
            EntityManager.AddComponentObject(rootCanvas, new RootCanvasView { RootCanvasAuthoring = rootCanvasView });
            EntityManager.DestroyEntity(entity);
        }
    }
}