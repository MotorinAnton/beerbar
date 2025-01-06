using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Cameras
{
    public struct MainCamera : IComponentData { }
    
    public class CameraView : IComponentData
    {
        public Camera Value;
    }
    
    public class SpawnCamera : IComponentData
    {
        public Camera CameraPrefab;
        public SpawnPointCamera Point;
    }
}