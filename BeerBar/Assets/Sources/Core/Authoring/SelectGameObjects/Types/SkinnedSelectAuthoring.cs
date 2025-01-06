using UnityEngine;

namespace Core.Authoring.SelectGameObjects.Types
{
    public class SkinnedSelectAuthoring : SelectObjectAuthoring
    {
        [SerializeField] private SkinnedMeshRenderer[] _skinned;
        
        public SkinnedMeshRenderer[] Skinned => _skinned;
        
        public override SelectObjectType SelectType => SelectObjectType.Skinned;
    }
}