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
            if (!_shooterAIAgent)
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
            _shooterAIAgent.isStopped = true;
            _globalDelayTimer = 0;

            //disable lpp
            ChaseBehaviour.lastPlayerPosition.gameObject.SetActive(false);

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
        /// Kills the Zombie.
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
            Destroy(_shooterAIAgent);

            //3. Destroy Colliders
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                Destroy(c);
            }

            //4. Destroy Rigidbody
            Destroy(GetComponent<Rigidbody>());

            //5. Destroy Respective Last Know position
            Destroy(ChaseBehaviour.lastPlayerPosition.gameObject);

            //6. Destroy Icon
            GameObject icon = transform.Find("MinimapIcon").gameObject;
            if(icon)
            {
                FindObjectOfType<gc_Minimap>().RemoveMinimapIcon(icon);
            }

            if (Health.FadeMesh)
            {
                //set all the materials to Fade
                foreach (Renderer r in Health.allRenderers)
                {
                    foreach (Material m in r.materials)
                    {
                        //assign it to the renderer
                        m.shader = Health.faderMaterial.shader;
                    }

                }

                //Finally, the fade coroutine,
                //which will fade the zombie alpha
                //then destroy game object
                StartCoroutine(Health.FadeOnDeath());
            }
            else
                Destroy(this);

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
            _shooterAIAgent.isStopped = true;

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

            //set stopping dist
            _shooterAIAgent.stoppingDistance = 1f;
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
                        if (PatrolPath[nextWaypoint] == null)
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

            if ( Vector3.Distance(_shooterAIAgent.transform.position, PatrolPath[currentWaypoint].position) <= _shooterAIAgent.stoppingDistance)
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
        [Tooltip("If true, Zombie will look towards the target he's Chasing.")]
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
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            lastPlayerPosition = new GameObject().transform;
            lastPlayerPosition.name = "Last Known Position";
            lastPlayerPosition.gameObject.layer = 11;
            lastPlayerPosition.gameObject.AddComponent<SphereCollider>().isTrigger = true;
            lastPlayerPosition.gameObject.tag = "Destination";

            //disable lpp
            lastPlayerPosition.gameObject.SetActive(false);
        }

        /// <summary>
        /// We check whether we can chase this target or not!
        /// </summary>
        /// <param name="t"></param>
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
            if (!_shooterAIAgent)
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
            t.gameObject.layer = LayerMask.NameToLayer("ShooterAISensor");
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

                //if the angle between Zombie and the direction is less then the view angle
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
                            return; //exit

                        if(target.tag == "ShooterAI")
                        {
                            //return if both are companion or both are of enemy types
                            if (_stateManager._AIType == target.GetComponent<ShooterAIStateManager>()._AIType)
                                return;
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
        /// <param name="angle"></param>
        /// <param name="isAngleGlobal"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets and sets the references of AI StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            

        }

        /// <summary>
        /// Using a Coroutine to optimize the searching process
        /// </summary>
        public IEnumerator SearchAttackTargetsCoroutine()
        {
            while (true)
            {
                //instead of calling every frame
                //we are calling it in every search iteration time
                yield return new WaitForSeconds(SearchIterationTime);
                SearchAttackTargets();
            }
        }

        /// <summary>
        /// Search the visible Targets as soon as they enter in our Vision Angle
        /// </summary>
        void SearchAttackTargets()
        {
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
                        return; //exit

                    //get the target
                    Transform target = targetsInViewRadius[i].transform;

                    //else
                    //we get the direction
                    Vector3 dirToTarget = (target.position - _stateManager.transform.position).normalized;

                    //if the angle between Zombie and the direction is less then the view angle
                    if (Vector3.Angle(_stateManager.transform.forward, dirToTarget) < Angle / 2)
                    {
                        //we see the distance of this target
                        //from our position
                        float distToTarget = Vector3.Distance(_stateManager.transform.position, target.position);

                        //if there's no obstacle in between
                        if (!Physics.Raycast(_stateManager.transform.position, dirToTarget, distToTarget, obstacleMask))
                        {
                            //we add it in our visible targets list
                            visibleTargets.Add(target);

                            //assign player
							if (target.GetComponent<ManoeuvreFPSController> ()) {
								Player = target.GetComponent<ManoeuvreFPSController> ();
								//start Attacking
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								//clear chase target of chase behaviour
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;

							}
                            //assign Shooter AI
                            else if (target.GetComponent<ShooterAIStateManager> ()) {
								if (target.GetComponent<ShooterAIStateManager> ()._AIType != _stateManager._AIType) {
									ShooterAI = target.GetComponent<ShooterAIStateManager> ();
									//start Attacking
									_stateManager.currentShooterState = ShooterAIStates.Firing;
									//clear chase target of chase behaviour
									_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
									_stateManager.ChaseBehaviour.targetPosition = null;
								}                                

							} 
							//assign Turret
							else if (target.GetComponent<Turret> ()) {
							
								Turret = target.GetComponent<Turret> ();
								//start Attacking
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								//clear chase target of chase behaviour
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;

							}
                            //assign Zombie AI
                            else
                            {
                                if (target.GetComponent<uzAI.uzAIZombieStateManager>())
                                {
                                    if (target.GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats. CurrentHealth > 0)
                                    {
                                        Zombie = target.GetComponent<uzAI.uzAIZombieStateManager>();
                                        //start Attacking
                                        _stateManager.currentShooterState = ShooterAIStates.Firing;
                                        //clear chase target of chase behaviour
                                        _stateManager.ChaseBehaviour.currentChasingTargetTag = "";
                                        _stateManager.ChaseBehaviour.targetPosition = null;
                                    }
                                        
                                }
                                else
                                { 
                                    //clear chase target of chase behaviour
                                    _stateManager.ChaseBehaviour.currentChasingTargetTag = "";
                                    _stateManager.ChaseBehaviour.targetPosition = null;

                                    _stateManager.currentShooterState = ShooterAIStates.Patrol;
                                }
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

        /// <summary>
        /// Complex Attack Mech
        /// </summary>
        public void Attack()
        {
            if (!_shooterAIAgent)
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

                //Attack Player
                //only if
                //There's No Zombie and No Shooter AI
				if (Player && Zombie == null && ShooterAI == null) {
					if (Player.Health.currentHealth > 0)
                        //Player.Health.OnDamage(Random.Range(_stateManager.WeaponBehaviour.minDamage, _stateManager.WeaponBehaviour.maxDamage));
                        _stateManager.WeaponBehaviour.ShootTarget (Player);
					else
                        //just shake player camera
                        Player.TakeDamageEffect ();

				}
                //Attack Shooter AI
                else if (ShooterAI) {
					_stateManager.WeaponBehaviour.ShootTarget (ShooterAI);
				}
                //Attack Zombie
                else if (Zombie) {
					_stateManager.WeaponBehaviour.ShootTarget (Zombie);
				} 
				//Attack Zombie
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
            //prepare rotation quaternion
            Vector3 destinationRotation = Vector3.zero;

            //rotate Spine
			if (Player) {
				destinationRotation = _stateManager.transform.position - Player.transform.position;
			} else if (ShooterAI) {
				destinationRotation = _stateManager.transform.position - ShooterAI.transform.position;
			} else if (Zombie) {
				destinationRotation = _stateManager.transform.position - Zombie.transform.position;

			} else if (Turret) {
				destinationRotation = _stateManager.transform.position - Turret.transform.position;
			}

            ////set rotation
            Quaternion newRot = Quaternion.identity;
            newRot = Quaternion.Euler(destinationRotation.x + _stateManager.AimIK.AimSpineOffset_X, _stateManager.AimIK.AimSpineOffset_Y, _stateManager.AimIK.AimSpineOffset_Z);
           _stateManager.AimIK.SpineTransform.localRotation = newRot;

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

        /// <summary>
        /// This is only used in the editor to make the arc
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="isAngleGlobal"></param>
        /// <returns></returns>
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

            //set muzzle flash
            _cacheMuzzleFX = GameObject.Instantiate(muzzleFlash) as ParticleSystem;
            _cacheMuzzleFX.transform.SetParent(muzzleLocation);
            _cacheMuzzleFX.transform.localEulerAngles = Vector3.zero;
            _cacheMuzzleFX.transform.localPosition = Vector3.zero;
            _cacheMuzzleFX.Stop();

            //if no weapon
            if (weaponObject == null)
                //enter unarmed state
                _anim.SetTrigger("Unarmed");

            //Add Audio Target and set it's range
            InitializeAudioTarget();
        }

        public void ShootTarget(ManoeuvreFPSController _controller)
        {
            //Emit Muzzle
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();
            else
                Debug.Log("Please assign a Muzzle Flash FX");

            _stateManager._audioManager.PlayAudioClip(FireSound);

            //we make a ray from muzzle location to player transform
            RaycastHit hit;

            Vector3 direction = _controller.transform.position - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction , out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if(hit.transform.tag == _controller.transform.tag)
                {
                    //apply damage if we hit 
                    if(!_stateManager.AimIK.DebugAimIK)
                        _controller.Health.OnDamage(Random.Range(minDamage, maxDamage));
                
                    //spawn hit fx
                    HitParticle[0].onHit(hit.transform, hit.point, hit.normal);
                }


            }

        }

        public void ShootTarget(ShooterAIStateManager _shooterAI)
        {
            //Emit Muzzle
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();
            else
                Debug.Log("Please assign a Muzzle Flash FX");

            //play Fire Sound
            _stateManager._audioManager.PlayAudioClip(FireSound);

            //we make a ray from muzzle location to player transform
            RaycastHit hit;

            Vector3 direction = (_shooterAI.transform.position + Vector3.up) - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _shooterAI.transform.tag)
                {
                    //apply damage if we hit 
                    if(!_stateManager.AimIK.DebugAimIK)
                        _shooterAI.Health.onDamage(Random.Range(minDamage, maxDamage));

                    //set it's target
                    _shooterAI.ChaseBehaviour.targetPosition = _stateManager.transform;
                    _shooterAI.AttackBehaviour.ShooterAI = _stateManager;

                    //spawn hit fx
                    HitParticle[2].onHit(hit.transform, hit.point, hit.normal);
                }

            }

        }

		public void ShootTarget(Turret _turret)
		{
			//Emit Muzzle
			if (_cacheMuzzleFX)
				_cacheMuzzleFX.Play();
			else
				Debug.Log("Please assign a Muzzle Flash FX");

			//play Fire Sound
			_stateManager._audioManager.PlayAudioClip(FireSound);

			//we make a ray from muzzle location to player transform
			RaycastHit hit;

			Vector3 direction = (_turret.transform.position + Vector3.up) - muzzleLocation.position;

			if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
			{
				if (hit.transform.tag == _turret.transform.tag)
				{
					//apply damage if we hit 
					if(!_stateManager.AimIK.DebugAimIK)
						_turret._turretHealth.onDamage(Random.Range(minDamage, maxDamage), _stateManager.transform);

					//spawn hit fx
					HitParticle[3].onHit(hit.transform, hit.point, hit.normal);
				}

			}

		}

        public void ShootTarget(uzAI.uzAIZombieStateManager _uzAI)
        {
            //Emit Muzzle
            if (_cacheMuzzleFX)
                _cacheMuzzleFX.Play();
            else
                Debug.Log("Please assign a Muzzle Flash FX");

            //play Fire Sound
            _stateManager._audioManager.PlayAudioClip(FireSound);

            //we make a ray from muzzle location to zombie transform
            RaycastHit hit;

            Vector3 direction = (_uzAI.transform.position + Vector3.up) - muzzleLocation.position;

            if (Physics.Raycast(muzzleLocation.position, direction, out hit,100, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _uzAI.transform.tag)
                {
                    //apply damage if we hit 
                    if(!_stateManager.AimIK.DebugAimIK)
                        _uzAI.ZombieHealthStats.onDamage(Random.Range(minDamage, maxDamage));

                    //spawn hit fx
                    HitParticle[01].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void RefillAmmo()
        {
            Ammo = cacheAmmo;
        }

        /// <summary>
        /// Call this method to Invoke This Audio Target
        /// </summary>
        void InitializeAudioTarget()
        {
            //instantiate new object for trigger
            myAwarenessTrigger = new GameObject();
            myAwarenessTrigger.transform.SetParent(_stateManager.transform);
            myAwarenessTrigger.transform.localPosition = Vector3.zero;
            myAwarenessTrigger.SetActive(false);
            //add trigger and set radius
            myAwarenessTrigger.AddComponent<SphereCollider>().radius = 0;
            myAwarenessTrigger.GetComponent<SphereCollider>().isTrigger = true;

            //set layer, tag and name
            myAwarenessTrigger.layer = LayerMask.NameToLayer("AwarenessTrigger");
            myAwarenessTrigger.tag = "AwarenessTrigger";
            myAwarenessTrigger.name = "AwarenessTrigger";

        }

        public void SetAwarenessTriggerVisibility(ShooterAIStates _state)
        {
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

        //Main State Manager
        ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        /// <summary>
        /// Gets and sets the references of AI StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

        }

        public void FollowPlayer()
        {
            //if chasing or attacking or die
            if (_stateManager.AttackBehaviour.Zombie || _stateManager.currentShooterState == ShooterAIStates.Die)
                return; //exit

            //We set Destination
            _shooterAIAgent.SetDestination(_stateManager.Player.transform.position);

            //override Patrol speed with our Companion Follow Speed
            _stateManager.PatrolBehaviour.PatrolAnimation = FollowAnimation;

            //set stopping Distance
            _shooterAIAgent.stoppingDistance = PlayerDistance;

            if (_shooterAIAgent.remainingDistance > _shooterAIAgent.stoppingDistance)
            {
                //use existing Patrol state to Patrol to the current destination
                //but with our speed
                _stateManager.currentShooterState = ShooterAIStates.Patrol;
                //_stateManager.PatrolBehaviour.PatrolTowardsPlayer();
            }
            else
            {
                //make him look at Player
                _shooterAIAgent.transform.LookAt(_stateManager.Player.transform.position);

                //stop the agent
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

        [Tooltip("if true, AI Mesh will fade after death.")]
        public bool FadeMesh = false;

        [Tooltip("A simple Standard Shader material which will replace the current" +
            "AI's Mesh Material on Death.")]
        public Material faderMaterial;

        [Tooltip("All child meshes of AI.")]
        public List<Renderer> allRenderers = new List<Renderer>();

        [Tooltip("The fade will start after this much delay.")]
        [Range(0f, 5f)]
        public float fadeDelay = 0.5f;

        [Tooltip("How fast you want to fade the body after death?")]
        [Range(0f, 5f)]
        public float fadeDuration = 0.5f;

        //[HideInInspector]
        public bool DisableMotion;
        //[HideInInspector]
        public bool cooldown;

        //float cacheSpeed = 0;
        public float _timer;

        [HideInInspector]
        public Healthbar healthBar;

        //Main State Manager
        ShooterAIStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _shooterAIAgent;

        /// <summary>
        /// Gets and sets the references of AI  StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

        }

        public void onDamage(int amt)
        {
            if(_stateManager._AIType == AIType.Enemy)
                Health -= amt;
            else if(_stateManager._AIType == AIType.Companion && _stateManager.CompanionBehaviour.AllowDamageFromPlayer)
                Health -= amt;

            if (healthBar)
                healthBar.StartLerp();

            if (Health <= 0)
                _stateManager.Die();
            else
                HitReaction();

        }

        int i = 1;
        void HitReaction()
        {
            //Play Hurt SFX
            if (HitSound.Count > 0 && !_stateManager._audioManager._source.isPlaying)
                _stateManager._audioManager.PlayAudioClip(HitSound[Random.Range(0, HitSound.Count)]);

            //set trigger
            _animator.SetTrigger("HitReaction");
            //set reaction ID
            _animator.SetInteger("HitID", i);

            if (lookAtCameraOnHit)
            {
                //look at camera
                if (_stateManager.AttackBehaviour.Player)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Player.transform.position);
                else if (_stateManager.AttackBehaviour.Zombie)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Zombie.transform.position);
                else if(_stateManager.AttackBehaviour.ShooterAI)
                    _stateManager.transform.LookAt(_stateManager.AttackBehaviour.ShooterAI.transform.position);

            }

            cooldown = false;

            //increment hitID
            if (i < hitReactionsAvailable)
                i++;
            else
                i = 1;
        }

        public void AIGotHit()
        {
            if (!_shooterAIAgent)
                return;

            if (_stateManager.currentShooterState == ShooterAIStates.Die)
                return;


            DisableMotion = _animator.GetBool("DisableMotion");

            if (DisableMotion)
            {
                _shooterAIAgent.isStopped = true;

                //lerp speed to 0
                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * 5f);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);
                _timer = 0;
                cooldown = true;

            }

            //delay
            while (_timer < _cooldownTimer && cooldown)
            {
                _timer += Time.deltaTime;
                return;
            }

            //force reset timer and cooldown 
            _timer = 0;
            cooldown = false;
            _stateManager.setHandsIK = true;
        }

        /// <summary>
        /// Simply lerps the alpha of the Mesh Color from 1-0
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        public IEnumerator FadeOnDeath()
        {
            //delay
            yield return new WaitForSeconds(fadeDelay);

            //stop animator
            _animator.enabled = false;

            //now start fade
            float et = 0;
            Color c = Color.white;

            while (et < fadeDuration)
            {
                c.a = Mathf.Lerp(c.a, 0, et / fadeDuration);
                foreach (Renderer r in allRenderers)
                {
                    foreach (Material m in r.materials)
                    {
                        //assign it to the renderer
                        m.color = c;
                    }
                }

                et += Time.deltaTime;
                yield return null;
            }

            //5. Destroy Self
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

            //Spine Transform
            SpineTransform = new GameObject().transform;
            SpineTransform.name = "Spine Bone IK";
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

            //add audio Source
            _source = _mgr.gameObject.AddComponent<AudioSource>();
        }

        public void PlayAudioClip(AudioClip clip)
        {
            _source.PlayOneShot(clip);
        }

    }

    [System.Serializable]
    public class DrawGizmos
    {
        [Tooltip("If true, Draws the Nav mesh Path from Zombie to Current Target")]
        public bool drawPathToCurrentTarget = true;
        [Tooltip("Set the color of Nav mesh Path.")]
        public Color pathGizmoColor = Color.cyan;
        [Tooltip("If true, Draws the Straight Line from Zombie to Current Target")]
        public bool drawLineToCurrentTarget = true;
        [Tooltip("Set the color of Line.")]
        public Color lineGizmoColor = Color.green;

        /// <summary>
        /// Draws the AI Gizmos
        /// </summary>
        public void DrawAIGizmos(NavMeshAgent _shooterAIAgent)
        {
            if (drawPathToCurrentTarget && _shooterAIAgent)
            {
                for (int i = 0; i < _shooterAIAgent.path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(_shooterAIAgent.path.corners[i], _shooterAIAgent.path.corners[i + 1], pathGizmoColor);

                }
            }

            if (drawLineToCurrentTarget && _shooterAIAgent)
            {
                Debug.DrawLine(_shooterAIAgent.transform.position, _shooterAIAgent.path.corners[_shooterAIAgent.path.corners.Length - 1], lineGizmoColor);
            }

        }

    }
}