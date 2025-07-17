using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class gc_Minimap : MonoBehaviour
    {
        public GameObject minimapIconPrefab;
        public float maxZoom = 20f;
        public float minZoom = 8f;
        public float zoomAmount = 2f;
        public float ZoomDuration = 2f;

        public List<MinimapIcon> MinimapIcons = new List<MinimapIcon>();

        public static gc_Minimap Instance;
        Transform mainCamera;

        [HideInInspector]
        public Camera minimapCamera;

        [HideInInspector]
        public List<GameObject> AllIcons = new List<GameObject>();

        // Use this for initialization
        void Awake()
        {
            Instance = this;
            mainCamera = Camera.main.transform ;
            minimapCamera = GameObject.Find("MinimapCamera").GetComponent<Camera>();

        }

        private void Start()
        {
            AttachMinimapIcon();

        }

        void FixedUpdate()
        {
            if (mainCamera)
            {
                minimapCamera.transform.eulerAngles = new Vector3(minimapCamera.transform.eulerAngles.x, mainCamera.eulerAngles.y, 0);
            }

            if (ManoeuvreFPSController.Instance.Inputs.zoomOutInput)
            {
                ManoeuvreFPSController.Instance.Inputs.zoomOutInput = false;
                StopAllCoroutines();
                StartCoroutine(ZoomMinimap(true));
            }
            if (ManoeuvreFPSController.Instance.Inputs.zoomInInput)
            {
                ManoeuvreFPSController.Instance.Inputs.zoomInInput = false;
                StopAllCoroutines();
                StartCoroutine(ZoomMinimap(false));
            }
        }

        IEnumerator ZoomMinimap(bool zoomOut)
        {
            float t = 0;
            float currentSize = minimapCamera.orthographicSize;
            float toSize = currentSize - zoomAmount;

            if (zoomOut)
                toSize = currentSize + zoomAmount;

            while(t < ZoomDuration)
            {
                minimapCamera.orthographicSize = Mathf.Lerp(currentSize, toSize,  t/ZoomDuration);
                minimapCamera.orthographicSize = Mathf.Clamp(minimapCamera.orthographicSize, minZoom, maxZoom);

                t += Time.deltaTime;

                yield return null;
            }
        }

        /// <summary>
        /// A complex method which will first look for every tag in Minimap Icons
        /// Then for that tag create a dynamic array
        /// Then for each element of that dynamic array add Icon and set it's color and size
        /// <param name="parentTransform"></param>
        void AttachMinimapIcon()
        {
            //Find transforms
            for (int i = 0; i < MinimapIcons.Count; i++)
            {

                if(MinimapIcons[i].Tag == "ShooterAI-Companion" )
                {
                    foreach (GameObject go in GameObject.FindGameObjectsWithTag("ShooterAI"))
                    {
                        if (go.GetComponent<ShooterAIStateManager>()._AIType == AIType.Companion)
                        {

                            GameObject icon = Instantiate(minimapIconPrefab) as GameObject;
                            icon.name = "MinimapIcon";
                            icon.transform.SetParent(go.transform);
                            icon.transform.localRotation = Quaternion.Euler(90, 0, 0);
                            icon.transform.localPosition = new Vector3(0, 2f, 0);

                            //add in list
                            AllIcons.Add(icon);
                        
                            for(int mi = 0; mi < MinimapIcons.Count; mi++)
                            {
                                if(MinimapIcons[mi].Tag == "ShooterAI-Companion")
                                {
                                    //set scale and color
                                    icon.transform.localScale = new Vector3(MinimapIcons[mi].iconScale, MinimapIcons[mi].iconScale, MinimapIcons[mi].iconScale);
                                    icon.GetComponent<Renderer>().material.color = MinimapIcons[mi].minimapIconColor;

                                }
                                
                            }
                           
                        }
                    }
                   
                }
                else if (MinimapIcons[i].Tag == "ShooterAI-Enemy")
                {
                    foreach (GameObject go in GameObject.FindGameObjectsWithTag("ShooterAI"))
                    {
                        if (go.GetComponent<ShooterAIStateManager>()._AIType == AIType.Enemy)
                        {

                            GameObject icon = Instantiate(minimapIconPrefab) as GameObject;
                            icon.name = "MinimapIcon";
                            icon.transform.SetParent(go.transform);
                            icon.transform.localRotation = Quaternion.Euler(90, 0, 0);
                            icon.transform.localPosition = new Vector3(0, 2f, 0);

                            //add in list
                            AllIcons.Add(icon);

                            for (int mi = 0; mi < MinimapIcons.Count; mi++)
                            {
                                if (MinimapIcons[mi].Tag == "ShooterAI-Enemy")
                                {
                                    //set scale and color
                                    icon.transform.localScale = new Vector3(MinimapIcons[mi].iconScale, MinimapIcons[mi].iconScale, MinimapIcons[mi].iconScale);
                                    icon.GetComponent<Renderer>().material.color = MinimapIcons[mi].minimapIconColor;

                                }

                            }

                        }
                    }

                }
                else
                {
                    foreach (GameObject go in GameObject.FindGameObjectsWithTag(MinimapIcons[i].Tag))
                    {
                        GameObject icon = Instantiate(minimapIconPrefab) as GameObject;
                        icon.name = "MinimapIcon";
                        icon.transform.SetParent(go.transform);
                        icon.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        icon.transform.localPosition = new Vector3(0, 2f, 0);

                        //set scale and color
                        icon.transform.localScale = new Vector3(MinimapIcons[i].iconScale, MinimapIcons[i].iconScale, MinimapIcons[i].iconScale);
                        icon.GetComponent<Renderer>().material.color = MinimapIcons[i].minimapIconColor;

                        //add in list
                        AllIcons.Add(icon);
                    }
                }

            }
            
        }

        /// <summary>
        /// Can be called from a Third Party Script if that wants itself to be included in the icons list
        /// </summary>
        /// <param name="Tag"></param>
        public void AttachMinimapIconManually(Transform Tag)
        {
            //Find transforms
            for (int i = 0; i < MinimapIcons.Count; i++)
            {
                if (MinimapIcons[i].Tag == Tag.tag)
                { 
                    GameObject icon = Instantiate(minimapIconPrefab) as GameObject;
                    icon.name = "MinimapIcon";
                    icon.transform.SetParent(Tag);
                    icon.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    icon.transform.localPosition = new Vector3(0, 2f, 0);

                    //set scale and color
                    icon.transform.localScale = new Vector3(MinimapIcons[i].iconScale, MinimapIcons[i].iconScale, MinimapIcons[i].iconScale);
                    icon.GetComponent<Renderer>().material.color = MinimapIcons[i].minimapIconColor;

                    //add in list
                    AllIcons.Add(icon);
                }
            }
        }

        /// <summary>
        /// Finds and removes the icon from the List and Destroy it
        /// </summary>
        /// <param name="icon"></param>
        public void RemoveMinimapIcon(GameObject icon)
        {
            for(int i =0; i< AllIcons.Count; i++)
            {
                if(AllIcons[i] == icon)
                {
                    Destroy(AllIcons[i]);
                    AllIcons.RemoveAt(i);
                    break;
                }
            }

        }

    }

    [System.Serializable]
    public class MinimapIcon
    {
        [Tooltip("Tag whose properties you are tweaking down below")]
        public string Tag = "uzAIZombie";
        
        [Tooltip("Color of the Icon")]
        public Color minimapIconColor = Color.white;

        [Tooltip("Scale of the Icon in Minimap")]
        [Range(1f,3f)]
        public float iconScale = 1f;

    }
}