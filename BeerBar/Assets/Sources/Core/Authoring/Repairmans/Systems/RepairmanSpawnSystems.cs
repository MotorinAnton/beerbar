using System.Collections.Generic;
using Core.Authoring.EventObjects;
using Core.Components;
using Core.Constants;
using Unity.Entities;
using Object = UnityEngine.Object;

namespace Core.Authoring.Repairmans.Systems
{
    public partial class RepairmanSpawnSystems : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnRepairman>().ForEach((Entity entity, in SpawnRepairman spawnRepairman) =>
            {
                SpawnRepairman(entity, spawnRepairman);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnRepairman(Entity entity, in SpawnRepairman spawnRepairman)
        {
            var repairmanEntity = EntityManager.CreateEntity();
            var repairmanView = Object.Instantiate(spawnRepairman.RepairmanData.RepairmanPrefab,
                spawnRepairman.Point.Position,
                spawnRepairman.Point.Rotation);
            repairmanView.PivotHand[0].gameObject.SetActive(false);
            EntityManager.SetName(repairmanEntity, EntityConstants.RepairmanEntityName);
            EntityManager.AddComponentObject(repairmanEntity,
                new NavMeshAgentView { Agent = repairmanView.NavMeshAgent });
            EntityManager.AddComponent<Repairman>(repairmanEntity);
            EntityManager.AddComponent<FreeRepairman>(repairmanEntity);
            EntityManager.AddComponentObject(repairmanEntity, new RepairmanDataComponent { Value = spawnRepairman.RepairmanData });
            EntityManager.AddComponentObject(repairmanEntity, new RepairmanView { Value = repairmanView });
            EntityManager.AddComponentObject(repairmanEntity , new TransformView{ Value = repairmanView.transform });
            EntityManager.AddComponentObject(repairmanEntity, new AnimatorView { Value = repairmanView.Animator });
            EntityManager.AddComponentObject(repairmanEntity, new OrderRepairman{ RepairObjectList = new List<Entity>()});
            

            var randomEventEntity = EntityManager.CreateEntity();
            EntityManager.AddComponent<RandomEventEntity>(randomEventEntity);
            repairmanView.Initialize(EntityManager, repairmanEntity);
            EntityManager.DestroyEntity(entity);
        }
    }
}