using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(UpgradeBarConfig))]
    public class UpgradeBarConfig : ScriptableObject
    {
        public float DescriptionShowDuration => _descriptionShowDuration;

        public float DescriptionHideDuration => _descriptionHideDuration;

        [SerializeField]
        private float _descriptionShowDuration;

        [SerializeField]
        private float _descriptionHideDuration;
    }

    public class UpgradeBarConfigData : IComponentData
    {
        public UpgradeBarConfig Config;
    }

    public struct UpgradeBarConfigEntity : IComponentData { }
}