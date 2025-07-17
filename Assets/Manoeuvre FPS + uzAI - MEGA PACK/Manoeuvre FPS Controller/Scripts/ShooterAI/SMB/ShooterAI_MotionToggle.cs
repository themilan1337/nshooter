using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class ShooterAI_MotionToggle : StateMachineBehaviour
    {
        [Tooltip("If true, AI will stop while being hit and continues to move towards target once hit animation ends.")]
        public bool disableMotionWhileHit = true;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //exit if disable motion is false
            if (!disableMotionWhileHit)
                return;

            animator.SetBool("DisableMotion", true);


            //set hit state
            if(animator.GetComponent<ShooterAIStateManager>())
                animator.GetComponent<ShooterAIStateManager>().setHandsIK = false;

        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("DisableMotion", false);

            ////end hit state
            //if(animator.GetComponent<ShooterAIStateManager>())
            //    animator.GetComponent<ShooterAIStateManager>().setHandsIK = true;

        }


    }
}