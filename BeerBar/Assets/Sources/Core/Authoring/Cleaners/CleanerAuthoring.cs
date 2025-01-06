using Core.Authoring.Characters;
using Core.Authoring.Points;
using Core.Configs;
using Unity.Entities;
using Unity.Mathematics;

namespace Core.Authoring.Cleaners
{
    public struct Cleaner : IComponentData { }
    public class CleanerView : IComponentData
    {
        public CharacterAuthoring Value;
        
        public void EnableMop() => Value.PivotHand[1].gameObject.SetActive(true);
        
        public void DisableMop() => Value.PivotHand[1].gameObject.SetActive(false);
    }
    
    public class SpawnCleaner : IComponentData
    {
        public CleanerConfig CleanerData;
        public SpawnPointCleaner Point;
    }
    
    public class CleanerDataComponent : IComponentData
    {
        public CleanerConfig Value;
    }

    public struct MoveExitCleaner : IComponentData { }

    public struct CleaningCompletedCleaner : IComponentData { }
    
    public struct FreeCleaner : IComponentData { }
    
    public struct CleaningTableCleaner : IComponentData { }
    public struct CleaningBreakBottleCleaner : IComponentData { }
    
    public struct CleanTableCleaner : IComponentData
    {
        public Point ClearTablePoint;
        public Entity Table;
    }
    public struct CleanBreakBottleCleaner : IComponentData
    {
        public float3 BreakBottlePosition;
    }
    
    public struct MopInHands : IComponentData { }
}