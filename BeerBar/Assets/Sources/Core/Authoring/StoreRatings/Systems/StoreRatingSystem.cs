using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Bartenders;
using Core.Authoring.Bartenders.AddBarmanFX;
using Core.Authoring.ButtonsUi.AddButton;
using Core.Authoring.Containers;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.Tables;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Authoring.Warehouses;
using Core.Configs;
using Core.Constants;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Container = Core.Authoring.Containers.Container;

namespace Core.Authoring.StoreRatings.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class StoreRatingSystem : SystemBase
    {
        private EntityQuery _warehouseQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _upLineQuery;
        private EntityQuery _fridgeQuery;
        private EntityQuery _fishSnackQuery;
        private EntityQuery _spillQuery;
        private EntityQuery _nutsQuery;
        private EntityQuery _spillCountQuery;
        private EntityQuery _nutsCountQuery;
        private EntityQuery _tableQuery;
        private EntityQuery _tableUiQuery;
        private EntityQuery _spawnPointsContainerQuery;
        private EntityQuery _spawnPointsTableQuery;
        private EntityQuery _spawnPointBarmanQuery;
        private EntityQuery _barmanQuery;
        private EntityQuery _addBarmanFXQuery;

        protected override void OnCreate()
        {
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseQuery = warehouseBuilder.WithAllRW<Warehouse>().Build(this);
            
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);
            
            using var fridgeBuilder = new EntityQueryBuilder(Allocator.Temp);
            _fridgeQuery = fridgeBuilder.WithAll<Fridge, ContainerDescription>().Build(this);
            
            using var fishSnackBuilder = new EntityQueryBuilder(Allocator.Temp);
            _fishSnackQuery = fishSnackBuilder.WithAll<FishSnack, ContainerDescription>().Build(this);
            
            using var spillBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spillQuery = spillBuilder.WithAll<Spill, ContainerDescription>().Build(this);
            
            using var nutsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _nutsQuery = nutsBuilder.WithAll<Nuts, ContainerDescription, UpgradeAndEventButtonUi>().Build(this);
            
            using var spillCountBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spillCountQuery = spillCountBuilder.WithAll<Spill>().WithNone<UpgradeAndEvenButtonUiView, BarmanPointContainer, CustomerPointContainer>().Build(this);
            
            using var nutsCountBuilder = new EntityQueryBuilder(Allocator.Temp);
            _nutsCountQuery = nutsCountBuilder.WithAll<Nuts>().WithNone<UpgradeAndEvenButtonUiView>().Build(this);
            
            using var tableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tableQuery = tableBuilder.WithAll<Table>().WithNone<UpgradeAndEvenButtonUiView>().Build(this);
            
            using var tableUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tableUiQuery = tableUiBuilder.WithAll<Table, UpgradeAndEventButtonUi>().Build(this);
            
            using var upLineBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upLineQuery = upLineBuilder.WithAll<UpLineEntity>().Build(this);

            using var spawnPointsContainerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointsContainerQuery = spawnPointsContainerBuilder.WithAll<SpawnPointContainer>().Build(this);// добавить BarmanContainerPoints
            
            using var spawnPointsTableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointsTableQuery = spawnPointsTableBuilder.WithAll<SpawnPointTable, PointAtTheTable>().Build(this);
            
            using var spawnPointBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointBarmanQuery = spawnPointBarmanBuilder.WithAll<BarmanSpawnPoint, SpawnPoint>().Build(this);
            
            using var barmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanQuery = barmanBuilder.WithAll<Barman, BarmanIndex>().Build(this);
            
            using var addBarmanFXBuilder = new EntityQueryBuilder(Allocator.Temp);
            _addBarmanFXQuery = addBarmanFXBuilder.WithAll<AddBarmanFXView>().Build(this);
        }
        protected override void OnUpdate()
        {
            if (_storeRatingQuery.IsEmpty)
            {
                return;
            }
            
            if (_warehouseQuery.IsEmpty)
            {
                return;
            }
            
            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();
            var ratingScore = storeRating.CurrentValue;
            var spawnPointsContainer =
                _spawnPointsContainerQuery.ToEntityArray(Allocator.Temp);
            var config = EntityUtilities.GetGameConfig();
            var upEntity = _upLineQuery.ToEntityArray(Allocator.Temp)[0];
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upEntity).AvailableUps;
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upEntity).CompleteUp;
            var productToBay = EntityManager.GetComponentObject<ProductToBay>(upEntity).Products;
            var upLine = EntityUtilities.GetGameConfig().UpConfig.UpLine;
            
            foreach (var upData in upLine)
            {
                if (ratingScore >= upData.Rating && !availableUp.Contains(upData) && !completedUp.Contains(upData))
                {
                    switch (upData.UpType)
                    {
                        case UpType.BottleBeerContainer:
                            
                            if (_fridgeQuery.IsEmpty)
                            {
                                var fridgeCount = 0;

                                for (var index = 0; index < spawnPointsContainer.Length; index++)
                                {
                                    var spawnContainerEntity = spawnPointsContainer[index];
                                    var spawnPoint =
                                        EntityManager.GetComponentData<SpawnPointContainer>(spawnContainerEntity);

                                    if (spawnPoint.Type != ProductType.BottleBeer || fridgeCount != 0)
                                    {
                                        continue;
                                    }

                                    CreateAddContainerButtonUi(config, spawnContainerEntity, upData);
                                    AddProductToBuyList(spawnContainerEntity, config, ProductType.BottleBeer, productToBay);
                                    availableUp.Add(upData);
                                    // var beerProductConfig = config.ProductConfig.Products.FirstOrDefault(product =>
                                    //     product is { ProductType: ProductType.BottleBeer, Level: 1 });
                                    // availableUp.Add(upData);
                                    // productToBay.Add(new ProductData
                                    // {
                                    //     ProductType = beerProductConfig.ProductType,
                                    //     Level = beerProductConfig.Level,
                                    //     PurchaseCost = beerProductConfig.PurchaseCost,
                                    //     SellPrice = beerProductConfig.SellPrice
                                    // });
                                    fridgeCount += 1;
                                }
                            }
                            else
                            {
                                var fridgeEntity = _fridgeQuery.ToEntityArray(Allocator.Temp)[0];
                                AddAvailableUpgradeButton(fridgeEntity, upData);
                                AddProductToBuyList(fridgeEntity, config, ProductType.BottleBeer, productToBay);
                                availableUp.Add(upData);
                            }

                            break;

                        case UpType.FishSnackContainer:

                            if (_fishSnackQuery.IsEmpty)
                            {
                                for (var index = 0; index < spawnPointsContainer.Length; index++)
                                {
                                    var spawnContainerEntity = spawnPointsContainer[index];
                                    var spawnPoint =
                                        EntityManager.GetComponentData<SpawnPointContainer>(spawnContainerEntity);

                                    if (spawnPoint.Type != ProductType.FishSnack)
                                    {
                                        continue;
                                    }

                                    CreateAddContainerButtonUi(config, spawnContainerEntity, upData);
                                    AddProductToBuyList(spawnContainerEntity, config, ProductType.FishSnack, productToBay);
                                    availableUp.Add(upData);
                                }
                            }
                            else
                            {
                                var fishSnackEntity = _fishSnackQuery.ToEntityArray(Allocator.Temp)[0];
                                AddAvailableUpgradeButton(fishSnackEntity, upData);
                                AddProductToBuyList(fishSnackEntity, config, ProductType.BottleBeer, productToBay);
                                availableUp.Add(upData);
                            }
                            
                            break;
                        
                        case UpType.NutsContainer:
                            
                            if (_nutsCountQuery.IsEmpty && _nutsQuery.IsEmpty)
                            {
                                var spawnPointsNuts = new List<Entity>();

                                for (var index = 0; index < spawnPointsContainer.Length; index++)
                                {
                                    var spawnContainerEntity = spawnPointsContainer[index];
                                    var spawnPoint =
                                        EntityManager.GetComponentData<SpawnPointContainer>(spawnContainerEntity);

                                    if (spawnPoint.Type == ProductType.Nuts)
                                    {
                                        spawnPointsNuts.Add(spawnContainerEntity);
                                    }
                                }

                                var nutsArray = _nutsCountQuery.ToEntityArray(Allocator.Temp);
                                CreateAddContainerButtonUi(config, spawnPointsNuts[nutsArray.Length], upData);
                                AddProductToBuyList(spawnPointsNuts[nutsArray.Length], config, ProductType.Nuts, productToBay);
                                availableUp.Add(upData);
                            }
                            else if (!_nutsQuery.IsEmpty)
                            {
                                var nutsEntity = _nutsQuery.ToEntityArray(Allocator.Temp)[0];
                                AddAvailableUpgradeButton(nutsEntity, upData);
                                AddProductToBuyList(nutsEntity, config, ProductType.BottleBeer, productToBay);
                                availableUp.Add(upData);
                            }


                            break;

                        case UpType.SpillContainer:
                            
                            var spawnPointsSpill = new List<Entity>();

                            for (var index = 0; index < spawnPointsContainer.Length; index++)
                            {
                                var spawnContainerEntity = spawnPointsContainer[index];
                                var spawnPoint =
                                    EntityManager.GetComponentData<SpawnPointContainer>(spawnContainerEntity);

                                if (spawnPoint.Type == ProductType.Spill)
                                {
                                    spawnPointsSpill.Add(spawnContainerEntity);
                                }
                            }

                            var sortedSpawnPoints = spawnPointsSpill.OrderBy(entity => entity).ThenBy(entity => EntityManager.GetBuffer<BarmanContainerPoint>(entity)[0].IndexPoint).ToArray();
                            var spillArray = _spillCountQuery.ToEntityArray(Allocator.Temp);
                            
                            EntityManager.AddComponentData(sortedSpawnPoints[spillArray.Length],
                                new SpillLevelContainer { Value = spillArray.Length + 1 });
                            CreateAddContainerButtonUi(config, sortedSpawnPoints[spillArray.Length], upData);
                          
                            availableUp.Add(upData);

                            var spillProductConfig = config.ProductConfig.Products.FirstOrDefault(product =>
                                product.ProductType == ProductType.Spill && product.Level == spillArray.Length + 1);

                                productToBay.Add(new ProductData
                            {
                                ProductType = spillProductConfig.ProductType, 
                                Level = spillProductConfig.Level,
                                PurchaseCost = spillProductConfig.PurchaseCost,
                                SellPrice = spillProductConfig.SellPrice
                            });
                            
                            break;
                        
                        case UpType.AddTable:

                            var spawnPointsTable =
                                _spawnPointsTableQuery.ToEntityArray(Allocator.Temp);

                            var tableArray = _tableQuery.ToEntityArray(Allocator.Temp);

                            if (tableArray.Length <= spawnPointsTable.Length)
                            {
                                CreateAddTableButtonUi(config, spawnPointsTable[tableArray.Length], upData, tableArray.Length);
                                availableUp.Add(upData);
                            }

                            break;
                        
                        case UpType.UpTable:
                            
                            var tableUi = _tableUiQuery.ToEntityArray(Allocator.Temp);

                            foreach (var tableEntity in tableUi)
                            {
                                AddAvailableUpgradeButton(tableEntity, upData); 
                            }
                            
                            availableUp.Add(upData);
                            
                            break;
                        
                        case UpType.AddBarman:
                            
                            SpawnAddBarmanFX();
                            availableUp.Add(upData);
                            
                            break;
                    }
                }
            }
            
            if (ratingScore < EntityConstants.LevelUpRatingValue * storeRating.Level)
            {
                return;
            }
            
            storeRating.Level += 1;
            _storeRatingQuery.SetSingleton(storeRating);
        }
        
        private void CreateAddContainerButtonUi(GameConfig config, Entity spawnPointContainerEntity, Up upData)
        {
            var addContainerButtonUi = EntityManager.CreateEntity();
            EntityManager.AddComponent<Container>(addContainerButtonUi);
            EntityManager.AddComponentObject(addContainerButtonUi, new SpawnAddButtonUi
            {
                AddButtonUiPrefab = config.UIConfig.AddButtonUiPrefab, 
                SpawnPointEntity = spawnPointContainerEntity, 
                UpData = upData
            });
        }
        
        private void CreateAddTableButtonUi(GameConfig config, Entity spawnPointTableEntity, Up upData, int indexLevelUpFX)
        {
            var addTableButtonUi = EntityManager.CreateEntity();
            EntityManager.AddComponent<Table>(addTableButtonUi);
            EntityManager.AddComponentObject(addTableButtonUi, new SpawnAddButtonUi
            {
                AddButtonUiPrefab = config.UIConfig.AddButtonUiPrefab, 
                SpawnPointEntity = spawnPointTableEntity, 
                UpData = upData, 
                IndexLevelUpFX = indexLevelUpFX
            });
        }
        
        private void SpawnAddBarmanFX()
        {
            var config = EntityUtilities.GetBarmanConfig();
            var barmanArray = _barmanQuery.ToEntityArray(Allocator.Temp);
            var addBarmanFXArray = _addBarmanFXQuery.ToEntityArray(Allocator.Temp);
            var indexBarman = barmanArray.Length + addBarmanFXArray.Length;
            var spawnPoint =
                _spawnPointBarmanQuery.ToComponentDataArray<SpawnPoint>(Allocator.Temp)[indexBarman];
            spawnPoint.Position.y += 2f;
            
            var addBarmanFXView = Object.Instantiate(config.AddBarmanFX, spawnPoint.Position, spawnPoint.Rotation);
            
            var addBarmanFXEntity = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(addBarmanFXEntity, new AddBarmanFXView { Value = addBarmanFXView });
            addBarmanFXView.Initialize(EntityManager, addBarmanFXEntity);
        }

        private void AddAvailableUpgradeButton(Entity entity, Up upData)
        {
            var buttonsEntity =
                EntityManager.GetComponentData<UpgradeAndEventButtonUi>(entity).Entity;

            if (EntityManager.HasComponent<UpButtonAvailable>(buttonsEntity))
            {
                var availableList =
                    EntityManager.GetComponentObject<UpButtonAvailable>(buttonsEntity).List;
                availableList.Add(upData);
            }
            else
            {
                var upList = new List<Up> { upData };
                EntityManager.AddComponentObject(buttonsEntity, new UpButtonAvailable { List = upList });
            }
        }

        private void AddProductToBuyList(Entity entity, GameConfig gameConfig, ProductType type, ISet<ProductData> productsData)
        {
            if (EntityManager.HasComponent<ContainerDescription>(entity))
            {
                var containerDescription =
                    EntityManager.GetComponentData<ContainerDescription>(
                        entity);
                var product =
                    gameConfig.ProductConfig.Products.FirstOrDefault(prod =>
                        prod.Level == containerDescription.Level + 1 &&
                        prod.ProductType == containerDescription.Type);

                productsData.Add(new ProductData
                {
                    ProductType = product.ProductType,
                    Level = product.Level,
                    PurchaseCost = product.PurchaseCost,
                    SellPrice = product.SellPrice
                });
                return;
            }
            
            var newProduct = gameConfig.ProductConfig.Products.FirstOrDefault(product => product.ProductType == type && product.Level == 1);
            
            productsData.Add(new ProductData
            {
                ProductType = newProduct.ProductType,
                Level = newProduct.Level,
                PurchaseCost = newProduct.PurchaseCost,
                SellPrice = newProduct.SellPrice
            });

        }

        public void SetRating(int value)
        {
            var storeRating = _storeRatingQuery.GetSingletonRW<StoreRating>();

            storeRating.ValueRW.CurrentValue = value;
        }
    }
    
}