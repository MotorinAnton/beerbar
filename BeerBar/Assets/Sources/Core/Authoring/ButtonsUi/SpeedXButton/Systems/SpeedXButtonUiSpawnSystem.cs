using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ButtonsUi.SpeedXButton.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class AddButtonUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnSpeedXButtonUi>().ForEach((Entity entity, in SpawnSpeedXButtonUi spawnSpeedXButtonUi) =>
            {
                SpawnAddContainerButtonUi(entity, spawnSpeedXButtonUi);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnAddContainerButtonUi(Entity entity, in SpawnSpeedXButtonUi spawnSpeedXButtonUi)
        {
            var speedXButtonUIEntity = EntityManager.CreateSingleton<SpeedXButtonUi>();
            var speedXButtonUiView = Object.Instantiate(spawnSpeedXButtonUi.SpeedXButtonUiPrefab);
          
            EntityManager.SetName(speedXButtonUIEntity, EntityConstants.SpeedXButtonUiName);
            EntityManager.AddComponentObject(speedXButtonUIEntity, new SpawnRootCanvasChild { Transform = speedXButtonUiView.transform });
            EntityManager.AddComponentObject(speedXButtonUIEntity,
                new SpeedXButtonUiView
                {
                    SpeedXButtonUiAuthoring = speedXButtonUiView ,
                    SpeedX = 1
                });
            speedXButtonUiView.Initialize(EntityManager, speedXButtonUIEntity);
            EntityManager.DestroyEntity(entity);
        }
    }
}