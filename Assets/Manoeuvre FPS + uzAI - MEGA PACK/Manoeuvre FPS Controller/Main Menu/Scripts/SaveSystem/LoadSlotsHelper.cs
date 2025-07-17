using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Manoeuvre
{
    public class LoadSlotsHelper : MonoBehaviour
    {
        public string SlotKey;
        public Text ProgressText;

        private Button _SlotButton;

        // Use this for initialization
        void Start()
        {
            //get references
            _SlotButton = GetComponentInChildren<Button>();

            //add listener to the slot's button
            _SlotButton.onClick.AddListener(OnClick_LoadSlot);

            RefreshUI();
        }

        public void RefreshUI()
        {
            //set progress message if any
            if (PlayerPrefs.HasKey(SlotKey + "_ProgressMsg"))
                ProgressText.text = PlayerPrefs.GetString(SlotKey + "_ProgressMsg");
        }

        void OnClick_LoadSlot()
        {
            if (!PlayerPrefs.HasKey(SlotKey + "_LoadedScene"))
                return;

            if (FindObjectOfType<MainMenuAudioManager>())
                FindObjectOfType<MainMenuAudioManager>().DestroyAudioManager();

            //make sure time scale is 1
            Time.timeScale = 1;

            PlayerPrefs.SetString("LastSavedKey", SlotKey);
            SceneManager.LoadScene(PlayerPrefs.GetString(SlotKey + "_LoadedScene"));
        }

    }
}