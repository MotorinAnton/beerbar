using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Customers;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Authoring.TVs;
using Core.Components.Wait;
using Core.Configs;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;
using RandomEvent = Core.Configs.RandomEvent;

namespace Core.Authoring.EventObjects.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class BreakdownRandomEventSystem : SystemBase
    {
        private EntityQuery _electricityQuery;
        private EntityQuery _tubeQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _breakdownTVQuery;
        private EntityQuery _tableQuery;
        private EntityQuery _tvQuery;

        protected override void OnCreate()
        {
            using var electricityBuilder = new EntityQueryBuilder(Allocator.Temp);
            _electricityQuery = electricityBuilder.WithAll<Electricity, ElectricityView>().Build(this);
            
            using var tubeBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tubeQuery = tubeBuilder.WithAll<Tube, TubeView>().Build(this);

            using var tvBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tvQuery = tvBuilder.WithAll<TVEntity, TVView>().Build(this);
            
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);
            
            using var tableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tableQuery = tableBuilder.WithAll<Table, TableView>().WithNone<Breakdown>()
                .Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<RandomEventEntity>().WithNone< WaitTime, RandomEventStart>().ForEach((Entity entity) =>
            {
                if (_tubeQuery.IsEmpty)
                {
                    var breakdownEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponent<Tube>(breakdownEntity);
                    return;
                }
                
                if (_electricityQuery.IsEmpty)
                {
                    var breakdownEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponent<Electricity>(breakdownEntity);
                    return;
                }
                
                var config = EntityUtilities.GetGameConfig();
                var randomEventConfig = config.RandomEventConfig;
                
                var afterTimeStartRandomEvent = Random.Range(randomEventConfig.MinTime, randomEventConfig.MaxTime);
                
                EntityManager.AddComponentData(entity, new WaitTime { Current = afterTimeStartRandomEvent });
                EntityManager.AddComponent<RandomEventStart>(entity);

            }).WithoutBurst().WithStructuralChanges().Run();
            
            Entities.WithAll<RandomEventEntity, RandomEventStart>().WithNone<WaitTime>().ForEach((Entity entity) =>
            {
                CreateRandomBreakdownEvent();
                
                EntityManager.RemoveComponent<RandomEventStart>(entity);

            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CreateRandomBreakdownEvent()
        {
            var config = EntityUtilities.GetGameConfig();
            var randomEventConfig = config.RandomEventConfig;
            var randomEventArray = randomEventConfig.Events;
            var storeRatingScore = _storeRatingQuery.GetSingleton<StoreRating>().CurrentValue;
            
            var availableRandomEvents = new List<RandomEvent>();

            foreach (var randomEvent in randomEventArray)
            {
                if (randomEvent.Rating <= storeRatingScore)
                {
                    availableRandomEvents.Add(randomEvent);
                }
            }

            if (RollRandomEvent(availableRandomEvents, out var selectedRandomEventObject))
            {
                CreateBreakdown(selectedRandomEventObject);
            }
        }

        private bool RollRandomEvent(List<RandomEvent> randomEvents, out BreakdownObject selectedObject)
        {
            var sumChance = randomEvents.Sum(randomEvent => randomEvent.Chance);

            var eventRoll = sumChance switch
            {
                <= 100 => Random.Range(0, 100),
                > 100 => Random.Range(0, sumChance)
            };

            var stepChance = 0;

            foreach (var randomEvent in randomEvents)
            {
                stepChance += randomEvent.Chance;

                if (eventRoll > stepChance)
                {
                    continue;
                }

                selectedObject = randomEvent.Object;
                return true;
            }

            selectedObject = default;
            return false;
        }
        private void CreateBreakdown(BreakdownObject breakdownObject)
        {
            switch (breakdownObject)
            {
                case BreakdownObject.Tube:
                {
                    var tubeEntity = _tubeQuery.ToEntityArray(Allocator.Temp)[0];
                    AddBreakdown(tubeEntity);
                    break;
                }

                case BreakdownObject.TV:
                    
                    if (_tvQuery.IsEmpty)
                    {
                        return;
                    }

                    var tvEntity = _tvQuery.ToEntityArray(Allocator.Temp)[0];
                    AddBreakdown(tvEntity);
                    break;
                
                case BreakdownObject.Electricity:
                    
                    var breakdownTubeEntity = _electricityQuery.ToEntityArray(Allocator.Temp)[0];
                    AddBreakdown(breakdownTubeEntity);
                    break;
                
                case BreakdownObject.Table:

                    if (_tableQuery.IsEmpty)
                    {
                        return;
                    }

                    var tableArray = _tableQuery.ToEntityArray(Allocator.Temp);
                    var randomTable = tableArray[Random.Range(0, tableArray.Length)];
                    AddBreakdown(randomTable);
                    break;
            }
        }

        private void AddBreakdown(Entity entity)
        {
            if (EntityManager.HasComponent<Breakdown>(entity))
            {
                return;
            }

            EntityManager.AddComponent<Breakdown>(entity);
            
            if (EntityManager.HasComponent<ElectricityView>(entity))
            {
                var electricityView = EntityManager.GetComponentObject<ElectricityView>(entity).Value;

                foreach (var particle in electricityView.Particles)
                {
                    particle.Play();
                }

                var openDoorAnimation =
                    electricityView.Animation.GetClip(BreakdownObjectConstants.ElectricalDoorOpen.Value);
                electricityView.Animation.clip = openDoorAnimation;
                electricityView.Animation.Play();
                RenderSettings.sun.DOIntensity(0, BreakdownObjectConstants.ElectricityFlashDuration)
                    .SetEase(Ease.InOutElastic).SetLoops(BreakdownObjectConstants.ElectricityFlashLoop);
                return;
            }

            if (EntityManager.HasComponent<TubeView>(entity))
            {
                var tubeView = EntityManager.GetComponentObject<TubeView>(entity).Value;

                foreach (var particle in tubeView.Particles)
                {
                    particle.Play();
                }
                
                if (EntityManager.HasComponent<WaitTime>(entity))
                {
                    var waitTime = EntityManager.GetComponentData<WaitTime>(entity);
                    var startWaitTime = EntityManager.GetComponentData<StartWaitTime>(entity).Start;
                    waitTime.Current = startWaitTime - waitTime.Current;
                    EntityManager.SetComponentData(entity, waitTime);
                }
                else
                {
                     EntityManager.AddComponentData(entity, new WaitTime { Current = BreakdownObjectConstants.FlowTime });
                }
                return;
            }

            if (EntityManager.HasComponent<TVView>(entity))
            {
                var tvView = EntityManager.GetComponentObject<TVView>(entity).Value;
                tvView.OnRenderer.gameObject.SetActive(false);
                return;
            }

            if (!EntityManager.HasComponent<TableView>(entity))
            {
                return;
            }

            var tableView = EntityManager.GetComponentObject<TableView>(entity);

            foreach (var pointEntity in tableView.AtTablePointsEntity)
            {
                EntityManager.AddComponent<PointDirtTable>(pointEntity);
            }
        }
    }
}