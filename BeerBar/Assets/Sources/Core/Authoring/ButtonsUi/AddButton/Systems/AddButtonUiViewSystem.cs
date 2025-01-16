using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Containers;
using Core.Authoring.Products;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Authoring.Warehouses;
using Core.Components.Destroyed;
using Core.Configs;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Container = Core.Authoring.Containers.Container;

namespace Core.Authoring.ButtonsUi.AddButton.Systems
{
    [RequireMatchingQueriesForUpdate]

    public partial class AddButtonUiViewSystem : SystemBase
    {
        private EntityQuery _bankQuery;
        private EntityQuery _completedUpQuery;
        private EntityQuery _warehouseProductQuery;

        protected override void OnCreate()
        {
            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);

            using var completedUpBuilder = new EntityQueryBuilder(Allocator.Temp);
            _completedUpQuery = completedUpBuilder.WithAll<CompletedUp>().Build(this);

            using var warehouseProductBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseProductQuery = warehouseProductBuilder.WithAll<WarehouseProduct>().Build(this);

        }

        protected override void OnUpdate()
        {
            Entities.WithAll<AddButtonUiView>()
                .ForEach((Entity entity, in AddButtonUiView addButtonUiView) =>
                {
                    CheckAvailableAddButton(entity, addButtonUiView);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Container, AddButtonUiView>().WithAll<Clicked>()
                .ForEach((Entity entity, in AddButtonUiView addButtonUiView) =>
                {
                    SpawnContainer(entity, addButtonUiView, 1);
                }).WithoutBurst().WithStructuralChanges().Run();

            Entities.WithAll<Table, AddButtonUiView>().WithAll<Clicked>()
                .ForEach((Entity entity, in AddButtonUiView addButtonUiView) =>
                {
                    SpawnTable(entity, addButtonUiView, 1);
                }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CheckAvailableAddButton(Entity entity, AddButtonUiView addButtonUiView)
        {
            var price = 0;

            if (EntityManager.HasComponent<Container>(entity))
            {
                var config = EntityUtilities.GetContainerConfig();
                var spawnPointData =
                    EntityManager.GetComponentData<SpawnPointContainer>(addButtonUiView.SpawnPointEntity);
                var container =
                    config.ContainersData.FirstOrDefault(container =>
                        container.Level == 1 && container.Type == spawnPointData.Type);

                price = container.Price;
            }

            if (EntityManager.HasComponent<Table>(entity))
            {
                var config = EntityUtilities.GetTableConfig();
                var table =
                    config.TablesData.First(container =>
                        container.Level == 1);
                price = table.Price;
            }

            AddButtonAvailability(addButtonUiView, price);
        }

        private void SpawnContainer(Entity entity, AddButtonUiView addButtonUiView, int level)
        {
            var config = EntityUtilities.GetContainerConfig();
            var bank = _bankQuery.GetSingletonRW<Bank>();
            var spawnPointData =
                EntityManager.GetComponentData<SpawnPointContainer>(addButtonUiView.SpawnPointEntity);

            if (addButtonUiView.UpData.UpType == UpType.SpillContainer)
            {
                level = EntityManager.GetComponentData<SpillLevelContainer>(entity).Value;
            }

            var container =
                config.ContainersData.First(container =>
                    container.Level == level && container.Type == spawnPointData.Type);

            if (bank.ValueRW.Coins < container.Price)
            {
                EntityManager.RemoveComponent<Clicked>(entity);
                return;
            }

            var spawnPoint = spawnPointData.SpawnPoint;
            var customerPoints = EntityManager.GetBuffer<CustomerContainerPoint>(addButtonUiView.SpawnPointEntity)
                .ToNativeArray(Allocator.Temp);
            var barmanPoints = EntityManager.GetBuffer<BarmanContainerPoint>(addButtonUiView.SpawnPointEntity)
                .ToNativeArray(Allocator.Temp);
            var spawnContainerEntity = EntityManager.CreateEntity();

            bank.ValueRW.Coins -= container.Price;

            EntityManager.AddComponentObject(spawnContainerEntity, new SpawnContainer
            {
                Prefab = container.Prefab,
                Level = container.Level,
                Capacity = container.Capacity,
                Point = spawnPoint,
                Type = container.Type,
                CustomerContainerPoints = customerPoints,
                BarmanContainerPoints = barmanPoints
            });

            AddCompletedUp(addButtonUiView.UpData);

            var warehouseProduct = _warehouseProductQuery.ToEntityArray(Allocator.Temp);
            var productConfig = EntityUtilities.GetGameConfig().ProductConfig.Products;
            var newProduct = productConfig.FirstOrDefault(product =>
                product.ProductType == container.Type && product.Level == container.Level);
            var availableInStock = false;

            foreach (var productEntity in warehouseProduct)
            {
                var product = EntityManager.GetComponentData<WarehouseProduct>(productEntity).ProductData;
                if (product.ProductType == newProduct.ProductType && product.Level == container.Level)
                {
                    availableInStock = true;
                }
            }

            if (!availableInStock)
            {
                AddProduct(new ProductData
                {
                    ProductType = container.Type,
                    Level = container.Level,
                    Count = 0,
                    PurchaseCost = newProduct.PurchaseCost,
                    SellPrice = newProduct.SellPrice
                });
            }

            EntityManager.AddComponent<Destroyed>(entity);
        }

        private void SpawnTable(Entity entity, AddButtonUiView addButtonUiView, int level)
        {
            var bank = _bankQuery.GetSingletonRW<Bank>();
            var gameConfig = EntityUtilities.GetGameConfig();
            var tableConfig = EntityUtilities.GetTableConfig();
            var table =
                tableConfig.TablesData.First(configTable =>
                    configTable.Level == level);

            if (bank.ValueRW.Coins < table.Price)
            {
                EntityManager.RemoveComponent<Clicked>(entity);
                return;
            }

            var spawnPointData =
                EntityManager.GetComponentData<SpawnPointTable>(addButtonUiView.SpawnPointEntity);
            var spawnPoint = spawnPointData.SpawnPoint;
            var atTablePoints = EntityManager.GetBuffer<PointAtTheTable>(addButtonUiView.SpawnPointEntity)
                .ToNativeArray(Allocator.Persistent);
            var onTablePoints = EntityManager.GetBuffer<PointOnTheTable>(addButtonUiView.SpawnPointEntity)
                .ToNativeArray(Allocator.Persistent);
            var spawnTableEntity = EntityManager.CreateEntity();
            var cleanTablePoint = EntityManager
                .GetComponentData<BarmanCleanTablePoint>(addButtonUiView.SpawnPointEntity).Point;

            bank.ValueRW.Coins -= table.Price;
            EntityManager.AddComponentObject(spawnTableEntity, new SpawnTable
            {
                Prefab = table.Prefab,
                Level = table.Level,
                SpawnPoint = spawnPoint,
                CleanTablePoint = cleanTablePoint,
                AtTablePoints = atTablePoints,
                OnTablePoints = onTablePoints,
                QuantityAtTablePoints = table.QuantityAtTablePoints,
                IndexLevelUpFx = addButtonUiView.IndexLevelUpFX,
                ClearArrow = gameConfig.ClearArrow,
                RepairArrow = gameConfig.RepairArrow
            });

            AddCompletedUp(addButtonUiView.UpData);

            EntityManager.AddComponent<Destroyed>(entity);
        }

        private void AddButtonAvailability(AddButtonUiView addButtonUiView, int tablePrise)
        {
            var bank = _bankQuery.GetSingleton<Bank>();

            if (bank.Coins < tablePrise)
            {
                addButtonUiView.AddButtonUiAuthoring.AddButton.image.color = Color.gray;
                addButtonUiView.AddButtonUiAuthoring.AddButton.enabled = false;
                return;
            }

            addButtonUiView.AddButtonUiAuthoring.AddButton.image.color = Color.white;
            addButtonUiView.AddButtonUiAuthoring.AddButton.enabled = true;
        }

        private void AddCompletedUp(Up upData)
        {
            var upCompletedEntity = _completedUpQuery.ToEntityArray(Allocator.Temp)[0];
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upCompletedEntity).AvailableUps;
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upCompletedEntity).CompleteUp;
            completedUp.Add(upData);
            availableUp.Remove(upData);
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