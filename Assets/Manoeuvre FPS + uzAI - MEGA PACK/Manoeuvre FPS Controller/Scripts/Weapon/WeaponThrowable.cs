using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class WeaponThrowable : MonoBehaviour
    {
        public string ItemName;
        public GameObject WeaponObject;
        public GameObject ItemRenderer;
        public Animation _Animation;
        public string throwAnimation = "Generic-Throw";
        public float AnimationSpeed = 1f;
        public float AnimationNormalizedTime = 0.5f;

        [HideInInspector]
        public bool isThrowing;

        // Use this for initialization
        void Start()
        {
            if(ItemRenderer)
            {
                foreach (Transform t in ItemRenderer.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }
        }

        public void Throw()
        {
            //if already throwing
            if (isThrowing)
                return; //exit

            //Start Throw Routine
            StartCoroutine(ThrowRoutine());
        }

        /// <summary>
        /// Throws the Item!
        /// </summary>
        IEnumerator ThrowRoutine()
        {
            //enable renderer
            if(ItemRenderer)
                ItemRenderer.SetActive(true);

            int id = GetComponentInParent<WeaponHandler>().GetWeaponID(gc_AmmoManager.Instance._currentWeapon);

            //unequip current weapon
            if (gc_AmmoManager.Instance._currentWeapon)
            {
                GetComponentInParent<WeaponHandler>().UnequipCurrentWeapon(id);
            }

            //set is throwing flag
            isThrowing = true;
            gc_AmmoManager.Instance.isThrowing = true;

            //set speed
            _Animation[throwAnimation].speed = AnimationSpeed;

            //play throw animation
            _Animation.Play(throwAnimation);

            //wait for animation to finish
            yield return new WaitForSeconds(_Animation.GetClip(throwAnimation).length);

            //set is throwing flag
            isThrowing = false;
            gc_AmmoManager.Instance.isThrowing = false;

            //equip current weapon back
            if (gc_AmmoManager.Instance._currentWeapon)
            {
                GetComponentInParent<WeaponHandler>().EquipCurrentWeapon(id);
            }

            //disable game object
            gameObject.SetActive(false);

        }

    }
}