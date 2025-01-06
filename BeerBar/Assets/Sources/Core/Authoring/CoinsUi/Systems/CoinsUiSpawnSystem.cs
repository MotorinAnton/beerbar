using Core.Authoring.RootCanvas;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.CoinsUi.Systems
{
    public partial class CoinsUiSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCoinsUi>().ForEach((Entity entity, in SpawnCoinsUi spawnCoinsUI) =>
            {
                SpawnCoinsUi(entity, spawnCoinsUI);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnCoinsUi(Entity entity, in SpawnCoinsUi spawnCoinsUI)
        {
            var coinsUI = EntityManager.CreateSingleton<CoinsUi>();
            var coinsUIView = Object.Instantiate(spawnCoinsUI.CoinsUiPrefab);
            
            EntityManager.AddComponentObject(coinsUI, new SpawnRootCanvasChild
            {
                Transform = coinsUIView.transform,
                SortingOrder = coinsUIView.SortingOrder
            });
            
            EntityManager.AddComponentObject(coinsUI, new CoinsUiView { Text = coinsUIView.Text });
            EntityManager.SetName(coinsUI, EntityConstants.CoinsUiName);
            coinsUIView.Initialize(EntityManager, coinsUI);
            EntityManager.DestroyEntity(entity);
        }
    }
}