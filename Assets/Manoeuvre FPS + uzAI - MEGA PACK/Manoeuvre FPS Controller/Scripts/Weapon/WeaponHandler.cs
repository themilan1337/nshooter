using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{

    public class WeaponHandler : MonoBehaviour
    {
        [Header("-- All Weapons --")]
        public List<WeaponClassification> Weapons;

        //[HideInInspector]
        public int currentWeapon_ID;
        //[HideInInspector]
        public int nextWeapon_ID;

        [HideInInspector]
        public float scrollWheelInput;

        void Awake()
        {
            StartCoroutine(AwakeCoroutine());
        }

        IEnumerator AwakeCoroutine()
        {
            yield return new WaitForSeconds(0.1f);

            if (Weapons.Count == 0)
                yield return null;

            //Assign All the Weapons respective IDs
            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[i].WeaponName = Weapons[i].WeaponObject.GetComponent<WeaponShooter>().WeaponName;
                else if (Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[0].WeaponName = Weapons[0].WeaponObject.GetComponent<WeaponMelee>().WeaponName;

                Weapons[i].Weapon_ID = i;

                if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[i].WeaponObject.GetComponent<WeaponShooter>().Weapon_ID = Weapons[i].Weapon_ID;
                else if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[i].WeaponObject.GetComponent<WeaponMelee>().Weapon_ID = Weapons[i].Weapon_ID;

                //checking if there are any upgrades available
                if (GetComponent<WeaponUpgradesHelper>())
                    GetComponent<WeaponUpgradesHelper>().UpgradeWeaponHandler();

                Weapons[i].SetAmmo();

                if (Weapons[i].originalWeaponPosition == Vector3.zero)
                    Weapons[i].InitializeOriginalValues();

                Weapons[i].SetWeaponTransform();
                Weapons[i].WeaponObject.transform.localScale = Weapons[i].WeaponScale;

                Weapons[i].WeaponObject.localPosition = Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetPositionForInit(Weapons[i].originalWeaponPosition);
                Weapons[i].WeaponObject.localRotation = Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetRotationForInit(Weapons[i].originalWeaponRotation);

                //and disable 
                Weapons[i].WeaponObject.gameObject.SetActive(false);

            }
        }

        public IEnumerator Start()
        {
            //wait for first frame to initialize all the instances
            yield return new WaitForSeconds(0.2f);

            if (Weapons.Count > 0)
            {
                //init the weapon being equipped in ammo manager
                gc_AmmoManager.Instance.Initialize(Weapons[0].WeaponObject, Weapons[0].UIIcon);

                if (Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[0].WeaponName = Weapons[0].WeaponObject.GetComponent<WeaponShooter>().WeaponName;
                else if(Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[0].WeaponName = Weapons[0].WeaponObject.GetComponent<WeaponMelee>().WeaponName;

                //Always Starting with the first Weapon as the equipped weapon
                Weapons[0].SetWeaponTransform();

                //start procedural equip Manoeuvre
                Vector3 _weaponPos = Weapons[0].WeaponPosition;
                Vector3 _weaponRot = Weapons[0].WeaponRotation;
                //Debug.Log("_weaponPos " + _weaponPos);
                //Debug.Log("_weaponRot " + _weaponRot);

                Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._weaponSway.defPos = _weaponPos;

                if (Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[0].WeaponObject.GetComponent<WeaponShooter>().recoilProperties.defRot = _weaponRot;
                else if (Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[0].WeaponObject.GetComponent<WeaponMelee>().recoilProperties.defRot = _weaponRot;


                //Weapons[0].WeaponObject.transform.localScale = Weapons[0].WeaponScale;
                //Weapons[0].WeaponObject.localPosition = Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetPositionForInit(Weapons[0].WeaponObject);
                //Weapons[0].WeaponObject.localRotation = Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetRotationForInit(Weapons[0].WeaponObject);

                //yield return new WaitForSeconds(0.5f);
                StartCoroutine(EquipFirstWeapon(_weaponPos, _weaponRot));

            }

        }

        /// <summary>
        /// Whenever a weapon is added in the Weapon Handler List 
        /// </summary>
        public void OnWeaponAdded()
        {
            //Assign All the Weapons respective IDs
            for (int i = 0; i < Weapons.Count; i++)
            {
                if(Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[i].WeaponName = Weapons[i].WeaponObject.GetComponent<WeaponShooter>().WeaponName;
                else if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[i].WeaponName = Weapons[i].WeaponObject.GetComponent<WeaponMelee>().WeaponName;

                Weapons[i].Weapon_ID = i;

                if(Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                    Weapons[i].WeaponObject.GetComponent<WeaponShooter>().Weapon_ID = Weapons[i].Weapon_ID;
                else if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                    Weapons[i].WeaponObject.GetComponent<WeaponMelee>().Weapon_ID = Weapons[i].Weapon_ID;

                if (Weapons[i].originalWeaponPosition == Vector3.zero)
                    Weapons[i].InitializeOriginalValues();

                Weapons[i].SetWeaponTransform();
                Weapons[i].WeaponObject.transform.localScale = Weapons[i].WeaponScale;

                //init pos and rot as soon as weapon is added
                if (Weapons[i].WeaponObject != gc_AmmoManager.Instance._currentWeapon)
                {
                    if (Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.myLocalPos == Vector3.zero) {

                        Weapons[i].WeaponObject.localPosition = Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetPositionForInit(Weapons[i].originalWeaponPosition);
                        Weapons[i].WeaponObject.localRotation = Weapons[i].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.GetOffsetRotationForInit(Weapons[i].originalWeaponRotation);

                    }

                    //and disable 
                    Weapons[i].WeaponObject.gameObject.SetActive(false);
                }

            }
        }

        /// <summary>
        /// When we start from zero weapons and this is the weapon we picked up first
        /// </summary>
        public IEnumerator EquipFirstWeapon(Vector3 _weaponPos, Vector3 _weaponRot)
        {

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Weapons[0].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.EquipSwayCoroutine(_weaponPos, _weaponRot, Weapons[0].WeaponObject));

        }

        /// <summary>
        /// Pass the Weapon ID of the weapon to holister 
        /// and the Weapon ID to to equip now
        /// </summary>
        /// <param name="from_Weapon_ID"></param>
        /// <param name="to_Weapon_ID"></param>
        public void EquipWeapon(int from_Weapon_ID, int to_Weapon_ID)
        {
            StartCoroutine(EquipWeaponCoroutine(from_Weapon_ID, to_Weapon_ID));
        }

        IEnumerator EquipWeaponCoroutine(int from_Weapon_ID, int to_Weapon_ID)
        {
            //init the weapon being equipped in ammo manager
            gc_AmmoManager.Instance.Initialize(Weapons[to_Weapon_ID].WeaponObject, Weapons[to_Weapon_ID].UIIcon);

            //disable current weapon
            WeaponSway _weaponSway = Weapons[from_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._weaponSway;
            Transform _weaponTransform = Weapons[from_Weapon_ID].WeaponObject;

            StartCoroutine(Weapons[from_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.Un_EquipSwayCoroutine(_weaponTransform, _weaponSway));

            //wait for the previous equipment sway coroutine to stop
            yield return new WaitForSeconds(Weapons[from_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.equipDuration);

            //set the weapon transform of the weapon to activate next
            Weapons[to_Weapon_ID].SetWeaponTransform();
           
            //start procedural equip Manoeuvre
            Vector3 _weaponPos = Weapons[to_Weapon_ID].WeaponPosition;
            Vector3 _weaponRot = Weapons[to_Weapon_ID].WeaponRotation;

            Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._weaponSway.defPos = _weaponPos;

            if(Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType== WeaponType.Shooter)
                Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponShooter>().recoilProperties.defRot = _weaponRot;
            else if(Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponMelee>().recoilProperties.defRot = _weaponRot;

            //Debug.Log("_weaponPos " + _weaponPos);
            //Debug.Log("_weaponRot " + _weaponRot);

            StartCoroutine(Weapons[to_Weapon_ID].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.EquipSwayCoroutine(_weaponPos, _weaponRot, Weapons[to_Weapon_ID].WeaponObject));

        }

        /// <summary>
        /// Get Next Weapon input
        /// </summary>
        private void Update()
        {
            scrollWheelInput = Input.GetAxis(GetComponent<ManoeuvreFPSController>().Inputs.mouseScrollWheel);

            //if the Current Weapon is null
            if (gc_AmmoManager.Instance._currentWeapon == null)
                return; //exit

            //we can't change weapon if we are reloading
            if (gc_AmmoManager.Instance.isReloading)
                return; //exit

            //we can't change weapon if we are 
            //in between equip sway already
            if (gc_AmmoManager.Instance.isEquipping)
                return; //exit

            //if we are throwing a grenade
            if (gc_AmmoManager.Instance.isThrowing)
                return; //exit

            //if we have only 1 weapon in Weapon Handler
            if (Weapons.Count == 1)
                return;

            //next weapon input via button
            if (ManoeuvreFPSController.Instance.Inputs.nextWeaponInput)
                TryEquipNextWeapon();

            //previous weapon input via button
            if (ManoeuvreFPSController.Instance.Inputs.previousWeaponInput)
                TryEquipNextWeapon();

            //next weapon input via scroll wheel
            if (scrollWheelInput > 0f)
                TryEquipNextWeapon();
            //previous weapon input via scroll wheel
            else if (scrollWheelInput < 0f)
                TryEquipPreviousWeapon();

        }

        /// <summary>
        /// Always Equips the next Weapon in the Weapons List
        /// </summary>
        void TryEquipNextWeapon()
        {
            //disable iron sight
            if(FindObjectOfType<WeaponShooter>())
                StartCoroutine(FindObjectOfType<WeaponShooter>().ironSightProperties.tweenIronSight(false));

            if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
            {
                //cache the current ID
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;
                //set the next ID
                nextWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID + 1;

            }
            else if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
            {
                //cache the current ID
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;
                //set the next ID
                nextWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID + 1;

            }

            //if the next ID is invalid
            if (Weapons.Count <= nextWeapon_ID)
                nextWeapon_ID = 0; //set it at the very first ID

            //finally invoke the equip weapon
            EquipWeapon(currentWeapon_ID, nextWeapon_ID);
        }

        void TryEquipPreviousWeapon()
        {
            //disable iron sight
            if(FindObjectOfType<WeaponShooter>())
                StartCoroutine(FindObjectOfType<WeaponShooter>().ironSightProperties.tweenIronSight(false));

            if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
            {
                //cache the current ID
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID;
                //set the previous ID
                nextWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponShooter>().Weapon_ID - 1;

            }
            else if (gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
            {
                //cache the current ID
                currentWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID;
                //set the next ID
                nextWeapon_ID = gc_AmmoManager.Instance._currentWeapon.GetComponent<WeaponMelee>().Weapon_ID - 1;

            }

            //if the previous ID is invalid
            if (nextWeapon_ID < 0)
                nextWeapon_ID = Weapons.Count-1; //set it at the very last ID

            //finally invoke the equip weapon
            EquipWeapon(currentWeapon_ID, nextWeapon_ID);
        }

        public void UnequipCurrentWeapon(int id)
        {
            //disable iron sight
            if(FindObjectOfType<WeaponShooter>())
                StartCoroutine(FindObjectOfType<WeaponShooter>().ironSightProperties.tweenIronSight(false));

            //disable current weapon
            WeaponSway _weaponSway = Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._weaponSway;
            Transform _weaponTransform = Weapons[id].WeaponObject;

            StartCoroutine(Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.Un_EquipSwayCoroutine(_weaponTransform, _weaponSway));
        }

        public void EquipCurrentWeapon(int id)
        {
            //disable iron sight
            if(FindObjectOfType<WeaponShooter>())
                StartCoroutine(FindObjectOfType<WeaponShooter>().ironSightProperties.tweenIronSight(false));

            //init the weapon being equipped in ammo manager
            gc_AmmoManager.Instance.Initialize(Weapons[id].WeaponObject, Weapons[id].UIIcon);

            //set the weapon transform of the weapon to activate next
            Weapons[id].SetWeaponTransform();

            //start procedural equip Manoeuvre
            Vector3 _weaponPos = Weapons[id].WeaponPosition;
            Vector3 _weaponRot = Weapons[id].WeaponRotation;

            Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._weaponSway.defPos = _weaponPos;

            if (Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Shooter)
                Weapons[id].WeaponObject.GetComponent<WeaponShooter>().recoilProperties.defRot = _weaponRot;
            else if(Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                Weapons[id].WeaponObject.GetComponent<WeaponMelee>().recoilProperties.defRot = _weaponRot;

            //Debug.Log("_weaponPos " + _weaponPos);
            //Debug.Log("_weaponRot " + _weaponRot);
            StartCoroutine(Weapons[id].WeaponObject.GetComponent<WeaponProceduralManoeuvre>()._equipSway.EquipSwayCoroutine(_weaponPos, _weaponRot, Weapons[id].WeaponObject));
        }

        /// <summary>
        /// Pass weapon transform and it will return it's ID
        /// </summary>
        /// <param name="_wpnTransform"></param>
        /// <returns></returns>
        public int GetWeaponID(Transform _wpnTransform)
        {
            int retval = 0;

            if (Weapons.Count < 1)
                return retval;

            for(int i = 0;i < Weapons.Count; i++)
            {
                if(Weapons[i].WeaponObject == _wpnTransform)
                {
                    retval = i;
                    break;
                }
            }

            return retval;
        }
    }

    [System.Serializable]
    public class WeaponClassification
    {
        public Transform WeaponObject;
        public WeaponType WeaponType;

        public int Ammo = 100;
        public int Capacity = 15;
        public int CurrentAmmo;

        public string WeaponName;
        public Sprite UIIcon;

        [HideInInspector]
        //"Will be auto assigned"
        public int Weapon_ID;

        /// <summary>
        /// Editor Used Variables
        /// </summary>
        [HideInInspector]
        public Vector3 WeaponPosition;
        [HideInInspector]
        public Vector3 WeaponRotation;
        [HideInInspector]
        public Vector3 WeaponScale;

        public Vector3 originalWeaponPosition;
        public Vector3 originalWeaponRotation;
        public Vector3 originalWeaponScale;

        bool hasSetPositions;

        public void InitializeOriginalValues()
        {
            if (hasSetPositions)
                return;

            hasSetPositions = true;

            originalWeaponPosition = WeaponObject.localPosition;
            originalWeaponRotation = WeaponObject.localEulerAngles;
            originalWeaponScale = WeaponObject.localScale;

        }

        /// <summary>
        /// Saves the Weapon Transform.
        /// This Method is run by the Editor's Save Button
        /// </summary>
        public void SaveWeaponTransform()
        {
            //Save Position
            PlayerPrefs.SetFloat("PosX_" + WeaponName, WeaponObject.localPosition.x);
            PlayerPrefs.SetFloat("PosY_" + WeaponName, WeaponObject.localPosition.y);
            PlayerPrefs.SetFloat("PosZ_" + WeaponName, WeaponObject.localPosition.z);

            //Save Rotation
            PlayerPrefs.SetFloat("RotX_" + WeaponName, WeaponObject.localEulerAngles.x);
            PlayerPrefs.SetFloat("RotY_" + WeaponName, WeaponObject.localEulerAngles.y);
            PlayerPrefs.SetFloat("RotZ_" + WeaponName, WeaponObject.localEulerAngles.z);

            //Save Scale
            PlayerPrefs.SetFloat("ScaleX_" + WeaponName, WeaponObject.localScale.x);
            PlayerPrefs.SetFloat("ScaleY_" + WeaponName, WeaponObject.localScale.y);
            PlayerPrefs.SetFloat("ScaleZ_" + WeaponName, WeaponObject.localScale.z);

        }

        /// <summary>
        /// Set The Weapon's Transform position.
        /// </summary>
        public void SetWeaponTransform()
        {
            if (PlayerPrefs.HasKey("PosX_" + WeaponName)) {
               
                //Setting Position
                float PosX = PlayerPrefs.GetFloat("PosX_" + WeaponName);
                float PosY = PlayerPrefs.GetFloat("PosY_" + WeaponName);
                float PosZ = PlayerPrefs.GetFloat("PosZ_" + WeaponName);
                WeaponPosition = new Vector3(PosX, PosY, PosZ);

                //Setting Rotation
                float RotX = PlayerPrefs.GetFloat("RotX_" + WeaponName);
                float RotY = PlayerPrefs.GetFloat("RotY_" + WeaponName);
                float RotZ = PlayerPrefs.GetFloat("RotZ_" + WeaponName);
                WeaponRotation = new Vector3(RotX, RotY, RotZ);
                Debug.Log("WeaponRotation" + WeaponRotation);

                //Setting Scale
                float ScaleX = PlayerPrefs.GetFloat("ScaleX_" + WeaponName);
                float ScaleY = PlayerPrefs.GetFloat("ScaleY_" + WeaponName);
                float ScaleZ = PlayerPrefs.GetFloat("ScaleZ_" + WeaponName);
                WeaponScale = new Vector3(ScaleX, ScaleY, ScaleZ);

            }
            else
            {
                //set default values
                WeaponPosition = originalWeaponPosition;
                WeaponRotation = originalWeaponRotation;
                WeaponScale = originalWeaponScale;
            }

          //Debug.Log(WeaponScale);

        }

        /// <summary>
        /// This will be called only once!
        /// </summary>
        public void SetAmmo()
        {
            //no ammo for melee weapon.... of course..... :P
            if (WeaponObject.GetComponent<WeaponProceduralManoeuvre>().weaponType == WeaponType.Melee)
                return;

            //Set Ammo and capacity
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo = Ammo;
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.ammoCapacity = Capacity;
            //Debug.Log("Capacity " + Capacity);
            WeaponObject.GetComponent<WeaponShooter>().shooterProperties.currentAmmo = CurrentAmmo == 0 ? Capacity : CurrentAmmo;

            //Debug.Log("Ammo : " + WeaponObject.GetComponent<WeaponShooter>().shooterProperties.totalAmmo);
            //Debug.Log("Capacity : " + WeaponObject.GetComponent<WeaponShooter>().shooterProperties.ammoCapacity);

        }

       
    }

}