using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class MainMenuShopController : MonoBehaviour
    {
        //UI Referencees
        public GameObject AttributePrefab;
        public Text WeaponNameText;
        public Text WeaponCurrentLevelText;
        public Text InGameCurrencyText;

        [Space]
        public Button NextWeaponBtn;
        public Button PreviousWeaponBtn;
        public Button UpgradeWeaponBtn;

        [Space]
        public Transform AttributesContainer;
        public Transform WeaponsMeshContainer;

        [Space]
        public string MainMenuSceneName = "Main Menu";

        [Space]
        public string ButtonClickSFX = "Button Click";
        public string ButtonHoverSFX = "Button Hover";

        [Space]
        public List<AttributesDefinition> attributesDefinitions = new List<AttributesDefinition>();

        //current selected weapon 
        int currentWpnIndex = 0;

        //cached attributes
        List<GameObject> SpawnedAttributes = new List<GameObject>();

        //current money player have 
        int InGameCurrency = 0;

        //current selected upgrade items
        int currentSelectedUpgradePrice;
        int currentSelectedUpgradeCount;
        string currentSelectedWeaponName;

        //ui event system
        UnityEngine.EventSystems.EventSystem eventSystem;

        // Use this for initialization
        void Start()
        {
            //fade out to scene
            if (MainMenuFader.Instance)
                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            else
            {
                GameObject _fader = GameObject.Instantiate(Resources.Load("Fader")) as GameObject;

                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            }

            //create Audio Manager
            if (!MainMenuAudioManager.Instance)
                Instantiate(Resources.Load("AudioManager"));

            //find event system
            eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            eventSystem.firstSelectedGameObject = UpgradeWeaponBtn.gameObject;

            SetCurrentValueAsDefaultValue();
            SetAttributesTypes();
            SpawnAllWeaponMeshes();
            ShowWeaponForUpgrade(currentWpnIndex);

            //add listeners
            NextWeaponBtn.onClick.AddListener(OnNextWeaponBtnClick);
            PreviousWeaponBtn.onClick.AddListener(OnPreviousWeaponBtnClick);
            UpgradeWeaponBtn.onClick.AddListener(OnUpgradeButtonClick);

            //update currency
            InGameCurrency = PlayerPrefs.GetInt("InGameCurrency", 1000000);
            InGameCurrencyText.text = InGameCurrency.ToString() + " $";

        }

        void SetCurrentValueAsDefaultValue()
        {
            for(int i = 0; i< attributesDefinitions.Count; i++)
            {
                currentSelectedWeaponName = attributesDefinitions[i].WeaponName;

                for(int j = 0; j < attributesDefinitions[i].AllAttributes.Count; j++)
                {
                    SetValueAccordingToType(attributesDefinitions[i].AllAttributes[j].weaponUpgradesTypes,
                        attributesDefinitions[i].AllAttributes[j].CurrentValue);
                }
            }
        }

        /// <summary>
        /// Spawn all the weapon meshes and disable them at start
        /// </summary>
        void SpawnAllWeaponMeshes()
        {
            foreach(AttributesDefinition ad in attributesDefinitions)
            {
                GameObject wpnMesh = Instantiate(ad.WeaponMesh);
                wpnMesh.transform.SetParent(WeaponsMeshContainer);
                wpnMesh.transform.localEulerAngles = Vector3.zero;
                wpnMesh.transform.localPosition = Vector3.zero;
                wpnMesh.transform.localScale = ad.WeaponMeshScale;

                //disable mesh
                wpnMesh.SetActive(false);

                //and set THIS as the mesh
                ad.WeaponMesh = wpnMesh;
            }
        }

        /// <summary>
        /// Just pass the index of the weapon which is to be showed.
        /// </summary>
        /// <param name="index"></param>
        void ShowWeaponForUpgrade(int index)
        {
            //set current weapon's index
            currentWpnIndex = index;

            //disable all the weapon meshes and..
            foreach (AttributesDefinition _ad in attributesDefinitions)
                _ad.WeaponMesh.SetActive(false);

            //... enable only the one needed
            attributesDefinitions[currentWpnIndex].WeaponMesh.SetActive(true);

            //set current wpn name
            currentSelectedWeaponName = attributesDefinitions[currentWpnIndex].WeaponName;
            WeaponNameText.text = currentSelectedWeaponName;

            //clear all previous attributes
            foreach (GameObject g in SpawnedAttributes)
                Destroy(g);

            SpawnedAttributes.Clear();

            CheckForAcquiredUpgrades();

            //spawn all attributes
            for (int i = 0; i < attributesDefinitions[currentWpnIndex].AllAttributes.Count; i++)
            {
                GameObject _at = Instantiate(AttributePrefab);
                _at.transform.SetParent(AttributesContainer);
                _at.transform.localEulerAngles = Vector3.zero;
                _at.transform.localPosition = Vector3.zero;
                _at.transform.localScale = Vector3.one;

                //add in list
                SpawnedAttributes.Add(_at);

                //cache all vars
                string attName = attributesDefinitions[currentWpnIndex].AllAttributes[i].AttributeName;
                float maxVal = attributesDefinitions[currentWpnIndex].AllAttributes[i].MaxValue;
                float minVal = attributesDefinitions[currentWpnIndex].AllAttributes[i].MinValue;
                float currVal = attributesDefinitions[currentWpnIndex].AllAttributes[i].CurrentValue;

                //we will get this from loop below
                float upgradeVal = 0; 

                for (int k = 0; k < attributesDefinitions[currentWpnIndex].AttributeUpgrades.Count; k++)
                {
                    if (!attributesDefinitions[currentWpnIndex].AttributeUpgrades[k].UpgradeAcquired)
                    {
                        foreach (AttributeUpgrade _au in attributesDefinitions[currentWpnIndex].AttributeUpgrades[k].AttributeUpgrade)
                            if (_au.AttributeName == attName)
                            {
                                currentSelectedUpgradeCount = k;
                                upgradeVal = _au.UpgradeValue;
                                break; //exit
                            }
                    }

                    //we see if the val has beeen updated or not..
                    if (upgradeVal != 0)
                    {
                        break;
                    }
                }

               bool isMax =  SetLevelTextAndUpgradePrice();

                //now set the UI of 'this i-th' spawned Attribute
                _at.GetComponent<ShopAttributeInitializer>().SetAttributeUI(attName, currVal, upgradeVal, maxVal, minVal, isMax);

            }
        }

        bool SetLevelTextAndUpgradePrice()
        {
            int upgradePrice = 0;
            int upgradeLevel = 0;
            bool isMax = false;

            for (int i = 0; i < attributesDefinitions.Count; i++)
            {
                if(attributesDefinitions[i].WeaponName == currentSelectedWeaponName)
                {
                    for (int j = 0; j < attributesDefinitions[i].AttributeUpgrades.Count; j++)
                    {
                        if (!attributesDefinitions[i].AttributeUpgrades[j].UpgradeAcquired)
                        {
                            if(upgradePrice == 0)
                                upgradePrice = attributesDefinitions[i].AttributeUpgrades[j].price;
                            
                        }
                        else
                            upgradeLevel++;

                    }

                    if (upgradeLevel == attributesDefinitions[i].AttributeUpgrades.Count)
                        isMax = true;
                }
                
            }

            //set upgrade btn text
            UpgradeWeaponBtn.GetComponentInChildren<Text>().text = "Upgrade [" + upgradePrice + "]";
            currentSelectedUpgradePrice = upgradePrice;

            //if we have reached max upgrade
            if(isMax)
            {
                //set upgrade btn text
                UpgradeWeaponBtn.GetComponentInChildren<Text>().text = "Upgrade [MAX]";
                currentSelectedUpgradePrice = 0;
            }

            //set level text
            WeaponCurrentLevelText.text = "LV " + (upgradeLevel /*+ 1*/);

            return isMax;

        }

        void SetAttributesTypes()
        {
            for (int i = 0; i < attributesDefinitions.Count; i++)
            {
                for (int j = 0; j < attributesDefinitions[i].AttributeUpgrades.Count; j++)
                {
                    for(int k = 0; k < attributesDefinitions[i].AttributeUpgrades[j].AttributeUpgrade.Count; k++)
                    {
                        attributesDefinitions[i].AttributeUpgrades[j].AttributeUpgrade[k].weaponUpgradesTypes = attributesDefinitions[i].AllAttributes[k].weaponUpgradesTypes;
                    
                    }
                }
            }

        }

        /// <summary>
        /// Simply checks which upgrade has been acquired.
        /// </summary>
        void CheckForAcquiredUpgrades()
        {
            for(int  i = 0;i<attributesDefinitions.Count; i++)
            {
                for(int j = 0; j < attributesDefinitions[i].AttributeUpgrades.Count; j++)
                {
                    if(attributesDefinitions[i].WeaponName == currentSelectedWeaponName)
                    {
                        if (PlayerPrefs.GetInt(currentSelectedWeaponName + "_" + j) == 1)
                        {
                            attributesDefinitions[i].AttributeUpgrades[j].UpgradeAcquired = true;

                            for (int k = 0; k < attributesDefinitions[i].AllAttributes.Count; k++)
                            {
                                attributesDefinitions[i].AllAttributes[k].CurrentValue = attributesDefinitions[i].AttributeUpgrades[j].AttributeUpgrade[k].UpgradeValue;
                            }

                        }
                    }
                    
                }
            }

        }

        /// <summary>
        /// On Click Event for Next Weapon Btn
        /// </summary>
        void OnNextWeaponBtnClick()
        {
            if (currentWpnIndex < attributesDefinitions.Count-1)
                currentWpnIndex++;
            else
                currentWpnIndex = 0;

            ShowWeaponForUpgrade(currentWpnIndex);

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        /// <summary>
        /// On Click Event for Previous Weapon Btn
        /// </summary>
        void OnPreviousWeaponBtnClick()
        {
            if (currentWpnIndex > 0)
                currentWpnIndex--;
            else
                currentWpnIndex = attributesDefinitions.Count-1;

            ShowWeaponForUpgrade(currentWpnIndex);

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        /// <summary>
        /// Saves the current weapon's current selected upgrade.
        /// </summary>
        void SetUpgradeAsAcquired()
        {
            for(int i = 0; i < attributesDefinitions.Count; i++)
            {
                if(currentSelectedWeaponName == attributesDefinitions[i].WeaponName)
                {
                    foreach (AttributeUpgrade _au in 
                        attributesDefinitions[i].AttributeUpgrades[currentSelectedUpgradeCount].AttributeUpgrade)
                    {
                        //Save Values
                        SetValueAccordingToType(_au.weaponUpgradesTypes, _au.UpgradeValue);

                    }

                    //mark this as acquired
                    attributesDefinitions[i].AttributeUpgrades[currentSelectedUpgradeCount].UpgradeAcquired = true;

                    //SAVE
                    PlayerPrefs.SetInt(currentSelectedWeaponName + "_" + currentSelectedUpgradeCount, 1);

                }
            }
        }

        void SetValueAccordingToType(WeaponUpgradesTypes _type, float val)
        {
            switch (_type)
            {
                case WeaponUpgradesTypes.Damage:
                    //set Damage
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_Damage", val);
                    break;

                case WeaponUpgradesTypes.FireRate:
                    //set Fire Rate
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_fireRate", val);
                    break;

                case WeaponUpgradesTypes.HearRange:
                    //set Hear Range
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_HearRange", val);
                    break;

                case WeaponUpgradesTypes.ShootRange:
                    //set Shoot Range
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_ShootRange", val);
                    break;

                case WeaponUpgradesTypes.AmmoCapacity:
                    //set Damage
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_AmmoCapacity", val);
                    break;

                case WeaponUpgradesTypes.MeleeAttackDistance:
                    //set melee attack distance
                    PlayerPrefs.SetFloat(currentSelectedWeaponName + "_AttackDistance", val);
                    break;
            }
        }

        /// <summary>
        /// Upgrades the current selected weapon.
        ///And consumes price amount from our In game Currency
        /// </summary>
        void OnUpgradeButtonClick()
        {
            if (currentSelectedUpgradePrice > InGameCurrency)
                return; // if you are making a mobile game, this is a good place to open a in-app purchase shop

            SetUpgradeAsAcquired();
            
            //consume currency
            InGameCurrency -= currentSelectedUpgradePrice;
            PlayerPrefs.SetInt("InGameCurrency", InGameCurrency);

            //update text
            InGameCurrencyText.text = InGameCurrency.ToString() + " $";

            //update upgrade window
            ShowWeaponForUpgrade(currentWpnIndex);

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        public void OpenMainMenu()
        {
            //fade out to scene
            if (MainMenuFader.Instance)
                StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, MainMenuSceneName));
            else
            {
                GameObject _fader = GameObject.Instantiate(Resources.Load("Fader")) as GameObject;

                StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, MainMenuSceneName));
            }

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        public void OnButtonHover()
        {
            //play hover sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonHoverSFX);
        }
    }

    [System.Serializable]
    public class AttributesDefinition
    {
        public string WeaponName;
        public GameObject WeaponMesh;
        public Vector3 WeaponMeshScale = Vector3.one;

        public List<UnitAttribute> AllAttributes = new List<UnitAttribute>();

        public List<AllAttributesCommonUpgrade> AttributeUpgrades = new List<AllAttributesCommonUpgrade>();
    }

    [System.Serializable]
    public class UnitAttribute
    {
        public string AttributeName;
        public WeaponUpgradesTypes weaponUpgradesTypes;

        [Space]
        public float MaxValue = 100;
        public float MinValue = 1;
        public float CurrentValue = 20;

    }

    [System.Serializable]
    public class AllAttributesCommonUpgrade
    {
        public List<AttributeUpgrade> AttributeUpgrade = new List<AttributeUpgrade>();
        public int price;
        public bool UpgradeAcquired;

    }

    [System.Serializable]
    public class AttributeUpgrade
    {
        public string AttributeName;
        public int UpgradeValue;
        public WeaponUpgradesTypes weaponUpgradesTypes;

    }

    public enum WeaponUpgradesTypes
    {
        ShootRange,
        FireRate,
        HearRange,
        Damage,
        MeleeAttackDistance,
        AmmoCapacity
    }

}