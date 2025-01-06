using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ParametersUi.Systems
{
    public partial class ParametersUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnParametersUi>().ForEach(
                (Entity entity, in SpawnParametersUi spawnParametersUi) =>
                {
                    SpawnParametersUi(entity, spawnParametersUi);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnParametersUi(Entity entity, in SpawnParametersUi spawnParametersUi)
        {
            var parametersUi = EntityManager.CreateSingleton<ParametersUi>();
            EntityManager.SetName(parametersUi, EntityConstants.ParametersUiName);
            var parametersUiView = Object.Instantiate(spawnParametersUi.ParametersUiPrefab);
            parametersUiView.Initialize(EntityManager, parametersUi);

            EntityManager.AddComponentObject(parametersUi,
                new SpawnRootCanvasChild
                {
                    Transform = parametersUiView.transform,
                    SortingOrder = parametersUiView.SortingOrder
                });

            EntityManager.AddComponentObject(parametersUi,
                new ParametersUiView { ParametersUiAuthoring = parametersUiView });

            parametersUiView.CloseParametersWindow();

            EntityManager.DestroyEntity(entity);
        }
    }
}