using Core.Authoring.Characters;
using Core.Authoring.MovementArrows;
using Core.Authoring.Repairmans;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.Tables;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Components.Destroyed;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.EventObjects.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class BreakdownEventObjectSystem : SystemBase
    {
        private EntityQuery _electricityQuery;
        private EntityQuery _repairmanQuery;
        private EntityQuery _breakdownPointsQuery;


        protected override void OnCreate()
        {
            using var electricityBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _electricityQuery = electricityBuilder.WithAll<Electricity>().WithNone<ElectricityView>().Build(this);

            using var repairmanBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _repairmanQuery = repairmanBuilder.WithAll<Repairman, RepairmanView>().Build(this);

            using var breakdownPointsBuilder = new EntityQueryBuilder(Allocator.Persistent);
            _breakdownPointsQuery = breakdownPointsBuilder.WithAll<BreakdownPoints>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Tube, Clicked>().WithNone<Breakdown>().ForEach(
                (Entity entity) =>
                {
                    EntityManager.RemoveComponent<Clicked>(entity);

                }).WithoutBurst().WithStructuralChanges().Run();

            
            Entities.WithAll<Electricity, Clicked>().WithNone<Breakdown>().ForEach(
                (Entity entity) =>
                {
                    EntityManager.RemoveComponent<Clicked>(entity);

                }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<Breakdown, Clicked>().WithNone<BreakBottleEntity, LossWalletEntity, Table>()
                .ForEach((Entity entity) =>
                {
                    CreateRepairOrder(entity);
                    
                }).WithoutBurst().WithStructuralChanges()
                .Run();

            Entities.WithAll<Tube, TubeView>().WithAll<StartWaitTime, WaitTime>().ForEach(
                (Entity entity, in TubeView tubeView, in WaitTime waitTime, in StartWaitTime startWaitTime) =>
                {
                    SetProgressPipeLeak(entity, tubeView, waitTime, startWaitTime);
                }).WithoutBurst().WithStructuralChanges().Run();


            Entities.WithAll<Table, UpgradeAndEvenButtonUiView>().WithAll<EventButtonClicked>().ForEach(
                (Entity entity, in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    if (EntityManager.HasComponent<Breakdown>(upgradeAndEvenButtonUiView.ObjectEntity))
                    {
                        CreateRepairOrder(upgradeAndEvenButtonUiView.ObjectEntity);
                    }

                }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<Tube>().WithNone<TubeView>().ForEach((Entity entity) => { SpawnBreakdownTube(entity); })
                .WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<Electricity>().WithNone<ElectricityView>().ForEach((Entity entity) =>
            {
                SpawnBreakdownElectricity(entity);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }


        private void CreateRepairOrder(Entity breakdownEntity)
        {
            if (EntityManager.HasComponent<Destroyed>(breakdownEntity))
            {
                EntityManager.RemoveComponent<Clicked>(breakdownEntity);
                return;
            }

            var repairmanEntity = _repairmanQuery.ToEntityArray(Allocator.Temp)[0];
            var orders = EntityManager.GetComponentObject<OrderRepairman>(repairmanEntity);
            
            var repairArrow = EntityManager.GetComponentObject<RepairMovementArrowView>(breakdownEntity).Arrow;
                        repairArrow.EnableArrow();
            
            EntityManager.RemoveComponent<Clicked>(breakdownEntity);

            if (orders.RepairObjectList.Contains(breakdownEntity))
            {
                return;
            }

            orders.RepairObjectList.Add(breakdownEntity);

            if (!EntityManager.HasComponent<MoveExitRepairman>(repairmanEntity))
            {
                return;
            }

            EntityManager.RemoveComponent<MoveCharacter>(repairmanEntity);
            EntityManager.RemoveComponent<MoveExitRepairman>(repairmanEntity);
            EntityManager.AddComponent<FreeRepairman>(repairmanEntity);

        }

        private void SpawnBreakdownElectricity(Entity entity)
        {
            var config = EntityUtilities.GetGameConfig();
            var breakdownPoints = _breakdownPointsQuery.ToComponentDataArray<BreakdownPoints>(Allocator.Temp)[0];
            var eventObject = config.EventObjectConfig.ElectricityPrefab;
            var electricityView = Object.Instantiate(eventObject, breakdownPoints.Electricity.Position,
                breakdownPoints.Electricity.Rotation);

            EntityManager.AddComponentObject(entity, new ElectricityView { Value = electricityView });
            
            foreach (var particle in electricityView.Particles)
            {
                particle.Stop();
            }

            AddRepairMovementArrow(electricityView.transform, entity);
            
            electricityView.Initialize(EntityManager, entity);
        }

        private void SpawnBreakdownTube(Entity entity)
        {
            var config = EntityUtilities.GetGameConfig();
            var breakdownPoints = _breakdownPointsQuery.ToComponentDataArray<BreakdownPoints>(Allocator.Temp)[0];
            var eventObject = config.EventObjectConfig.TubePrefab;
            var tubeView =
                Object.Instantiate(eventObject, breakdownPoints.Tube.Position, breakdownPoints.Tube.Rotation);
            
            EntityManager.AddComponentObject(entity, new TubeView { Value = tubeView });

            tubeView.ProgressMeshRenderers[0].gameObject.SetActive(true);
            tubeView.ProgressMeshRenderers[0].material.SetFloat(BreakdownObjectConstants.PipeLeak, 0f);
            tubeView.ProgressMeshRenderers[1].gameObject.SetActive(false);

            foreach (var particle in tubeView.Particles)
            {
                particle.Stop();
            }

            AddRepairMovementArrow(tubeView.transform, entity);

            tubeView.Initialize(EntityManager, entity);
        }
        
        private void SetProgressPipeLeak(Entity entity ,TubeView tubeView, in WaitTime waitTime, in StartWaitTime startWaitTime)
        {
            float progress;
            
            if (EntityManager.HasComponent<Breakdown>(entity))
            { 
                var step = waitTime.Current / startWaitTime.Start;
                progress = 1f - step;
                tubeView.Value.ProgressMeshRenderers[0].material.SetFloat(BreakdownObjectConstants.PipeLeak, progress);
                return;
            }
            
            progress = waitTime.Current / startWaitTime.Start;
            tubeView.Value.ProgressMeshRenderers[0].material.SetFloat(BreakdownObjectConstants.PipeLeak, progress);
        }
        
        private void AddRepairMovementArrow(Transform parentTransform, Entity entity)
        {
            var config = EntityUtilities.GetGameConfig();
            var arrowPoint = parentTransform.position;
      
            arrowPoint.y += BreakdownObjectConstants.MovementArrowTubeOffset;

            if (EntityManager.HasComponent<Tube>(entity))
            {
                arrowPoint.x -= BreakdownObjectConstants.MovementArrowTubeOffset;
            }
            
            if (EntityManager.HasComponent<Electricity>(entity))
            {
                
                arrowPoint.y += BreakdownObjectConstants.MovementArrowElectricityOffset;
                arrowPoint.z += BreakdownObjectConstants.MovementArrowElectricityOffset;
            }

            var repairArrow = Object.Instantiate(config.RepairArrow, arrowPoint, parentTransform.rotation, parentTransform);
            //repairArrow.transform.SetParent(parentTransform);
            repairArrow.gameObject.SetActive(false);
            EntityManager.AddComponentObject(entity, new RepairMovementArrowView { Arrow = repairArrow});
        }
    }
}