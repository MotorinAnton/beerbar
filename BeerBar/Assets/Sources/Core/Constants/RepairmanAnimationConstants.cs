using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class RepairmanAnimationConstants
    {
        public static FixedString64Bytes WorkerWaterRepair = "Worker_water_repair";
        public static FixedString64Bytes RepairElectric = "repair_electric";
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int TubeRepair = Animator.StringToHash("TubeRepair");
        public static readonly int ElectricityRepair = Animator.StringToHash("ElectricityRepair");
        public const float ProfitOffsetY = 2f;
    }
}