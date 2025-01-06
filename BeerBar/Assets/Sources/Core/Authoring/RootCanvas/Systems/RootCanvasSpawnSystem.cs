using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.RootCanvas.Systems
{
    public partial class RootCanvasSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnRootCanvas>().ForEach((Entity entity, in SpawnRootCanvas spawnRootCanvas) =>
            {
                SpawnRootCanvas(entity, spawnRootCanvas);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnRootCanvas(Entity entity, in SpawnRootCanvas spawnRootCanvas)
        {
            var rootCanvas = EntityManager.CreateSingleton<RootCanvas>();
            EntityManager.SetName(rootCanvas, EntityConstants.RootCanvasName);
            var rootCanvasView = Object.Instantiate(spawnRootCanvas.RootCanvasPrefab);
            rootCanvasView.Initialize(EntityManager, rootCanvas);
            EntityManager.AddComponentObject(rootCanvas, new RootCanvasView { RootCanvasAuthoring = rootCanvasView });
            EntityManager.DestroyEntity(entity);
        }
    }
}