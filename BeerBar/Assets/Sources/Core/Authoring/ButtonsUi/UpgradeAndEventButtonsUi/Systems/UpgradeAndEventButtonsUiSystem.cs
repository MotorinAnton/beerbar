using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Bartenders;
using Core.Authoring.Cameras;
using Core.Authoring.Cleaners;
using Core.Authoring.Containers;
using Core.Authoring.EventObjects;
using Core.Authoring.MovementArrows;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Authoring.Warehouses;
using Core.Components;
using Core.Components.Destroyed;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Container = Core.Authoring.Containers.Container;

namespace Core.Authoring.ButtonsUi.UpgradeAndEventButtonsUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpgradeAndEventButtonsUiSystem : SystemBase
    {
        private EntityQuery _cleanerQuery;
        private EntityQuery _customerContainerPointsQuery;
        private EntityQuery _barmanContainerPointsQuery;
        private EntityQuery _atTablePointQuery;
        private EntityQuery _onTablePointQuery;
        private EntityQuery _bankQuery;
        private EntityQuery _upCompletedQuery;
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var cleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanerQuery = cleanerBuilder.WithAll<Cleaner, NavMeshAgentView>().Build(this);

            using var customerContainerPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _customerContainerPointsQuery =
                customerContainerPointsBuilder.WithAll<CustomerPointContainer>().Build(this);

            using var barmanContainerPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanContainerPointsQuery = barmanContainerPointsBuilder.WithAll<BarmanPointContainer>().Build(this);

            using var drinkAtTheTablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _atTablePointQuery = drinkAtTheTablePointBuilder.WithAll<AtTablePoint>().Build(this);

            using var onTablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _onTablePointQuery = onTablePointBuilder.WithAll<OnTablePoint>().Build(this);

            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);

            using var upCompletedBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upCompletedQuery = upCompletedBuilder.WithAll<AvailableUp>().Build(this);

            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<UpgradeAndEvenButtonUiView>()
                .ForEach((in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    TornButtonToCamera(upgradeAndEvenButtonUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<UpgradeAndEvenButtonUiView>()
                .ForEach((Entity entity, in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    CheckAvailableUpgradeButton(entity, upgradeAndEvenButtonUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Container, UpgradeAndEvenButtonUiView>().WithAll<UpgradeButtonClicked>()
                .ForEach((Entity entity, in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    UpgradeContainer(entity, upgradeAndEvenButtonUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Container, UpgradeAndEvenButtonUiView>().WithAll<EventButtonClicked>().ForEach(
                (in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    EntityManager.AddComponent<AddNewProducts>(upgradeAndEvenButtonUiView
                        .ObjectEntity);
                    upgradeAndEvenButtonUiView.DisableUpgradeAndEvenButtons();

                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Table, UpgradeAndEvenButtonUiView>().WithAll<UpgradeButtonClicked>()
                .ForEach((Entity entity, in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    UpgradeTable(entity, upgradeAndEvenButtonUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Table, UpgradeAndEvenButtonUiView>().WithAll<EventButtonClicked>().WithNone<Breakdown>()
                .ForEach(
                    (in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                    {
                        if (EntityManager.HasComponent<Breakdown>(upgradeAndEvenButtonUiView.ObjectEntity))
                        {
                            var repairArrow =
                                EntityManager
                                    .GetComponentObject<RepairMovementArrowView>(
                                        upgradeAndEvenButtonUiView.ObjectEntity).Arrow;

                            repairArrow.EnableArrow();
                        }
                        else
                        {
                            EntityManager.AddComponent<CleanTable>(upgradeAndEvenButtonUiView.ObjectEntity);
                            var movementArrows =
                                EntityManager
                                    .GetComponentObject<ClearMovementArrowView>(upgradeAndEvenButtonUiView.ObjectEntity)
                                    .Arrow;

                            movementArrows.EnableArrow();
                        }

                    }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Table, UpgradeAndEvenButtonUiView>().ForEach(
                (in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView) =>
                {
                    if (EntityManager.HasComponent<Breakdown>(upgradeAndEvenButtonUiView.ObjectEntity))
                    {
                        upgradeAndEvenButtonUiView.SetTextRepairsEventButton();
                    }
                    else
                    {
                        upgradeAndEvenButtonUiView.SetTextClearEventButton();
                    }
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CheckAvailableUpgradeButton(Entity entity, UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView)
        {
            if (!upgradeAndEvenButtonUiView.UpgradeAndEventButton.isActiveAndEnabled)
            {
                return;
            }

            if (!EntityManager.HasComponent<UpButtonAvailable>(entity))
            {
                upgradeAndEvenButtonUiView.UpgradeAndEventButton.UpgradeButton.enabled = false;
                return;
            }

            var bank = _bankQuery.GetSingleton<Bank>();

            var price = 0;

            if (EntityManager.HasComponent<Container>(entity))
            {
                var config = EntityUtilities.GetContainerConfig();
                var containerDescription =
                    EntityManager.GetComponentData<ContainerDescription>(
                        upgradeAndEvenButtonUiView.ObjectEntity);
                var nextContainer =
                    config.ContainersData.FirstOrDefault(containerNext =>
                        containerNext.Level == containerDescription.Level + 1 &&
                        containerNext.Type == containerDescription.Type);


                price = nextContainer.Price;
            }

            if (EntityManager.HasComponent<Table>(entity))
            {
                var tableConfig = EntityUtilities.GetTableConfig();
                var tableDescription =
                    EntityManager.GetComponentData<Table>(upgradeAndEvenButtonUiView.ObjectEntity);
                var nextTable =
                    tableConfig.TablesData.FirstOrDefault(containerNext =>
                        containerNext.Level == tableDescription.Level + 1);
                price = nextTable.Price;
            }

            if (bank.Coins >= price)
            {
                upgradeAndEvenButtonUiView.UpgradeAndEventButton.UpgradeButton.enabled = true;
                return;
            }

            upgradeAndEvenButtonUiView.UpgradeAndEventButton.UpgradeButton.enabled = false;
        }


        private void UpgradeContainer(Entity entity, UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView)
        {
            var config = EntityUtilities.GetContainerConfig();
            var bank = _bankQuery.GetSingleton<Bank>();

            var containerDescription =
                EntityManager.GetComponentData<ContainerDescription>(upgradeAndEvenButtonUiView.ObjectEntity);
            var container =
                config.ContainersData.FirstOrDefault(container =>
                    container.Level == containerDescription.Level + 1 && container.Type == containerDescription.Type);

            if (bank.Coins < container.Price)
            {
                return;
            }

            var upCompletedEntity = _upCompletedQuery.ToEntityArray(Allocator.Temp)[0];
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upCompletedEntity).AvailableUps;
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upCompletedEntity).CompleteUp;

            var upDataList = EntityManager.GetComponentObject<UpButtonAvailable>(entity).List;
            completedUp.Add(upDataList[0]);
            availableUp.Remove(upDataList[0]);
            upDataList.Remove(upDataList[0]);

            if (upDataList.Count == 0)
            {
                EntityManager.RemoveComponent<UpButtonAvailable>(entity);
            }

            bank.Coins -= container.Price;
            _bankQuery.SetSingleton(bank);

            var containerEntity = upgradeAndEvenButtonUiView.ObjectEntity; //TODO
            var containerLevel = container.Level;

            var containerView = EntityManager
                .GetComponentObject<ContainerView>(containerEntity).Value;
            var containerTransform = containerView.transform;
            var upgradeContainerEntity = EntityManager.CreateEntity();
            var barmanPoints = _barmanContainerPointsQuery.ToEntityArray(Allocator.Temp);
            var barmanPointList = new HashSet<Entity>();

            foreach (var pointEntity in barmanPoints)
            {
                var containerPoint = EntityManager.GetComponentData<BarmanPointContainer>(pointEntity);
                if (containerPoint.Container == containerEntity)
                {
                    barmanPointList.Add(pointEntity);
                }
            }

            foreach (var pointEntity in barmanPointList)
            {
                var point = EntityManager.GetComponentData<BarmanPointContainer>(pointEntity);
                point.Container = upgradeContainerEntity;
                EntityManager.SetComponentData(pointEntity, point);
            }

            var customerPoints = _customerContainerPointsQuery.ToEntityArray(Allocator.Temp);
            var customerPointList = new HashSet<Entity>();

            foreach (var pointEntity in customerPoints)
            {
                var containerPoint = EntityManager.GetComponentData<CustomerPointContainer>(pointEntity);
                if (containerPoint.Container == containerEntity)
                {
                    customerPointList.Add(pointEntity);
                }
            }

            foreach (var pointEntity in customerPointList)
            {
                var point = EntityManager.GetComponentData<CustomerPointContainer>(pointEntity);
                point.Container = upgradeContainerEntity;
                EntityManager.SetComponentData(pointEntity, point);
            }

            SetNameEntity(containerEntity, upgradeContainerEntity, containerLevel);

            AddProduct(new ProductData
            {
                Count = 0,
                ProductType = container.Type,
                Level = container.Level,
                PurchaseCost = 1 * container.Level,
                SellPrice = 1 * container.Level * 2
            });

            var upgradeContainerView =
                Object.Instantiate(container.Prefab, containerTransform.position, containerTransform.rotation);
            upgradeAndEvenButtonUiView.UpgradeAndEventButton.gameObject.transform.SetParent(upgradeContainerView
                .transform);
            var upgradeTransformFX = upgradeContainerView.ParticleSystem.transform;

            var upgradePosition = containerView.ParticleSystem.transform;
            upgradeTransformFX.position = upgradePosition.position;
            upgradeTransformFX.rotation = upgradePosition.rotation;

            EntityManager.AddComponent<Destroyed>(containerEntity);
            upgradeAndEvenButtonUiView.ObjectEntity = upgradeContainerEntity;

            EntityManager.AddComponentObject(upgradeContainerEntity,
                new ContainerView { Value = upgradeContainerView });
            EntityManager.AddComponent<Container>(upgradeContainerEntity);
            EntityManager.AddComponentData(upgradeContainerEntity,
                new ContainerDescription
                {
                    Capacity = container.Capacity,
                    Type = container.Type,
                    Level = container.Level
                });
            upgradeContainerView.Initialize(EntityManager, upgradeContainerEntity);
            upgradeAndEvenButtonUiView.DisableUpgradeAndEvenButtons();
        }

        private void UpgradeTable(Entity entity, UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView)
        {
            var bank = _bankQuery.GetSingleton<Bank>();
            var config = EntityUtilities.GetGameConfig();
            var tableView = EntityManager
                .GetComponentObject<TableView>(upgradeAndEvenButtonUiView.ObjectEntity);
            var tableLevel = EntityManager.GetComponentData<Table>(upgradeAndEvenButtonUiView.ObjectEntity).Level;
            var table =
                config.TableConfig.TablesData.First(table => table.Level == tableLevel + 1);

            if (bank.Coins < table.Price)
            {
                return;
            }

            var upCompletedEntity = _upCompletedQuery.ToEntityArray(Allocator.Temp)[0];
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upCompletedEntity).AvailableUps;
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upCompletedEntity).CompleteUp;

            var upDataList = EntityManager.GetComponentObject<UpButtonAvailable>(entity).List;
            completedUp.Add(upDataList[0]);
            availableUp.Remove(upDataList[0]);
            upDataList.Remove(upDataList[0]);

            if (upDataList.Count == 0)
            {
                EntityManager.RemoveComponent<UpButtonAvailable>(entity);
            }

            bank.Coins -= table.Price;
            _bankQuery.SetSingleton(bank);

            var tableEntity = upgradeAndEvenButtonUiView.ObjectEntity;

            var upgradeTableEntity = EntityManager.CreateEntity();
            var atTablePoints = _atTablePointQuery.ToEntityArray(Allocator.Temp);
            var onTablePoints = _onTablePointQuery.ToEntityArray(Allocator.Temp);
            var dirtTableViewEntities =
                EntityManager.GetComponentObject<DirtTableViewEntities>(tableEntity);

            foreach (var dirtEntity in dirtTableViewEntities.DirtTableObjectEntities)
            {
                Object.Destroy(EntityManager.GetComponentObject<DirtTableView>(dirtEntity).DirtObject
                    .gameObject);
                EntityManager.AddComponent<Destroyed>(dirtEntity);
            }

            var atTheTablePointList = new HashSet<Entity>();

            for (var index = 0; index < tableView.AtTablePointsEntity.Count; index++)
            {
                var pointEntity = tableView.AtTablePointsEntity[index];

                if (EntityManager.HasComponent<PointNotAvailable>(pointEntity))
                {
                    EntityManager.RemoveComponent<PointNotAvailable>(pointEntity);
                }

                if (index >= table.QuantityAtTablePoints)
                {
                    EntityManager.AddComponent<PointNotAvailable>(pointEntity);
                }
            }

            foreach (var pointEntity in atTablePoints)
            {
                var atTablePoint = EntityManager.GetComponentData<AtTablePoint>(pointEntity);
                if (atTablePoint.Table == tableEntity)
                {
                    atTheTablePointList.Add(pointEntity);
                }
            }

            foreach (var pointEntity in atTheTablePointList)
            {
                var point = EntityManager.GetComponentData<AtTablePoint>(pointEntity);
                point.Table = upgradeTableEntity;
                EntityManager.SetComponentData(pointEntity, point);
            }

            var onTheTablePointList = new HashSet<Entity>();

            foreach (var pointEntity in onTablePoints)
            {
                var onTablePoint = EntityManager.GetComponentData<OnTablePoint>(pointEntity);
                if (onTablePoint.Table == tableEntity)
                {
                    onTheTablePointList.Add(pointEntity);
                }
            }

            foreach (var pointEntity in onTheTablePointList)
            {
                var point = EntityManager.GetComponentData<OnTablePoint>(pointEntity);
                point.Table = upgradeTableEntity;
                EntityManager.SetComponentData(pointEntity, point);
            }

            var tableTransform = tableView.Value.transform;
            EntityManager.AddComponentData(upgradeTableEntity, new Table { Level = table.Level, DirtValue = 0 });

            var upgradeTable =
                Object.Instantiate(table.Prefab, tableTransform.position, tableTransform.rotation);

            var upgradeTransformFX = upgradeTable.ParticleSystem.transform;

            var transformParticleSystem = tableView.Value.ParticleSystem.transform;

            upgradeTransformFX.position = transformParticleSystem.position;
            upgradeTransformFX.rotation = transformParticleSystem.rotation;

            upgradeTable.NavMeshObstacle.enabled = true;

            var upgradeTableView = new TableView
            {
                Value = upgradeTable,
                AtTablePointsEntity = tableView.AtTablePointsEntity,
                CleanTablePoint = new Point
                {
                    Position = tableView.CleanTablePoint.Position,
                    Rotation = tableView.CleanTablePoint.Rotation
                }
            };

            var arrowPoint = upgradeTable.transform.position;

            arrowPoint.y += BreakdownObjectConstants.MovementArrowTableOffsetY;

            var clearArrow = Object.Instantiate(config.ClearArrow, arrowPoint,
                new Quaternion());
            clearArrow.transform.SetParent(upgradeTable.transform);
            clearArrow.gameObject.SetActive(false);
            var repairArrow = Object.Instantiate(config.RepairArrow, arrowPoint,
                new Quaternion());
            repairArrow.transform.SetParent(upgradeTable.transform);
            repairArrow.gameObject.SetActive(false);

            EntityManager.AddComponentObject(upgradeTableEntity, new ClearMovementArrowView { Arrow = clearArrow });
            EntityManager.AddComponentObject(upgradeTableEntity, new RepairMovementArrowView { Arrow = repairArrow });


            var cleanerEntityArray = _cleanerQuery.ToEntityArray(Allocator.Temp);

            foreach (var cleanerEntity in cleanerEntityArray)
            {
                if (!EntityManager.HasComponent<CleanTableCleaner>(cleanerEntity))
                {
                    continue;
                }

                var clearTable = EntityManager.GetComponentData<CleanTableCleaner>(cleanerEntity);

                if (clearTable.Table != tableEntity)
                {
                    continue;
                }

                clearTable.Table = upgradeTableEntity;
                EntityManager.SetComponentData(cleanerEntity, clearTable);
            }

            SetNameEntity(tableEntity, upgradeTableEntity, table.Level);
            var upgradeAndEventButtonUi = EntityManager.GetComponentData<UpgradeAndEventButtonUi>(tableEntity);
            EntityManager.AddComponentData(upgradeTableEntity, upgradeAndEventButtonUi);
            
            EntityManager.AddComponent<Destroyed>(tableEntity);
            upgradeAndEvenButtonUiView.ObjectEntity = upgradeTableEntity;
            upgradeAndEvenButtonUiView.UpgradeAndEventButton.transform.SetParent(upgradeTable.gameObject.transform);


            EntityManager.AddComponentObject(upgradeTableEntity, upgradeTableView);
            EntityManager.AddComponentObject(upgradeTableEntity,
                new NavMeshAgentView { Obstacle = upgradeTable.NavMeshObstacle });

            var dirtTableViewHashSet = new HashSet<Entity>();

            EntityManager.AddComponentObject(upgradeTableEntity,
                new DirtTableViewEntities { DirtTableObjectEntities = dirtTableViewHashSet });

            upgradeTable.Initialize(EntityManager, upgradeTableEntity);
            upgradeAndEvenButtonUiView.DisableUpgradeAndEvenButtons();
        }


        private void SetNameEntity(Entity containerEntity, Entity upgradeContainerEntity, int level)
        {
            if (TrySetName<FishSnack>(containerEntity, upgradeContainerEntity, level, EntityConstants.FishSnackName))
            {
                return;
            }

            if (TrySetName<Fridge>(containerEntity, upgradeContainerEntity, level, EntityConstants.FridgeName))
            {
                return;
            }

            if (TrySetName<Nuts>(containerEntity, upgradeContainerEntity, level, EntityConstants.NutsName))
            {
                return;
            }

            if (TrySetName<Spill>(containerEntity, upgradeContainerEntity, level, EntityConstants.SpillName))
            {
                return;
            }

            if (TrySetName<Table>(containerEntity, upgradeContainerEntity, level, EntityConstants.TableName)) { }
        }

        private bool TrySetName<T>(Entity containerEntity, Entity upgradeContainerEntity, int level,
            FixedString64Bytes name) where T : IComponentData
        {
            if (!EntityManager.HasComponent<T>(containerEntity))
            {
                return false;
            }
            
            EntityManager.AddComponent<T>(upgradeContainerEntity);
            EntityManager.SetName(upgradeContainerEntity, name + level.ToString());
            return true;
        }

        private void TornButtonToCamera(in UpgradeAndEvenButtonUiView upgradeAndEvenButtonUiView)
        {
            if (_mainCameraQuery.IsEmpty)
            {
                return;
            }

            var cameraEntity = _mainCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var mainCamera = EntityManager.GetComponentObject<CameraView>(cameraEntity);
            var transform = upgradeAndEvenButtonUiView.UpgradeAndEventButton.transform;
            var rotation = mainCamera.Value.transform.rotation;

            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }

        private void AddProduct(ProductData productData)
        {
            var warehouseProductEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(warehouseProductEntity, new WarehouseProduct
            {
                ProductData = productData
            });
            
            EntityManager.SetName(warehouseProductEntity,
                $"WarehouseProduct {productData.ProductType} {productData.Level}");
        }
    }
}