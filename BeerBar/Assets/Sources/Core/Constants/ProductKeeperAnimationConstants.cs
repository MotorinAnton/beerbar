using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class ProductKeeperAnimationConstants
    {
        public static FixedString64Bytes BoxAnim = "Kladovshik_box_anim";
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int WalkWithBox = Animator.StringToHash("WalkWithBox");
        public static readonly int BoxUpload = Animator.StringToHash("BoxUpload");

        public const float MaxSpillYPosition = 0.45f;
        public const float DurationMoveSpillY = 5f;
        public const float DelayMoveSpillY = 0.2f;
    }
}