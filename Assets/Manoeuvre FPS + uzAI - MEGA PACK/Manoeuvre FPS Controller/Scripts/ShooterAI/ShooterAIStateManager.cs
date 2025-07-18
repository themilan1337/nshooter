using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Manoeuvre
{
    public enum AIType
    {
        Companion,
        Enemy
    }

    public enum ShooterAIStates
    {
        Idle,
        Patrol,
        Chase,
        Firing,
        Reload,
        Hit,
        Die
    }

    public class ShooterAIStateManager : MonoBehaviour
    {
        public ShooterAIStates currentShooterState = ShooterAIStates.Idle;

        //Set AI Type First
        public AIType _AIType = AIType.Companion;
        public ManoeuvreFPSController Player;

        //Animator Val
        public float walkAnimation = 0;

        [Space]

        //0. Companion Behaviour
        public ShooterAICompanionBehaviour CompanionBehaviour;

        //1. Idle Behaviour
        public ShooterAIIdleBehaviour IdleBehaviour;

        //2. Patrol Behaviour
        public ShooterAIPatrolBehaviour PatrolBehaviour;

        //3. Chase Behaviour
        public ShooterAIChaseBehaviour ChaseBehaviour;

        //4. Sight Behaviour
        public ShooterAISightBehaviour SightBehaviour;

        //5. Attack behaviour
        public ShooterAIAttackBehaviour AttackBehaviour;

        //6. Weapon Behaviour
        public ShooterAIWeaponBehaviour WeaponBehaviour;

        //7. Health
        public ShooterAIHealth Health;

        //8. Aim IK
        public ShooterAIAimIK AimIK;

        //Gizmos Behaviour
        public DrawGizmos DrawGizmosBehaviour;

        Animator _animator;
        NavMeshAgent _shooterAIAgent;

        //[HideInInspector]
        public float _globalDelayTimer;
        [HideInInspector]
        public bool setHandsIK = true;

        [HideInInspector]
        public ShooterAIAudioManager _audioManager;

        [HideInInspector]
        public UnityEngine.Events.UnityEvent OnDeath;

        //editor var
        public int tabCount1;
        public int tabCount2;
        public int tabCount3;
        public string tabName;

        private void Awake()
        {
            //get references
            _animator = GetComponent<Animator>();
            _shooterAIAgent = GetComponent<NavMeshAgent>();

        }

        // Use this for initialization
        void Start()
        {
            //init behaviours
            CompanionBehaviour.Initialize(this, _shooterAIAgent, _animator);
            IdleBehaviour.Initialize(this, _shooterAIAgent, _animator);
            PatrolBehaviour.Initialize(this, _shooterAIAgent, _animator);
            ChaseBehaviour.Initialize(this, _shooterAIAgent, _animator);
            AttackBehaviour.Initialize(this, _shooterAIAgent, _animator);
            WeaponBehaviour.Initialize(this, _animator);
            AimIK.Initialize(this);
            Health.Initialize(this, _shooterAIAgent, _animator);

            //init audio manager
            _audioManager.Initialize(this);

            //we also start the coroutine which will be checking all the colliders in the 
            //specified range
            StartCoroutine(SightBehaviour.SearchTargetsCoroutine());
            //Now create the trigger
            SightBehaviour._stateManager = this;
            SightBehaviour.CreateSensorTrigger();
            //now we start the Coroutine to search nearby attack targets
            StartCoroutine(AttackBehaviour.SearchAttackTargetsCoroutine());

        }

        /// <summary>
        /// Overriding Animator Behaviour here.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh)
                return; // exit

            //else
            //we make sure root motion is enabled
            _animator.applyRootMotion = true;

            //and set the speed of the Nav Agent according to the root motion of the Animator
            _shooterAIAgent.speed = (_animator.deltaPosition / Time.deltaTime).magnitude;


        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_animator)
                return;

            if (!AimIK.LeftHandIK)
                return;

            //make sure to look at player
            if ((currentShooterState == ShooterAIStates.Patrol || currentShooterState == ShooterAIStates.Idle) && _AIType == AIType.Companion)
            {
                if (Player)
                    _animator.SetLookAtPosition(Player.transform.position + Vector3.up);

                _animator.SetLookAtWeight(1);
            }



            if (!WeaponBehaviour.weaponObject)
                return;

            //set Pos and Rot via IK of Left Hands 
            if (setHandsIK && currentShooterState != ShooterAIStates.Die && !AttackBehaviour.isReloading)
            {

                _animator.SetIKPosition(AvatarIKGoal.LeftHand, AimIK.LeftHandIK.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, AimIK.LeftHandIK.rotation);
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

            }

            //set head tracking
            if (ChaseBehaviour.useHeadTrack)
            {
                if (currentShooterState == ShooterAIStates.Chase && ChaseBehaviour.targetPosition)
                {
                    _animator.SetLookAtPosition(ChaseBehaviour.targetPosition.position + Vector3.up);
                    _animator.SetLookAtWeight(1);
                }
                else if (currentShooterState == ShooterAIStates.Firing)
                {
                    if (AttackBehaviour.Player)
                    {
                        _animator.SetLookAtPosition(AttackBehaviour.Player.transform.position + Vector3.up);
                    }
                    else if (AttackBehaviour.Zombie)
                    {
                        _animator.SetLookAtPosition(AttackBehaviour.Zombie.transform.position + Vector3.up);
                    }
                    else if(AttackBehaviour.ShooterAI)
                    {
                        _animator.SetLookAtPosition(AttackBehaviour.ShooterAI.transform.position + Vector3.up);

                    }
                    else if (AttackBehaviour.Turret)
                    {
                        _animator.SetLookAtPosition(AttackBehaviour.Turret.transform.position + Vector3.up);

                    }

                    _animator.SetLookAtWeight(1);

                }

            }

            //if Firing 
            if (currentShooterState == ShooterAIStates.Firing)
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, AimIK.LeftHandAimIK.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, AimIK.LeftHandAimIK.rotation);
                _animator.SetBoneLocalRotation(HumanBodyBones.Spine, AimIK.SpineTransform.localRotation);
            }

            if(Health.DisableMotion)
            {
                AttackBehaviour.RotateSpine();
            }
        }

        private void FixedUpdate()
        {
           
            //make sure, there's no calculation once the agent dies
            if (currentShooterState == ShooterAIStates.Die)
                return;

            //Check is the AI currently being hit?
            Health.AIGotHit();

            if (Health.DisableMotion)
                return;

            if (Health.cooldown)
                return;

            //identify state
            IdentifyStates();

            //set state
            SetState();
        }

        /// <summary>
        /// Call On Destination Reached when we enter the waypoint trigger
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (currentShooterState == ShooterAIStates.Firing)
                return;

            //if we enter the Awareness Trigger
            if (other.gameObject.tag == "AwarenessTrigger" && _AIType == AIType.Enemy)
            {
                //if we hear the sound of Player
                if (other.gameObject.GetComponentInParent<Transform>().tag == "Player")
                {
                    ChaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);
                    // FIX: Corrected typo from GetComponentInparent to GetComponentInParent
                    SightBehaviour.visibleTargets.Add(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);
                }
                //else if we hear other audio
                else
                {
                    ChaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Transform>().transform);
                }
            }

            //if we entered the collider tagged Destination
            //we call the on destination reached flag
            if (other.gameObject.tag == "Destination")
            {
                OnDestinationReached();
            }

        }

        /// <summary>
        /// Check if player has gone out of the trigger
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            SetLastPlayerPositionVector(other.transform);

        }

        public void IdentifyStates()
        {
            //if we are Chasing 
            if (currentShooterState == ShooterAIStates.Chase)
                return; // exit

            //make sure visible targets list is empty
            if (SightBehaviour.visibleTargets.Count > 0)
                return; //exit

            if (AttackBehaviour.Player || AttackBehaviour.Zombie)
                return;
             
            if(_AIType == AIType.Companion)
            {
                CompanionBehaviour.FollowPlayer();
            }
            else if (_AIType == AIType.Enemy)
            {
                //else we see if Timer is less then PatrolDelay
                if (_globalDelayTimer < PatrolBehaviour.PatrolDelay)
                    currentShooterState = ShooterAIStates.Idle; // go to Idle
                else if (_globalDelayTimer > PatrolBehaviour.PatrolDelay)
                    currentShooterState = ShooterAIStates.Patrol; // else Patrol
            }
           

        }

        public void SetState()
        {
            switch (currentShooterState)
            {
                case ShooterAIStates.Idle:
                    //Don't Aim
                    _animator.SetBool("isAiming", false);
                    IdleBehaviour.Idle();
                    break;

                case ShooterAIStates.Patrol:
                    //Don't Aim
                    _animator.SetBool("isAiming", false);
                    PatrolBehaviour.Patrol();
                    break;

                case ShooterAIStates.Chase:
                    //Aim
                    _animator.SetBool("isAiming", true);
                    ChaseBehaviour.Chase();
                    break;

                case ShooterAIStates.Firing:
                    //Aim
                    _animator.SetBool("isAiming", true);
                    AttackBehaviour.Attack();
                    break;
            }

            //set awareness trigger visibility
            WeaponBehaviour.SetAwarenessTriggerVisibility(currentShooterState);

            //draw debugs
            DrawGizmosBehaviour.DrawAIGizmos(_shooterAIAgent);
        }

        /// <summary>
        /// Gets the transform of the player and set that as the last known position
        /// </summary>
        /// <param name="p"></param>
        public void SetLastPlayerPositionVector(Transform p)
        {
            if (ChaseBehaviour.lastPlayerPosition == null)
                return;

            if (p.tag == "Player" || p.tag == "AwarenessTrigger")
            {
                //Debug.Log("currentChasingTargetTag : " + chaseBehaviour.currentChasingTargetTag);

                //if the last thing we were chasing was player
                if (ChaseBehaviour.currentChasingTargetTag == "Player" || ChaseBehaviour.currentChasingTargetTag == "AwarenessTrigger")
                {
                    //reset the current chasing target
                    ChaseBehaviour.currentChasingTargetTag = "";

                    ChaseBehaviour.lastPlayerPosition.position = p.position;
                    ChaseBehaviour.targetPosition = ChaseBehaviour.lastPlayerPosition;

                    //enable lpp
                    ChaseBehaviour.lastPlayerPosition.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// This will reset the timer and stops the agent on reaching the destination
        /// </summary>
        public void OnDestinationReached()
        {
            //Reset Delay
            Debug.Log("Destination Reached");
            if (_shooterAIAgent != null && _shooterAIAgent.isOnNavMesh)
            {
                _shooterAIAgent.isStopped = true;
            }
            _globalDelayTimer = 0;

            //disable lpp
            if (ChaseBehaviour.lastPlayerPosition != null)
            {
                ChaseBehaviour.lastPlayerPosition.gameObject.SetActive(false);
            }
            
            ChaseBehaviour.targetPosition = null;
            AttackBehaviour.Player = null;
            AttackBehaviour.ShooterAI = null;
            AttackBehaviour.Zombie =  null;
            AttackBehaviour.Turret =  null;

            //reset path
            PatrolBehaviour.resetPath = true;

            //set idle state
            currentShooterState = ShooterAIStates.Idle;
        }

        /// <summary>
        /// Kills the AI.
        /// </summary>
        public void Die()
        {
            //set state
            currentShooterState = ShooterAIStates.Die;

            //Play dialogue
            if (Manoeuvre.gc_PlayerDialoguesManager.Instance)
                Manoeuvre.gc_PlayerDialoguesManager.Instance.PlayDialogueClip(Manoeuvre.gc_PlayerDialoguesManager.DialogueType.Kills);

            //Play Death Sound Clip
             _audioManager.PlayAudioClip(Health.DeathSound);

            //--->> Destroy all the components <<---//

            //1. Randomize Mirror Death
            _animator.SetBool("DeathMirror", Random.value < 0.5f);

            _animator.SetTrigger("Death");

            _animator.SetInteger("DeathID", Health.DeathID);

            //2. Destroy nav mesh
            if (_shooterAIAgent != null)
            {
                Destroy(_shooterAIAgent);
            }

            //3. Destroy Colliders
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                Destroy(c);
            }

            //4. Destroy Rigidbody
            if(GetComponent<Rigidbody>() != null)
            {
                Destroy(GetComponent<Rigidbody>());
            }
            
            //5. Destroy Respective Last Known position
            if (ChaseBehaviour.lastPlayerPosition != null)
            {
                Destroy(ChaseBehaviour.lastPlayerPosition.gameObject);
            }

            //6. Destroy Icon
            Transform iconTransform = transform.Find("MinimapIcon");
            if(iconTransform != null)
            {
                GameObject icon = iconTransform.gameObject;
                if(icon != null)
                {
                    //gc_Minimap minimap = FindObjectOfType<gc_Minimap>();
                    //if(minimap != null)
                    //{
                    //    minimap.RemoveMinimapIcon(icon);
                    //}
                }
            }

            if (Health.FadeMesh)
            {
                //set all the materials to Fade
                foreach (Renderer r in Health.allRenderers)
                {
                    if (r == null) continue;
                    foreach (Material m in r.materials)
                    {
                        if (m == null || Health.faderMaterial == null) continue;
                        //assign it to the renderer
                        m.shader = Health.faderMaterial.shader;
                    }

                }

                //Finally, the fade coroutine,
                //which will fade the AI alpha
                //then destroy game object
                StartCoroutine(Health.FadeOnDeath());
            }
            else
            {
                Destroy(this.gameObject, 5f); // Destroy the game object after a delay, not just the script.
            }

            OnDeath.Invoke();
        }

        private void OnDrawGizmos()
        {
            if(AimIK.LeftHandIK)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(AimIK.LeftHandIK.position, .05f);
            }

            if (AimIK.LeftHandAimIK)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(AimIK.LeftHandAimIK.position, .05f);
            }


            if (!WeaponBehaviour.muzzleLocation)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(WeaponBehaviour.muzzleLocation.position, .05f);
        }
    }

    [System.Serializable]
    public class ShooterAIIdleBehaviour
    {

        [Range(0.1f, 10f)]
        [Tooltip("How fast you want to transition to Current State")]
        public float idleTransitionDuration = 5f;

        [Range(0f, 1f)]
        public float idleAnimation = 0f;

        //Main State Manager
        ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        /// <summary>
        /// Gets and sets the references of Shooter AI's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            //set Idle Transitions
            _animator.SetFloat("Vertical", 0);
        }

        /// <summary>
        /// This will set the nav mesh agent speed to Zero
        /// and make the animator go to Idle Animation
        /// </summary>
        public void Idle()
        {
            //making sure the agent is STOPPED!
            if (_shooterAIAgent != null && _shooterAIAgent.isOnNavMesh)
            {
                _shooterAIAgent.isStopped = true;
            }

            //set speed to 0
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * idleTransitionDuration);

            //set animation to Idle
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);

            //disable root motion
            _animator.applyRootMotion = false;

            //if our timer is less then the Patrol Delay
            if (_stateManager._globalDelayTimer < _stateManager.PatrolBehaviour.PatrolDelay)
                _stateManager._globalDelayTimer += Time.deltaTime; // increment
            else
                return; // exit

            //Debug.Log("Inside Idle State");
        }
    }

    [System.Serializable]
    public class ShooterAIPatrolBehaviour
    {
        [Tooltip("Assign the Patrolling Path of this Agent")]
        public List<Transform> PatrolPath;

        [Tooltip("This is the Patrol Speed  i.e it simply sets the movement in the Blend Tree of the Animator")]
        [Range(0.5f, 2f)]
        public float PatrolAnimation = 1f;

        [Tooltip("How much you want agent to stop before moving on to next Waypoint!")]
        [Range(0.1f, 10f)]
        public float PatrolDelay = 3f;

        public bool resetPath;
        //current waypoint we ARE heading towards
        int currentWaypoint;
        //the next waypoint we WILL BE heading towards
        int nextWaypoint;

        //Main State Manager
        ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        /// <summary>
        /// Gets and sets the references of Shooter AI's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            if (_shooterAIAgent != null)
            {
                //set stopping dist
                _shooterAIAgent.stoppingDistance = 1f;
            }
        }

        /// <summary>
        /// Patrol Behaviour
        /// </summary>
        public void Patrol()
        {
            //if we are not in patrolling state
            if (_stateManager.currentShooterState != ShooterAIStates.Patrol)
                return; // exit

            if (_stateManager._AIType == AIType.Companion)
            {
                PatrolPath.Clear();
                PatrolTowardsPlayer();
                return;
            }

            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh) return;

            //if there's no patrol path
            if (PatrolPath == null)
                return;

            //if there's no patrol point
            if (PatrolPath.Count < 1)
                return; // exit

            //else we patrol
            //.....................

            //Debug.Log("Patrolling");

            //if we have a path
            if (_shooterAIAgent.hasPath)
            {
                _shooterAIAgent.isStopped = false;
                _shooterAIAgent.updatePosition = true;

                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, PatrolAnimation, Time.deltaTime * 5);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);

            }

            //if the remaining distance is less then the stopping distance
            //i.e we have reched the Destination
            if (_shooterAIAgent.remainingDistance <= _shooterAIAgent.stoppingDistance || _shooterAIAgent.pathStatus != NavMeshPathStatus.PathComplete
                || _shooterAIAgent.isPathStale || resetPath)
            {
                //if we don't have a path or the velocity is 0
                if (!_shooterAIAgent.hasPath || _shooterAIAgent.velocity.sqrMagnitude == 0 || resetPath)
                {
                    if (_stateManager._globalDelayTimer >= PatrolDelay || resetPath)
                    {
                        if (PatrolPath.Count > nextWaypoint && PatrolPath[nextWaypoint] == null)
                            return;

                        //we set the current waypoint as the next waypoint
                        currentWaypoint = nextWaypoint;
                        _shooterAIAgent.SetDestination(PatrolPath[currentWaypoint].position);

                        //increment next waypoint accordingly
                        if (nextWaypoint < PatrolPath.Count - 1)
                            nextWaypoint++;
                        else
                            nextWaypoint = 0;

                        resetPath = false;
                    }
                }
            }

            if (PatrolPath.Count > currentWaypoint && PatrolPath[currentWaypoint] != null &&
                Vector3.Distance(_shooterAIAgent.transform.position, PatrolPath[currentWaypoint].position) <= _shooterAIAgent.stoppingDistance)
            {
                //reset global timer
                _stateManager._globalDelayTimer = 0;
            }
        }

        /// <summary>
        /// If AI Type is Companion, we just simply patrol towards Player
        /// </summary>
        public void PatrolTowardsPlayer()
        {
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh) return;

            //if we have a path
            if (_shooterAIAgent.hasPath)
            {
                _shooterAIAgent.isStopped = false;
                _shooterAIAgent.updatePosition = true;

                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, PatrolAnimation, Time.deltaTime * 5);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);
            }
        }
    }

    [System.Serializable]
    public class ShooterAIChaseBehaviour
    {
        [Tooltip("What is the current Target 'Tag' we are chasing?")]
        public string currentChasingTargetTag = "";
        [Tooltip("Target's position where we are heading towards.")]
        public Transform targetPosition;
        [Tooltip("Chase speed used to Blend in the Animation States.")]
        [Range(1f, 2f)]
        public float chaseAnimation = 2f;
        [Tooltip("If true, AI will look towards the target he's Chasing.")]
        public bool useHeadTrack = true;

        public Transform lastPlayerPosition;

        //Main State Manager
        ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        /// <summary>
        /// Gets and sets the references of AI StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            lastPlayerPosition = new GameObject().transform;
            lastPlayerPosition.name = "Last Known Position";
            lastPlayerPosition.gameObject.AddComponent<SphereCollider>().isTrigger = true;
            
            // This will throw an error if the layer/tag is not defined in Project Settings.
            // It's the user's responsibility to set them up.
            lastPlayerPosition.gameObject.layer = LayerMask.NameToLayer("Default"); // Layer 11 may not exist, safer to default. User can change.
            lastPlayerPosition.gameObject.tag = "Destination";

            //disable lpp
            lastPlayerPosition.gameObject.SetActive(false);
        }

        /// <summary>
        /// We check whether we can chase this target or not!
        /// </summary>
        public void CanChaseThisTarget(Transform t)
        {
            //if t is the same target we are chasing
            if (t.tag == currentChasingTargetTag)
                return;

            //if it is companion, make sure
            //we don't chase Player
            if (_stateManager._AIType == AIType.Companion && t.tag == "Player")
                return; //exit

            //See if we are not chasing anything as of now...
            if (string.IsNullOrEmpty(currentChasingTargetTag))
            {
                // We get the transform
                targetPosition = t;

                // Change State
                _stateManager.currentShooterState = ShooterAIStates.Chase;
                // Set Tag
                currentChasingTargetTag = t.tag;
            }
        }

        /// <summary>
        /// Simply chases the Target Position
        /// </summary>
        public void Chase()
        {
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh)
                return;

            //if we got hit
            if (_stateManager.currentShooterState == ShooterAIStates.Hit)
                return;

            //if we are not Firing
            if (_stateManager.currentShooterState == ShooterAIStates.Firing)
                return;

            //if there's no target position
            if (!targetPosition)
            {
                _stateManager.currentShooterState = ShooterAIStates.Idle;
                return;
            }
            //if we are not already chasing the current target
            if (_shooterAIAgent.destination != targetPosition.position)
            {
                //set it
                _shooterAIAgent.SetDestination(targetPosition.position);
            }

            //make sure agent isn't stopped
            _shooterAIAgent.isStopped = false;

            //lerp speed
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, chaseAnimation, Time.deltaTime * 5);

            //set locomotion
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);
        }
    }

    [System.Serializable]
    public class ShooterAISightBehaviour
    {
        [Header("Sight Properties")]
        [Tooltip("Define how far this agent can see / hear.")]
        [Range(1f, 25f)]
        public float Range = 10f;

        [Tooltip("View Angle of this Agent, It is Recommended that you change it in Editor for better results.")]
        [Range(0f, 360f)]
        public float Angle;

        [Tooltip("How fast you want to check for Threats.")]
        public float SearchIterationTime = 0.25f;

        [Tooltip("What is the Target Layer Mask.")]
        public LayerMask targetMask;
        [Tooltip("What is the Obstacles Layer Mask.")]
        public LayerMask obstacleMask;

        //[HideInInspector]
        //Currently visible targets
        public List<Transform> visibleTargets = new List<Transform>();

        [HideInInspector]
        public ShooterAIStateManager _stateManager;

        /// <summary>
        /// Create a sensor trigger
        /// </summary>
        public void CreateSensorTrigger()
        {
            Transform t = new GameObject().transform;
            t.SetParent(_stateManager.transform);
            t.gameObject.AddComponent<SphereCollider>().radius = Range;
            t.gameObject.GetComponent<SphereCollider>().isTrigger = true;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.name = "Sensor Trigger";
            // This will throw an error if the layer is not defined in Project Settings.
            // It's the user's responsibility to set it up.
            int layer = LayerMask.NameToLayer("ShooterAISensor");
            if (layer != -1)
            {
                t.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// Using a Coroutine to optimize the searching process
        /// </summary>
        public IEnumerator SearchTargetsCoroutine()
        {
            while (true)
            {
                //instead of calling every frame
                //we are calling it in every search iteration time
                yield return new WaitForSeconds(SearchIterationTime);
                SearchVisibleTarget();
            }
        }

        /// <summary>
        /// Search the visible Targets as soon as they enter in our Vision Angle
        /// </summary>
        void SearchVisibleTarget()
        {
            if (_stateManager == null) return;
            //Clear all previously added targets
            visibleTargets.Clear();

            //We get all the colliders in our Range
            Collider[] targetsInViewRadius = Physics.OverlapSphere(_stateManager.transform.position, Range, targetMask);

            //we loop through all the above registered colliders
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                //get the target
                Transform target = targetsInViewRadius[i].transform;

                //else
                //we get the direction
                Vector3 dirToTarget = (target.position - _stateManager.transform.position).normalized;

                //if the angle between AI and the direction is less then the view angle
                if (Vector3.Angle(_stateManager.transform.forward, dirToTarget) < Angle / 2)
                {
                    //we see the distance of this target
                    //from our position
                    float distToTarget = Vector3.Distance(_stateManager.transform.position, target.position);

                    //if there's no obstacle in between
                    if (!Physics.Raycast(_stateManager.transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        //if ai type is companion, ignore Player Tag
                        if (_stateManager._AIType == AIType.Companion && target.tag == "Player")
                            continue; // Use continue to check other targets

                        if(target.tag == "ShooterAI")
                        {
                            var otherAI = target.GetComponent<ShooterAIStateManager>();
                            //return if both are companion or both are of enemy types
                            if (otherAI != null && _stateManager._AIType == otherAI._AIType)
                                continue; // Use continue
                        }

						if (target.tag == "uzAIZombie" || target.tag == "ShooterAI" || target.tag == "Player" || target.tag == "Turret") {    
							//we add it in our visible targets list
							visibleTargets.Add (target);
							//start chasing
							_stateManager.ChaseBehaviour.CanChaseThisTarget (target.transform);
						}
                    }
                    //if there is an obstacle in between
                    else
                    {
                        //break the line of sight and make that our last know poasition
                        _stateManager.SetLastPlayerPositionVector(target);
                    }
                }
            }
        }

        /// <summary>
        /// This is only used in the editor to make the arc
        /// </summary>
        public Vector3 DirFromAngle(float angle, bool isAngleGlobal)
        {
            if (_stateManager)
            {
                if (!isAngleGlobal)
                {
                    angle += _stateManager.transform.eulerAngles.y;
                }
                float retAngle = angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(retAngle), 0, Mathf.Cos(retAngle));
            }
            return Vector3.zero;
        }
    }

    [System.Serializable]
    public class ShooterAIAttackBehaviour
    {
        [Header("Attack Properties")]
        public ManoeuvreFPSController Player;
        public ShooterAIStateManager ShooterAI;
		public Turret Turret;
		public uzAI.uzAIZombieStateManager Zombie;

        [Tooltip("Define from how far this agent can Attack.")]
        [Range(1.5f, 15f)]
        public float AttackDistance = 10f;

        [Tooltip("Attack Angle of this Agent, It is Recommended that you change it in Editor for better results.")]
        [Range(0f, 360f)]
        public float Angle;

        [Tooltip("How fast you want to check for Threats.")]
        public float SearchIterationTime = 0.25f;

        [Tooltip("What is the Target Layer Mask.")]
        public LayerMask targetMask;
        [Tooltip("What is the Obstacles Layer Mask.")]
        public LayerMask obstacleMask;

        [Tooltip("Wait between 2 consecutive attacks.")]
        [Range(0f, 5f)]
        public float FireDelay = 1f;

        [HideInInspector]
        public bool isReloading;
        
        //Main State Manager
        [HideInInspector]
        public ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        public bool hasAttacked;
        float _timer;

        //[HideInInspector]
        //Currently visible targets
        public List<Transform> visibleTargets = new List<Transform>();

        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;
        }

        public IEnumerator SearchAttackTargetsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(SearchIterationTime);
                SearchAttackTargets();
            }
        }

        void SearchAttackTargets()
        {
            if (_stateManager == null) return;
            //Clear all previously added targets
            visibleTargets.Clear();

            //We get all the colliders in our Range
            Collider[] targetsInViewRadius = Physics.OverlapSphere(_stateManager.transform.position, AttackDistance, targetMask);

            //we loop through all the above registered colliders
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                //we are only interested in 'Player', 'Zombie', 'Shooter AI' and 'Turret'
				if (targetsInViewRadius[i].tag == "Player" || targetsInViewRadius[i].tag == "uzAIZombie" || targetsInViewRadius[i].tag == "ShooterAI" || targetsInViewRadius[i].tag == "Turret")
                {
                    //if ai type is companion, ignore Player Tag
                    if (_stateManager._AIType == AIType.Companion && targetsInViewRadius[i].tag == "Player")
                        continue;

                    //get the target
                    Transform target = targetsInViewRadius[i].transform;

                    //we get the direction
                    Vector3 dirToTarget = (target.position - _stateManager.transform.position).normalized;

                    //if the angle between AI and the direction is less then the view angle
                    if (Vector3.Angle(_stateManager.transform.forward, dirToTarget) < Angle / 2)
                    {
                        //we see the distance of this target from our position
                        float distToTarget = Vector3.Distance(_stateManager.transform.position, target.position);

                        //if there's no obstacle in between
                        if (!Physics.Raycast(_stateManager.transform.position, dirToTarget, distToTarget, obstacleMask))
                        {
                            //we add it in our visible targets list
                            visibleTargets.Add(target);

                            //assign player
							if (target.GetComponent<ManoeuvreFPSController> ()) {
								Player = target.GetComponent<ManoeuvreFPSController> ();
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;
							}
                            //assign Shooter AI
                            else if (target.GetComponent<ShooterAIStateManager> ()) {
                                var otherAI = target.GetComponent<ShooterAIStateManager>();
								if (otherAI != null && otherAI._AIType != _stateManager._AIType) {
									ShooterAI = otherAI;
									_stateManager.currentShooterState = ShooterAIStates.Firing;
									_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
									_stateManager.ChaseBehaviour.targetPosition = null;
								}
							} 
							//assign Turret
							else if (target.GetComponent<Turret> ()) {
								Turret = target.GetComponent<Turret> ();
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;
							}
                            //assign Zombie AI
                            else if (target.GetComponent<uzAI.uzAIZombieStateManager>())
                            {
                                var zombie = target.GetComponent<uzAI.uzAIZombieStateManager>();
                                if (zombie != null && zombie.ZombieHealthStats.CurrentHealth > 0)
                                {
                                    Zombie = zombie;
                                    _stateManager.currentShooterState = ShooterAIStates.Firing;
                                    _stateManager.ChaseBehaviour.currentChasingTargetTag = "";
                                    _stateManager.ChaseBehaviour.targetPosition = null;
                                }
                            }
                            else
                            { 
                                _stateManager.ChaseBehaviour.currentChasingTargetTag = "";
                                _stateManager.ChaseBehaviour.targetPosition = null;
                                _stateManager.currentShooterState = ShooterAIStates.Patrol;
                            }
                        }
                        //if there is an obstacle in between
                        else
                        {
                            _stateManager.currentShooterState = ShooterAIStates.Chase;
                        }
                    }
                }
            }
        }

        public void Attack()
        {
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh)
                return;

            isReloading = _animator.GetBool("isReloading");

            if (isReloading)
                return;

            //stop nav agent
            _shooterAIAgent.isStopped = true;

            //rotate Spine
            RotateSpine();

            //Rotate Towards Target
            RotateTowardsTarget();

            //go to Idle
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * 5);
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);

            //see if the delay has been made
            if (_timer >= FireDelay )
            {
                //reset timer
                _timer = 0;
                //reset has Attack flag
                hasAttacked = false;
            }
            else
            {
                //delay
                _timer += Time.deltaTime;
            }

            //ONLY IF we haven't attacked already
            if (!hasAttacked && CanAttack() && _stateManager.WeaponBehaviour.weaponObject)
            {
                //set attack trigger in Animator
                _animator.SetTrigger("isFiring");

                //reduce ammo
                _stateManager.WeaponBehaviour.Ammo--;

				if (Player && Zombie == null && ShooterAI == null) {
					if (Player.Health.currentHealth > 0)
                        _stateManager.WeaponBehaviour.ShootTarget (Player);
					else
                        Player.TakeDamageEffect ();
				}
                else if (ShooterAI) {
					_stateManager.WeaponBehaviour.ShootTarget (ShooterAI);
				}
                else if (Zombie) {
					_stateManager.WeaponBehaviour.ShootTarget (Zombie);
				} 
				else if (Turret) {
					_stateManager.WeaponBehaviour.ShootTarget (Turret);
				}

                //enable flag to say that we have attacked at this frame
                hasAttacked = true;
            }
            //if we have to attack but we can't attack
            else if (!hasAttacked && !CanAttack())
            {
                hasAttacked = true;

                //set animation
                _animator.SetTrigger("Reload");

                //Play Reload clip
                if (_stateManager.WeaponBehaviour.ReloadSound)
                    _stateManager._audioManager.PlayAudioClip(_stateManager.WeaponBehaviour.ReloadSound);
            }
        }

        public void RotateSpine()
        {
            Vector3 destinationRotation = Vector3.zero;

			if (Player) {
				destinationRotation = _stateManager.transform.position - Player.transform.position;
			} else if (ShooterAI) {
				destinationRotation = _stateManager.transform.position - ShooterAI.transform.position;
			} else if (Zombie) {
				destinationRotation = _stateManager.transform.position - Zombie.transform.position;
			} else if (Turret) {
				destinationRotation = _stateManager.transform.position - Turret.transform.position;
			}

            if (_stateManager.AimIK.SpineTransform != null)
            {
                Quaternion newRot = Quaternion.Euler(destinationRotation.x + _stateManager.AimIK.AimSpineOffset_X, _stateManager.AimIK.AimSpineOffset_Y, _stateManager.AimIK.AimSpineOffset_Z);
                _stateManager.AimIK.SpineTransform.localRotation = newRot;
            }
        }

        void RotateTowardsTarget()
        {
			if (Player && Zombie == null && ShooterAI == null && Turret == null)
                _stateManager.transform.LookAt(Player.transform.position);
            else if (ShooterAI)
            {
                if (ShooterAI.Health.Health > 0)
                    _stateManager.transform.LookAt(ShooterAI.transform.position);
                else
                    ShooterAI = null;
            }
            else if (Zombie)
            {
                if (Zombie.ZombieHealthStats.CurrentHealth > 0)
                    _stateManager.transform.LookAt(Zombie.transform.position);
                else
                    Zombie = null;
            }
			else if (Turret)
			{
				if (Turret._turretHealth.Health > 0)
					_stateManager.transform.LookAt(Turret.transform.position);
				else
					Turret = null;
			}
        }

        bool CanAttack()
        {
            if (_stateManager.AimIK.DebugAimIK)
                return true;

            if(_stateManager.WeaponBehaviour.Ammo > 0)
                return true;

            return false;
        }

        public Vector3 DirFromAngle(float angle, bool isAngleGlobal)
        {
            if (_stateManager)
            {
                if (!isAngleGlobal)
                {
                    angle += _stateManager.transform.eulerAngles.y;
                }
                float retAngle = angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(retAngle), 0, Mathf.Cos(retAngle));
            }
            return Vector3.zero;
        }
    }

    [System.Serializable]
    public class ShooterAIWeaponBehaviour
    {
        public Transform weaponObject;
        public Transform muzzleLocation;
        public ParticleSystem muzzleFlash;
        public AudioClip FireSound;
        public AudioClip ReloadSound;
        [HideInInspector]
        public GameObject myAwarenessTrigger;

        public int Ammo = 20;
        [Range(1, 15)]
        public int maxDamage = 10;
        [Range(1, 15)]
        public int minDamage = 5;

        public List<hitInfo> HitParticle = new List<hitInfo>();

        ShooterAIStateManager _stateManager;
        ParticleSystem _cacheMuzzleFX;
        int cacheAmmo;

        public void Initialize(ShooterAIStateManager _smgr, Animator _anim)
        {
            _stateManager = _smgr;
            cacheAmmo = Ammo;

            if (muzzleFlash != null && muzzleLocation != null)
            {
                _cacheMuzzleFX = GameObject.Instantiate(muzzleFlash, muzzleLocation.position, muzzleLocation.rotation, muzzleLocation);
                _cacheMuzzleFX.Stop();
            }

            if (weaponObject == null)
                _anim.SetTrigger("Unarmed");

            InitializeAudioTarget();
        }

        public void ShootTarget(ManoeuvreFPSController _controller)
        {
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();
            
            _stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = _controller.transform.position - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if(hit.transform.tag == _controller.transform.tag)
                {
                    if(!_stateManager.AimIK.DebugAimIK)
                        _controller.Health.OnDamage(Random.Range(minDamage, maxDamage));
                    if (HitParticle.Count > 0 && HitParticle[0] != null)
                        HitParticle[0].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void ShootTarget(ShooterAIStateManager _shooterAI)
        {
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();

            _stateManager._audioManager.PlayAudioClip(FireSound);
            
            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = (_shooterAI.transform.position + Vector3.up) - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _shooterAI.transform.tag)
                {
                    if(!_stateManager.AimIK.DebugAimIK)
                        _shooterAI.Health.onDamage(Random.Range(minDamage, maxDamage));

                    _shooterAI.ChaseBehaviour.targetPosition = _stateManager.transform;
                    _shooterAI.AttackBehaviour.ShooterAI = _stateManager;

                    if (HitParticle.Count > 2 && HitParticle[2] != null)
                        HitParticle[2].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

		public void ShootTarget(Turret _turret)
		{
			if (_cacheMuzzleFX)
				_cacheMuzzleFX.Play();

			_stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
			RaycastHit hit;
			Vector3 direction = (_turret.transform.position + Vector3.up) - muzzleLocation.position;

			if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
			{
				if (hit.transform.tag == _turret.transform.tag)
				{
					if(!_stateManager.AimIK.DebugAimIK)
						_turret._turretHealth.onDamage(Random.Range(minDamage, maxDamage), _stateManager.transform);

                    if (HitParticle.Count > 3 && HitParticle[3] != null)
					    HitParticle[3].onHit(hit.transform, hit.point, hit.normal);
				}
			}
		}

        public void ShootTarget(uzAI.uzAIZombieStateManager _uzAI)
        {
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();

            _stateManager._audioManager.PlayAudioClip(FireSound);
            
            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = (_uzAI.transform.position + Vector3.up) - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, 100, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _uzAI.transform.tag)
                {
                    if(!_stateManager.AimIK.DebugAimIK)
                        _uzAI.ZombieHealthStats.onDamage(Random.Range(minDamage, maxDamage));

                    if (HitParticle.Count > 1 && HitParticle[1] != null)
                        HitParticle[1].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void RefillAmmo()
        {
            Ammo = cacheAmmo;
        }

        void InitializeAudioTarget()
        {
            myAwarenessTrigger = new GameObject("AwarenessTrigger");
            myAwarenessTrigger.transform.SetParent(_stateManager.transform);
            myAwarenessTrigger.transform.localPosition = Vector3.zero;
            myAwarenessTrigger.SetActive(false);
            myAwarenessTrigger.AddComponent<SphereCollider>().isTrigger = true;
            
            int layer = LayerMask.NameToLayer("AwarenessTrigger");
            if(layer != -1)
                myAwarenessTrigger.layer = layer;
            
            myAwarenessTrigger.tag = "AwarenessTrigger";
        }

        public void SetAwarenessTriggerVisibility(ShooterAIStates _state)
        {
            if (myAwarenessTrigger == null) return;

            if (_state == ShooterAIStates.Firing)
            {
                myAwarenessTrigger.GetComponent<SphereCollider>().radius = _stateManager.SightBehaviour.Range;
                myAwarenessTrigger.SetActive(true);
            }
            else
            {
                myAwarenessTrigger.GetComponent<SphereCollider>().radius = 0;
                myAwarenessTrigger.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class ShooterAICompanionBehaviour
    {
        public float PlayerDistance = 1;
        public float FollowAnimation = 2;
        public bool AllowDamageFromPlayer = true;

        ShooterAIStateManager _stateManager;
        Animator _animator;
        NavMeshAgent _shooterAIAgent;

        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;
        }

        public void FollowPlayer()
        {
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh || _stateManager.Player == null) return;

            if (_stateManager.AttackBehaviour.Zombie || _stateManager.currentShooterState == ShooterAIStates.Die)
                return;

            _shooterAIAgent.SetDestination(_stateManager.Player.transform.position);
            _stateManager.PatrolBehaviour.PatrolAnimation = FollowAnimation;
            _shooterAIAgent.stoppingDistance = PlayerDistance;

            if (_shooterAIAgent.remainingDistance > _shooterAIAgent.stoppingDistance)
            {
                _stateManager.currentShooterState = ShooterAIStates.Patrol;
            }
            else
            {
                _shooterAIAgent.transform.LookAt(_stateManager.Player.transform.position);
                _stateManager.currentShooterState = ShooterAIStates.Idle;

                if (_stateManager.AimIK.DebugAimIK)
                    _stateManager._AIType = AIType.Enemy;
            }
        }
    }

    [System.Serializable]
    public class ShooterAIHealth
    {
        [Range(0, 200)]
        public int Health = 100;

        [Tooltip("How many Hit Reaction animations are there in the Animator." )]
        public int hitReactionsAvailable = 3;

        [Tooltip("Death Animation ID you want to play.")]
        public int DeathID = 1;

        [Tooltip("How long you want to wait to transit from hit animation to other state.")]
        [Range(0.15f, 5f)]
        public float _cooldownTimer = 0.85f;

        [Tooltip("if true, AI will look at player on getting hit [ONLY if he's looking somewhere else]")]
        public bool lookAtCameraOnHit = true;

        public AudioClip DeathSound;
        public List<AudioClip> HitSound = new List<AudioClip>();

        public bool FadeMesh = false;
        public Material faderMaterial;
        public List<Renderer> allRenderers = new List<Renderer>();

        [Range(0f, 5f)]
        public float fadeDelay = 0.5f;
        [Range(0f, 5f)]
        public float fadeDuration = 0.5f;

        public bool DisableMotion;
        public bool cooldown;
        public float _timer;

        [HideInInspector]
        public Healthbar healthBar;

        ShooterAIStateManager _stateManager;
        Animator _animator;
        NavMeshAgent _shooterAIAgent;

        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;
        }

        public void onDamage(int amt)
        {
            if (Health <= 0) return; // Already dead

            if(_stateManager._AIType == AIType.Enemy)
                Health -= amt;
            else if(_stateManager._AIType == AIType.Companion && _stateManager.CompanionBehaviour.AllowDamageFromPlayer)
                Health -= amt;

            if (healthBar)
                healthBar.StartLerp();

            if (Health <= 0)
            {
                Health = 0;
                _stateManager.Die();
            }
            else
            {
                HitReaction();
            }
        }

        int i = 1;
        void HitReaction()
        {
            if (HitSound.Count > 0 && _stateManager._audioManager._source != null && !_stateManager._audioManager._source.isPlaying)
                _stateManager._audioManager.PlayAudioClip(HitSound[Random.Range(0, HitSound.Count)]);

            _animator.SetTrigger("HitReaction");
            _animator.SetInteger("HitID", i);

            if (lookAtCameraOnHit)
            {
                if (_stateManager.AttackBehaviour.Player)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Player.transform.position);
                else if (_stateManager.AttackBehaviour.Zombie)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Zombie.transform.position);
                else if(_stateManager.AttackBehaviour.ShooterAI)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.ShooterAI.transform.position);
            }

            cooldown = false;
            
            if (i < hitReactionsAvailable)
                i++;
            else
                i = 1;
        }

        public void AIGotHit()
        {
            if (_shooterAIAgent == null) // The agent might have been destroyed
                return;

            if (_stateManager.currentShooterState == ShooterAIStates.Die)
                return;

            DisableMotion = _animator.GetBool("DisableMotion");

            if (DisableMotion)
            {
                if (_shooterAIAgent.isOnNavMesh)
                {
                    _shooterAIAgent.isStopped = true;
                }
                
                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * 5f);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);
                _timer = 0;
                cooldown = true;
            }

            if(cooldown)
            {
                _timer += Time.deltaTime;
                if (_timer >= _cooldownTimer)
                {
                    _timer = 0;
                    cooldown = false;
                    _stateManager.setHandsIK = true;
                }
            }
        }

        public IEnumerator FadeOnDeath()
        {
            yield return new WaitForSeconds(fadeDelay);

            if(_animator != null)
                _animator.enabled = false;

            float et = 0;
            Color c = Color.white;

            while (et < fadeDuration)
            {
                c.a = Mathf.Lerp(1f, 0f, et / fadeDuration);
                foreach (Renderer r in allRenderers)
                {
                    if (r == null) continue;
                    foreach (Material m in r.materials)
                    {
                        if (m == null) continue;
                        m.color = c;
                    }
                }
                et += Time.deltaTime;
                yield return null;
            }

            if (_stateManager != null)
                GameObject.Destroy(_stateManager.gameObject);
        }
    }
    
    [System.Serializable]
    public class ShooterAIAimIK
    {
        public Transform SpineTransform;
        public Transform LeftHandIK;
        public Transform LeftHandAimIK;

        public bool DebugAimIK;

        public float AimSpineOffset_X;
        public float AimSpineOffset_Y;
        public float AimSpineOffset_Z;

        ShooterAIStateManager _stateManager;

        public void Initialize(ShooterAIStateManager _mgr)
        {
            _stateManager = _mgr;

            SpineTransform = new GameObject("Spine Bone IK").transform;
            SpineTransform.SetParent(_stateManager.transform);
            SpineTransform.localRotation = Quaternion.identity;
            SpineTransform.localPosition = Vector3.zero;
        }
    }

    [System.Serializable]
    public class ShooterAIAudioManager
    {
        ShooterAIStateManager _stateManager;
        public AudioSource _source;

        public void Initialize(ShooterAIStateManager _mgr)
        {
            _stateManager = _mgr;
            if (_mgr.GetComponent<AudioSource>() == null)
            {
                _source = _mgr.gameObject.AddComponent<AudioSource>();
            }
            else
            {
                _source = _mgr.GetComponent<AudioSource>();
            }
        }

        public void PlayAudioClip(AudioClip clip)
        {
            if (clip != null && _source != null)
            {
                _source.PlayOneShot(clip);
            }
        }
    }

    [System.Serializable]
    public class DrawGizmos
    {
        [Tooltip("If true, Draws the Nav mesh Path from AI to Current Target")]
        public bool drawPathToCurrentTarget = true;
        public Color pathGizmoColor = Color.cyan;
        [Tooltip("If true, Draws the Straight Line from AI to Current Target")]
        public bool drawLineToCurrentTarget = true;
        public Color lineGizmoColor = Color.green;

        public void DrawAIGizmos(NavMeshAgent _shooterAIAgent)
        {
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh || !_shooterAIAgent.hasPath)
                return;

            if (drawPathToCurrentTarget)
            {
                for (int i = 0; i < _shooterAIAgent.path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(_shooterAIAgent.path.corners[i], _shooterAIAgent.path.corners[i + 1], pathGizmoColor);
                }
            }

            if (drawLineToCurrentTarget && _shooterAIAgent.path.corners.Length > 0)
            {
                Debug.DrawLine(_shooterAIAgent.transform.position, _shooterAIAgent.path.corners[_shooterAIAgent.path.corners.Length - 1], lineGizmoColor);
            }
        }
    }
}