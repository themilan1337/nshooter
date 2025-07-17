using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class gc_StateManager : MonoBehaviour
    {
        public PlayerStates currentPlayerState;
        public WeaponState currentWeaponState;

        public static gc_StateManager Instance;

        GameObject Player;

        [HideInInspector]
        public GameObject sensorCollider;
        [HideInInspector]
        [Range(0f, 30f)]
        public float sensorRadius = 5f;

        [HideInInspector]
        public float radiusWhileRunning;
        [HideInInspector]
        public float radiusWhileShooting;

        // Use this for initialization
        void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player");

            Instance = this;

            AddTriggerToPlayer();
        }

        /// <summary>
        /// This trigger will tell the surrounding enemies the location of the Player
        /// </summary>
        void AddTriggerToPlayer()
        {
            //Add trigger 
            sensorCollider = new GameObject();
            sensorCollider.name = "Awareness Sensor";
            sensorCollider.AddComponent<SphereCollider>().radius = 0;
            sensorCollider.GetComponent<SphereCollider>().isTrigger = true;
            
            //initialize
            sensorCollider.layer = 13;
            sensorCollider.tag = "AwarenessTrigger"; 
            sensorCollider.transform.SetParent(Player.transform);
            sensorCollider.transform.localPosition = Vector3.zero;
            sensorCollider.transform.localRotation = Quaternion.identity;

        }

        public void SetTriggerRadius(float radius)
        {
            sensorRadius = radius;
            sensorCollider.GetComponent<SphereCollider>().radius = sensorRadius;

        }

        private void FixedUpdate()
        {
            //forcing radius to 0
            SetTriggerRadius(0);

            //if we are running and not firing
            if (currentPlayerState == PlayerStates.Running && currentWeaponState != WeaponState.Firing)
            {
                SetTriggerRadius(radiusWhileRunning);
            }
            //if we are firing
            else if (!gc_AmmoManager.Instance.isReloading && !gc_AmmoManager.Instance.isEquipping && gc_AmmoManager.Instance.currentWeaponCurrentAmmo != 0)
            {
                if (currentWeaponState == WeaponState.Firing)
                    if(!Inventory.Instance.inventoryIsOpen)
                        SetTriggerRadius(radiusWhileShooting);
            }
               
        }

    }
}