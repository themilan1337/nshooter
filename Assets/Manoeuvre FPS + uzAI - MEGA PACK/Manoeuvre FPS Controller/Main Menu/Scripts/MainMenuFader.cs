using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Manoeuvre
{
    public class MainMenuFader : MonoBehaviour
    {
        public Image BlackFader;

        public static MainMenuFader Instance;

        UnityEngine.EventSystems.EventSystem eventSystem;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(Instance.gameObject);
        }

        public IEnumerator Fade(float alpha, float duration)
        {
            eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

            //disable all input
            if (eventSystem)
                eventSystem.enabled = false;

            BlackFader.enabled = true;
            float et = 0;

            while (et < duration)
            {
                BlackFader.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(BlackFader.GetComponent<CanvasGroup>().alpha, alpha, et / duration);
                et += Time.deltaTime;
                yield return null;
            }

            BlackFader.GetComponent<CanvasGroup>().alpha = alpha;

            //hide image for interactability
            if (alpha == 0)
                BlackFader.enabled = false;

            //enable all input
            if (eventSystem)
                eventSystem.enabled = true;

        }

        public IEnumerator FadeToScene(float duration, string SceneName)
        {
            eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

            //disable all input
            if (eventSystem)
                eventSystem.enabled = false;

            BlackFader.enabled = true;
            float et = 0;

            while (et < duration)
            {
                BlackFader.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1, et / duration);
                et += Time.deltaTime;
                yield return null;
            }

            BlackFader.GetComponent<CanvasGroup>().alpha = 1;

            //enable all input
            if (eventSystem)
                eventSystem.enabled = true;

            LoadThisScene(SceneName);

        }

        public void LoadThisScene(string SceneName)
        {
            SceneManager.LoadScene(SceneName);
        }

    }
}