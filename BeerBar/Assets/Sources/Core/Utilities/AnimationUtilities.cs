using Unity.Collections;
using UnityEngine;

namespace Core.Utilities
{
    public static class AnimationUtilities
    {
        public static float AnimationLength(Animator animator, FixedString64Bytes name)
        {
            var ac = animator.runtimeAnimatorController;

            foreach (var animation in ac.animationClips)
            {
                if(animation.name == name)
                {
                    return animation.length;
                }
            }

            return default;
        }
    }
}