using Core.Components;
using Core.Constants;
using Unity.Entities;
using Object = UnityEngine.Object;

namespace Core.Authoring.Cleaners.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CleanerSpawnSystems : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCleaner>().ForEach((Entity entity, in SpawnCleaner spawnCleaner) =>
            {
                SpawnCleaner(entity, spawnCleaner);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnCleaner(Entity entity, in SpawnCleaner spawnCleaner)
        {
            var cleanerEntity = EntityManager.CreateEntity();
            var cleanerView = Object.Instantiate(spawnCleaner.CleanerData.CleanerPrefab,
                spawnCleaner.Point.Position,
                spawnCleaner.Point.Rotation);
            cleanerView.PivotHand[0].gameObject.SetActive(false);
            EntityManager.SetName(cleanerEntity, EntityConstants.CleanerEntityName);
            EntityManager.AddComponentObject(cleanerEntity,
                new NavMeshAgentView { Agent = cleanerView.NavMeshAgent });
            EntityManager.AddComponent<Cleaner>(cleanerEntity);
            EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
            EntityManager.AddComponentObject(cleanerEntity, new CleanerDataComponent { Value = spawnCleaner.CleanerData });
            EntityManager.AddComponentObject(cleanerEntity, new CleanerView { Value = cleanerView });
            EntityManager.AddComponentObject(cleanerEntity , new TransformView{ Value = cleanerView.transform });
            EntityManager.AddComponentObject(cleanerEntity, new AnimatorView { Value = cleanerView.Animator });
            cleanerView.Initialize(EntityManager, cleanerEntity);
            
            EntityManager.DestroyEntity(entity);
        }
    }
}