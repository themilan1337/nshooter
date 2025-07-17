using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class CrosshairProceduralManoeuvre : MonoBehaviour
    {
        [Tooltip("How much you want this crosshair to scale up?")]
        public float scalingFactor = 0.2f;
        [Tooltip("How fast you want this crosshair scale to return back down?")]
        public float cooldown = 0.1f;

        [HideInInspector]
        public Vector2 defWidthHeight = Vector2.zero;

        [HideInInspector]
        public bool disableCrosshair;

        RectTransform rectTransform;

        PlayerStates lastPlayerState;
        WeaponState lastWeaponState;

        // Use this for initialization
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            defWidthHeight = rectTransform.sizeDelta;

            lastPlayerState = gc_StateManager.Instance.currentPlayerState;
            lastWeaponState = gc_StateManager.Instance.currentWeaponState;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(disableCrosshair)
            {
                GetComponent<CanvasGroup>().alpha = 0;
                return;
            }

            AnimateCrosshairViaPlayer();

            AnimateCrosshairViaWeapon();

        }

        /// <summary>
        /// Animate Crosshair procedurally based on weapon state
        /// </summary>
        /// <param name="state"></param>
        public void AnimateCrosshairViaWeapon()
        {
            //if weapon is of type melee
            //hide crosshair
            if (gc_AmmoManager.Instance._currentWeapon)
            {
                if (!gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>())
                {
                    GetComponent<CanvasGroup>().alpha = 0;
                    return;
                }

            }
           
            //in ironsight mode, we hide crosshair
            if (ManoeuvreFPSController.Instance.Inputs.ironsightInput || 
                ManoeuvreFPSController.Instance.Inputs.inventoryInput || ManoeuvreFPSController.Instance.gameObject.GetComponent<WeaponHandler>().Weapons.Count < 1)
                GetComponent<CanvasGroup>().alpha = 0;
            else
                GetComponent<CanvasGroup>().alpha = 1;

            //while throwing object, we hide crosshair
            if (gc_AmmoManager.Instance.isThrowing)
                GetComponent<CanvasGroup>().alpha = 0;

            //if the state is not changed
            if (lastWeaponState == gc_StateManager.Instance.currentWeaponState)
                return;
            //else proceed

            //reset last weapon state 
            lastWeaponState = gc_StateManager.Instance.currentWeaponState;

            //checking current state with game controller
            if (gc_StateManager.Instance.currentWeaponState == WeaponState.Firing)
            {
                //inc size
                StopAllCoroutines();
                StartCoroutine(tweenScale(defWidthHeight * scalingFactor, Color.red));

            }
            else if (gc_StateManager.Instance.currentWeaponState == WeaponState.Idle)
                return;

        }

        /// <summary>
        /// Animate Crosshair procedurally based on Player state
        /// </summary>
        public void AnimateCrosshairViaPlayer()
        {
            //if both states are not changed
            if (lastPlayerState == gc_StateManager.Instance.currentPlayerState 
                && lastWeaponState == gc_StateManager.Instance.currentWeaponState)
                return;
            //else proceed

            //if we are not firing
            if (gc_StateManager.Instance.currentWeaponState == WeaponState.Firing)
                return;

            //reset last player state 
            lastPlayerState = gc_StateManager.Instance.currentPlayerState;

            //then only animate crosshair based on
            //player states
            switch (gc_StateManager.Instance.currentPlayerState)
            {
                case PlayerStates.Idle:
                    StopAllCoroutines();
                    StartCoroutine(tweenScale(defWidthHeight , Color.white));
                    break;

                case PlayerStates.Walking:
                    StopAllCoroutines();
                    StartCoroutine(tweenScale(defWidthHeight * scalingFactor * 1.25f , Color.white));
                    break;

                case PlayerStates.Running:
                    StopAllCoroutines();
                    StartCoroutine(tweenScale(defWidthHeight * scalingFactor * 2 , Color.white));
                    break;

                case PlayerStates.Crouching:
                    StopAllCoroutines();
                    StartCoroutine(tweenScale(defWidthHeight * scalingFactor * 1.25f, Color.white));
                    break;
            }


        }

        /// <summary>
        /// Tween cross hair
        /// </summary>
        /// <param name="to"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public IEnumerator tweenScale(Vector2 to, Color c) {

            float elapsedTime = 0;
            
            //change color back to white
            Image[] ch = GetComponentsInChildren<Image>();
            foreach (Image i in ch)
            {
                i.color = c;
            }

            while (elapsedTime < cooldown)
            {
                //slowly gets it back to what it was before
                rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, to, elapsedTime / cooldown);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

        }

        
    }
}