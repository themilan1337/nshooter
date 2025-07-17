using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Manoeuvre
{
    [RequireComponent(typeof(BoxCollider))]
    public class DoorKey : MonoBehaviour
    {
        public List<DoorAction> CorrespondingSwitches = new List<DoorAction>();
        public AudioClip pickupSound;

        public string InteractionText = "Pickup Key";

        public bool autoPickup = false;
        public bool rotateKey = true;
        public float rotateSpeed = 5f;


        public UnityEvent OnPickup;

        CanvasGroup InteractionIcon;
        Text InteractionTextComponent;
        CrosshairProceduralManoeuvre _cpm;

        bool hasPicked;

        // Use this for initialization
        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            InteractionIcon = GameObject.Find("InteractionIcon").GetComponent<CanvasGroup>();
            InteractionTextComponent = GameObject.Find("InteractionText").GetComponent<Text>();
            _cpm = FindObjectOfType<CrosshairProceduralManoeuvre>();

            GetComponent<Collider>().isTrigger = true;

        }

        private void OnTriggerStay(Collider other)
        {
            if(other.tag == "Player")
            {
                if (hasPicked)
                    return;

                if (ManoeuvreFPSController.Instance.Inputs.interactionInput || autoPickup)
                    PickupKey();

                ToggleInteractionText(true);

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
                ToggleInteractionText(false);
        }

        void PickupKey()
        {
            //disable key check
            for (int i = 0; i < CorrespondingSwitches.Count; i++)
            {
                CorrespondingSwitches[i].needsKey = false;
            }

            //Play audio clip
            if (pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound,transform.position);

            //fire event
            OnPickup.Invoke();

            ///disable text
            ToggleInteractionText(false);

            //enable flag
            hasPicked = true;

            //Destroy Key
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }

        void ToggleInteractionText(bool showText)
        {
            if (showText && !hasPicked)
            {
                //show interaction icon and text
                InteractionIcon.alpha = 1;
                //disable crosshair
                _cpm.disableCrosshair = true;

                InteractionTextComponent.text = InteractionText;

            }
            else
            {

                //hide interaction icon and text
                InteractionIcon.alpha = 0;

                //enable crosshair
                _cpm.disableCrosshair = false;

            }
        }

        private void Update()
        {
            if (rotateKey)
                transform.Rotate(transform.up, rotateSpeed);
        }
    }
}