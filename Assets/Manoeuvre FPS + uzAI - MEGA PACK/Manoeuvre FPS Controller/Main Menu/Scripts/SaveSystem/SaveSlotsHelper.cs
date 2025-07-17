using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class SaveSlotsHelper : MonoBehaviour
    {
        public string SlotKey ;
        public Text ProgressText;

        private Button _SlotButton;
        private PlayerSaver _PlayerSaver;

        // Use this for initialization
        void Start()
        {
            //get references
            _SlotButton = GetComponentInChildren<Button>();
            _PlayerSaver = FindObjectOfType<PlayerSaver>();

            //set progress message if any
            if (PlayerPrefs.HasKey(SlotKey + "_ProgressMsg"))
                ProgressText.text = PlayerPrefs.GetString(SlotKey + "_ProgressMsg");

            //add listener to the slot's button
            _SlotButton.onClick.AddListener(OnClick_SaveSlot);
        }

        void OnClick_SaveSlot()
        {
            if (_PlayerSaver)
                _PlayerSaver.SavePlayerData(SlotKey);

            //set progress text here
            ProgressText.text = SaveUIController.Instance.ProgressMessage;

            //save it too
            PlayerPrefs.SetString(SlotKey + "_ProgressMsg", SaveUIController.Instance.ProgressMessage);
        }

    }
}