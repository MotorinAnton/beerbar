using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ParametersButtonUi.Systems
{
    public partial class MainMenuButtonUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnParametersButtonUi>().ForEach(
                (Entity entity, in SpawnParametersButtonUi spawnParametersButtonUi) =>
                {
                    SpawnParametersButtonUi(entity, spawnParametersButtonUi);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnParametersButtonUi(Entity entity, in SpawnParametersButtonUi spawnParametersButtonUi)
        {
            var parametersButtonUi = EntityManager.CreateSingleton<ParametersButtonUi>();
            var parametersButtonUiView = Object.Instantiate(spawnParametersButtonUi.ParametersButtonUiPrefab);
            
            EntityManager.AddComponentObject(parametersButtonUi,
                new SpawnRootCanvasChild
                {
                    Transform = parametersButtonUiView.transform,
                    SortingOrder = parametersButtonUiView.SortingOrder
                });
            
            EntityManager.SetName(parametersButtonUi, EntityConstants.ParametersButtonUiName);
            EntityManager.SetName(parametersButtonUi, EntityConstants.ParametersButtonUiName);
            parametersButtonUiView.Initialize(EntityManager, parametersButtonUi);
            EntityManager.DestroyEntity(entity);
        }
    }
}