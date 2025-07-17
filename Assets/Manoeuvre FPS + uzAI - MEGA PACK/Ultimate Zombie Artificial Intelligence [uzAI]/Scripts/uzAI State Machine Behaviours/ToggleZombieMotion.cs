using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class ToggleZombieMotion : StateMachineBehaviour
    {
        [Tooltip("If true, zombie will stop while being hit and continues to move towards target once hit animation ends.")]
        public bool disableMotionWhileHit = true;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //exit if disable motion is false
            if (!disableMotionWhileHit)
                return;

            animator.SetBool("DisableMotion", true);
            
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("DisableMotion", false);
        }


    }
}