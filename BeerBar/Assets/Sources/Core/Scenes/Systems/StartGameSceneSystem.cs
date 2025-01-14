using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Bartenders;
using Core.Authoring.ButtonsUi.AddButton;
using Core.Authoring.ButtonsUi.SpeedXButton;
using Core.Authoring.Cameras;
using Core.Authoring.Cleaners;
using Core.Authoring.CoinsUi;
using Core.Authoring.Containers;
using Core.Authoring.LoadingUi;
using Core.Authoring.ParametersButtonUi;
using Core.Authoring.ParametersUi;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.ProductKeepers;
using Core.Authoring.Products;
using Core.Authoring.Repairmans;
using Core.Authoring.RootCanvas;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Authoring.TextProductTablesUI;
using Core.Authoring.TVs;
using Core.Authoring.UpgradeUi;
using Core.Authoring.WarehouseUi;
using Core.Configs;
using Core.Constants;
using Core.Scenes.Components;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Container = Core.Authoring.Containers.Container;
using Table = Core.Authoring.Tables.Table;

namespace Core.Scenes.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class StartGameSceneSystem : SystemBase
    {
        private EntityQuery _sceneQuery;
        private EntityQuery _spawnPointCameraQuery;
        private EntityQuery _spawnPointBarmanQuery;
        private EntityQuery _spawnPointProductKeeperQuery;
        private EntityQuery _spawnPointRepairmanQuery;
        private EntityQuery _spawnPointCleanerQuery;
        private EntityQuery _spawnPointsContainerQuery;
        private EntityQuery _spawnPointsTableQuery;
        private EntityQuery _mainCameraQuery;

        protected override void OnCreate()
        {
            using var sceneBuilder = new EntityQueryBuilder(Allocator.Temp);
            _sceneQuery = sceneBuilder.WithAll<SceneLoaded>().Build(this);
            
            using var spawnPointCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointCameraQuery = spawnPointCameraBuilder.WithAll<SpawnPointCamera>().Build(this);
            
            using var spawnPointBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointBarmanQuery = spawnPointBarmanBuilder.WithAll<BarmanSpawnPoint, SpawnPoint>().Build(this);
            
            using var spawnPointProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointProductKeeperQuery = spawnPointProductKeeperBuilder.WithAll<SpawnPointProductKeeper>().Build(this);
            
            using var spawnPointRepairmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointRepairmanQuery = spawnPointRepairmanBuilder.WithAll<SpawnPointRepairman>().Build(this);
            
            using var spawnPointCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointCleanerQuery = spawnPointCleanerBuilder.WithAll<SpawnPointCleaner>().Build(this);
         
            using var spawnPointsContainerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointsContainerQuery = spawnPointsContainerBuilder.WithAll<SpawnPointContainer, CustomerContainerPoint>().Build(this);// добавить BarmanContainerPoints
            
            using var spawnPointsTableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointsTableQuery = spawnPointsTableBuilder.WithAll<SpawnPointTable, PointAtTheTable>().Build(this);
            
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera, CameraView>().Build(this);
        }

        protected override void OnUpdate()
        {
            if (_sceneQuery.IsEmpty)
            {
                return;
            }

            if (_spawnPointCameraQuery.IsEmpty)
            {
                return;
            }

            if (_spawnPointBarmanQuery.IsEmpty)
            {
                return;
            }

            if (_spawnPointProductKeeperQuery.IsEmpty)
            {
                return;
            }

            if (_spawnPointsContainerQuery.IsEmpty)
            {
                return;
            }

            if (_spawnPointsTableQuery.IsEmpty)
            {
                return;
            }
            
            var config = EntityUtilities.GetGameConfig();
            CreateMainCamera(config);
            CreateStoreRating();
            CreateBank();
            CreateProductKeeper(config);
            CreateRepairman(config);
            CreateCleaner(config);
            CreateRootCanvas(config);
            CreateCoinsUi(config);
            CreateStoreRatingUi(config);
            CreateWarehouseUi(config);
            CreatePhraseCustomerUi(config);
            CreateParametersButtonUi(config);
            CreateParametersUi(config);
            CreateLoadingScreenUi(config);
            CreateTextProductTableUI(config);
            CreateTV(config);
            CreateSpeedXButtonUi(config);
            CreateUpgradeUi(config);
            CreateSelectMaterialEntity(config);

            var spawnPointBarmanEntity = _spawnPointBarmanQuery.ToComponentDataArray<SpawnPoint>(Allocator.Temp);
            
            for (var index = 0; index < 1; index++)
            {
                var spawnPoint = spawnPointBarmanEntity[index];
                CreateBarman(config, spawnPoint, index);
            }
            var array = _sceneQuery.ToEntityArray(Allocator.Temp);
            
            EntityManager.DestroyEntity(array[0]);
        }

        private void CreateProductContainer(GameConfig config, ProductType type, int level, SpawnPoint spawnPoint,
            NativeArray<CustomerContainerPoint> customerPoints, NativeArray<BarmanContainerPoint> barmanPoints)
        {
            var spawnContainerEntity = EntityManager.CreateEntity();
            var container =
                config.ContainerConfig.ContainersData.First(container =>
                    container.Level == level && container.Type == type);

            EntityManager.AddComponentObject(spawnContainerEntity, new SpawnContainer
            {
                Prefab = container.Prefab, Level = container.Level, Capacity = container.Capacity,
                Point = spawnPoint, Type = container.Type, CustomerContainerPoints = customerPoints,
                BarmanContainerPoints = barmanPoints
            });
        }

        private void CreateBank()
        {
            var bank = EntityManager.CreateSingleton<Bank>();
            
            EntityManager.SetComponentData(bank, new Bank { Coins = 1000 });
            EntityManager.SetName(bank, EntityConstants.BankName);
        }
        
        private void CreateStoreRating()
        {
            var storeRating = EntityManager.CreateSingleton<StoreRating>();
            
            EntityManager.SetComponentData(storeRating, new StoreRating{CurrentValue = 0, Level = 1});
            EntityManager.SetName(storeRating, EntityConstants.StoreRatingName);

            var upLineEntity = EntityManager.CreateEntity();
            EntityManager.AddComponent<UpLineEntity>(upLineEntity);
            EntityManager.AddComponentObject(upLineEntity, new AvailableUp { AvailableUps = new List<Up>() });
            EntityManager.AddComponentObject(upLineEntity, new CompletedUp { CompleteUp = new List<Up>() });
            EntityManager.AddComponentObject(upLineEntity, new ProductToBay { Products = new HashSet<ProductData>()});
        }

        private void CreateMainCamera(GameConfig config)
        {
            var spawnPointCameraEntity = _spawnPointCameraQuery.ToEntityArray(Allocator.Temp)[0];
            var spawnPoint = EntityManager.GetBuffer<SpawnPointCamera>(spawnPointCameraEntity)[0];
            
            if (!_mainCameraQuery.IsEmpty)
            {
                var startCameraArray = Object.FindObjectsOfType<Camera>();
                var mainCameraEntity = _mainCameraQuery.GetSingletonEntity();
                var cameraView = EntityManager.GetComponentObject<CameraView>(mainCameraEntity);
                
                foreach (var cam in startCameraArray)
                {
                    if (cam.GetInstanceID() == cameraView.Value.GetInstanceID())
                    {
                        var transform = cameraView.Value.transform;
                        transform.position = spawnPoint.Position;
                        transform.rotation = spawnPoint.Rotation;
                    }
                    else
                    {
                        Object.Destroy(cam.gameObject);
                    }
                }
                return;
            }
            
            var cameraEntity = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(cameraEntity, new SpawnCamera
            {
                CameraPrefab = config.CameraConfig.CameraPrafab, Point = spawnPoint
            });
        }

        private void CreateBarman(GameConfig config, SpawnPoint spawnPoint, int indexBarman)
        {
            var barman = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(barman, new SpawnBarman
            {
                BarmanData = config.BarmanConfig, Point = spawnPoint, IndexBarman = indexBarman
            });
        }
        private void CreateProductKeeper(GameConfig config)
        {
            var spawnPointEntity = _spawnPointProductKeeperQuery.ToEntityArray(Allocator.Temp)[0];
            var spawnPoint = EntityManager.GetComponentData<SpawnPointProductKeeper>(spawnPointEntity);
            var productKeeper = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(productKeeper, new SpawnProductKeeper
            {
                ProductKeeper = config.ProductKeeperConfig, Point = spawnPoint
            });
        }
        
        private void CreateRepairman(GameConfig config)
        {
            var spawnPointEntity = _spawnPointRepairmanQuery.ToEntityArray(Allocator.Temp)[0];
            var spawnPoint = EntityManager.GetComponentData<SpawnPointRepairman>(spawnPointEntity);
            var repairmanEntity = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(repairmanEntity, new SpawnRepairman
            {
                RepairmanData = config.RepairmanConfig, Point = spawnPoint
            });
        }
        
        private void CreateCleaner(GameConfig config)
        {
            var spawnPointEntity = _spawnPointCleanerQuery.ToEntityArray(Allocator.Temp)[0];
            var spawnPoint = EntityManager.GetComponentData<SpawnPointCleaner>(spawnPointEntity);
            var cleanerEntity = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(cleanerEntity, new SpawnCleaner
            {
                CleanerData = config.CleanerConfig, Point = spawnPoint
            });
        }

        private void CreateRootCanvas(GameConfig config)
        {
            var rootCanvas = EntityManager.CreateEntity();
            
            EntityManager.AddComponentObject(rootCanvas, new SpawnRootCanvas
            {
                RootCanvasPrefab = config.UIConfig.RootCanvasPrefab
            });
        }

        private void CreateCoinsUi(GameConfig config)
        {
            var coinsUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(coinsUi, new SpawnCoinsUi
            {
                CoinsUiPrefab = config.UIConfig.CoinsUiPrefab
            });
        }
        
        
        private void CreateSpeedXButtonUi(GameConfig config)
        {
            var speedXButtonUiEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(speedXButtonUiEntity, new SpawnSpeedXButtonUi
            {
                SpeedXButtonUiPrefab = config.UIConfig.SpeedXButtonUiPrefab
            });
        }
        private void CreateStoreRatingUi(GameConfig config)
        {
            var storeRatingUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(storeRatingUi, new SpawnStoreRatingUi
            {
                StoreRatingUiPrefab = config.UIConfig.StoreRatingUiPrefab
            });
        }

        private void CreateWarehouseUi(GameConfig config)
        {
            var warehouseUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(warehouseUi, new SpawnWarehouseUi
            {
                WarehouseUiPrefab = config.UIConfig.WarehouseUiPrefab
            });
        }
        
        private void CreatePhraseCustomerUi(GameConfig config)
        {
            var phraseCustomerUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(phraseCustomerUi, new SpawnPhraseCustomerUiManager
            {
                PhraseCustomerUiPrefab = config.UIConfig.PhraseCustomerUiPrefab
            });
        }
        
        private void CreateAddContainerButtonUi(GameConfig config, Entity spawnPointContainerEntity)
        {
            var addContainerButtonUi = EntityManager.CreateEntity();
            EntityManager.AddComponent<Container>(addContainerButtonUi);
            EntityManager.AddComponentObject(addContainerButtonUi, new SpawnAddButtonUi
            {
                AddButtonUiPrefab = config.UIConfig.AddButtonUiPrefab, SpawnPointEntity = spawnPointContainerEntity
            });
        }
        
        private void CreateAddTableButtonUi(GameConfig config, Entity spawnPointTableEntity)
        {
            var addTableButtonUi = EntityManager.CreateEntity();
            EntityManager.AddComponent<Table>(addTableButtonUi);
            EntityManager.AddComponentObject(addTableButtonUi, new SpawnAddButtonUi
            {
                AddButtonUiPrefab = config.UIConfig.AddButtonUiPrefab, SpawnPointEntity = spawnPointTableEntity
            });
        }

        private void CreateParametersButtonUi(GameConfig config)
        {
            var parametersButtonUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(parametersButtonUi, new SpawnParametersButtonUi
            {
                ParametersButtonUiPrefab = config.UIConfig.ParametersButtonUiPrefab
            });
        }
        
        private void CreateParametersUi(GameConfig config)
        {
            var parametersUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(parametersUi, new SpawnParametersUi
            {
                ParametersUiPrefab = config.UIConfig.ParametersUiPrefab
            });
        }

        private void CreateLoadingScreenUi(GameConfig config)
        {
            var loadingScreenUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(loadingScreenUi, new SpawnLoadingScreenUi
            {
                LoadingScreenUiAuthoring = config.UIConfig.LoadingScreenUiPrefab
            });
        }
        
        private void CreateTextProductTableUI(GameConfig config)
        {
            var textProductTableEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(textProductTableEntity, new SpawnTextProductTableUI
            {
                TextProductTablesUIPrefab = config.UIConfig.TextProductTableUIPrefab
            });
        }
        
        private void CreateTV(GameConfig config)
        {
            var tVEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(tVEntity, new SpawnTV
            {
                TVPrefab = config.EventObjectConfig.TVPrefab, RepairArrow = config.RepairArrow
            });
        }

        private void CreateUpgradeUi(GameConfig config)
        {
            var upgradeBarUi = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(upgradeBarUi, new SpawnUpgradeUi
            {
                UpgradeBarUiPrefab = config.UIConfig.UpgradeBarUiPrefab,
                UpgradeElementUiSmallPrefab = config.UIConfig.UpgradeElementUiSmallPrefab,
                UpgradeElementUiBigPrefab = config.UIConfig.UpgradeElementUiBigPrefab,
                UpgradeDescriptionUiPrefab = config.UIConfig.UpgradeDescriptionUiPrefab
            });
        }
        
        private void CreateSelectMaterialEntity(GameConfig config)
        {
            var selectMaterialEntity = EntityManager.CreateEntity();

            EntityManager.AddComponentObject(selectMaterialEntity, new SelectMaterial
            {
                RendererObject = config.SelectMaterial, 
                ParticleBreakBottleRendererObject = config.BreakBottleSelectMaterial,
                ParticleSprayRendererObject = config.TubeSpraySelectMaterial
            });
        }
    }
}