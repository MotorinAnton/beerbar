using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class BarmanAnimationConstants
    {
        public static FixedString64Bytes BarmanIdleToDraft = "Barman_idle_to_draft";
        public static FixedString64Bytes BarmanDraftProcess = "barman_draft_process";
        public static FixedString64Bytes BarmanDraftEnd = "Barman_draft_end";
        public static FixedString64Bytes BarmanPickProduct = "Barman_pick_product";
        public static FixedString64Bytes Take = "Take";
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int Give = Animator.StringToHash("Give");
        public static readonly int Draft = Animator.StringToHash("Draft");
        public static readonly int TakeBottle = Animator.StringToHash("TakeBottle");
        public static readonly int TakeSnack = Animator.StringToHash("TakeSnack");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int WalkBottle = Animator.StringToHash("WalkBottle");
        public static readonly int BarmanPutOnTableX2SpeedPlease = Animator.StringToHash("Barman_put_on_table_x2_speed_please");
        public const float ServiceTime = 1f;
        
        public const float MaxPositionYSpillProduct = 0.45f;
        public const float SpillMoveDuration = 5f;
        public const float SpillMoveDelay = 0.2f;
    }
}