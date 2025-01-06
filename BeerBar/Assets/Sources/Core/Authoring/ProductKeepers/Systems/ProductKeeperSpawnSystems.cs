using Core.Components;
using Core.Constants;
using Unity.Entities;
using Object = UnityEngine.Object;

namespace Core.Authoring.ProductKeepers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class ProductKeeperSpawnSystems : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnProductKeeper>().ForEach((Entity entity, in SpawnProductKeeper spawnProductKeeper) =>
            {
                SpawnProductKeeper(entity, spawnProductKeeper);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnProductKeeper(Entity entity, in SpawnProductKeeper spawnProductKeeper)
        {
            var productKeeper = EntityManager.CreateEntity();
            var productKeeperView = Object.Instantiate(spawnProductKeeper.ProductKeeper.ProductKeeperPrefab,
                spawnProductKeeper.Point.Position,
                spawnProductKeeper.Point.Rotation);
            productKeeperView.PivotHand[0].gameObject.SetActive(false);
            EntityManager.SetName(productKeeper, EntityConstants.ProductKeeperEntityName);
            EntityManager.AddComponentObject(productKeeper,
                new NavMeshAgentView { Agent = productKeeperView.NavMeshAgent });
            EntityManager.AddComponent<ProductKeeper>(productKeeper);
            EntityManager.AddComponent<FreeProductKeeper>(productKeeper);
            EntityManager.AddComponentObject(productKeeper, new ProductKeeperDataComponent { Value = spawnProductKeeper.ProductKeeper });
            EntityManager.AddComponentObject(productKeeper, new ProductKeeperView { Value = productKeeperView });
            EntityManager.AddComponentObject(productKeeper , new TransformView{ Value = productKeeperView.transform });
            EntityManager.AddComponentObject(productKeeper, new AnimatorView { Value = productKeeperView.Animator });
            productKeeperView.Initialize(EntityManager, productKeeper);
            EntityManager.DestroyEntity(entity);
        }
    }
}