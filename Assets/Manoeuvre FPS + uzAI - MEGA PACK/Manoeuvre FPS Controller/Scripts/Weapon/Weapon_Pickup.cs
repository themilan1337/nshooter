using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public enum ItemTypes { Guns, Pickups }

    public class Weapon_Pickup : MonoBehaviour
    {
        [HideInInspector]
        public AudioClip pickupSound;

        [HideInInspector]
        public Transform WeaponObject;

        [HideInInspector]
        public int Ammo = 100;

        [HideInInspector]
        public int Capacity = 20;

        [HideInInspector]
        public bool equipOnPickup = false;

        [HideInInspector]
        public Sprite UIIcon;

        [HideInInspector]
        public GameObject PickupTextPrefab;

        public UnityEngine.Events.UnityEvent OnPickup;

        //UI Canvas element
        GameObject PickupMessagesContainer;

        private void Awake()
        {
            //get UI reference
            PickupMessagesContainer = GameObject.Find("PickupMessagesContainer");

        }

        private void OnTriggerStay(Collider other)
        {
            //if the weapon is not being current reloading
            if (gc_AmmoManager.Instance.isReloading || gc_AmmoManager.Instance.isEquipping)
                return;

            if (other.gameObject.tag == "Player")
            {
                //Debug.Log("Picked up : " + name);

                //Add the weapon
                AddItemtoWeaponHandler(other.gameObject.GetComponent<WeaponHandler>());

                //show message
                ShowPickupMessage();

                //play pickup sound
                AudioSource.PlayClipAtPoint(pickupSound,transform.position);

                //fire pickup event
                OnPickup.Invoke();

                this.gameObject.SetActive(false);
            }
        }

        void ShowPickupMessage()
        {
            //show pickup message
            GameObject msg = Instantiate(PickupTextPrefab);
            if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                msg.GetComponent<Text>().text = "Picked " + WeaponObject.name + " (+" + Ammo + ")";
            else
                msg.GetComponent<Text>().text = "Picked " + WeaponObject.name;

            //init scale and pos
            msg.transform.SetParent(PickupMessagesContainer.transform);
            msg.transform.localPosition = Vector3.zero;
            msg.transform.localScale = Vector3.one;
            msg.transform.localEulerAngles = Vector3.zero;

            //destroy msg
            Destroy(msg, 1f);

        }

        void AddItemtoWeaponHandler(WeaponHandler _WeaponHandler)
        {
            
            if (WeaponObject)
            {
                //Play Dialogue
                gc_PlayerDialoguesManager.Instance.PlayDialogueClip(gc_PlayerDialoguesManager.DialogueType.Pickup);

                if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    AddShooterWeaponToHandler(_WeaponHandler);
                else
                    AddMeleeWeaponToHandler(_WeaponHandler);
            }
            else
                Debug.Log("Please assign the weapon object");

        }

        void AddShooterWeaponToHandler(WeaponHandler _WeaponHandler)
        {
            bool exist = false;

            for (int i = 0; i < _WeaponHandler.Weapons.Count; i++)
            {
                if (_WeaponHandler.Weapons[i].WeaponName == WeaponObject.GetComponent<WeaponShooter>().WeaponName)
                    exist = true;
            }

            if (exist)
            {
                int id_current = 0;
                int id_this = WeaponObject.GetComponent<WeaponShooter>().Weapon_ID;

                if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    id_current = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;
                else
                    id_current = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;

                //Add just the ammo if already has this weapon
                AddAmmo();

                if (equipOnPickup && (id_this != id_current))
                    //this means, we are already equipped with one weapon
                    EquipGun(_WeaponHandler);

                return;

            }

            WeaponClassification _WeaponClassification = new WeaponClassification();
            _WeaponClassification.WeaponObject = WeaponObject;

            _WeaponHandler.Weapons.Add(_WeaponClassification);
            _WeaponHandler.OnWeaponAdded();

            //reset the ammo count of the weapon we have 
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.ammoCapacity = Capacity;
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo = 0;
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.currentAmmo = 0;

            //Add ammo 
            AddAmmo();

            //also set this handler's properties
            for (int i = 0; i < _WeaponHandler.Weapons.Count; i++)
            {
                if (_WeaponHandler.Weapons[i].WeaponName == WeaponObject.GetComponent<WeaponShooter>().WeaponName)
                {
                    _WeaponHandler.Weapons[i].UIIcon = UIIcon;
                    _WeaponHandler.Weapons[i].Capacity = Capacity;
                    _WeaponHandler.Weapons[i].Ammo = Ammo;
                    _WeaponHandler.Weapons[i].SetAmmo();

                    //set icon in HUD manually
                    gc_AmmoManager.Instance.currentWeaponImage.sprite = _WeaponHandler.Weapons[i].UIIcon;

                }
            }

            //Update Inventory
            Inventory.Instance.AddInventorySlot_Weapons(WeaponObject.GetComponent<WeaponShooter>().Weapon_ID, UIIcon, WeaponObject.GetComponent<WeaponShooter>().WeaponName);

            //we see if this is the first weapon we picked up
            if (_WeaponHandler.Weapons.Count == 1)
            {
                //we forcefully try to equip it
                _WeaponHandler.Start();
                return;
            }

            if (equipOnPickup)
                //this means, we are already equipped with one weapon
                EquipGun(_WeaponHandler);
        }

        void AddMeleeWeaponToHandler(WeaponHandler _WeaponHandler)
        {
            bool exist = false;

            for (int i = 0; i < _WeaponHandler.Weapons.Count; i++)
            {
                if (_WeaponHandler.Weapons[i].WeaponName == WeaponObject.GetComponent<WeaponMelee>().WeaponName)
                    exist = true;
            }

            if (exist)
            {
                int id_current = 0; 
                int id_this = WeaponObject.GetComponent<WeaponMelee>().Weapon_ID;
                if(gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                     id_current = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;
                else
                    id_current = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;

                if (equipOnPickup && (id_this != id_current))
                    //this means, we are already equipped with one weapon
                    EquipGun(_WeaponHandler);

                return;

            }

            WeaponClassification _WeaponClassification = new WeaponClassification();
            _WeaponClassification.WeaponObject = WeaponObject;
            _WeaponClassification.WeaponType = WeaponType.Melee;
            _WeaponHandler.Weapons.Add(_WeaponClassification);
            _WeaponHandler.OnWeaponAdded();

            //also set this handler's properties
            for (int i = 0; i < _WeaponHandler.Weapons.Count; i++)
            {
                if (_WeaponHandler.Weapons[i].WeaponName == WeaponObject.GetComponent<WeaponMelee>().WeaponName)
                {
                    //set icon in HUD manually
                    _WeaponHandler.Weapons[i].UIIcon = UIIcon;
                    gc_AmmoManager.Instance.currentWeaponImage.sprite = _WeaponHandler.Weapons[i].UIIcon;

                }
            }

            //Update Inventory
            Inventory.Instance.AddInventorySlot_Weapons(WeaponObject.GetComponent<WeaponMelee>().Weapon_ID, UIIcon, WeaponObject.GetComponent<WeaponMelee>().WeaponName);

            //we see if this is the first weapon we picked up
            if (_WeaponHandler.Weapons.Count == 1)
            {
                //we forcefully try to equip it
                _WeaponHandler.Start();
                return;
            }

            if (equipOnPickup)
                //this means, we are already equipped with one weapon
                EquipGun(_WeaponHandler);
        }

        /// <summary>
        /// Main method to equip the Gun
        /// </summary>
        void EquipGun(WeaponHandler _WeaponHandler)
        {
            int currentWeapon_ID = 0;
            int nextWeapon_ID = 0;

            //take current ID depending on weapon type
            if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;
            else if(gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;

            //take next ID depending on weapon type

            if(WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                nextWeapon_ID = WeaponObject.GetComponent<WeaponShooter>().Weapon_ID;
            else if(WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                nextWeapon_ID = WeaponObject.GetComponent<WeaponMelee>().Weapon_ID;

            _WeaponHandler.EquipWeapon(currentWeapon_ID, nextWeapon_ID);
        }

        void AddAmmo()
        {
            //Adding ammo to original weapon shooter
            //Debug.Log("WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo : " +
                //WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo);
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo += Ammo;

            //now update UI only if this is same as the currently equipped weapon
            if(gc_AmmoManager.Instance._currentWeapon == WeaponObject)
            {

                gc_AmmoManager.Instance.currentWeaponTotalAmmo = WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo;
                gc_AmmoManager.Instance.currentWeaponCurrentAmmo = WeaponObject.GetComponent<WeaponShooter>().shooterProperties.currentAmmo;

                //Debug.Log("gc_AmmoManager.Instance.currentWeaponTotalAmmo" + gc_AmmoManager.Instance.currentWeaponTotalAmmo);
                //Debug.Log("gc_AmmoManager.Instance.currentWeaponCurrentAmmo" + gc_AmmoManager.Instance.currentWeaponCurrentAmmo);

                gc_AmmoManager.Instance.SetAmmoUI();

            }

        }

    }
}