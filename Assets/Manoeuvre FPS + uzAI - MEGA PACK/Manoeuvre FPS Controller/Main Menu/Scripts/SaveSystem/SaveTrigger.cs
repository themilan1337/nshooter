using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Manoeuvre
{
    public class SaveTrigger : MonoBehaviour
    {
        public string ProgressMessage;

        public List<SaveTrigger> PreviousSaveTriggers = new List<SaveTrigger>();

        public UnityEvent OnResumeFromHere;

        Transform _camera;
        RaycastHit hitInfo;

        CanvasGroup InteractionIcon;
        Text InteractionTextComponent;
        CrosshairProceduralManoeuvre _cpm;

        bool _canInteract;

        // Use this for initialization
        void Start()
        {
            _camera = Camera.main.transform;

            //make sure it is triggered
            GetComponent<Collider>().isTrigger = true;

            InteractionIcon = GameObject.Find("InteractionIcon").GetComponent<CanvasGroup>();
            InteractionTextComponent = GameObject.Find("InteractionText").GetComponent<Text>();
            _cpm = FindObjectOfType<CrosshairProceduralManoeuvre>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag != "Player")
                return;

            _canInteract = true;
            ToggleInteractionText(true);
        }

        private void OnTriggerExit(Collider other)
        {
            _canInteract = false;
            ToggleInteractionText(false);

        }

        void ToggleInteractionText(bool showText)
        {
            if (!InteractionIcon)
                return;

            if (showText)
            {
                //show interaction icon and text
                InteractionIcon.alpha = 1;
                //disable crosshair
                _cpm.disableCrosshair = true;

                //set interaction text 
                InteractionTextComponent.text = "Save Game";

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
            if (!_canInteract || SaveUIController.Instance.isOpen)
                return;

            if (ManoeuvreFPSController.Instance.Inputs.interactionInput)
            {
                OpenSaveUIFromTrigger();
            }
        }

        void OpenSaveUIFromTrigger()
        {
            SaveUIController.Instance.OpenSaveUI();

            //set progress message
            SaveUIController.Instance.ProgressMessage = ProgressMessage;
        }

        public void InvokeResumeEvents()
        {
            for (int i = 0; i < PreviousSaveTriggers.Count; i++)
                PreviousSaveTriggers[i].OnResumeFromHere.Invoke();

            //invoke this trigger's event as well
            OnResumeFromHere.Invoke();
        }
    }
}