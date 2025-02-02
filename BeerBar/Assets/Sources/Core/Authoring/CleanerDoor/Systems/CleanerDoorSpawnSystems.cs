using Unity.Entities;
using Object = UnityEngine.Object;

namespace Core.Authoring.CleanerDoor.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CleanerDoorSpawnSystems : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCleanerDoor>().ForEach((Entity entity, in SpawnCleanerDoor spawnCleanerDoor) =>
            {
                SpawnCleanerDoor(entity, spawnCleanerDoor);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnCleanerDoor(Entity entity, in SpawnCleanerDoor spawnCleanerDoor)
        {
            var cleanerDoorEntity = EntityManager.CreateSingleton<DoorCleaner>();
            var cleanerDoorView = Object.Instantiate(spawnCleanerDoor.DoorPrefab);
            
            EntityManager.AddComponentObject(cleanerDoorEntity, new CleanerDoorView { Value = cleanerDoorView });
            cleanerDoorView.Initialize(EntityManager, cleanerDoorEntity);
            EntityManager.DestroyEntity(entity);
        }
    }
}