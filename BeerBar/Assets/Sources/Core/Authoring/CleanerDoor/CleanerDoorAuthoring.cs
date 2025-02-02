using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.CleanerDoor
{
    public class CleanerDoorAuthoring : EntityBehaviour 
    { 
        [SerializeField] private Animation _doorAnimation;
        
        public Animation DoorAnimation => _doorAnimation;
    }
    
    public struct DoorCleaner : IComponentData { }
    
    public struct CleanerDoorIsOpen : IComponentData { }
    
    public class CleanerDoorView : IComponentData
    {
        public CleanerDoorAuthoring Value;
    }
    
    public class SpawnCleanerDoor : IComponentData
    {
        public CleanerDoorAuthoring DoorPrefab;
    }
}