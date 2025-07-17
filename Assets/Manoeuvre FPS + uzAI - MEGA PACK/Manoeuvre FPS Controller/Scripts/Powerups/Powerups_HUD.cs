using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Powerups_HUD : MonoBehaviour
    {
        public PowerupType _PowerupType;

        public Slider durationSlider;
        public Image Icon;

        public float duration;

        Image FillImage;
        CanvasGroup _CanvasGroup;

        // Use this for initialization
        public void Initialize(float _duration , Sprite _icon, PowerupType type)
        {
            _PowerupType = type;

            Icon.sprite = _icon;

            duration = _duration;

            _CanvasGroup = GetComponent<CanvasGroup>();

            FillImage = durationSlider.transform.Find("Fill Area/Fill").GetComponent<Image>();

            //show HUD
            StartCoroutine(LerpAlpha(1));

            //start slider lerp
            StartCoroutine(LerpSlider());

        }


        /// <summary>
        /// Lerps the Alpha from current val to passed val
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        IEnumerator LerpAlpha(float a)
        {
            float t = 0;
            float alpha = _CanvasGroup.alpha;

            while (t < 0.5f)
            {
                _CanvasGroup.alpha = Mathf.Lerp(alpha, a, t / 0.5f);
                t += Time.deltaTime;
                yield return null;

            }

        }

        IEnumerator LerpSlider()
        {
            float val = durationSlider.value;
            float t = 0;

            Color c = FillImage.color;

            while (t< duration)
            {
                durationSlider.value = Mathf.Lerp(val, 0, t / duration);

                FillImage.color = Color.Lerp(c, Color.red, t / duration);
                
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            //hide HUD
            StartCoroutine(LerpAlpha(0));

            //Disable Active status of this power
            DisableActiveStatus();

            //Destroy GameObject
            Destroy(this.gameObject,0.1f);

        }

        void DisableActiveStatus()
        {
            switch (_PowerupType)
            {
                case PowerupType.DamageMultiplier:
                    PowerupsManager.Instance._DamageMultiplier.isActive = false;
                    PowerupsManager.Instance._DamageMultiplier._DamageMultiplierSlot.isActive = false;
                    break;

                case PowerupType.InfiniteAmmo:
                    PowerupsManager.Instance._InfiniteAmmo.isActive = false;
                    PowerupsManager.Instance._InfiniteAmmo._InfiniteAmmoSlot.isActive = false;
                    break;

                case PowerupType.Invincibility:
                    PowerupsManager.Instance._Invincibility.isActive = false;
                    PowerupsManager.Instance._Invincibility._InvincibilitySlot.isActive = false;
                    break;

                case PowerupType.Speedboost:
                    PowerupsManager.Instance._SpeedBoost.isActive = false;
                    PowerupsManager.Instance._SpeedBoost._SpeedBoostSlot.isActive = false;
                    break;
            }
        }

    }
}