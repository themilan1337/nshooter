using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    [RequireComponent(typeof(BoxCollider))]
    public class Powerups_Pickup : MonoBehaviour
    {

        public PowerupType _PowerupType;
        public AudioClip pickupSound;
        public GameObject PickupTextPrefab;

        //HEALTH KIT

        [Tooltip("How much health to be given?")]
        public int healthAmount = 50;

        //INVINCIBILITY

        [Tooltip("How long this power will stay active?")]
        public float InvincibilityDuration = 10f;

        //SPEEDBOOST

        [Tooltip("This Amount will be added in current speed!")]
        public float SpeedBoostAmount = 5f;

        [Tooltip("How long this power will stay active?")]
        public float SpeedBoostDuration = 10f;

        //DAMAGE MULTIPLIER

        [Tooltip("This Amount will be Multiplied in current Damage!")]
        public int DamageMultiplierAmount = 2;

        [Tooltip("How long this power will stay active?")]
        public float DamageMultiplierDuration = 10f;

        //INFINITE AMMO

        [Tooltip("How long this power will stay active?")]
        public float InfiniteAmmoDuration = 10;

        Transform PickupHUD;

        //UI Canvas element
        GameObject PickupMessagesContainer;

        private void Awake()
        {
            PickupMessagesContainer = GameObject.Find("PickupMessagesContainer");

        }

        // Use this for initialization
        void Start()
        {
            //make sure the trigger is set to trigger
            GetComponent<Collider>().isTrigger = true;

            //also init UI 
            SetChildUI();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                //add to inventory
                PowerupsManager.Instance.InitializePowerup(_PowerupType, this);

                //show message
                ShowPickupMessage();

                //play sound
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                //Play Dialogue
                gc_PlayerDialoguesManager.Instance.PlayDialogueClip(gc_PlayerDialoguesManager.DialogueType.Pickup);

                this.gameObject.SetActive(false);
            }
        }

        void ShowPickupMessage()
        {
            //show pickup message
            GameObject msg = Instantiate(PickupTextPrefab);
            
            //set text
            msg.GetComponent<Text>().text = "Picked " + _PowerupType.ToString();

            //init scale and pos
            msg.transform.SetParent(PickupMessagesContainer.transform);
            msg.transform.localPosition = Vector3.zero;
            msg.transform.localScale = Vector3.one;
            msg.transform.localEulerAngles = Vector3.zero;

            //destroy msg
            Destroy(msg, 1f);

        }

        void SetChildUI()
        {

            PickupHUD = transform.Find("PickupHUD").transform;
            Text text = GetComponentInChildren<Text>();
            Image icon = transform.Find("PickupHUD/Container/Icon").GetComponent<Image>();

            if (text)
                text.text = "Pickup\n" + _PowerupType.ToString();

            //setting icon the long way...
            if (icon)
            {
                switch (_PowerupType)
                {
                    case PowerupType.Healthkit:
                        icon.sprite = PowerupsManager.Instance._HealthKit.icon;
                        break;
                    case PowerupType.DamageMultiplier:
                        icon.sprite = PowerupsManager.Instance._DamageMultiplier.icon;
                        break;
                    case PowerupType.InfiniteAmmo:
                        icon.sprite = PowerupsManager.Instance._InfiniteAmmo.icon;
                        break;
                    case PowerupType.Invincibility:
                        icon.sprite = PowerupsManager.Instance._Invincibility.icon;
                        break;
                    case PowerupType.Speedboost:
                        icon.sprite = PowerupsManager.Instance._SpeedBoost.icon;
                        break;

                }
            }

            //invert HUD Scale to look properly
            PickupHUD.localScale = new Vector3(- PickupHUD.localScale.x, PickupHUD.localScale.y, PickupHUD.localScale.z);

            //always look at player
            StartCoroutine(SetLookAt());
        }

        IEnumerator SetLookAt()
        {
            while (true)
            {
                if (PickupHUD)
                    PickupHUD.LookAt(Camera.main.transform);

                yield return null;
            }
        }
    }
}