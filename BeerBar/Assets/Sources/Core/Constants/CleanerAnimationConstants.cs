using Unity.Collections;
using UnityEngine;

namespace Core.Constants
{
    public static class CleanerAnimationConstants
    {
        public static FixedString64Bytes TableWash = "Barman_table_wash";
        public static FixedString64Bytes MopAnimation = "Barman_mop_animation";
        public static FixedString64Bytes BarmanDraftEnd = "Barman_draft_end";
        public static FixedString64Bytes BarmanPickProduct = "Barman_pick_product";
        public static FixedString64Bytes WalkSpeed = "WalkSpeed";
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int WalkMop = Animator.StringToHash("WalkMop");
        public static readonly int Mop = Animator.StringToHash("Mop");
        public static readonly int WashTable = Animator.StringToHash("WashTable");
        public static readonly int BarmanPutOnTableX2SpeedPlease =
            Animator.StringToHash("Barman_put_on_table_x2_speed_please");
        public const float ServiceTime = 1f;
        public const int LevelUpRatingValue = 7;
    }
}