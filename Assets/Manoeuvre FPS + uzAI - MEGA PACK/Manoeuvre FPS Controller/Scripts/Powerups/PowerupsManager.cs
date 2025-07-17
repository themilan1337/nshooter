using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public enum PowerupType { Healthkit, Invincibility, Speedboost, DamageMultiplier, InfiniteAmmo }

    public class PowerupsManager : MonoBehaviour
    {
        [HideInInspector]
        public GameObject PowerupsHUD;
        [Header("-- Assign Prefab --")]
        public GameObject PowerupsHUDPrefab;

        [Header("-- Assign Respective Icons --")]
        public HealthKit _HealthKit;
        public Invincibility _Invincibility;
        public SpeedBoost _SpeedBoost;
        public DamageMultiplier _DamageMultiplier;
        public InfiniteAmmo _InfiniteAmmo;

        public static PowerupsManager Instance;

        private void Awake()
        {
            Instance = this;

            PowerupsHUD = GameObject.Find("PowerupsHUD");
        }

        // Use this for initialization
        void Start()
        {

        }

        /// <summary>
        /// We take the type and the Instance of the pickup from where exactly we have Picked Up the Powerup
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pickupInstance"></param>
        public void InitializePowerup(PowerupType type, Powerups_Pickup pickupInstance)
        {
            switch (type)
            {
                case PowerupType.Healthkit:
                
                    //init the health kit with the picked up _healthkit properties
                    _HealthKit.Initialize(pickupInstance.healthAmount);
                    break;

                case PowerupType.Invincibility:
                    
                    //init the Invincibility with the picked up _Invincibility properties
                    _Invincibility.Initialize(pickupInstance.InvincibilityDuration);
                    break;

                case PowerupType.Speedboost:

                    //init the Speedboost with the picked up _Speedboost properties
                    _SpeedBoost.Initialize(pickupInstance.SpeedBoostAmount, pickupInstance.SpeedBoostDuration);
                    break;

                case PowerupType.DamageMultiplier:

                    //init the DamageMultiplier with the picked up _DamageMultiplier properties
                    _DamageMultiplier.Initialize(pickupInstance.DamageMultiplierAmount, pickupInstance.DamageMultiplierDuration);
                    break;

                case PowerupType.InfiniteAmmo:

                    //init the InfiniteAmmo with the picked up _InfiniteAmmo properties
                    _InfiniteAmmo.Initialize(pickupInstance.InfiniteAmmoDuration);
                    break;

            }
        }

        public void InstantiatePowerupHUD(float duration, Sprite icon, PowerupType type)
        {
            //Instantiate Powerup HUD
            GameObject p_HUD = Instantiate(PowerupsHUDPrefab) as GameObject;

            p_HUD.transform.SetParent(PowerupsHUD.transform);
            p_HUD.transform.localEulerAngles = Vector3.zero;
            p_HUD.transform.localPosition = Vector3.zero;
            p_HUD.transform.localScale = Vector3.one;

            //Now Initialize it
            p_HUD.GetComponent<Powerups_HUD>().Initialize(duration, icon, type);
        }
    }

    [System.Serializable]
    public class HealthKit
    {
       [HideInInspector]
        [Tooltip("How much health to be given?")]
        public int healthAmount = 50;

        [Tooltip("Icon to be displayed in Inventory and HUD!")]
        public Sprite icon;

       [HideInInspector]
        public int powerupCount = 0;

       [HideInInspector]
        //cache the slot generated
        public ItemIdentifier_Powerups _healthkitSlot;

        public void Initialize(int _healthAmount)
        {
            healthAmount = _healthAmount;

            powerupCount++;

            //Add in the inventory only for the first time
            if (powerupCount == 1)
                _healthkitSlot = Inventory.Instance.AddInventorySlot_Powerups(PowerupType.Healthkit);
            else
                //simply just update the UI if already added
                _healthkitSlot.powerupsCountText.text = powerupCount.ToString();

            Debug.Log("Picked up HealthKit");
        }

        /// <summary>
        /// Consumes the picked up item
        /// </summary>
        public void Consume()
        {
            //Health kit consumption logic
            ManoeuvreFPSController _controller = GameObject.FindObjectOfType<ManoeuvreFPSController>();

            //add health
            _controller.HealthkitPickup(healthAmount);

            //deduct one power from Inventory
            powerupCount--;

            //update Slot UI
            _healthkitSlot.powerupsCountText.text = powerupCount.ToString();

            //Destroy if it was last pickup
            if (powerupCount == 0)
                GameObject.Destroy(_healthkitSlot.gameObject);

            Debug.Log("Consumed Healthkit and added : " + healthAmount + " health points");

        }

    }

    [System.Serializable]
    public class Invincibility
    {
       [HideInInspector]
        [Tooltip("How long this power will stay active?")]
        public float InvincibilityDuration = 10f;

        [Tooltip("Icon to be displayed in Inventory and HUD!")]
        public Sprite icon;

        [Tooltip("As soon as Player consumes this power, this bool will be activated and will remain so" +
                " for the duration specified!")]
        public bool isActive;

       [HideInInspector]
        public int powerupCount = 0;

       [HideInInspector]
        //cache the slot generated
        public ItemIdentifier_Powerups _InvincibilitySlot;

        public void Initialize(float _InvincibilityDuration)
        {

            InvincibilityDuration = _InvincibilityDuration;

            powerupCount++;

            //Add in the inventory only for the first time
            if (powerupCount == 1)
                _InvincibilitySlot = Inventory.Instance.AddInventorySlot_Powerups(PowerupType.Invincibility);
            else
                //simply just update the UI if already added
                _InvincibilitySlot.powerupsCountText.text = powerupCount.ToString();

            Debug.Log("Picked up Invincibility");

        }

        /// <summary>
        /// Consumes the picked up item
        /// </summary>
        public void Consume()
        {
            //Init HUD
            PowerupsManager.Instance.InstantiatePowerupHUD(InvincibilityDuration, icon, PowerupType.Invincibility);

            //Invincibility consumption logic
            isActive = true;

            //deduct one power from Inventory
            powerupCount--;

            //update Slot UI
            _InvincibilitySlot.powerupsCountText.text = powerupCount.ToString();

            //Destroy if it was last pickup
            if (powerupCount == 0)
                GameObject.Destroy(_InvincibilitySlot.gameObject);
        }
    }

    [System.Serializable]
    public class SpeedBoost
    {
       [HideInInspector]
        [Tooltip("This Amount will be added in current speed!")]
        public float SpeedBoostAmount = 5f;

       [HideInInspector]
        [Tooltip("How long this power will stay active?")]
        public float SpeedBoostDuration = 10f;

        [Tooltip("Icon to be displayed in Inventory and HUD!")]
        public Sprite icon;

        [Tooltip("As soon as Player consumes this power, this bool will be activated and will remain so" +
                " for the duration specified!")]
        public bool isActive;

       [HideInInspector]
        public int powerupCount = 0;

       [HideInInspector]
        //cache the slot generated
        public ItemIdentifier_Powerups _SpeedBoostSlot;

        public void Initialize(float _SpeedBoostAmount, float _SpeedBoostDuration)
        {
            SpeedBoostAmount = _SpeedBoostAmount;
            SpeedBoostDuration = _SpeedBoostDuration;

            powerupCount++;

            //Add in the inventory only for the first time
            if (powerupCount == 1)
                _SpeedBoostSlot = Inventory.Instance.AddInventorySlot_Powerups(PowerupType.Speedboost);
            else
                //simply just update the UI if already added
                _SpeedBoostSlot.powerupsCountText.text = powerupCount.ToString();


            Debug.Log("Picked up SpeedBoost");

        }

        /// <summary>
        /// Consumes the picked up item
        /// </summary>
        public void Consume()
        {
            //Init HUD
            PowerupsManager.Instance.InstantiatePowerupHUD(SpeedBoostDuration, icon, PowerupType.Speedboost);

            //SpeedBoost consumption logic
            isActive = true;

            //deduct one power from Inventory
            powerupCount--;

            //update Slot UI
            _SpeedBoostSlot.powerupsCountText.text = powerupCount.ToString();

            //Destroy if it was last pickup
            if (powerupCount == 0)
                GameObject.Destroy(_SpeedBoostSlot.gameObject);
        }
    }

    [System.Serializable]
    public class DamageMultiplier
    {
       [HideInInspector]
        [Tooltip("This Amount will be Multiplied in current Damage!")]
        public int DamageMultiplierAmount = 2;

       [HideInInspector]
        [Tooltip("How long this power will stay active?")]
        public float DamageMultiplierDuration = 10f;

        [Tooltip("Icon to be displayed in Inventory and HUD!")]
        public Sprite icon;

        [Tooltip("As soon as Player consumes this power, this bool will be activated and will remain so" +
                " for the duration specified!")]
        public bool isActive;

       [HideInInspector]
        public int powerupCount = 0;

       [HideInInspector]
        //cache the slot generated
        public ItemIdentifier_Powerups _DamageMultiplierSlot;

        public void Initialize(int _DamageMultiplierAmount, float _DamageMultiplierDuration)
        {
            DamageMultiplierAmount = _DamageMultiplierAmount;
            DamageMultiplierDuration = _DamageMultiplierDuration;

            powerupCount++;

            //Add in the inventory only for the first time
            if (powerupCount == 1)
                _DamageMultiplierSlot = Inventory.Instance.AddInventorySlot_Powerups(PowerupType.DamageMultiplier);
            else
                //simply just update the UI if already added
                _DamageMultiplierSlot.powerupsCountText.text = powerupCount.ToString();


            Debug.Log("Picked up DamageMultiplier");

        }

        /// <summary>
        /// Consumes the picked up item
        /// </summary>
        public void Consume()
        {
            //Init HUD
            PowerupsManager.Instance.InstantiatePowerupHUD(DamageMultiplierDuration, icon, PowerupType.DamageMultiplier);

            // DamageMultiplier consumption logic
            isActive = true;

            //deduct one power from Inventory
            powerupCount--;

            //update Slot UI
            _DamageMultiplierSlot.powerupsCountText.text = powerupCount.ToString();

            //Destroy if it was last pickup
            if (powerupCount == 0)
                GameObject.Destroy(_DamageMultiplierSlot.gameObject);
        }

    }

    [System.Serializable]
    public class InfiniteAmmo
    {
       [HideInInspector]
        [Tooltip("How long this power will stay active?")]
        public float InfiniteAmmoDuration = 10;

        [Tooltip("Icon to be displayed in Inventory and HUD!")]
        public Sprite icon;

        [Tooltip("As soon as Player consumes this power, this bool will be activated and will remain so" +
                " for the duration specified!")]
        public bool isActive;

       [HideInInspector]
        public int powerupCount = 0;

       [HideInInspector]
        //cache the slot generated
        public ItemIdentifier_Powerups _InfiniteAmmoSlot;

        public void Initialize(float _InfiniteAmmoDuration)
        {
            InfiniteAmmoDuration = _InfiniteAmmoDuration;

            powerupCount++;

            //Add in the inventory only for the first time
            if (powerupCount == 1)
                _InfiniteAmmoSlot = Inventory.Instance.AddInventorySlot_Powerups(PowerupType.InfiniteAmmo);
            else
                //simply just update the UI if already added
                _InfiniteAmmoSlot.powerupsCountText.text = powerupCount.ToString();

            Debug.Log("Picked up InfiniteAmmo");

        }

        /// <summary>
        /// Consumes the picked up item
        /// </summary>
        public void Consume()
        {
            //Init HUD
            PowerupsManager.Instance.InstantiatePowerupHUD(InfiniteAmmoDuration, icon, PowerupType.InfiniteAmmo);

            //InfiniteAmmo consumption logic
            isActive = true;

            //deduct one power from Inventory
            powerupCount--;

            //update Slot UI
            _InfiniteAmmoSlot.powerupsCountText.text = powerupCount.ToString();

            //Destroy if it was last pickup
            if (powerupCount == 0)
                GameObject.Destroy(_InfiniteAmmoSlot.gameObject);

        }
    }

}