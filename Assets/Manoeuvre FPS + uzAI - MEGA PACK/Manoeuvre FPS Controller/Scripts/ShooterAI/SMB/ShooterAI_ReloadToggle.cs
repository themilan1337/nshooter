using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class ShooterAI_ReloadToggle : StateMachineBehaviour
    {
        //cache current shooter state
        ShooterAIStates cacheState;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

            cacheState = animator.GetComponent<ShooterAIStateManager>().currentShooterState;
            animator.GetComponent<ShooterAIStateManager>().currentShooterState = ShooterAIStates.Reload;

            animator.SetBool("isReloading", true);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //refill ammo
            if (animator.GetComponent<ShooterAIStateManager>())
            {
                animator.GetComponent<ShooterAIStateManager>().WeaponBehaviour.RefillAmmo();

                animator.GetComponent<ShooterAIStateManager>().currentShooterState = cacheState;
            }

            animator.SetBool("isReloading", false);
        }

    }
}