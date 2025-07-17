using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Manoeuvre
{
    public class SaveUIController : MonoBehaviour
    {
        public string MainMenuSceneName = "MainMenu";

        public GameObject ManoeuvreFPSUI;
        public GameObject PauseMenuUI;
        public GameObject LoadUI;
        public GameObject SaveUI;

        [HideInInspector]
        public string ProgressMessage;

        public static SaveUIController Instance;

        [HideInInspector]
        public bool isOpen;

        ManoeuvreFPSController fPSController;

        private void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            fPSController = ManoeuvreFPSController.Instance;
        }

        private void Update()
        {
            if (isOpen)
                return;

            if (ManoeuvreFPSController.Instance.Inputs.pauseInput)
                OpenPauseMenuUI();
        }

        public void OpenSaveUI()
        {
            //almost pause
            Time.timeScale = 0.00001f;

            //enable correct UI
            SaveUI.SetActive(true);
            ManoeuvreFPSUI.SetActive(false);

            //disable controller
            fPSController.GetComponent<CharacterController>().enabled = false;

            //update flag
            isOpen = true;
        }
       
        void OpenPauseMenuUI()
        {
            //almost pause
            Time.timeScale = 0.00001f;

            //enable correct UI
            SaveUI.SetActive(false);
            PauseMenuUI.SetActive(true);
            ManoeuvreFPSUI.SetActive(false);

            //disable controller
            fPSController.GetComponent<CharacterController>().enabled = false;

            //update flag
            isOpen = true;
        }

        public void OnClick_LoadGame()
        {
            //enable correct UI
            ManoeuvreFPSUI.SetActive(false);
            PauseMenuUI.SetActive(false);
            SaveUI.SetActive(false);
            LoadUI.SetActive(true);

            LoadSlotsHelper[] _helpers = LoadUI.GetComponentsInChildren<LoadSlotsHelper>();

            foreach (LoadSlotsHelper _h in _helpers)
                _h.RefreshUI();

        }

        public void OnClick_MainMenu()
        {
            //make sure time scale is 1
            Time.timeScale = 1;

            PlayerPrefs.DeleteKey("LastSavedKey");
            SceneManager.LoadScene(MainMenuSceneName);

        }

        public void CloseGameplayUI()
        {
            //resume time
            Time.timeScale = 01;

            //enable controller
            fPSController.GetComponent<CharacterController>().enabled = true;

            //enable correct UI
            SaveUI.SetActive(false);
            PauseMenuUI.SetActive(false);
            LoadUI.SetActive(false);

            ManoeuvreFPSUI.SetActive(true);

            //update flag
            isOpen = false;


        }

        public void OnClick_CloseLoadGame()
        {
            //enable correct UI
            ManoeuvreFPSUI.SetActive(false);
            PauseMenuUI.SetActive(true);
            SaveUI.SetActive(false);
            LoadUI.SetActive(false);
        }

    }
}