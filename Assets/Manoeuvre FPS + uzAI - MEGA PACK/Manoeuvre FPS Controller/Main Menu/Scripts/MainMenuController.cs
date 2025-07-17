using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class MainMenuController : MonoBehaviour
    {
        public Text ButtonInfoText;
        public string StartBtnHoverText;
        public string LoadBtnHoverText;
        public string ShopBtnHoverText;
        public string OptionsBtnHoverText;
        public string QuitBtnHoverText;
        public string LoadSlotBtnHoverText;

        [Space]
        public string TextureQualityText;
        public string AntiAliasingText;
        public string ShadowsText;
        public string VSyncText;
        public string ResolutionText;

        [Space]
        public Animator _animator;
        public string OptionsMenu_OpenAnimation;
        public string OptionsMenu_CloseAnimation;
        public string LoadMenu_OpenAnimation;
        public string LoadMenu_CloseAnimation;

        [Space]
        public string SceneSelectName;
        public string ShopSelectName;

        [Space]
        public string ButtonClickSFX = "Button Click";
        public string ButtonHoverSFX = "Button Hover";

        [Space]
        public bool HideCursor;

        // Use this for initialization
        void Start()
        {
            //fade out to scene
            if(MainMenuFader.Instance)
                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            else
            {
                GameObject _fader = GameObject.Instantiate(Resources.Load("Fader")) as GameObject;

                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            }

            //create Audio Manager
            if (!MainMenuAudioManager.Instance)
                Instantiate(Resources.Load("AudioManager"));

            //toggle cursor visibility
            ToggleCursor();
        }

        void ToggleCursor()
        {
            if (HideCursor)
            {
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;

            }
            else
            {
                //Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
        }

        public void SceneSelect()
        {
            //Fade In Scene
            StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, SceneSelectName));

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        public void ShopSelect()
        {
            //Fade In Scene
            StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, ShopSelectName));

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        /// <summary>
        /// Changes the Text On Hover of any button.
        /// </summary>
        public void OnHoverChangeText(string ButtonName)
        {
            switch (ButtonName)
            {
                case "Start":
                    ButtonInfoText.text = StartBtnHoverText;
                    break;

                case "Load":
                    ButtonInfoText.text = LoadBtnHoverText;
                    break;

                case "Shop":
                    ButtonInfoText.text = ShopBtnHoverText;
                    break;

                case "Options":
                    ButtonInfoText.text = OptionsBtnHoverText;
                    break;

                case "Quit":
                    ButtonInfoText.text = QuitBtnHoverText;
                    break;

                case "Texture Quality":
                    ButtonInfoText.text = TextureQualityText;
                    break;

                case "Anti Aliasing":
                    ButtonInfoText.text = AntiAliasingText;
                    break;

                case "Shadows":
                    ButtonInfoText.text = ShadowsText;
                    break;

                case "VSync":
                    ButtonInfoText.text = VSyncText;
                    break;

                case "Resolution":
                    ButtonInfoText.text = ResolutionText;
                    break;

                case "Load Slot":
                    ButtonInfoText.text = LoadSlotBtnHoverText;
                    break;

            }

            //play hover sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonHoverSFX);
        }

        /// <summary>
        /// Opens the options panel.
        /// </summary>
        public void ToggleOptionsMenu(bool open)
        {
            if (open)
                _animator.Play(OptionsMenu_OpenAnimation);
            else
                _animator.Play(OptionsMenu_CloseAnimation);

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);

        }

        /// <summary>
        /// Opens the options panel.
        /// </summary>
        public void ToggleLoadMenu(bool open)
        {
            if (open)
                _animator.Play(LoadMenu_OpenAnimation);
            else
                _animator.Play(LoadMenu_CloseAnimation);

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);

        }

        public void SetInputButton(GameObject Btn)
        {
            StartCoroutine(SelectButtonInUI(Btn));
        }

        IEnumerator SelectButtonInUI(GameObject Btn)
        {
            yield return new WaitForSeconds(0.5f);

            FindObjectOfType<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(Btn);
            Btn.GetComponent<Button>().Select();
        }

        /// <summary>
        /// Exit the Game
        /// </summary>
        public void ExitApplication()
        {
            Application.Quit();

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }
    }
}