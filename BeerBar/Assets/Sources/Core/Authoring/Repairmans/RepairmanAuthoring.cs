using System.Collections.Generic;
using Core.Authoring.Characters;
using Core.Configs;
using Unity.Entities;

namespace Core.Authoring.Repairmans
{
    public struct Repairman : IComponentData { }
    public class RepairmanView : IComponentData
    {
        public CharacterAuthoring Value;
        public float Speed;
    }
    
    public class SpawnRepairman : IComponentData
    {
        public RepairmanConfig RepairmanData;
        public SpawnPointRepairman Point;
        public int Level;
    }
    
    public class RepairmanDataComponent : IComponentData
    {
        public RepairmanConfig Value;
    }
    
    public struct MoveRepairsRepairman : IComponentData { }
    
    public struct MoveExitRepairman : IComponentData { }
    
    public struct RepairsRepairman : IComponentData { }
    
    public struct RepairsCompletedRepairman : IComponentData { }
    
    public struct FreeRepairman : IComponentData { }
    
    public class OrderRepairman : IComponentData 
    {
        public List<Entity> RepairObjectList;
    }
}