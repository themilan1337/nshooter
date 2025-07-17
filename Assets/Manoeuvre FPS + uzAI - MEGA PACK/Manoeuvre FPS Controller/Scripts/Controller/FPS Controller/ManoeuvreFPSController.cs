using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre{

	public enum PlayerStates { Idle, Walking, Running, Jumping, Landing, Crouching}

	[RequireComponent(typeof(CharacterController))]
	public class ManoeuvreFPSController : MonoBehaviour {

        #region Variables
        public ManoeuvreFPSInputs Inputs;
        public Locomotion Locomotion;
        public PlayerHealth Health;
        public bool lockMovement;

        CameraController camController;
        Camera _camera;
        Vector3 moveDir  = Vector3.zero;
		bool wasGrounded;
		bool Walking = true;
		bool Jumping = false;
        public float fallTimer = 0f;
        CharacterController charController;
        public static ManoeuvreFPSController Instance;

        //Editor Variable
        [HideInInspector]
        public int propertyTab;

        [HideInInspector]
        public bool showInputs, showLocomotion, showHealth;
        #endregion

        #region Initialize

        private void Awake()
        {
            Instance = this;
        }

        void Start(){

            Initialize();
           
		}

        void Initialize()
        {
            //get character controller ref
            charController = GetComponent<CharacterController>();

            //get camera ref
            _camera = Camera.main;
            camController = GetComponentInChildren<CameraController>();
            //init player state to idle
            Locomotion.CurrentPlayerState = PlayerStates.Idle;

            //create source
            Locomotion.footStepSource = gameObject.AddComponent<AudioSource>();
            //start foot step coroutine
            StartCoroutine(Locomotion.PlayFootSound());

            //reset fall time to 0
            fallTimer = 0f;

            //set hear range
            gc_StateManager.Instance.radiusWhileRunning = Locomotion.HearRange;

            Inputs.camController = camController;

            //Initialize Health
            Health.Initialize(this);
            gc_PlayerHealthManager.Instance.Initialize(Health.Health, Health.currentHealth);

        }

        #endregion

        #region Manoeuvre Methods

        void LateUpdate()
		{
            //Handles the Player Manoeuvre
            HandleManoeuvre();
        }

        /// <summary>
        /// Handles the Player Manoeuvre
        /// </summary>
        void HandleManoeuvre()
        {
            //if movement is locked, stop all calculation
            if (lockMovement)
                return;

            //manage movement calculations
            Move();

            //manage crouching
            Crouch();
           
        }

        void Move()
        {
            //retrieve walking input
            Walking = !Inputs.runInput;

            //set speed accordingly
            float speed = Walking ? Locomotion.walkSpeed : Locomotion.runSpeed;
            speed = Inputs.crouchInput ? Locomotion.crouchSpeed : speed;
            speed += Locomotion.SpeedBoostEnabled();
            //create vector based on horz / vert axis wrt camera direction
            Vector3 desiredMove = _camera.transform.forward * Inputs.inputVector.y + _camera.transform.transform.right * Inputs.inputVector.x;

            //alter move direction with the speed
            moveDir.x = desiredMove.x * speed;
            moveDir.z = desiredMove.z * speed;

            //if player is on ground
            if (charController.isGrounded)
            {
                //manage jumping
                Jump();

            }
            else
            {
                //if we are not on ground
                //multiply all the directions with the gravity
                moveDir += Physics.gravity * Locomotion.gravityEffector * Time.deltaTime;
            }

            //finally move the character
            if(charController.enabled)
                charController.Move(moveDir * Time.fixedDeltaTime);

            //change foot step delay
            Locomotion.footSoundDelay = 1 / speed;

        }

        void Crouch()
        {
            //change height if crouching
            charController.height = Inputs.crouchInput ? Locomotion.crouchHeight : Locomotion.normalHeight;

        }

        void Jump() {
            //force y dir to fall speed
            moveDir.y = -Locomotion.fallSpeed;

            //if player jumps
            if (Inputs.jumpInput)
            {
                //make move dir to jump speed
                moveDir.y = Locomotion.jumpSpeed;
                //set jump input to false
                Inputs.jumpInput = false;
                //set jumping flag to true
                Jumping = true;
                //jump sound
                Locomotion.PlayJumpNLandSound(true);
            }
        }

        #endregion

        #region Input and State Management

        void Update(){

            //Step 1 --> Handle Inputs first before anything
           Inputs.HandleInputs();

            //Step 2 --> Now manage states based upon the Inputs
            ManagePlayerStates();

		}
	
        /// <summary>
        /// Managing the player states
        /// </summary>
        void ManagePlayerStates()
        {
            //if the player is grounded
            if (!charController.isGrounded)
            //    //force timer = 0
            //    fallTimer = 0;
            //else
                //else increment with delta time
                fallTimer += Time.deltaTime;

            //if we are not on ground before but now we are on ground 
            if (!wasGrounded && charController.isGrounded)
            {

                //Cause Fall Damage if the amount of time spend in air is higher then fall damage threshold
                Health.CalculateFallDamage(fallTimer);

                //play land sound
                Locomotion.PlayJumpNLandSound(false);

                //reset the move direction y value
                moveDir.y = 0;
                //reset jumping bool
                Jumping = false;
                //change state to Landing
                Locomotion.CurrentPlayerState = PlayerStates.Landing;

                fallTimer = 0;
            }
            //if we are not grounded
            else if (!charController.isGrounded)
            {
                //change state to Jumping
                Locomotion.CurrentPlayerState = PlayerStates.Jumping;
            }
            //if controller speed ~ 0
            else if (charController.velocity.sqrMagnitude < 0.01f)
            {
                //change the state to Idle
                Locomotion.CurrentPlayerState = PlayerStates.Idle;

                
            }
            //if we know controller is walking
            else if (Walking && !Inputs.crouchInput)
            {
                //change the state to Walking
                Locomotion.CurrentPlayerState = PlayerStates.Walking;

               
            }
            //if speed is > 0 and not walking 
            else if(charController.velocity.sqrMagnitude > 0.01f && !Walking && !Inputs.crouchInput)
            {
                //change the state to Running
                Locomotion.CurrentPlayerState = PlayerStates.Running;

            }
            else if(Inputs.crouchInput)
            {
                //change the state to Crouching
                Locomotion.CurrentPlayerState = PlayerStates.Crouching;
                
            }

            //Refreshing was grounded check every frame
            wasGrounded = charController.isGrounded;

            //update state in game controller
            gc_StateManager.Instance.currentPlayerState = Locomotion.CurrentPlayerState;
        }

        #endregion

        #region Health Management

        public void HealthkitPickup(int amount)
        {
            //make sure we won't exceed the total health while adding the value
            Health.currentHealth = Mathf.Clamp(Health.currentHealth, 0, Health.Health);

            Health.currentHealth += amount;

            //also set health manager's health
            gc_PlayerHealthManager.Instance.currentHealth = Health.currentHealth;

            //also lerp Health and Damage Sliders
            StartCoroutine(gc_PlayerHealthManager.Instance.LerpHealthSlider(false));
        }

        /// <summary>
        /// This is just the Damage Effect method.
        /// If you want to apply damage, see ApplyDamage() of PlayerHealth class
        /// </summary>
        public void TakeDamageEffect()
        {
            if (Health.deathManoeuvre.insideCoroutine)
                return;

            //shake camera
            StartCoroutine(camController.ShakeCamera(Health.ShakeDuration, Health.ShakeAmount));
            //show vignette
            StartCoroutine(Health.ShowDamageVignette());

            //also lerp Health and Damage Sliders
            gc_PlayerHealthManager.Instance.LerpSliders(Health.currentHealth);

            //we play hit sound
            Health.PlaySound(Health.HitSounds);
        }

        /// <summary>
        /// As soon as we Die,
        /// We disable all the scripts and call the final Death Manoeuvre!
        /// </summary>
        public void Die()
        {
            //make sure, time scale is 1
            Time.timeScale = 1f;

            //we play death sound
            Health.PlaySound(Health.DeathSounds);

            //Disable all Player Behaviours
            GetComponentInChildren<CameraController>().enabled = false;
            if(GetComponentInChildren<WeaponProceduralManoeuvre>())
                GetComponentInChildren<WeaponProceduralManoeuvre>().enabled = false;
            if(GetComponentInChildren<WeaponShooter>())
                GetComponentInChildren<WeaponShooter>().enabled = false;

            //Add force to Weapon
            if (gc_AmmoManager.Instance._currentWeapon && Health.deathManoeuvre.dismemberWeapon)
            {

                GameObject _currentWeapon = gc_AmmoManager.Instance._currentWeapon.gameObject;
                gc_AmmoManager.Instance._currentWeapon = null;

                _currentWeapon.AddComponent<BoxCollider>().size = new Vector3(0.5f, 0.5f, 0.5f);
                if(!_currentWeapon.GetComponent<Rigidbody>())
                    _currentWeapon.AddComponent<Rigidbody>().AddForce(_currentWeapon.transform.forward * Health.deathManoeuvre.WeaponDismembermentForce,
                                                                    ForceMode.Impulse);

                _currentWeapon.GetComponent<Rigidbody>().AddRelativeTorque(_currentWeapon.transform.forward * Health.deathManoeuvre.WeaponDismembermentForce,
                                                                                ForceMode.Impulse);

                _currentWeapon.transform.SetParent(null);
                foreach (Transform t in _currentWeapon.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
            else
            {
                if (gc_AmmoManager.Instance._currentWeapon)
                    gc_AmmoManager.Instance._currentWeapon.gameObject.SetActive(false);
            }

            //also lerp Health and Damage Sliders
            gc_PlayerHealthManager.Instance.LerpSliders(Health.currentHealth);

            //Start Death Manoeuvre Coroutine
            StartCoroutine(Health.ShowDamageVignette());
            StartCoroutine(Health.deathManoeuvre.DeathManoeuvreCoroutine(Camera.main.transform));

            //hide UI
            gc_PlayerHealthManager.Instance.DisableUI();

            GetComponent<ManoeuvreFPSController>().enabled = false;

        }
        #endregion
    }

    #region Serialized Classes

    [System.Serializable]
	public class Locomotion
    {
        [Header("-- Player Current State --")]
        public PlayerStates CurrentPlayerState = PlayerStates.Idle;

        [Header("-- Define Locomotion Properties --")]
        //FPS Locomotion Settings
        [Range(0.1f,5f)]
        public float walkSpeed = 1f;
        [Range(0.1f,5f)]
        public float crouchSpeed = 1f;
        [Range(0.1f,15f)]
        public float runSpeed = 4.5f;
        [Range(0.1f,15f)]
        public float jumpSpeed = 7.5f;
        [Range(0.1f,15f)]
        public float fallSpeed = 5f;
        [Range(0.1f,15f)]
        public float gravityEffector = 2.5f;
        [Range(0.1f,2f)]

        [Space(5)]

        public float crouchHeight = 1f;
        [Range(0.1f,5f)]
        public float normalHeight = 2f;
        [Range(0.1f,15f)]

        [Space(5)]

        [Tooltip("The range within which zombies can hear you if you are running.")]
        public float HearRange = 3f;

        [Space(5)]

        [Tooltip("Assign all the Foot Step Sounds.")]
        public List<AudioClip> FootStepSounds_Slow = new List<AudioClip>();
        public List<AudioClip> FootStepSounds_Fast = new List<AudioClip>();
        public List<AudioClip> JumpSound = new List<AudioClip>();
        public List<AudioClip> LandSound = new List<AudioClip>();

        [HideInInspector]
        public AudioSource footStepSource;
        [HideInInspector]
        public float footSoundDelay;

        public IEnumerator PlayFootSound()
        {

            while (true)
            {

                if (CurrentPlayerState != PlayerStates.Idle && ManoeuvreFPSController.Instance.Health.currentHealth > 0)
                {
                    if(CurrentPlayerState == PlayerStates.Walking || CurrentPlayerState == PlayerStates.Crouching)
                    {
                        int clip = Random.Range(0, FootStepSounds_Slow.Count);
                        footStepSource.PlayOneShot(FootStepSounds_Slow[clip]);
                        footStepSource.pitch = Random.Range(1f, 1.2f);
                    }

                    if(CurrentPlayerState == PlayerStates.Running)
                    {
                        int clip = Random.Range(0, FootStepSounds_Fast.Count);
                        footStepSource.PlayOneShot(FootStepSounds_Fast[clip]);
                        footStepSource.pitch = Random.Range(1f, 1.2f);
                    }
                }

                yield return new WaitForSeconds(footSoundDelay * 2);
            }

        }

        public void PlayJumpNLandSound(bool isJumping)
        {
            if(isJumping)
            {
                int clip = Random.Range(0, JumpSound.Count);
                footStepSource.PlayOneShot(JumpSound[clip]);
            }
            else
            {
                int clip = Random.Range(0, LandSound.Count);
                footStepSource.PlayOneShot(LandSound[clip]);
            }

        }

        public float SpeedBoostEnabled()
        {
            float retVal = 0;

            if (PowerupsManager.Instance._SpeedBoost.isActive)
                retVal = PowerupsManager.Instance._SpeedBoost.SpeedBoostAmount;

            return retVal;
        }
	}
    
    [System.Serializable]
    public class ManoeuvreFPSInputs
    {
        [Header("-- Define Inputs --")]

        [Tooltip("Assign Horizontal axis.")]
        public string Horizontal = "Horizontal";
        [Tooltip("Assign Vertical axis.")]
        public string Vertical = "Vertical";

        public string mouseScrollWheel = "Mouse ScrollWheel";

        [HideInInspector]
        public float horizontal;
        [HideInInspector]
        public float vertical;
        [HideInInspector]
        public Vector2 inputVector = Vector2.zero;
        [HideInInspector]
        public bool runInput;
        [Space(5)]

        [Tooltip("Assign 1 of the below fields.")]
        public string jumpButton;
        public KeyCode jumpKey;
        [HideInInspector]
        public bool jumpInput;

        [Space(5)]

        [Tooltip("Assign 1 of the below fields.")]
        public string crouchButton;
        public KeyCode crouchKey;
        [HideInInspector]
        public bool crouchInput;

        [Space(5)]

        [Tooltip("Assign 1 of the below fields.")]
        public string runButton;
        public KeyCode runKey;

        [Space(5)]
        [Tooltip("Shoot Input Key.")]
        public KeyCode shootKey = KeyCode.Mouse0;
        public string shootButton;
        public bool shootInput;

        public string ironSightButton = "Ironsight";
        public KeyCode ironSightInputKey = KeyCode.Mouse1;
        public bool ironsightInput;

        public string ReloadButton = "Reload";
        public KeyCode ReloadKey;
        public bool reloadInput;

        public string NextWeaponButton = "NextWeapon";
        public KeyCode NextWeaponKey;
        public bool nextWeaponInput;

        public string PreviousWeaponButton = "PreviousWeapon";
        public KeyCode PreviousWeaponKey;
        public bool previousWeaponInput;

        [Space(5)]
        public KeyCode InventoryKey;
        public string InventoryButton = "Inventory";
        public bool inventoryInput;

        [Space(5)]
        public KeyCode InteractionKey;
        public string InteractionButton = "Interaction";
        public bool interactionInput;

        [Space(5)]
        public KeyCode PauseKey;
        public string PauseButton = "Pause";
        public bool pauseInput;

        [Space(5)]
        public KeyCode ThrowItemKey;
        public string ThrowItemButton = "ThrowItem";
        public bool throwItemInput;

        [Space(5)]
        public KeyCode ZoomOutKey = KeyCode.KeypadMinus;
        public string ZoomOutButton;
        public bool zoomOutInput;

        public KeyCode ZoomInKey = KeyCode.KeypadPlus;
        public string ZoomInButton;
        public bool zoomInInput;

        [Space(5)]
        public bool usingMobileInputs;

        public CameraController camController;

        /// <summary>
        /// Handle All the Inputs
        /// </summary>
        public void HandleInputs()
        {
            //handle walk input
            if (!usingMobileInputs)
            {
                //Taking input from our keyboard
                horizontal = Input.GetAxis(Horizontal);
                vertical = Input.GetAxis(Vertical);
                inputVector = new Vector2(horizontal, vertical);

            }
            else
            {
                //Taking input from our mobile joystick
                inputVector = new Vector2(JoystickInput.Instance.InputVector.x, JoystickInput.Instance.InputVector.z);
            }

            if (inputVector.sqrMagnitude > 1)
                inputVector.Normalize();

            if (!usingMobileInputs) {
                
                //handle jump input
                if (!jumpInput)
                {
                    if (!string.IsNullOrEmpty(jumpButton))
                        jumpInput = Input.GetButtonDown("Jump");
                    else
                        jumpInput = Input.GetKeyDown(jumpKey);

                }
            }

            //handle run Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(runButton))
                    runInput = Input.GetButton(runButton);
                else
                    runInput = Input.GetKey(runKey);
            }

            //handle crouch Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(crouchButton))
                    crouchInput = Input.GetButton(crouchButton);
                else
                    crouchInput = Input.GetKey(crouchKey);
            }

            //handle iron sight input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(ironSightButton))
                    ironsightInput =  Input.GetButton(ironSightButton);
                else
                    ironsightInput = Input.GetKey(ironSightInputKey);

            }

            //handle shoot input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(shootButton))
                    shootInput = Input.GetButton(shootButton);
                else
                    shootInput = Input.GetKey(shootKey);

            }

            //handle reload input
            if (!usingMobileInputs )
            {
                if (!string.IsNullOrEmpty(ReloadButton))
                    reloadInput = Input.GetButton(ReloadButton);
                else
                    reloadInput = Input.GetKey(ReloadKey);

            }

            //handle next weapon Input
            if (!usingMobileInputs )
            {
                if (!string.IsNullOrEmpty(NextWeaponButton))
                    nextWeaponInput = Input.GetButton(NextWeaponButton);
                else
                    nextWeaponInput = Input.GetKey(NextWeaponKey);

                if (!nextWeaponInput && !previousWeaponInput)
                    nextWeaponInput = Input.GetAxis(NextWeaponButton) == 0 ? false : true;
            }

            //handle Previous weapon Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(PreviousWeaponButton))
                    previousWeaponInput = Input.GetButton(PreviousWeaponButton);
                else
                    previousWeaponInput = Input.GetKey(PreviousWeaponKey);

                if (!nextWeaponInput && !previousWeaponInput)
                    previousWeaponInput = Input.GetAxis(PreviousWeaponButton) == 0 ? false : true;
            }

            //handle Inventory Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(InventoryButton))
                    inventoryInput = Input.GetButton(InventoryButton);
                else
                    inventoryInput = Input.GetKey(InventoryKey);

                //set cursor toggle from Inventory Input as well
                if (inventoryInput)
                    camController.ToggleCursor(false);
                else
                    camController.ToggleCursor(camController.hideCursor);

            }

            //handle Inventory Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(InteractionButton))
                    interactionInput = Input.GetButton(InteractionButton);
                else
                    interactionInput = Input.GetKey(InteractionKey);
            }

            //handle Inventory Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(PauseButton))
                    pauseInput = Input.GetButton(PauseButton);
                else
                    pauseInput = Input.GetKey(PauseKey);
            }

            //handle Throw Item Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(ThrowItemButton))
                    throwItemInput = Input.GetButton(ThrowItemButton);
                else
                    throwItemInput = Input.GetKey(ThrowItemKey);
            }

            //handle Zoom out Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(ZoomOutButton))
                    zoomOutInput = Input.GetButton(ZoomOutButton);
                else
                    zoomOutInput = Input.GetKey(ZoomOutKey);
            }

            //handle Zoom in Input
            if (!usingMobileInputs)
            {
                if (!string.IsNullOrEmpty(ZoomInButton))
                    zoomInInput = Input.GetButton(ZoomInButton);
                else
                    zoomInInput = Input.GetKey(ZoomInKey);
            }

        }

    }
    
    [System.Serializable]
    public class PlayerHealth
    {
        [Range(1,200)]
        public int Health = 100;

        [Range(0.1f, 15f)]
        public float fallDamageTimer = 2f;

        [Range(0.1f, 100f)]
        public float fallDamagePerSecond = 25f;

        [Space(5)]
        [Range(0.01f, 0.5f)]
        public float ShakeDuration = 0.1f;
        [Range(0.01f, 1f)]
        public float ShakeAmount = 0.25f;

        [Space(5)]
        [Range(0.1f, 2f)]
        public float DamageVignetteDuration = 0.35f;
        public CanvasGroup DamageVignette;

        [Space(5)]
        public List<AudioClip> HitSounds = new List<AudioClip>();
        public List<AudioClip> DeathSounds = new List<AudioClip>();
        public AudioSource source;

        [Space(5)]
        public DeathManoeuvre deathManoeuvre;

        ManoeuvreFPSController _controller;
        [HideInInspector]
        public int currentHealth = 0;

        public void Initialize(ManoeuvreFPSController controller)
        {
            _controller = controller;

            if(currentHealth == 0)
                currentHealth = Health;

            DamageVignette = GameObject.Find("DamageVignett").GetComponent<CanvasGroup>();

            //Add sound source
            source = controller.gameObject.AddComponent<AudioSource>();
        }

        public void OnDamage(int amount)
        {
            //make sure, invincibility is not activated
            if(!PowerupsManager.Instance._Invincibility.isActive)
                //we decrease current health
                currentHealth -= amount;

            //if it's 0
            if(currentHealth <= 0)
            {
                //DIE
                _controller.Die();
            }
            else
            {
                //Shake Camera and Enable Damage Vignette
                _controller.TakeDamageEffect();
                
            }
        }

        public void CalculateFallDamage(float _timer)
        {
            //if our air time is greater than fall damage timer threshold
            if(_timer >= fallDamageTimer)
            {
                int damageAmount = (int) (_timer * fallDamagePerSecond);

                Debug.Log("Fall Damage : " + damageAmount);

                //give damage
                OnDamage(damageAmount);

            }
        }

        public IEnumerator ShowDamageVignette()
        {
            float et = 0;
            //show vignette
            while(et < DamageVignetteDuration/3)
            {
                DamageVignette.alpha = Mathf.Lerp(DamageVignette.alpha, 1.1f, et / (DamageVignetteDuration / 3));

                et += Time.deltaTime;
                yield return null;
            }

            //delay
            float t = 0;
            while (t < DamageVignetteDuration)
            {
                t += Time.deltaTime;
            }

           
            et = 0;
            //hide vignette
            while (et < DamageVignetteDuration / 3)
            {
                DamageVignette.alpha = Mathf.Lerp(DamageVignette.alpha, 0, et / (DamageVignetteDuration / 3));


                et += Time.deltaTime;
                yield return null;
            }
            
        }

        public void PlaySound(List<AudioClip> ac)
        {
            source.pitch = Random.Range(1f, 1.2f);
            int clip = Random.Range(0, ac.Count);

            if (!source.isPlaying)
                source.PlayOneShot(ac[clip]);
        }
    }

    [System.Serializable]
    public class DeathManoeuvre
    {

        public Vector3 cameraPositionOffset;
        public Vector3 cameraRotationOffset = new Vector3(0,0,30);

        [Range(0.5f, 10f)]
        public float deathDuration = 2f;

        public bool dismemberWeapon;
        [Range(1f,25f)]
        public float WeaponDismembermentForce = 5f;

        [HideInInspector]
        public bool insideCoroutine = false;

        public IEnumerator DeathManoeuvreCoroutine(Transform cameraTransform)
        {
            insideCoroutine = true;

            float et = 0;

            while(et < deathDuration)
            {

                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraPositionOffset, et / deathDuration);
                cameraTransform.localEulerAngles = Vector3.Lerp(cameraTransform.localEulerAngles, cameraRotationOffset, et / deathDuration);
                et += Time.deltaTime;

                yield return null;
            }

            insideCoroutine = false;

        }
    }

    #endregion

}