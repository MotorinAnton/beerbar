using Core.Configs;
using Core.Configs.Systems;
using Core.Constants;
using Core.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using InputSystem = Core.Inputs.InputSystem;

namespace Core.Utilities
{
    public static class EntityUtilities
    {
        public static GameConfig GetGameConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetGameConfig();
        }

        public static CameraConfig GetCameraConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetCameraConfig();
        }

        public static BarmanConfig GetBarmanConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetBarmanConfig();
        }

        public static CustomerConfig GetCustomerConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetCustomerConfig();
        }

        public static ContainerConfig GetContainerConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetContainerConfig();
        }
        
        public static TableConfig GetTableConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetTableConfig();
        }

        public static ProductConfig GetProductConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetProductConfig();
        }

        public static UIConfig GetUIConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld.EntityManager.World
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetUIConfig();
        }

        public static WarehouseConfig GetWarehouseConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetWarehouseConfig();
        }
        
        public static EventObjectConfig GetEventObjectConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetEventObjectConfig();
        }

        public static UpgradeBarConfig GetUpgradeBarConfig()
        {
            var system = World.DefaultGameObjectInjectionWorld
                .GetExistingSystemManaged<ConfigSystem>();
            return system.GetUpgradeBarConfig();
        }

        public static PlayerActions GetInputActions()
        {
            var system = World.DefaultGameObjectInjectionWorld
                .GetExistingSystemManaged<InputSystem>();
            return system.GetActions();
        }

        public static bool CheckItInPosition(Transform transform, Vector3 position)
        {
            var distanceToPosition = Vector3.Distance(transform.position, position);
            return distanceToPosition <= EntityConstants.StoppingDistance;
        }
        

        public static void AddOneFrameComponent<T>(Entity entity)
        {
            GetOneFrameComponentSystem().AddOneFrameComponent<T>(entity);
        }

        public static void AddOneFrameComponent(Entity entity, ComponentType componentType)
        {
            GetOneFrameComponentSystem().AddOneFrameComponent(entity, componentType);
        }

        public static void AddOneFrameComponent<T>(NativeArray<Entity> entities)
        {
            GetOneFrameComponentSystem().AddOneFrameComponent<T>(entities);
        }

        public static void AddOneFrameComponent(NativeArray<Entity> entities, ComponentType componentType)
        {
            GetOneFrameComponentSystem().AddOneFrameComponent(entities, componentType);
        }

        public static void AddOneFrameComponentData<T>(Entity entity, T componentData)
            where T : unmanaged, IComponentData
        {
            GetOneFrameComponentSystem().AddOneFrameComponentData(entity, componentData);
        }

        public static OneFrameComponentSystem GetOneFrameComponentSystem()
        {
            return World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<OneFrameComponentSystem>();
        }
    }
}