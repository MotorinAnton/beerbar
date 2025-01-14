using Core.Authoring.MovementArrows;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(GameConfig))]
    public sealed class GameConfig : ScriptableObject
    {
        public CameraConfig CameraConfig;
        public BarmanConfig BarmanConfig;
        public CustomerConfig CustomerConfig;
        public ProductKeeperConfig ProductKeeperConfig;
        public RepairmanConfig RepairmanConfig;
        public CleanerConfig CleanerConfig;
        public ContainerConfig ContainerConfig;
        public ProductConfig ProductConfig;
        public UIConfig UIConfig;
        public TableConfig TableConfig;
        public EventObjectConfig EventObjectConfig;
        public GameAudioConfig AudioConfig;
        public UpConfig UpConfig;
        public RandomEventConfig RandomEventConfig;
        public Material SelectMaterial;
        public Material[] BreakBottleSelectMaterial;
        public Material[] TubeSpraySelectMaterial;
        public MovementArrowAuthoring ClearArrow;
        public MovementArrowAuthoring RepairArrow;
    }

    public class GameConfigData : IComponentData
    {
        public GameConfig Config;
    }
    
    public struct ConfigEntity : IComponentData { }
}