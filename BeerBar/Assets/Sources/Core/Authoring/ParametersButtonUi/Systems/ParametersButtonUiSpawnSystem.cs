using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ParametersButtonUi.Systems
{
    public partial class ParametersButtonUiSpawnSystem : SystemBase
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
            EntityManager.SetName(parametersButtonUi, EntityConstants.ParametersButtonUiName);

            var parametersButtonUiView = Object.Instantiate(spawnParametersButtonUi.ParametersButtonUiPrefab);
            parametersButtonUiView.Initialize(EntityManager, parametersButtonUi);

            EntityManager.AddComponentObject(parametersButtonUi,
                new SpawnRootCanvasChild
                {
                    Transform = parametersButtonUiView.transform,
                    SortingOrder = parametersButtonUiView.SortingOrder
                });

            EntityManager.DestroyEntity(entity);
        }
    }
}