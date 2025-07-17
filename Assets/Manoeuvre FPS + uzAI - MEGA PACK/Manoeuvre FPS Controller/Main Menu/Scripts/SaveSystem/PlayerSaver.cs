using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manoeuvre
{

    public class PlayerSaver : MonoBehaviour
    {
        [HideInInspector]
        public string SlotKey;

        [HideInInspector]
        public int PlayerCurrentHealth;
        [HideInInspector]
        public Vector3 PlayerPosition;
        [HideInInspector]
        public List<WeaponClassification> PlayerWeapons;
        [HideInInspector]
        public List<Throwables> PlayerThrowables = new List<Throwables>();

        bool InvokeOnResumeEvent;

        private SaveComponentsHelper _saveComponentsHelper;

        private void OnEnable()
        {
            //get save helper reference 
            _saveComponentsHelper = FindObjectOfType<SaveComponentsHelper>();

            //get the last loaded slot key
            SlotKey = PlayerPrefs.GetString("LastSavedKey"); //eg. Slot 1

            if (PlayerPrefs.HasKey(SlotKey))
                SetPlayerData();
        }

        /// <summary>
        /// Overwrite saver script if key is present
        /// </summary>
        void SetPlayerData()
        {
            Debug.Log("Data Retrieved from Key : " + SlotKey);

            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SlotKey), this);

            //now set player position
            transform.position = PlayerPosition;

            //set player weapons
            RetrieveWeaponsFromHelper();

            //set player Throwables
            if (GetComponent<ThrowablesHandler>())
                RetrieveThrowablesFromHelper();

            //set Player's health
            GetComponent<ManoeuvreFPSController>().Health.currentHealth = PlayerCurrentHealth;

            //if there is any on resume event here, it will be called
            InvokeOnResumeEvent = true;
        }

        private void RetrieveWeaponsFromHelper()
        {
            //clear what we have so far..
            GetComponent<WeaponHandler>().Weapons.Clear();

            //retrieve weapons' transforms via helper
            foreach (WeaponClassification PlayerWeapons_wc in PlayerWeapons)
            {
                //if weapon is of type Shooter
                if (PlayerWeapons_wc.WeaponType == WeaponType.Shooter)
                {
                    PlayerWeapons_wc.WeaponObject = _saveComponentsHelper.ReturnShooterWeapon(PlayerWeapons_wc.WeaponName);
                    PlayerWeapons_wc.WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo = PlayerWeapons_wc.Ammo;
                    PlayerWeapons_wc.WeaponObject.GetComponent<WeaponShooter>().shooterProperties.currentAmmo = PlayerWeapons_wc.CurrentAmmo;
                }

                //if weapon is of type melee
                if (PlayerWeapons_wc.WeaponType == WeaponType.Melee)
                {
                    PlayerWeapons_wc.WeaponObject = _saveComponentsHelper.ReturnMeleeWeapon(PlayerWeapons_wc.WeaponName);
                }
            }

            //now set it inside handler
            GetComponent<WeaponHandler>().Weapons = PlayerWeapons;
        }

        private void RetrieveThrowablesFromHelper()
        {
            //clear what we have so far
            GetComponent<ThrowablesHandler>().AllThrowables.Clear();

            //retrieve throwables transform from helper
            foreach(Throwables t in PlayerThrowables)
                t.Throwable = _saveComponentsHelper.ReturnThrowableWeapon(t.ItemName);

            //now set it inside handler
            GetComponent<ThrowablesHandler>().AllThrowables = PlayerThrowables;

        }

        public void SavePlayerData(string _sKey)
        {
            //first we retrieve all data entries needed to be saved
            GetPlayerData();

            //now save using JSON
            PlayerPrefs.SetString(_sKey, JsonUtility.ToJson(this, false));
            PlayerPrefs.SetString("LastSavedKey", _sKey);
            PlayerPrefs.SetString(_sKey + "_LoadedScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

            //set this as last saved slot key, so when player dies, game will resume from HERE.

            Debug.Log("Data Saved with key : " + _sKey);

        }

        private void GetPlayerData()
        {
            //take player's position
            PlayerPosition = transform.position;

            //take player's weapon Handler
            PlayerWeapons =  GetComponent<WeaponHandler>().Weapons;

            //take every weapon's current ammo separately
            foreach (WeaponClassification PlayerWeapons_wc in PlayerWeapons)
            {
                if (PlayerWeapons_wc.WeaponObject.GetComponent<WeaponShooter>())
                {
                    PlayerWeapons_wc.Ammo = PlayerWeapons_wc.WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo;
                    PlayerWeapons_wc.CurrentAmmo = PlayerWeapons_wc.WeaponObject.GetComponent<WeaponShooter>().shooterProperties.currentAmmo;

                    PlayerWeapons_wc.originalWeaponPosition = Vector3.zero;
                    PlayerWeapons_wc.originalWeaponRotation = Vector3.zero;
                }
            }

            //get throwables
            if (GetComponent<ThrowablesHandler>())
                PlayerThrowables = GetComponent<ThrowablesHandler>().AllThrowables;

            ///get player's health
            PlayerCurrentHealth = gc_PlayerHealthManager.Instance.currentHealth;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!InvokeOnResumeEvent)
                return;


            if (other.GetComponent<SaveTrigger>())
            {
                other.GetComponent<SaveTrigger>().InvokeResumeEvents();
                InvokeOnResumeEvent = false;

            }

        }

    }
}