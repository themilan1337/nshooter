using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class MainMenuSceneSelection : MonoBehaviour
    {
        public int MaxAllowedSceneObjects = 3;
        [Space]
        public Transform SceneObjectsContainer;
        public GameObject SceneObjectPrefab;

        [Space]
        public Button NextButton;
        public Button PreviousButton;

        [Space]
        public string ButtonClickSFX = "Button Click";
        public string ButtonHoverSFX = "Button Hover";

        [Space]
        public List<MainMenuSceneObject> MainMenuSceneObject = new List<MainMenuSceneObject>();

        //this will be selected as soon as game starts
        GameObject FirstSceneObject;

        //ui event system
        UnityEngine.EventSystems.EventSystem eventSystem;

        //all the spawned scene objects
        List<GameObject> AllSceneObjects = new List<GameObject>();

        //see which indexed item is enabled 
        int lastItemEnabledIndex = 0;

        // Use this for initialization
        void Start()
        {
            //fade out to scene
            if (MainMenuFader.Instance)
                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            else
            {
                GameObject _fader = GameObject.Instantiate(Resources.Load("Fader")) as GameObject;

                StartCoroutine(MainMenuFader.Instance.Fade(0, 1f));
            }

            //create Audio Manager
            if (!MainMenuAudioManager.Instance)
                Instantiate(Resources.Load("AudioManager"));

            //find event system
            eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();

            //spawn scene objects from start
            SpawnSceneObjects();

            //enable from 0 index at start
            EnableSceneObjects_Forward();

            //add next and prvs btn listeners
            NextButton.onClick.AddListener(OnNextButtonClick);
            PreviousButton.onClick.AddListener(OnPreviousButtonClick);

        }

        /// <summary>
        /// Spawn the scene objects which are added in the inspector
        /// </summary>
        void SpawnSceneObjects()
        {
            for(int i = 0; i< MainMenuSceneObject.Count; i++)
            {
                GameObject _so = Instantiate(SceneObjectPrefab);
                _so.transform.SetParent(SceneObjectsContainer);
                _so.transform.localEulerAngles = Vector3.zero;
                _so.transform.localPosition = Vector3.zero;
                _so.transform.localScale = Vector3.one;

                _so.GetComponent<SceneObject>().Initialize(MainMenuSceneObject[i].SceneImage, 
                    MainMenuSceneObject[i].SceneNameToDisplay, 
                    MainMenuSceneObject[i].SceneNameInBuildSettings,
                    i);

                MainMenuSceneObject[i].mySceneObject = _so;

                //hide it
                _so.SetActive(false);

                //add in list
                AllSceneObjects.Add(_so);

                //get first instantiated scene object
                if (!FirstSceneObject)
                    FirstSceneObject = _so;
            }

            eventSystem.firstSelectedGameObject = FirstSceneObject;
            FirstSceneObject.GetComponent<Button>().Select();
        }

        /// <summary>
        /// Next button to display next set of scene objects 
        /// </summary>
        void OnNextButtonClick()
        {
            EnableSceneObjects_Forward();

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        /// <summary>
        /// Previous button to display previous set of scene objects
        /// </summary>
        void OnPreviousButtonClick()
        {
            EnableSceneObjects_Backward();

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        /// <summary>
        /// Enable the scene objects from the given index to max allowed count
        /// </summary>
        /// <param name="fromIndex"></param>
        void EnableSceneObjects_Forward()
        {
            foreach (GameObject g in AllSceneObjects)
                g.SetActive(false);

            for(int i = lastItemEnabledIndex; i <= AllSceneObjects.Count; i++)
            {
                if(i < AllSceneObjects.Count)
                {
                    if (i < (lastItemEnabledIndex + MaxAllowedSceneObjects))
                        AllSceneObjects[i].SetActive(true);
                    else
                        AllSceneObjects[i].SetActive(false);

                    if (i == (lastItemEnabledIndex + MaxAllowedSceneObjects))
                    {
                        lastItemEnabledIndex = i;
                        break;
                    }
                }
                
            }
        }

        void EnableSceneObjects_Backward()
        {
            ///if the first scene object is enabled than there's no point of going back
            if (AllSceneObjects[0].activeInHierarchy)
                return;
           
            int j = 0;

            //we get the first item that's enabled
            for(int k = 0; k< AllSceneObjects.Count; k++)
            {
                if(AllSceneObjects[k].activeInHierarchy)
                {
                    j = k;
                    break;
                }
            }

            //set last item enabled
            lastItemEnabledIndex = j;

            foreach (GameObject g in AllSceneObjects)
                g.SetActive(false);

            //now rolling back from j
            for (int i = (j-1); i > -1; i--)
            {
                if (i > (j - 1 - MaxAllowedSceneObjects))
                {
                    AllSceneObjects[i].SetActive(true);
                    
                }
            }

        }

        public void OnMainMenuClick()
        {
            if (MainMenuFader.Instance)
                StartCoroutine(MainMenuFader.Instance.FadeToScene(1f, "Main Menu"));

            //play click sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonClickSFX);
        }

        public void OnButtonHover()
        {
            //play hover sound
            if (MainMenuAudioManager.Instance)
                MainMenuAudioManager.Instance.PlayAudioClip(ButtonHoverSFX);
        }
    }

    [System.Serializable]
    public class MainMenuSceneObject
    {
        public string SceneNameToDisplay;
        public string SceneNameInBuildSettings;

        public Sprite SceneImage;

        public GameObject mySceneObject;
    }
}