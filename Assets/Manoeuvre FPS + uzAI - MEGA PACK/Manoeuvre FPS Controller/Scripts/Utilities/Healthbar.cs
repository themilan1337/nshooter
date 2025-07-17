using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class Healthbar : MonoBehaviour
    {

        public float lerpDuration = 0.2f;

        Slider healthSlider;
        TurretHealth _th_health;
        DestructibleProps _dp_health;
        ShooterAIStateManager _sAI_health;
        GameObject mainCamera;

        // Use this for initialization
        void Start()
        {
            if (GetComponentInParent<Turret>())
            {
                _th_health = GetComponentInParent<Turret>()._turretHealth;
                healthSlider = GetComponentInChildren<Slider>();
                _th_health.healthBar = this;
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                if (_th_health != null && healthSlider != null)
                {
                    healthSlider.maxValue = _th_health.Health;
                    healthSlider.value = _th_health.Health;
                }
            }

            if (GetComponentInParent<DestructibleProps>())
            {
                _dp_health = GetComponentInParent<DestructibleProps>();
                healthSlider = GetComponentInChildren<Slider>();
                _dp_health.healthBar = this;
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                if (_dp_health != null && healthSlider != null)
                {
                    healthSlider.maxValue = _dp_health.Health;
                    healthSlider.value = _dp_health.Health;
                }

            }

            if (GetComponentInParent<ShooterAIStateManager>())
            {
                _sAI_health = GetComponentInParent<ShooterAIStateManager>();
                healthSlider = GetComponentInChildren<Slider>();
                _sAI_health.Health.healthBar = this;
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

                if (_sAI_health != null && healthSlider != null)
                {
                    healthSlider.maxValue = _sAI_health.Health.Health;
                    healthSlider.value = _sAI_health.Health.Health;
                }

            }
        }

        public void StartLerp()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (GetComponentInParent<Turret>())
                StartCoroutine(TurretLerpSlider());
            else if (GetComponentInParent<DestructibleProps>())
                StartCoroutine(DestructiblePropsLerpSlider());
            else if(GetComponentInParent<ShooterAIStateManager>())
                StartCoroutine(ShooterAILerpSlider());
        }

        IEnumerator TurretLerpSlider()
        {
            GetComponent<CanvasGroup>().alpha = 1;

            float et = 0;
            while (et < lerpDuration)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, _th_health.Health, et / lerpDuration);
                et += Time.deltaTime;

                yield return null;
            }

            if (_th_health.Health <= 0)
                Destroy(this.gameObject);
        }

        IEnumerator DestructiblePropsLerpSlider()
        {
            GetComponent<CanvasGroup>().alpha = 1;

            float et = 0;
            while (et < lerpDuration)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, _dp_health.Health, et / lerpDuration);
                et += Time.deltaTime;

                yield return null;
            }

            if (_dp_health.Health <= 0)
                Destroy(this.gameObject);
        }

        IEnumerator ShooterAILerpSlider()
        {
            GetComponent<CanvasGroup>().alpha = 1;

            float et = 0;
            while (et < lerpDuration)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, _sAI_health.Health.Health, et / lerpDuration);
                et += Time.deltaTime;

                yield return null;
            }

            if (_sAI_health.Health.Health <= 0)
                Destroy(this.gameObject);
        }


        private void Update()
        {
            if (mainCamera)
                transform.LookAt(mainCamera.transform);
        }

    }

}