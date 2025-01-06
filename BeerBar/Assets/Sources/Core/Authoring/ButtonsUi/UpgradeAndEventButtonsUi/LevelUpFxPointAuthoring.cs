using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Authoring.UpgradeAndEventButtonsUi
{
    public class LevelUpFxPointAuthoring : MonoBehaviour
    {
        public LevelUpFxPoints LevelUpFxPoints;
        public class LevelUpFxPointAuthoringBaker : Baker<LevelUpFxPointAuthoring>
        {
            public override void Bake(LevelUpFxPointAuthoring authoring)
            {
                var tablePointsEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var tableBufferPoints = AddBuffer<LevelUpFxPoint>(tablePointsEntity);
                AddComponent<TableLevelUpFxPoint>(tablePointsEntity);
                
                for (var i = 0; i < authoring.LevelUpFxPoints.Table.Length; i++)
                {
                    var transform = authoring.LevelUpFxPoints.Table[i];
                    
                    tableBufferPoints.Add(
                        new LevelUpFxPoint { Position = transform.position, Rotation = transform.rotation });
                }
                
                var spillPointsEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var spillBufferPoints = AddBuffer<LevelUpFxPoint>(spillPointsEntity);
                AddComponent<SpillLevelUpFxPoint>(spillPointsEntity);
                
                for (var i = 0; i < authoring.LevelUpFxPoints.Spill.Length; i++)
                {
                    var transform = authoring.LevelUpFxPoints.Spill[i];
                    
                    spillBufferPoints.Add(
                        new LevelUpFxPoint { Position = transform.position, Rotation = transform.rotation });
                }
                
                var nutsPointsEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var nutsBufferPoints = AddBuffer<LevelUpFxPoint>(nutsPointsEntity);
                AddComponent<NutsLevelUpFxPoint>(nutsPointsEntity);
                
                for (var i = 0; i < authoring.LevelUpFxPoints.Nuts.Length; i++)
                {
                    var transform = authoring.LevelUpFxPoints.Nuts[i];
                    
                    nutsBufferPoints.Add(
                        new LevelUpFxPoint { Position = transform.position, Rotation = transform.rotation });
                }
                
                var fridgePointEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var transformFridgeFx = authoring.LevelUpFxPoints.Fridge;
                var bufferFridgePoint = AddBuffer<LevelUpFxPoint>(fridgePointEntity);
                AddComponent<FridgeLevelUpFxPoint>(fridgePointEntity);
                bufferFridgePoint.Add(
                    new LevelUpFxPoint { Position = transformFridgeFx.position, Rotation = transformFridgeFx.rotation });
               
                var snackPointEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                var transformSnackFx = authoring.LevelUpFxPoints.Snack;
                var bufferSnackPoint = AddBuffer<LevelUpFxPoint>(snackPointEntity);
                AddComponent<SnackLevelUpFxPoint>(snackPointEntity);
                bufferSnackPoint.Add(
                    new LevelUpFxPoint { Position = transformSnackFx.position, Rotation = transformSnackFx.rotation });
            }
        }
    }
    
    public struct LevelUpFxPoint : IBufferElementData
    {
        public float3 Position;
        public quaternion Rotation;
    }
    
    [System.Serializable]
    public class LevelUpFxPoints
    {
        public Transform[] Table;
        public Transform[] Nuts;
        public Transform[] Spill;
        public Transform Fridge;
        public Transform Snack;
    }
    
    public struct TableLevelUpFxPoint : IComponentData { }
    
    public struct FridgeLevelUpFxPoint : IComponentData { }
    
    public struct SnackLevelUpFxPoint : IComponentData { }
    
    public struct SpillLevelUpFxPoint : IComponentData { }
    
    public struct NutsLevelUpFxPoint : IComponentData { }
}