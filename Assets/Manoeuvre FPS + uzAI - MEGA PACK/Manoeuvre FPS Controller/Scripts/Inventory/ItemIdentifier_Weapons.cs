using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{

    public class ItemIdentifier_Weapons : MonoBehaviour
    {

        public int Weapon_ID;
        public Image Icon;

        WeaponHandler _WeaponHandler;

        bool _isHolestered;

        // Use this for initialization
        void Start()
        {
            _WeaponHandler = FindObjectOfType<WeaponHandler>();

        }

        private void Update()
        {
            //we can't change weapon if we are in middle of any co routine
            if (gc_AmmoManager.Instance.isReloading || gc_AmmoManager.Instance.isEquipping || gc_AmmoManager.Instance.isThrowing)
            {
                GetComponent<Button>().enabled = false;
                GetComponent<CanvasGroup>().alpha = 0.2f;
            }
            else
            {
                GetComponent<Button>().enabled = true;
                GetComponent<CanvasGroup>().alpha = 1f;
            }
        }

        public void OnItemSelect()
        {
            int currentWeapon_ID = 0;

            if(gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;
            else if(gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;

            if (_isHolestered && currentWeapon_ID == Weapon_ID)
            {
                _WeaponHandler.EquipCurrentWeapon(Weapon_ID);
                _isHolestered = false;
            }
            //if we select the same ID Item Slot
            //unequip
            else if(currentWeapon_ID == Weapon_ID)
            {
                _WeaponHandler.UnequipCurrentWeapon(Weapon_ID);
                _isHolestered = true;

                //hide HUD
                gc_AmmoManager.Instance.WeaponHUD.alpha = 0;
            }
            //now we see do we have more then 1 weapon
            else if (_WeaponHandler.Weapons.Count > 1)
            {
                _WeaponHandler.EquipWeapon(currentWeapon_ID, Weapon_ID);
            }
            //if we have only one weapon with us
            else if (_WeaponHandler.Weapons.Count == 1)
            {
                //we forcefully try to equip it
                _WeaponHandler.Start();
            }

        }

        public void SetUI(Sprite UIIcon)
        {
            Icon.sprite = UIIcon;
        }

    }

}