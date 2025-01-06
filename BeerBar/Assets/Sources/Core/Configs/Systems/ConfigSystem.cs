using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs.Systems
{
    public partial class ConfigSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var config = Resources.Load<GameConfig>(ResourceConstants.GameConfig);
            var configEntity = EntityManager.CreateSingleton<ConfigEntity>();
            EntityManager.AddComponentObject(configEntity, new GameConfigData{Config = config});
            
            var cameraConfig = Resources.Load<CameraConfig>(ResourceConstants.CameraConfig);
            var cameraConfigEntity = EntityManager.CreateSingleton<CameraConfigEntity>();
            EntityManager.AddComponentObject(cameraConfigEntity, new CameraConfigData { Config = cameraConfig });
            
            var barmanConfig = Resources.Load<BarmanConfig>(ResourceConstants.BarmanConfig);
            var barmanConfigEntity = EntityManager.CreateSingleton<BarmanConfigEntity>();
            EntityManager.AddComponentObject(barmanConfigEntity, new BarmanConfigData { Config = barmanConfig });
            
            var customerConfig = Resources.Load<CustomerConfig>(ResourceConstants.CustomerConfig);
            var customerConfigEntity = EntityManager.CreateSingleton<CustomerConfigEntity>();
            EntityManager.AddComponentObject(customerConfigEntity, new CustomerConfigs { Config = customerConfig });
            
            var containerConfig = Resources.Load<ContainerConfig>(ResourceConstants.ContainerConfig);
            var containerConfigEntity = EntityManager.CreateSingleton<ContainerConfigEntity>();
            EntityManager.AddComponentObject(containerConfigEntity, new ContainerConfigData { Config = containerConfig });
            
            var tableConfig = Resources.Load<TableConfig>(ResourceConstants.TableConfig);
            var tableConfigEntity = EntityManager.CreateSingleton<TableConfigEntity>();
            EntityManager.AddComponentObject(tableConfigEntity, new TableConfigData { Config = tableConfig });

            var productConfig = Resources.Load<ProductConfig>(ResourceConstants.ProductConfig);
            var productConfigEntity = EntityManager.CreateSingleton<ProductConfigEntity>();
            EntityManager.AddComponentObject(productConfigEntity, new ProductConfigData { Config = productConfig });
            
            var configUI = Resources.Load<UIConfig>(ResourceConstants.UIConfig);
            var configUIEntity = EntityManager.CreateSingleton<UIConfigEntity>();
            EntityManager.AddComponentObject(configUIEntity, new UIConfigData { Config = configUI });

            var configWarehouse = Resources.Load<WarehouseConfig>(ResourceConstants.WarehouseConfig);
            var configWarehouseEntity = EntityManager.CreateSingleton<WarehouseConfigEntity>();
            EntityManager.AddComponentObject(configWarehouseEntity, new WarehouseConfigData { Config = configWarehouse });
            
            var eventObjectConfig = Resources.Load<EventObjectConfig>(ResourceConstants.CustomerConfig);
            var eventObjectEntity = EntityManager.CreateSingleton<EventObjectConfigEntity>();
            EntityManager.AddComponentObject(eventObjectEntity, new EventObjectConfigData { Config = eventObjectConfig });

            var upgradeBarConfig = Resources.Load<UpgradeBarConfig>(ResourceConstants.UpgradeBarConfig);
            var upgradeBarEntity = EntityManager.CreateSingleton<UpgradeBarConfigEntity>();
            EntityManager.AddComponentObject(upgradeBarEntity, new UpgradeBarConfigData { Config = upgradeBarConfig });
        }

        public GameConfig GetGameConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<ConfigEntity>();
            var config = EntityManager.GetComponentObject<GameConfigData>(configEntity);
            return config.Config;
        }
        public CameraConfig GetCameraConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<CameraConfigEntity>();
            var config = EntityManager.GetComponentObject<CameraConfigData>(configEntity);
            return config.Config;
        }
        
        public BarmanConfig GetBarmanConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<BarmanConfigEntity>();
            var config = EntityManager.GetComponentObject<BarmanConfigData>(configEntity);
            return config.Config;
        }
        
        public CustomerConfig GetCustomerConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<CustomerConfigEntity>();
            var config = EntityManager.GetComponentObject<CustomerConfigs>(configEntity);
            return config.Config;
        }
        
        public ContainerConfig GetContainerConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<ContainerConfigEntity>();
            var config = EntityManager.GetComponentObject<ContainerConfigData>(configEntity);
            return config.Config;
        }
        
        public TableConfig GetTableConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<TableConfigEntity>();
            var config = EntityManager.GetComponentObject<TableConfigData>(configEntity);
            return config.Config;
        }
        
        
        public ProductConfig GetProductConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<ProductConfigEntity>();
            var config = EntityManager.GetComponentObject<ProductConfigData>(configEntity);
            return config.Config;
        }
        
        public UIConfig GetUIConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<UIConfigEntity>();
            var config = EntityManager.GetComponentObject<UIConfigData>(configEntity);
            return config.Config;
        }

        public WarehouseConfig GetWarehouseConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<WarehouseConfigEntity>();
            var config = EntityManager.GetComponentObject<WarehouseConfigData>(configEntity);
            return config.Config;
        }
        
        public EventObjectConfig GetEventObjectConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<EventObjectConfigEntity>();
            var config = EntityManager.GetComponentObject<EventObjectConfigData>(configEntity);
            return config.Config;
        }

        public UpgradeBarConfig GetUpgradeBarConfig()
        {
            var configEntity = SystemAPI.GetSingletonEntity<UpgradeBarConfigEntity>();
            var config = EntityManager.GetComponentObject<UpgradeBarConfigData>(configEntity);
            return config.Config;
        }

        protected override void OnUpdate()
        {
            
        }
    }
}