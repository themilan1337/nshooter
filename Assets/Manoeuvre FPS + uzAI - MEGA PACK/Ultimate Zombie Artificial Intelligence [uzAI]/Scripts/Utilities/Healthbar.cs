using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uzAI
{
    public class Healthbar : MonoBehaviour
    {

        public float lerpDuration = 0.2f;

        Slider healthSlider;
        uzAIZombieHealth health;
        GameObject mainCamera;

        // Use this for initialization
        void Start()
        {
            health = GetComponentInParent<uzAIZombieStateManager>().ZombieHealthStats;
            healthSlider = GetComponentInChildren<Slider>();
            health.healthBar = this;

            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

            if (health != null && healthSlider != null)
            {
                healthSlider.maxValue = health.Health;
                healthSlider.value = health.Health;
            }


        }

        public IEnumerator LerpSlider()
        {

            GetComponent<CanvasGroup>().alpha = 1;

            float et = 0;
            while (et < lerpDuration)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, health.CurrentHealth, et / lerpDuration);
                et += Time.deltaTime;

                yield return null;
            }

            if (health.CurrentHealth <= 0)
                Destroy(this.gameObject);
        }

        public IEnumerator HideHealthBar()
        {
            float t = 0;
            float a = GetComponent<CanvasGroup>().alpha;

            //lerp slider alpha to 0
            while (t < 0.15f)
            {
                a = Mathf.Lerp(a, 0, t / 0.15f);
                t += Time.deltaTime;

                GetComponent<CanvasGroup>().alpha = a;
                yield return null;
            }
        }

        private void Update()
        {
            if(mainCamera)
                transform.LookAt(mainCamera.transform);
        }

    }

}