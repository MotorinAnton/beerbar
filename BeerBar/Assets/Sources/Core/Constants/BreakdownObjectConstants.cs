using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class BreakdownObjectConstants
    {
        public static FixedString64Bytes ElectricalDoorOpen = "Electrical_door_open";
        public static FixedString64Bytes ElectricalDoorClose = "Electrical_door_close";
        
        public static readonly int PipeLeak = Shader.PropertyToID("_Progress");
        
        public const float IntensityEndValue = 4f;
        public const float IntensityTweenDuration = 0.08f;
        public const int IntensityTweenLoops = 15;
        public const float IntensityTweenToNormalDuration = 0.2f;
        public const int IntensityTweenToNormalLoops = 10;
        
        public const float FlowTime = 20f;
        
        public const float MovementArrowTubeOffset = 0.3f;
        public const float MovementArrowElectricityOffset = 0.2f;
        public const float MovementArrowTableOffsetY = 1.5f;
        
        public const float ElectricityFlashDuration = 0.2f;
        public const int ElectricityFlashLoop = 10;
        
        
    }
}