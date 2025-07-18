using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class gc_Minimap : MonoBehaviour
    {
        [Header("Setup")]
        public GameObject minimapIconPrefab;
        public List<MinimapIcon> MinimapIcons = new List<MinimapIcon>();

        [Header("Zoom Settings")]
        public float maxZoom = 20f;
        public float minZoom = 8f;
        public float zoomAmount = 2f;
        public float ZoomDuration = 2f;

        public static gc_Minimap Instance;

        [HideInInspector]
        public Camera minimapCamera;
        [HideInInspector]
        public List<GameObject> AllIcons = new List<GameObject>();
        
        private Transform mainCamera;

        void Awake()
        {
            // --- FIX: Proper Singleton Pattern ---
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning("Duplicate instance of gc_Minimap found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            // --- FIX: Added robust checks to prevent NullReferenceException ---
            // 1. Check for the Main Camera
            if (Camera.main == null)
            {
                Debug.LogError("gc_Minimap Error: No camera in the scene is tagged 'MainCamera'. Please tag your main player camera and ensure it's active. Disabling minimap script.", this);
                this.enabled = false;
                return;
            }
            mainCamera = Camera.main.transform;

            // 2. Check for the Minimap Camera GameObject
            GameObject minimapCameraGO = GameObject.Find("MinimapCamera");
            if (minimapCameraGO == null)
            {
                Debug.LogError("gc_Minimap Error: Could not find a GameObject named 'MinimapCamera' in the scene. Disabling minimap script.", this);
                this.enabled = false;
                return;
            }

            // 3. Check for the Camera component on the Minimap Camera
            minimapCamera = minimapCameraGO.GetComponent<Camera>();
            if (minimapCamera == null)
            {
                Debug.LogError("gc_Minimap Error: The GameObject 'MinimapCamera' is missing the 'Camera' component. Disabling minimap script.", this);
                this.enabled = false;
                return;
            }
        }

        private void Start()
        {
            // --- FIX: Ensure the icon prefab is assigned in the inspector ---
            if (minimapIconPrefab == null)
            {
                Debug.LogError("gc_Minimap Error: 'Minimap Icon Prefab' is not assigned in the Inspector. Cannot create icons. Disabling minimap script.", this);
                this.enabled = false;
                return;
            }

            AttachMinimapIcon();
        }

        void FixedUpdate()
        {
            // Check if mainCamera is valid (it might have been destroyed)
            if (mainCamera != null)
            {
                minimapCamera.transform.eulerAngles = new Vector3(minimapCamera.transform.eulerAngles.x, mainCamera.eulerAngles.y, 0);
            }

            // --- FIX: Check if the FPS controller instance exists before using it ---
            if (ManoeuvreFPSController.Instance != null && ManoeuvreFPSController.Instance.Inputs != null)
            {
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
        }

        IEnumerator ZoomMinimap(bool zoomOut)
        {
            float t = 0;
            float currentSize = minimapCamera.orthographicSize;
            float targetSize = zoomOut ? currentSize + zoomAmount : currentSize - zoomAmount;
            
            // Clamp the target size to stay within min/max bounds
            targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

            while (t < ZoomDuration)
            {
                minimapCamera.orthographicSize = Mathf.Lerp(currentSize, targetSize, t / ZoomDuration);
                t += Time.deltaTime;
                yield return null;
            }
            
            // Ensure the final size is set exactly
            minimapCamera.orthographicSize = targetSize;
        }

        /// <summary>
        /// A complex method which will first look for every tag in Minimap Icons
        /// Then for that tag create a dynamic array
        /// Then for each element of that dynamic array add Icon and set it's color and size
        /// </summary>
        // --- REFACTORED for efficiency and clarity ---
        void AttachMinimapIcon()
        {
            // Iterate through each icon definition provided in the inspector
            foreach (MinimapIcon iconSettings in MinimapIcons)
            {
                // Special handling for ShooterAI types which are distinguished by a component value, not a tag
                if (iconSettings.Tag == "ShooterAI-Companion" || iconSettings.Tag == "ShooterAI-Enemy")
                {
                    AIType targetAIType = (iconSettings.Tag == "ShooterAI-Companion") ? AIType.Companion : AIType.Enemy;
                    
                    // Find all GameObjects with the generic "ShooterAI" tag
                    GameObject[] shooterAIs = GameObject.FindGameObjectsWithTag("ShooterAI");

                    foreach (GameObject go in shooterAIs)
                    {
                        ShooterAIStateManager stateManager = go.GetComponent<ShooterAIStateManager>();
                        // Check if the AI has the state manager and matches the desired type
                        if (stateManager != null && stateManager._AIType == targetAIType)
                        {
                            CreateIconFor(go, iconSettings);
                        }
                    }
                }
                else // Handle all other standard tags
                {
                    GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(iconSettings.Tag);
                    foreach (GameObject go in taggedObjects)
                    {
                        CreateIconFor(go, iconSettings);
                    }
                }
            }
        }
        
        // --- NEW helper method to reduce code duplication ---
        private void CreateIconFor(GameObject target, MinimapIcon iconSettings)
        {
            if (target == null || iconSettings == null) return;

            GameObject icon = Instantiate(minimapIconPrefab);
            icon.name = "MinimapIcon";
            icon.transform.SetParent(target.transform, false); // 'false' keeps local orientation
            icon.transform.localRotation = Quaternion.Euler(90, 0, 0);
            icon.transform.localPosition = new Vector3(0, 2f, 0);

            // Set scale and color from settings
            icon.transform.localScale = Vector3.one * iconSettings.iconScale;
            
            Renderer iconRenderer = icon.GetComponent<Renderer>();
            if(iconRenderer != null)
            {
                iconRenderer.material.color = iconSettings.minimapIconColor;
            }
            else
            {
                Debug.LogWarning("Minimap Icon Prefab is missing a Renderer component.", minimapIconPrefab);
            }

            AllIcons.Add(icon);
        }

        /// <summary>
        /// Can be called from an external script to add an object to the minimap.
        /// </summary>
        /// <param name="targetTransform">The transform of the object to add.</param>
        // --- REFACTORED for clarity and to use the helper method ---
        public void AttachMinimapIconManually(Transform targetTransform)
        {
            if (targetTransform == null) return;

            // Find the icon settings for this object's tag
            foreach (MinimapIcon iconSettings in MinimapIcons)
            {
                if (iconSettings.Tag == targetTransform.tag)
                {
                    CreateIconFor(targetTransform.gameObject, iconSettings);
                    return; // Exit after finding a match and creating the icon
                }
            }
            
            Debug.LogWarning($"No MinimapIcon settings found for tag '{targetTransform.tag}'. Cannot create icon manually.", targetTransform);
        }

        /// <summary>
        /// Finds and removes the icon from the List and destroys the GameObject.
        /// </summary>
        /// <param name="icon">The icon's GameObject to remove.</param>
        // --- REFACTORED for simplicity and safety ---
        public void RemoveMinimapIcon(GameObject icon)
        {
            if (icon != null && AllIcons.Contains(icon))
            {
                AllIcons.Remove(icon);
                Destroy(icon);
            }
        }
    }

    [System.Serializable]
    public class MinimapIcon
    {
        [Tooltip("The GameObject Tag to find.")]
        public string Tag = "uzAIZombie";
        
        [Tooltip("Color of the Icon on the minimap.")]
        public Color minimapIconColor = Color.white;

        [Tooltip("Scale of the Icon on the minimap.")]
        [Range(1f, 3f)]
        public float iconScale = 1f;
    }
}