using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manoeuvre
{
    public class GameOver : MonoBehaviour
    {

        public float RestartTextDelay =  4f;

        [HideInInspector]
        public GameObject GameOverUI;
        [HideInInspector]
        public GameObject RestartText;

        bool canRestart;

        public static GameOver Instance;

        [HideInInspector]
        public bool isGameOver;

        private void Awake()
        {
            Instance = this;

            RestartText = GameObject.Find("RestartText");
            RestartText.SetActive(false);

            GameOverUI = GameObject.Find("GameOver");
            GameOverUI.SetActive(false);
            GameOverUI.GetComponent<CanvasGroup>().alpha = 0;


        }

        public void EnableGameOver()
        {
            isGameOver = true;

            GameOverUI.SetActive(true);
            StartCoroutine(ShowGameOverUI());

            Invoke("EnableRestartText", RestartTextDelay);
        }

        IEnumerator ShowGameOverUI()
        {
            float t = 0;
            while(t < 1f)
            {
                GameOverUI.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(GameOverUI.GetComponent<CanvasGroup>().alpha, 1, t);
                t += Time.deltaTime;
                yield return null;
            }
        }

        void EnableRestartText()
        {
            RestartText.SetActive(true);
            canRestart = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!canRestart)
                return;

            if (Input.anyKeyDown)
                RestartThisScene();
        }

        public void RestartThisScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}