using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI {

    public class ZombieAttack: StateMachineBehaviour
    {

        [Header("Normalized Attack Times of the Animation")]
        public float attackStartTime;
        public float attackEndTime;

        ZombieAttackBehaviour _attackBehaviour;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("isAttacking", true);
            _attackBehaviour = animator.GetComponent<uzAIZombieStateManager>().attackBehaviour;
        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.normalizedTime >= attackStartTime && animatorStateInfo.normalizedTime < attackEndTime)
            {
                _attackBehaviour.canAttack = true;
                //Debug.Log("Is Attacking");

            }else
            {
                _attackBehaviour.canAttack = false;

            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("isAttacking", false);
        }

    }
}
