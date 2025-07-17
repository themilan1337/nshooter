using UnityEngine.AI;
using UnityEngine;

namespace uzAI
{
    public class ZombieTurnOnSpot : StateMachineBehaviour
    {

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger("ZombieTurn", 0);
            animator.SetBool("ZombieIsTurning", true);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("ZombieIsTurning", false);
        }


    }
}