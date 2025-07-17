using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Manoeuvre
{
    public enum DoorActionType
    {
        Animation,
        LerpPosition,
        LerpRotation,
        LerpPositionRotation
    }

    [RequireComponent(typeof(BoxCollider))]
    public class DoorAction : MonoBehaviour
    {

        public DoorActionType _DoorActionType;
        [Space]
        public bool needsKey = false;
        //public Transform myKeyObject;
        [Space]
        public string InteractionText_Open = "Open Door";
        public string InteractionText_Close = "Close Door";
        public string InteractionText_Locked = "Locked !!!";

        [Space]
        public Animation DoorAnimation;
        public string OpenAnimation = "Door_Open";
        public string CloseAnimation = "Door_Close";

        [Space]
        public float LerpDuration = 2f;
        public Vector3 FromRotation;
        public Vector3 ToRotation;
        [Space]
        public Vector3 FromPosition;
        public Vector3 ToPosition;
        [Space]
        public AudioClip OpenSound;
        public AudioClip CloseSound;
        public AudioClip LockedSound;
        [Space]
        public List<DoorAction> CorrespondingSwitches = new List<DoorAction>();
        [Space]
        public UnityEvent OnOpen;
        public UnityEvent OnClose;
        public UnityEvent OnLocked;

        AudioSource _source;
        Transform cam;
        public bool isOpen;
        public bool canInteract;
        RaycastHit hitInfo;
        List<Transform> allTransforms = new List<Transform>();
        CanvasGroup InteractionIcon;
        Text InteractionTextComponent;
        CrosshairProceduralManoeuvre _cpm;
        public bool inCorutine;

        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            _source = gameObject.AddComponent<AudioSource>();

            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                if (t != this.transform)
                    allTransforms.Add(t);
            }

            InteractionIcon = GameObject.Find("InteractionIcon").GetComponent<CanvasGroup>();
            InteractionTextComponent = GameObject.Find("InteractionText").GetComponent<Text>();
            _cpm = FindObjectOfType<CrosshairProceduralManoeuvre>();

            GetComponent<Collider>().isTrigger = true;

            //update all corresponding buttons
            UpdateCorrespondingSwitches();
        }

        private void Update()
        {
            if (!canInteract)
                return;

            if (DoorAnimation)
            {
                if (DoorAnimation.IsPlaying(OpenAnimation) || DoorAnimation.IsPlaying(CloseAnimation))
                    inCorutine = true;
                else
                    inCorutine = false;

            }

            if (inCorutine)
                return;

            ///if we can interact
            if (canInteract)
            {
                if (ManoeuvreFPSController.Instance.Inputs.interactionInput)
                    CheckDoorStatus();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {

                //player entered, get camera
                cam = other.GetComponentInChildren<CameraController>().transform;

            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag == "Player")
            {
                if (!cam)
                    return;

                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 10))
                {
                    //debug
                    Vector3 fwd = cam.transform.forward * 10;
                    Debug.DrawRay(cam.transform.position, fwd, Color.green);

                    //check each transform we have...
                    for(int i =0; i < allTransforms.Count; i++)
                    {
                        if (hitInfo.transform == allTransforms[i])
                        {
                            //we are looking at the door
                            canInteract = true;
                            ToggleInteractionText(true);
                            return;
                        }
                    }

                }

                canInteract = false;
                ToggleInteractionText(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                canInteract = false;

                ToggleInteractionText(false);
            }
        }

        /// <summary>
        /// Here we see if the door is ready to be opened, i.e
        /// Do we have a key or not...
        /// </summary>
        void CheckDoorStatus()
        {
            //if door is closed, we TRY opening the door
            if(!isOpen)
            {
                if (needsKey)
                {
                    //play sfx
                    if (LockedSound && !_source.isPlaying)
                        _source.PlayOneShot(LockedSound);

                    //call ON LOCK event
                    OnLocked.Invoke();

                    return;
                }
                else
                {
                    //if no need of key, open door directly
                    OpenDoor();
                    return;

                }
            }
            //if door is open, we just close the door
            else if (isOpen)
            {
                CloseDoor();
                return;
            }
        }

        /// <summary>
        /// Main Method to open the door!
        /// </summary>
        void OpenDoor()
        {
            if (isOpen)
                return;

            //enable flag to see that the door is now open
            isOpen = true;

            //play sfx
            if (OpenSound)
                _source.PlayOneShot(OpenSound);

            //Debug.Log("Open Door");

            switch (_DoorActionType)
            {
                //open via Animation
                case DoorActionType.Animation:
                    DoorAnimation.Play(OpenAnimation);
                    break;

                //open via Coroutine
                case DoorActionType.LerpPosition:
                    StartCoroutine(ToggleDoorCoroutine_Position(true));
                    break;
                
                //open via Coroutine
                case DoorActionType.LerpRotation:
                    StartCoroutine(ToggleDoorCoroutine_Rotation(true));
                    break;

                //open via Coroutine
                case DoorActionType.LerpPositionRotation:
                    StartCoroutine(ToggleDoorCoroutine_Position(true));
                    StartCoroutine(ToggleDoorCoroutine_Rotation(true));

                    break;

            }

            //update the corresponding buttons
            UpdateCorrespondingSwitches();

            //call ON OPEN event
            OnOpen.Invoke();

        }

        /// <summary>
        /// Main Method to Close the door!
        /// </summary>
        void CloseDoor()
        {
            if (!isOpen)
                return;

            //disable flag to see that the door is now closed
            isOpen = false;

            //play sfx
            if (CloseSound)
                _source.PlayOneShot(CloseSound);

            //Debug.Log("Close Door");

            switch (_DoorActionType)
            {
                //open via Animation
                case DoorActionType.Animation:
                    DoorAnimation.Play(CloseAnimation);
                    break;

                //open via Coroutine
                case DoorActionType.LerpPosition:
                    StartCoroutine(ToggleDoorCoroutine_Position(false));
                    break;

                //open via Coroutine
                case DoorActionType.LerpRotation:
                    StartCoroutine(ToggleDoorCoroutine_Rotation(false));
                    break;

                //open via Coroutine
                case DoorActionType.LerpPositionRotation:
                    StartCoroutine(ToggleDoorCoroutine_Position(false));
                    StartCoroutine(ToggleDoorCoroutine_Rotation(false));

                    break;

            }

            //update the corresponding buttons
            UpdateCorrespondingSwitches();

            //call ON CLOSE event
            OnClose.Invoke();

        }

        IEnumerator ToggleDoorCoroutine_Position(bool shouldOpen)
        {
            float et = 0;

            while(et < LerpDuration)
            {
                inCorutine = true;

                //lerp position
                if (shouldOpen)
                    transform.position = Vector3.Lerp(FromPosition, ToPosition, et / LerpDuration);
                else
                    transform.position = Vector3.Lerp(ToPosition, FromPosition, et / LerpDuration);

                et += Time.deltaTime;
                yield return null;
            }

            inCorutine = false;

        }

        IEnumerator ToggleDoorCoroutine_Rotation(bool shouldOpen)
        {
            float et = 0;

            while (et < LerpDuration)
            {
                inCorutine = true;
              
                //lerp position
                if (shouldOpen)
                    transform.rotation = Quaternion.Lerp(Quaternion.Euler(FromRotation), Quaternion.Euler(ToRotation), et / LerpDuration);
                else
                    transform.rotation = Quaternion.Lerp(Quaternion.Euler(ToRotation), Quaternion.Euler(FromRotation), et / LerpDuration);

                et += Time.deltaTime;
                yield return null;
            }

            inCorutine = false;

        }

        void ToggleInteractionText(bool showText)
        {
            if (showText)
            {
                //show interaction icon and text
                InteractionIcon.alpha = 1;
                //disable crosshair
                _cpm.disableCrosshair = true;

                //set interaction text accordingly
                if (!needsKey)
                    InteractionTextComponent.text = InteractionText_Open;
                else if (needsKey)
                    InteractionTextComponent.text = InteractionText_Locked;

                if(isOpen)
                    InteractionTextComponent.text = InteractionText_Close;

            }
            else
            {

                //hide interaction icon and text
                InteractionIcon.alpha = 0;
                
                //enable crosshair
                _cpm.disableCrosshair = false;

            }
        }

        void UpdateCorrespondingSwitches()
        {
            if (CorrespondingSwitches.Count > 0)
            {
                for(int i =0; i< CorrespondingSwitches.Count; i++)
                {
                    CorrespondingSwitches[i].CorrespondingSwitches = CorrespondingSwitches;

                    if (!CorrespondingSwitches[i].CorrespondingSwitches.Contains(this))
                        CorrespondingSwitches[i].CorrespondingSwitches.Add(this);
                }

            }

            foreach (DoorAction _da in CorrespondingSwitches)
            {
                _da.isOpen = isOpen;
                _da.inCorutine = inCorutine;
            }

        }

        /// <summary>
        /// Handy method to lock/unlock this door, you can call this from an event
        /// or from any external script.
        /// </summary>
        public void ToogleDoorFromMethod(bool val)
        {
            needsKey = val;
        }
    }
}