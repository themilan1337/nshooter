using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class DynamicBarricades : MonoBehaviour
    {
        //all child barricades
        public List<Transform> ChildBarricades = new List<Transform>();
        [Space]
        public float totalContructionLength = 2f;
        public LayerMask BarricadeLayer;
        public bool startDisabled = true;
        public ParticleSystem AddFx;
        public AudioClip AddSFX;
        public AudioClip CompletionSFX;
        public bool allDequed;
        public uzAI.uzAIZombieStateManager Zombie;

        //private vars
        Transform _cam;
        List<Vector3> cachePositions = new List<Vector3>();
        List<ParticleSystem> cacheFX = new List<ParticleSystem>();
        float eachBarricadeDuration;
        public bool isAddingBarricade;
        public int BarricadeIndex;
        public int lastDestroyedBarricade;
        public float cooldown = 10f;
        public float cooldownTimer;

        AudioSource _source;
        
        //ui
        CanvasGroup BarricadesHUD;
        Slider BarricadesSlider;
        Transform BarricadeRotary;
        Text BarricadeText;
        int SliderAmount;
        int cacheSliderValue;

        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            //get camera ref
            _cam = Camera.main.transform;

            //set cooldown
            cooldownTimer = cooldown;

            //set last destroyed barricade index
            lastDestroyedBarricade = ChildBarricades.Count-1;

            //get UI references
            if (GameObject.Find("BarricadesHUD"))
            {
                BarricadesHUD = GameObject.Find("BarricadesHUD").GetComponent<CanvasGroup>();
                BarricadesSlider = GameObject.Find("BarricadesSlider").GetComponent<Slider>();
                BarricadeRotary = GameObject.Find("BarricadeRotary").GetComponent<Transform>();
                BarricadeText = GameObject.Find("BarricadeText").GetComponent<Text>();

                //hide UI
                BarricadesHUD.alpha = 0;

                //init slider
                BarricadesSlider.minValue = 0;
                BarricadesSlider.maxValue = 100;

                //get slider amount to be increased every time we add a barrier
                SliderAmount = 100 / ChildBarricades.Count;

            }


            //get each barricade contstruction coroutine length
            eachBarricadeDuration = totalContructionLength / ChildBarricades.Count;

            //add audio source
            _source = gameObject.AddComponent<AudioSource>();

            //init each barricade
            foreach (Transform t in ChildBarricades)
            {
                //set status
                t.gameObject.SetActive(!startDisabled);
                //make sure, rigidbody is kinematic
                t.GetComponent<Rigidbody>().isKinematic = true;
                //make this trigger
                t.GetComponent<Collider>().isTrigger = true;

                //cache position
                cachePositions.Add(t.localPosition);

                //add fx inside each barricade
                if (AddFx)
                {
                    ParticleSystem fx = Instantiate(AddFx) as ParticleSystem;
                    fx.transform.SetParent(t);
                    fx.transform.localPosition = Vector3.zero;
                    fx.gameObject.SetActive(false);
                    cacheFX.Add(fx);
                }

                //if barricades are disabled, add offset to each one's z axis
                if (startDisabled)
                {
                    Vector3 newPos = new Vector3(0, 0, t.localPosition.z * 2.5f);
                    t.localPosition += newPos;

                    //set flag
                    allDequed = false;
                    
                }
                else
                {
                    //disable this Barricades Add behaviour
                    BarricadeIndex = ChildBarricades.Count;
                    isAddingBarricade = true;
                    t.GetComponent<Collider>().isTrigger = false;

                    //set text
                    if(BarricadeText)
                        BarricadeText.text = "Done";

                    //set Slider
                    if(BarricadesSlider)
                        BarricadesSlider.value = BarricadesSlider.maxValue;

                    
                }

            }
        }

        private void Update()
        {
            if (cooldownTimer >= cooldown)
                return;

            cooldownTimer += Time.deltaTime;
        }

        //set UI on enter
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                //Set Slider
                BarricadesSlider.value = cacheSliderValue;

                //Set Text
                BarricadeText.text = BarricadesSlider.value + " %";

                if (BarricadeIndex >= ChildBarricades.Count)
                {
                    //set Slider to max
                    BarricadesSlider.value = BarricadesSlider.maxValue;

                    //Set Text to Done
                    BarricadeText.text = "Done";
                }
            }

           
        }

        //We only put barriers if player is close enough
        private void OnTriggerStay(Collider other)
        {
            //start adding barricades
            if (other.tag == "Player")
            {
                StartContruction();
            }

        }

        //Reset UI on exit
        private void OnTriggerExit(Collider other)
        {
            if(other.tag == "Player")
            {
                //HideUI
                BarricadesHUD.alpha = 0;
            }

        }

        void StartContruction()
        {
           
            //if we are not looking at barricade
            if (!LookingAtBarricade())
                return; //exit

            //if it is being attacked, we can't add it at that time
            if (Zombie != null)
            {
                BarricadeText.text = "Under Attack!";
                return;
            }

            //if we are cooling down for the moment
            if (cooldownTimer < cooldown)
            {
                BarricadesHUD.alpha = 1;
                BarricadeText.text = "Preparing Barricades!";

                //Rotate UI Image
                Vector3 newRot = new Vector3(BarricadeRotary.localEulerAngles.x, BarricadeRotary.localEulerAngles.y, BarricadeRotary.localEulerAngles.z + 10);
                BarricadeRotary.localEulerAngles = newRot;

                return;
            }

            //make sure is Adding Barricade is false
            if (isAddingBarricade)
                return;

            //if everything is fine, add barricade
            StartCoroutine(AddBarricades());

        }

        IEnumerator AddBarricades()
        {
            //enable flag, so we don't run this co routine again and again
            isAddingBarricade = true;

            //make sure it is kinematic
            ChildBarricades[BarricadeIndex].GetComponent<Rigidbody>().isKinematic = true;

            //set it back to it's cache pos
            ChildBarricades[BarricadeIndex].transform.localPosition = new Vector3(cachePositions[BarricadeIndex].x, cachePositions[BarricadeIndex].y, cachePositions[BarricadeIndex].z * 2.5f);

            //enable this Barricade
            ChildBarricades[BarricadeIndex].gameObject.SetActive(true);

            Vector3 localPos = ChildBarricades[BarricadeIndex].transform.localPosition;
            //Vector3 cacheLocalPos = new Vector3(ChildBarricades[BarricadeIndex].transform.localPosition.x, ChildBarricades[BarricadeIndex].transform.localPosition.y, cachePositions[BarricadeIndex]);
            Vector3 cacheLocalPos = cachePositions[BarricadeIndex];

            int nextSliderVal = (int) BarricadesSlider.value + SliderAmount;

            float et = 0;
            while (et <= eachBarricadeDuration)
            {
                //lerp position of barricades
                ChildBarricades[BarricadeIndex].transform.localPosition = Vector3.Lerp(localPos, cacheLocalPos, et / eachBarricadeDuration);

                //lerp UI Slider values
                BarricadesSlider.value = Mathf.Lerp(BarricadesSlider.value, nextSliderVal, et / eachBarricadeDuration);
                cacheSliderValue = (int)BarricadesSlider.value;

                //Set UI Text
                if(BarricadesSlider.value == BarricadesSlider.maxValue)
                    BarricadeText.text = "Done";
                else
                    BarricadeText.text = BarricadesSlider.value + " %";

                //Rotate UI Image
                Vector3 newRot = new Vector3(BarricadeRotary.localEulerAngles.x, BarricadeRotary.localEulerAngles.y, BarricadeRotary.localEulerAngles.z + 10);
                BarricadeRotary.localEulerAngles = newRot;

                et += Time.deltaTime;
                yield return null;
            }

            //set flag
            allDequed = false;

            //play fx
            if (AddFx)
                cacheFX[BarricadeIndex].gameObject.SetActive(true);

            //play sfx
            if (AddSFX)
            {
                float pitch = Random.Range(1, 1.2f);
                _source.pitch = pitch;
                _source.PlayOneShot(AddSFX, 1);
            }

            //disable trigger
            ChildBarricades[BarricadeIndex].GetComponent<Collider>().isTrigger = false;

            //increment
            BarricadeIndex++;

            //disable flag only if there's still barricades left
            if (BarricadeIndex < ChildBarricades.Count)
            {
                isAddingBarricade = false;
            }
            else if (BarricadeIndex >= ChildBarricades.Count)
            {
                //set text
                BarricadeText.text = "Done";
                //set Slider
                BarricadesSlider.value = BarricadesSlider.maxValue;
                //play completion sfx
                _source.PlayOneShot(CompletionSFX, 1);
            }
        }

        /// <summary>
        /// This is uzAI Specific method
        /// </summary>
        public void OnDamage()
        {
            StartCoroutine(DequeBarricade());
        }

        IEnumerator DequeBarricade()
        {
            isAddingBarricade = false;

            //reset cooldown
            cooldownTimer = 0;

            //each time zombie attacks, it will remove one barricade
            //always starting from the last one added
            ChildBarricades[lastDestroyedBarricade].GetComponent<Rigidbody>().isKinematic = false;
            ChildBarricades[lastDestroyedBarricade].GetComponent<Rigidbody>().AddForce(-ChildBarricades[lastDestroyedBarricade].forward * 1.2f, ForceMode.Impulse);
            ChildBarricades[lastDestroyedBarricade].GetComponent<Collider>().isTrigger = true;

            //decrement count if it's not the last one
            if (lastDestroyedBarricade > 0)
                lastDestroyedBarricade--;
            else
                allDequed = true;

            BarricadeIndex --;

            yield return new WaitForSeconds(1.5f);
            
            //set new Index
            int newIndex;

            if (!allDequed)
                newIndex = lastDestroyedBarricade + 1;
            else
                newIndex = lastDestroyedBarricade;


            ChildBarricades[lastDestroyedBarricade].GetComponent<Rigidbody>().isKinematic = true;
            //hide it
            ChildBarricades[newIndex].gameObject.SetActive(false);
            //reposition it
            Vector3 newPos = new Vector3(cachePositions[lastDestroyedBarricade].x, cachePositions[lastDestroyedBarricade].y, cachePositions[lastDestroyedBarricade].z * 2.5f);
            ChildBarricades[newIndex].localPosition = newPos;

        }

        /// <summary>
        /// we cast a ray from camera to the forward direction
        /// if it hit the barricades look at, we start constructing it
        /// </summary>
        /// <returns></returns>
        bool LookingAtBarricade()
        {
            Debug.DrawRay(_cam.transform.position, _cam.transform.forward * 3f, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, 3f, BarricadeLayer))
            {
                if (hit.transform.tag == "BarricadesLookAt")
                {
                    //show HUD
                    BarricadesHUD.alpha = 1;

                    return true;
                }
            }

            //Hide HUD
            BarricadesHUD.alpha = 0;

            return false;
        }

        
    }
}