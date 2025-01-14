using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Cameras.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class SpawnCameraSystem : SystemBase
    {
        private EntityQuery _mainCameraQuery;
        protected override void OnCreate()
        {
            using var mainCameraBuilder = new EntityQueryBuilder(Allocator.Temp);
            _mainCameraQuery = mainCameraBuilder.WithAll<MainCamera>().Build(this);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCamera>().ForEach((Entity entity, in SpawnCamera spawnCamera) =>
            {
                if (!_mainCameraQuery.IsEmpty)
                {
                    EntityManager.DestroyEntity(entity);
                    return;
                }
                
                SpawnCamera(entity, spawnCamera);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void SpawnCamera(Entity entity, in SpawnCamera spawnCamera)
        {
            var startCameraArray = Object.FindObjectsOfType<Camera>();
            
            foreach (var cam in startCameraArray)
            {
                Object.Destroy(cam.gameObject);
            }
            
            var camera = EntityManager.CreateSingleton<MainCamera>();
            EntityManager.SetName(camera, EntityConstants.MainCameraEntityName);
            var cameraView = Object.Instantiate(spawnCamera.CameraPrefab ,
                spawnCamera.Point.Position,spawnCamera.Point.Rotation);
            Object.DontDestroyOnLoad(cameraView);
            
            EntityManager.AddComponentObject(camera, new CameraView { Value = cameraView });
            EntityManager.DestroyEntity(entity);
        }
    }
}