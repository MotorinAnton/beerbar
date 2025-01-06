using Core.Authoring.MovementArrows;
using Core.Components;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.TVs.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class TVSpawnSystem : SystemBase
    {
        private EntityQuery _spawnPointTVQuery;

        protected override void OnCreate()
        {
            using var spawnPointTVBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointTVQuery = spawnPointTVBuilder.WithAll<SpawnPointTV>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnTV>().ForEach((Entity entity, in SpawnTV spawnTV) =>
            {
                SpawnTV(entity, spawnTV);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnTV(Entity entity, in SpawnTV spawnTV)
        {
            var tVEntity = EntityManager.CreateEntity();
            EntityManager.SetName(tVEntity, EntityConstants.TVName);
            var spawnPoint =
                _spawnPointTVQuery.ToComponentDataArray<SpawnPointTV>(Allocator.Temp)[0];
            var tVView = Object.Instantiate(spawnTV.TVPrefab , spawnPoint.Position, spawnPoint.Rotation);
            
            var arrowPoint = tVView.transform.position;
            arrowPoint.y += 0.5f;

            var repairArrow = Object.Instantiate(spawnTV.RepairArrow, arrowPoint, tVView.transform.rotation);
            repairArrow.transform.SetParent(tVView.transform);
            repairArrow.gameObject.SetActive(false);
            EntityManager.AddComponentObject(tVEntity, new RepairMovementArrowView { Arrow = repairArrow});
            
            tVView.Initialize(EntityManager, tVEntity);
            EntityManager.AddComponent<TVEntity>(tVEntity);
            EntityManager.AddComponentObject(tVEntity, new TVView { Value = tVView , Chanal = 0});
            EntityManager.AddComponentObject(tVEntity, new AudioSourceView { Value = tVView.AudioSource });
            EntityManager.DestroyEntity(entity);
        }
    }
}