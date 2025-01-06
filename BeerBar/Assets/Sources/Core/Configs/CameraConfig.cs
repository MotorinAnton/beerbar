using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(CameraConfig))]
    public sealed class CameraConfig : ScriptableObject
    {
        public Camera CameraPrafab;
    }
    public class CameraConfigData : IComponentData
    {
        public CameraConfig Config;
    }
    public struct CameraConfigEntity : IComponentData { }
}