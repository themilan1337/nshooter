using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class gc_AmmoManager : MonoBehaviour
    {
        Shoot currentWeaponShooterProperties;

        [Header("-- UI Weapon HUD References --")]
        [HideInInspector]
        public Image currentWeaponImage;
        [HideInInspector]
        public Text currentWeaponCurrentAmmoText;
        [HideInInspector]
        public Text currentWeaponTotalAmmoText;
        [HideInInspector]
        public CanvasGroup WeaponHUD;

        [Header("-- Ammo --")]
        [HideInInspector]
        public Transform _currentWeapon;
        [HideInInspector]
        public int currentWeaponCurrentAmmo;
        [HideInInspector]
        public int currentWeaponCapacity;
        [HideInInspector]
        public int currentWeaponTotalAmmo;


        //static reference
        public static gc_AmmoManager Instance;

        // Global Flags for current weapon state
        [HideInInspector]
        public bool isReloading;
        [HideInInspector]
        public bool isEquipping;
        [HideInInspector]
        public bool isThrowing;


        // Use this for initialization
        void Awake()
        {
            Instance = this;

            //Assign UI References as well
            currentWeaponImage = GameObject.Find("CurrentWeaponImage").GetComponent<Image>();
            currentWeaponCurrentAmmoText = GameObject.Find("currentWeaponCurrentAmmoText").GetComponent<Text>();
            currentWeaponTotalAmmoText = GameObject.Find("currentWeaponTotalAmmoText").GetComponent<Text>();
            WeaponHUD = GameObject.Find("WeaponHUD").GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// This is a public method
        /// which will be called whenever the weapon is initialized from Weapon Handler!
        /// </summary>
        /// <param name="shooterProperties">Current equipped weapon's shooter properties</param>
        public void Initialize(Transform currentWeapon, Sprite UIIcon)
        {
            //cache color
            Color c1 = currentWeaponCurrentAmmoText.color;
            Color c2 = currentWeaponTotalAmmoText.color;

            //set current Weapon
            _currentWeapon = currentWeapon.transform;

            //Set Weapon Image separately
            currentWeaponImage.sprite = UIIcon;

            //HUD Alpha back to 1
            WeaponHUD.alpha = 1;

            //we only set if current weapon is a Shooter
            if(currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
            {
                //retrieve current weapon's properties
                currentWeaponShooterProperties = currentWeapon.GetComponent<WeaponShooter>().shooterProperties;

                //set our values
                //1 --> First we get the total capacity i.e total ammo we have
                currentWeaponTotalAmmo = currentWeaponShooterProperties.totalAmmo;

                //2 --> Cache the max Ammo amount we will give to this weapon 
                currentWeaponCapacity = currentWeaponShooterProperties.ammoCapacity;

                //3 --> Finally calculate the current ammo this weapon has 
                currentWeaponCurrentAmmo = currentWeaponShooterProperties.currentAmmo;

                //Debug.Log("currentWeaponTotalAmmo : " + currentWeaponTotalAmmo);
                //Debug.Log("currentWeaponCapacity : " + currentWeaponCapacity);
                //Debug.Log("currentWeaponCurrentAmmo : " + currentWeaponCurrentAmmo);

                //4 --> Set UI
                SetAmmoUI();

                //5 --> Make sure alpha is back to 1
                c1.a = 1;
                c2.a = 1;
                currentWeaponCurrentAmmoText.color = c1;
                currentWeaponTotalAmmoText.color = c2;

            }
            else if(currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
            {
                //make sure to hide the Ammo Text
                c1.a = 0;
                c2.a = 0;
                currentWeaponCurrentAmmoText.color = c1;
                currentWeaponTotalAmmoText.color = c2;
            }

        }

        private void Update()
        {
            //if no weapon
            if (!_currentWeapon)
                return;

            //if equip coroutine is looping
            if (isEquipping)
                return;

            //if Inventory is open
            if (Inventory.Instance.inventoryIsOpen)
                return;

            // no more reloading now!
            if (currentWeaponTotalAmmo == 0)
                return;
            
            //if there's an input to reload
            if(ManoeuvreFPSController.Instance.Inputs.reloadInput && 
                !isReloading &&
                currentWeaponCurrentAmmo < currentWeaponCapacity &&
                _currentWeapon.GetComponent<WeaponShooter>())
                ReloadCurrentWeapon();

            //if we try to fire but there's no ammo, we reload
            if(gc_StateManager.Instance.currentWeaponState == WeaponState.Firing && currentWeaponCurrentAmmo == 0)
            {
                //reset state
                gc_StateManager.Instance.currentWeaponState = WeaponState.Reloading;

                //make sure we are not reloading already
                if (!isReloading)
                    ReloadCurrentWeapon();
            }

        }

        public void ReloadCurrentWeapon()
        {
            //disable reload input flag
            ManoeuvreFPSController.Instance.Inputs.reloadInput = false;

            //if in between equip-unequip sway
            if (isEquipping)
                return;

            //Play reload sound
            if (_currentWeapon.GetComponent<WeaponShooter>().weaponSoundProperties.reloadSound)
                _currentWeapon.GetComponent<WeaponShooter>().weaponSoundProperties.PlayAudio(_currentWeapon.GetComponent<WeaponShooter>().weaponSoundProperties.reloadSound, false, _currentWeapon.transform.position);

            //reload ammo
            ReloadAmmo();

            //RUN A COROUTINE FOR RELOAD
            _currentWeapon.GetComponent<WeaponProceduralManoeuvre>().StartProceduralReloadManoeuvre(_currentWeapon);

        }

        int GetCurrentAmmo()
        {
            int retVal = 0;

            if (currentWeaponCurrentAmmo == currentWeaponCapacity)
                retVal = currentWeaponCurrentAmmo;
            //if there's a single bullet less in the total capacity and total ammo is greater then capacity
            else if (currentWeaponCurrentAmmo < currentWeaponCapacity && currentWeaponCurrentAmmo > 0 && currentWeaponTotalAmmo >= currentWeaponCapacity)
            {
                currentWeaponTotalAmmo -= (currentWeaponCapacity - currentWeaponCurrentAmmo);

                //update shooter as well
                currentWeaponShooterProperties.totalAmmo = currentWeaponTotalAmmo;

                retVal = currentWeaponCapacity;
            }
            //if the total ammo is greater then the capacity 
            //and our current ammo reaches 0
            else if (currentWeaponCurrentAmmo == 0 && currentWeaponTotalAmmo >= currentWeaponCapacity)
            {
                currentWeaponTotalAmmo = (currentWeaponTotalAmmo - currentWeaponCapacity);

                //update shooter as well
                currentWeaponShooterProperties.totalAmmo = currentWeaponTotalAmmo;

                retVal = currentWeaponCapacity;

            }
            //again if current ammo reaches 0 but at this time
            //total ammo is also less then the capacity
            else if (currentWeaponCurrentAmmo == 0 && currentWeaponTotalAmmo <= currentWeaponCapacity)
            {
                retVal = currentWeaponTotalAmmo;

                currentWeaponTotalAmmo = 0;

                //update shooter as well
                currentWeaponShooterProperties.totalAmmo = currentWeaponTotalAmmo;

            }
            //--> This conditions occurs mostly when we reload using Player Input
            //if current ammo is less then capacity BUT current ammo is greater then 0 AND 
            //total ammo left is GREATER then the ammo needed to REFILL the capacity 
            else if (currentWeaponCurrentAmmo < currentWeaponCapacity
                && currentWeaponCurrentAmmo > 0
                && (currentWeaponCapacity - currentWeaponCurrentAmmo) < currentWeaponTotalAmmo)
            {
                currentWeaponTotalAmmo -= (currentWeaponCapacity - currentWeaponCurrentAmmo);

                //update shooter as well
                currentWeaponShooterProperties.totalAmmo = currentWeaponTotalAmmo;

                retVal = currentWeaponCapacity;
            }
            //--> This conditions occurs mostly when we reload using Player Input
            //if if current ammo is less then capacity BUT current ammo is greater then 0 AND 
            //total ammo left is SMALLER then the ammo needed to REFILL the capacity
            else if (currentWeaponCurrentAmmo < currentWeaponCapacity
                && currentWeaponCurrentAmmo > 0
                && (currentWeaponCapacity - currentWeaponCurrentAmmo) > currentWeaponTotalAmmo)
            {
                retVal = currentWeaponCurrentAmmo + currentWeaponTotalAmmo;

                currentWeaponTotalAmmo = 0;

                //update shooter as well
                currentWeaponShooterProperties.totalAmmo = currentWeaponTotalAmmo;

            }

            return retVal;
        }

        /// <summary>
        /// This Reduces 1 ammo count from the current ammo
        /// </summary>
        /// <returns></returns>
        public int ReduceAmmo()
        {
            int retVal = 0;

            //if we have at least 1 bullet
            if(currentWeaponCurrentAmmo > 0)
            {
                //we only reduce ammo if our power up is not Active
                if (!PowerupsManager.Instance._InfiniteAmmo.isActive)
                    currentWeaponCurrentAmmo --;

                currentWeaponShooterProperties.currentAmmo = currentWeaponCurrentAmmo;

                //setting ret val
                retVal = currentWeaponCurrentAmmo;
            }

            //Set UI
            SetAmmoUI();

            return retVal;
        }

        public int ReloadAmmo()
        {
            int retVal = 0;

            //we try to reload and get the current count
            currentWeaponCurrentAmmo = GetCurrentAmmo();

            //set it at our shooter
            currentWeaponShooterProperties.currentAmmo = currentWeaponCurrentAmmo;

            //setting ret val   
            retVal = currentWeaponCurrentAmmo;

            //Set UI
            SetAmmoUI();

            return retVal;
        }

        public void SetAmmoUI()
        {
            currentWeaponCurrentAmmoText.text = "" + currentWeaponCurrentAmmo;
            currentWeaponTotalAmmoText.text = "" + currentWeaponTotalAmmo;
           
        }

    }

}