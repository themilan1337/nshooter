using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    [RequireComponent(typeof(Button))]
    public class SceneObject : MonoBehaviour
    {
        public Image SceneImage;
        public Text SceneNameText;
        public Text SceneCountText;

        string SceneToLoad;

        public void Initialize(Sprite _img, string displayName, string sceneName, int Count)
        {
            SceneToLoad = sceneName;

            //set UI
            SceneImage.sprite = _img;
            SceneNameText.text = displayName;
            SceneCountText.text = (Count + 1).ToString();

            //add on click listener
            GetComponent<Button>().onClick.AddListener(OnSceneObjectClick);

        }

        void OnSceneObjectClick()
        {
            //delete the saved key
            PlayerPrefs.DeleteKey("LastSavedKey");

            if (MainMenuFader.Instance)
               StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, SceneToLoad));

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(FindObjectOfType<MainMenuSceneSelection>().ButtonClickSFX);

            //stop Audio Manager
            if (FindObjectOfType<MainMenuAudioManager>())
                FindObjectOfType<MainMenuAudioManager>().DestroyAudioManager();

        }

        public void OnSceneObjectHover()
        {
            //play hover sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(FindObjectOfType<MainMenuSceneSelection>().ButtonHoverSFX);
        }
    }
}