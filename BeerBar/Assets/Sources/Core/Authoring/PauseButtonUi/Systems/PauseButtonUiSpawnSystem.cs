using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.PauseButtonUi.Systems
{
    public partial class PauseButtonUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnPauseButtonUi>().ForEach(
                (Entity entity, in SpawnPauseButtonUi spawnPauseButtonUi) =>
                {
                    SpawnPauseButtonUi(entity, spawnPauseButtonUi);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnPauseButtonUi(Entity entity, in SpawnPauseButtonUi spawnPauseButtonUi)
        {
            var pauseButtonUi = EntityManager.CreateSingleton<PauseButtonUi>();
            var pauseButtonUiView = Object.Instantiate(spawnPauseButtonUi.PauseButtonUiPrefab);
            
            EntityManager.AddComponentObject(pauseButtonUi,
                new SpawnRootCanvasChild
                {
                    Transform = pauseButtonUiView.transform,
                    SortingOrder = pauseButtonUiView.SortingOrder
                });
            
            EntityManager.SetName(pauseButtonUi, EntityConstants.ParametersButtonUiName);
            pauseButtonUiView.Initialize(EntityManager, pauseButtonUi);
            EntityManager.DestroyEntity(entity);
        }
    }
}