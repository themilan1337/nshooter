using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class ItemIdentifier_Powerups : MonoBehaviour
    {
        public PowerupType _PowerupType;
        public Image Icon;
        public Text powerupsCountText;

        PowerupsManager _PowerupsManager;
        public bool isActive;

        // Use this for initialization
        void Start()
        {
            _PowerupsManager = FindObjectOfType<PowerupsManager>();

            SetUI();
        }

        /// <summary>
        /// Sets the UI based on the Item Type
        /// </summary>
        public void SetUI()
        {
            switch (_PowerupType)
            {
                case PowerupType.Healthkit:
                    Icon.sprite = _PowerupsManager._HealthKit.icon;
                    powerupsCountText.text = _PowerupsManager._HealthKit.powerupCount.ToString();
                    break;

                case PowerupType.Invincibility:
                    Icon.sprite = _PowerupsManager._Invincibility.icon;
                    powerupsCountText.text = _PowerupsManager._Invincibility.powerupCount.ToString();
                    break;

                case PowerupType.Speedboost:
                    Icon.sprite = _PowerupsManager._SpeedBoost.icon;
                    powerupsCountText.text = _PowerupsManager._SpeedBoost.powerupCount.ToString();
                    break;

                case PowerupType.DamageMultiplier:
                    Icon.sprite = _PowerupsManager._DamageMultiplier.icon;
                    powerupsCountText.text = _PowerupsManager._DamageMultiplier.powerupCount.ToString();
                    break;

                case PowerupType.InfiniteAmmo:
                    Icon.sprite = _PowerupsManager._InfiniteAmmo.icon;
                    powerupsCountText.text = _PowerupsManager._InfiniteAmmo.powerupCount.ToString();
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(isActive)
            {
                GetComponent<Button>().enabled = false;
                GetComponent<CanvasGroup>().alpha = 0.2f;
            }else
            {
                GetComponent<Button>().enabled = true;
                GetComponent<CanvasGroup>().alpha = 1f;
            }
        }

        public void OnItemSelect()
        {
            isActive = true;

            switch (_PowerupType)
            {
                case PowerupType.Healthkit:

                    //Consume the health kit 
                    _PowerupsManager._HealthKit.Consume();

                    break;

                case PowerupType.Invincibility:

                    //Consume the Invincibility with the picked up _Invincibility properties
                    _PowerupsManager._Invincibility.Consume();

                    break;

                case PowerupType.Speedboost:

                    //Consume the Speedboost with the picked up _Speedboost properties
                    _PowerupsManager._SpeedBoost.Consume(); 
                    break;

                case PowerupType.DamageMultiplier:

                    //Consume the DamageMultiplier with the picked up _DamageMultiplier properties
                    _PowerupsManager._DamageMultiplier.Consume();
                    break;

                case PowerupType.InfiniteAmmo:

                    //Consume the InfiniteAmmo with the picked up _InfiniteAmmo properties
                    _PowerupsManager._InfiniteAmmo.Consume();
                    break;
            }

        }
    }
}