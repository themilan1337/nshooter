using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace uzAI
{
    //Main ENUM where all the Zombie States are defined
    public enum ZombieStates
    {
        Idle,
        Patrolling,
        Chasing,
        Hit,
        Attacking,
        Eating,
        Die,
    }

    public class uzAIZombieStateManager : MonoBehaviour
    {
        [Header("Current State of the Zombie")]
        [Tooltip("This wil be changing at runtime depending upon the scenarios this agent will face!")]
        public ZombieStates currentZombieState = ZombieStates.Idle;

        [Space(15)]

        [Header("All the Zombie Behaviours")]

        [Tooltip("Locomotion Properties of this agent.")]
        public ZombieLocomotionBehaviour Locomotion;

        [Tooltip("Define the Idle behaviour specific to this Agent.")]
        public ZombieIdleBehaviour idleBehaviour;

        [Tooltip("Define the Patrol behaviour specific to this Agent.")]
        public ZombiePatrolBehaviour patrolBehaviour;

        [Tooltip("Define the Enemy Sight behaviour specific to this Agent.")]
        public ZombieSightBehaviour sightBehaviour;

        [Tooltip("Define the Target Chasing behaviour specific to this Agent.")]
        public ZombieChaseTargetBehaviour chaseBehaviour;

        [Tooltip("Define the Target Attack behaviour specific to this Agent.")]
        public ZombieAttackBehaviour attackBehaviour;

        [Tooltip("Define the Target Attack behaviour specific to this Agent.")]
        public ZombieEatingBehaviour eatingBehaviour;

        public ZombieOffMeshBehaviour offmeshBehaviour;

        [Header("Draw Zombie Path Gizmos")]
        [Tooltip("Draws the gizmos for this Agent's Current Path.")]
        public DrawGizmos drawGizmos;

        [Space(15)]

        [Header("Zombie Stats")]
        public uzAIZombieHealth ZombieHealthStats;

        [Space(15)]

        [Header("Zombie SFX")]
        public uzAIZombieSFX ZombieSFX;

        Animator _animator;

        [HideInInspector]
        public float _globalDelayTimer = 0;

        List<Rigidbody> AllChildRBodies = new List<Rigidbody>();
        List<Collider> AllColliders = new List<Collider>();

        //Editor Variables
        [HideInInspector]
        public bool locomotionToggle;
        [HideInInspector]
        public bool idleToggle;
        [HideInInspector]
        public bool patrolToggle;
        [HideInInspector]
        public bool sightToggle;
        [HideInInspector]
        public bool chaseToggle;
        [HideInInspector]
        public bool attackToggle;
        [HideInInspector]
        public bool healthToggle;
        [HideInInspector]
        public bool zSFXToggle;
        [HideInInspector]
        public bool gizmoToggle;
        [HideInInspector]
        public bool eatingToggle;
        [HideInInspector]
        public bool offmeshToggle;

        // Use this for initialization
        void Awake()
        {
            //set Tag as uzAIZombie
            tag = "uzAIZombie";

            //get references
            Locomotion.uzAIAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            //Randomize Locomotion moveset
            _animator.SetBool("LocomotionMirror", Locomotion.mirrorLocomotion);

            //initialize Behaviours
            idleBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);
            patrolBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);
            chaseBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);
            attackBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);
            eatingBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);
            offmeshBehaviour.Initialize(this, Locomotion.uzAIAgent, _animator);

            //we also start the coroutine which will be checking all the colliders in the 
            //specified range
            StartCoroutine(sightBehaviour.SearchTargetsCoroutine());
            //Now create the trigger
            sightBehaviour._stateManager = this;
            sightBehaviour.CreateSensorTrigger();
            //now we start the Coroutine to search nearby attack targets
            StartCoroutine(attackBehaviour.SearchAttackTargetsCoroutine());

            //initialize Stats
            ZombieHealthStats.Initialize(this, Locomotion.uzAIAgent, _animator);

            //initialize SFX transform
            ZombieSFX.Initialize(this.transform);

            //Start Hunger Depletion Coroutine
            StartCoroutine(eatingBehaviour.DepleteHunger());

            //See if there's a Spawn Helper
            if (GetComponent<SpawnHelper>())
            {
                //set target
                chaseBehaviour.targetPosition = GetComponent<SpawnHelper>().Target;

                //create minimap icon manually
                if(Manoeuvre.gc_Minimap.Instance)
                    Manoeuvre.gc_Minimap.Instance.AttachMinimapIconManually(transform);
            }

            //we get all the rigidbodies from children components
            foreach(Rigidbody r in GetComponentsInChildren<Rigidbody>())
            {
                if(r.transform != this.transform)
                {
                    AllChildRBodies.Add(r);
                    r.isKinematic = true;

                    if (r.GetComponent<Collider>())
                        r.GetComponent<Collider>().enabled = false;

                    
                }
            }

            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                AllColliders.Add(c);
            }
        }

        /// <summary>
        /// Overriding Animator Behaviour here.
        /// </summary>
        private void OnAnimatorMove()
        {
            //if we are NOT Patrolling or NOT in Chasing
            if (currentZombieState != ZombieStates.Patrolling && currentZombieState != ZombieStates.Chasing)
                return; // exit

            if (!Locomotion.uzAIAgent)
                return; // exit

            if (_animator.GetBool("ZombieIsTurning"))
            {
                Locomotion.uzAIAgent.speed = 0.1f;
                return;
            }

            //else
            //we make sure root motion is enabled
            _animator.applyRootMotion = true;

            //and set the speed of the Nav Agent according to the root motion of the Animator
            Locomotion.uzAIAgent.speed = (_animator.deltaPosition / Time.deltaTime).magnitude;
        }

        /// <summary>
        /// Overriding Animator IK here.
        /// </summary>
        /// <param name="layerIndex"></param>
        private void OnAnimatorIK(int layerIndex)
        {
            //if we are NOT Attacking or Chasing
            if (currentZombieState != ZombieStates.Attacking && currentZombieState != ZombieStates.Chasing)
                return; // exit

            //else
            //we set our Agent's head to look at the Target
            if(chaseBehaviour.targetPosition != null && chaseBehaviour.useHeadTrack)
            {
                _animator.SetLookAtPosition(chaseBehaviour.targetPosition.position + Vector3.up);
                _animator.SetLookAtWeight(1);
            }

        }

        /// <summary> 
        /// Calling the Iterations every Fixed Update
        /// </summary>
        void FixedUpdate()
        {
            //make sure, there's no calculation once the agent dies
            if (currentZombieState == ZombieStates.Die)
                return;

            //Check is the zombie currently being hit?
            ZombieHealthStats.ZombieGotHit();

            if (ZombieHealthStats.DisableMotion)
                return;

            if (ZombieHealthStats.cooldown)
                return;

            //1 --> We Identify the enemy state
            IdentifyStates();

            //2 --> We set the identified state
            SetState();
        }

        /// <summary>
        /// Here we Identify in which state are we currently In.
        /// </summary>
        void IdentifyStates()
        {

            if (Locomotion.uzAIAgent == null)
                return;

            if (attackBehaviour.isAttackingBarricades)
                return;

            //if we are now in Attack State
            if (currentZombieState == ZombieStates.Attacking)
                return; // exit

            //else, we make sure if we are eating
            if (currentZombieState == ZombieStates.Eating)
                return;

            if (Locomotion.uzAIAgent.isOnOffMeshLink)
            {
                TraverseOffMesh();
                return; //exit
            }

            //if we re already traversing, exit
            if (offmeshBehaviour.isTraversing && currentZombieState != ZombieStates.Patrolling)
                return;

            //else, we make sure if we are Chasing 
            if (currentZombieState == ZombieStates.Chasing)
                return; // exit

            //See if there's a Spawn Helper
            if (GetComponent<SpawnHelper>() && attackBehaviour.Barricade == null)
            {
                chaseBehaviour.targetPosition = GetComponent<SpawnHelper>().Target;
                currentZombieState = ZombieStates.Chasing;
                return;
            }

            //else we see if Timer is less then Patrol Delay
            if (_globalDelayTimer < patrolBehaviour.PatrolDelay)
                currentZombieState = ZombieStates.Idle; // go to Idle
            else if (_globalDelayTimer > patrolBehaviour.PatrolDelay)
                currentZombieState = ZombieStates.Patrolling; // else Patrol

        }

        /// <summary>
        /// Set the Correct Behaviour based on the Current Zombie State
        /// </summary>
        void SetState()
        {
            //Simple Switch Case usage to call the Main Method of every state 
            //depending on Current Zombie State
            switch (currentZombieState)
            {
                //IDLE
                case ZombieStates.Idle:
                    idleBehaviour.Idle();
                    break;

                //PATROLLING
                case ZombieStates.Patrolling:
                    patrolBehaviour.Patrol();
                    break;

                //CHASING
                case ZombieStates.Chasing:
                    chaseBehaviour.Chase();
                    break;
                
                //ATTACKING
                case ZombieStates.Attacking:
                    attackBehaviour.Attack();
                    break;
                //EATING
                case ZombieStates.Eating:
                    eatingBehaviour.Eat();
                    break;
            }

            //draw debugs
            drawGizmos.DrawAIGizmos(Locomotion.uzAIAgent);
        }

        /// <summary>
        /// Call On Destination Reached when we enter the waypoint trigger
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (attackBehaviour.isAttackingBarricades)
                return;

            if (GetComponent<SpawnHelper>())
                return;

            //if we enter the Awareness Trigger
            if (other.gameObject.tag == "AwarenessTrigger")
            {
                //if we are already chasing this Tag
                if (chaseBehaviour.currentChasingTargetTag == "Player" ||
                    chaseBehaviour.currentChasingTargetTag == other.gameObject.GetComponentInParent<Transform>().transform.tag)
                    return;

                //if we hear the sound of Player
                if (other.gameObject.GetComponentInParent<Transform>().tag == "Player")
                {

                    chaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);
                    sightBehaviour.visibleTargets.Add(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);

                }
                //else if we hear other audio
                else
                {
                    chaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Transform>().transform);
                }
            }

            //if we entered the collider tagged Destination
            //we call the on destination reached flag
            //Also, wo don't have any food source to be eaten
            if (other.gameObject.tag == "Destination"  && eatingBehaviour.currentFoodSource == null)
            {
               OnDestinationReached();
            }

            //if we enter food tag
            if (other.transform == eatingBehaviour.currentFoodSource)
            {
                //We set state
                currentZombieState = ZombieStates.Eating;
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
        
        public void TraverseOffMesh()
        {
            //We must check if we haven't already been traversing
            if(!offmeshBehaviour.hasTraversed)
                StartCoroutine(offmeshBehaviour.TraverseOffmesh());
            
        }

        /// <summary>
        /// Gets the transform of the player and set that as the last known position
        /// </summary>
        /// <param name="p"></param>
        public void SetLastPlayerPositionVector(Transform p)
        {
            if (chaseBehaviour.lastPlayerPosition == null)
                return;

            if (p.tag == "Player" || p.tag == "AwarenessTrigger")
            {
                //Debug.Log("tag we were chasing : " + p.tag);
                //Debug.Log("currentChasingTargetTag : " + chaseBehaviour.currentChasingTargetTag);

                //if the last thing we were chasing was player
                //if (chaseBehaviour.currentChasingTargetTag == p.tag)
                if(chaseBehaviour.currentChasingTargetTag == "Player" || chaseBehaviour.currentChasingTargetTag == "AwarenessTrigger")
                {
                    //reset the current chasing target
                    chaseBehaviour.currentChasingTargetTag = "";

                    //finding random nav mesh point
                    //Vector3 randomLastPlayerPosition = enemySightBehaviour.RandomNavmeshPoint(p.position, enemySightBehaviour.hearingDeviation, -1);

                    chaseBehaviour.lastPlayerPosition.position = p.position;
                    chaseBehaviour.targetPosition = chaseBehaviour.lastPlayerPosition;

                    //enable lpp
                    chaseBehaviour.lastPlayerPosition.gameObject.SetActive(true);
                    
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
            Locomotion.uzAIAgent.isStopped = true;
            _globalDelayTimer = 0;

            //disable lpp
            chaseBehaviour.lastPlayerPosition.gameObject.SetActive(false);

            //reset path
            patrolBehaviour.resetPath = true;

            //set idle state
            currentZombieState = ZombieStates.Idle;
        }

        /// <summary>
        /// Kills the Zombie.
        /// </summary>
        public void Die()
        {
            //set state
            currentZombieState = ZombieStates.Die;

            //Play dialogue
            if(Manoeuvre.gc_PlayerDialoguesManager.Instance)
                Manoeuvre.gc_PlayerDialoguesManager.Instance.PlayDialogueClip(Manoeuvre.gc_PlayerDialoguesManager.DialogueType.Kills);

            //check if there's items to drop
            if (GetComponent<Manoeuvre.DropItems>())
                GetComponent<Manoeuvre.DropItems>().Drop(ZombieHealthStats.fadeDelay);

            //--->> Destroy all the components <<---//

            //1. Randomize Mirror Death
            _animator.SetBool("DeathMirror", Random.value < 0.5f);

            _animator.SetTrigger("Death");

            _animator.SetInteger("DeathID", ZombieHealthStats.DeathID);

            //2. Destroy nav mesh
            if(!offmeshBehaviour.isTraversing)
                Destroy(Locomotion.uzAIAgent);

            //3. Disable Colliders
            foreach(Collider c in AllColliders)
            {
                c.enabled = false;
            }

            //4. Destroy Rigidbody
            if (!GetComponent<ConfigurableJoint>())
                Destroy(GetComponent<Rigidbody>());

            //5. Destroy Respective Last Know position
            Destroy(chaseBehaviour.lastPlayerPosition.gameObject);

            //6. See if there's a spawn Helper
            if (GetComponent<SpawnHelper>())
                GetComponent<SpawnHelper>().DetachFromWave(); //make sure to detach it!

            if (ZombieHealthStats.fadeZombieMesh)
            {
                //set all the materials to Fade
                foreach (Renderer r in ZombieHealthStats.allRenderers)
                { 
                    //assign it to the renderer
                    r.material.shader = ZombieHealthStats.faderMaterial.shader;
                }

                //Finally, the fade coroutine,
                //which will fade the zombie alpha
                //then destroy game object
                StartCoroutine(ZombieHealthStats.FadeOnDeath());
            }
            //else
            //    Destroy(this);

        }

        public void PerformRagdollDeath(float force, Vector3 pos, float radius)
        {
            bool cacheFade = ZombieHealthStats.fadeZombieMesh;

            ZombieHealthStats.fadeZombieMesh = false;

            _animator.enabled = false;

            Die();

            foreach (Rigidbody r in AllChildRBodies)
            {
                r.isKinematic = false;
                r.AddExplosionForce(force, pos, radius);

                if (r.GetComponent<Collider>())
                    r.GetComponent<Collider>().enabled = true;

                r.transform.tag = "uzAIZombie";
            }

            ZombieHealthStats.fadeZombieMesh = cacheFade;

            if (ZombieHealthStats.fadeZombieMesh)
            {

                //--- now fade mesh ---//
                //set all the materials to Fade
                foreach (Renderer r in ZombieHealthStats.allRenderers)
                {
                    //assign it to the renderer
                    r.material.shader = ZombieHealthStats.faderMaterial.shader;
                }

                //Finally, the fade coroutine,
                //which will fade the zombie alpha
                //then destroy game object
                StartCoroutine(ZombieHealthStats.FadeOnDeath());

                return;
            }
            else 
            {
                //Debug.Log("dont fade");

                Destroy(this);
            }

        }
    }

    #region Zombie Behaviours

    /// <summary>
    /// The Zombie Movement Properties
    /// </summary>
    [System.Serializable]
    public class ZombieLocomotionBehaviour
    {
        [HideInInspector]
        //Main Nav Mesh Agent
        public NavMeshAgent uzAIAgent;

        [Tooltip("0 - Idle || 1 - Coming Towards Target OR Patrolling || 2 - Close To Target")]
        [Range(0,2)]
        public float walkAnimation = 0;

        [Tooltip("If true, it will mirror the locomotion animations. Great to create random movements with sigle animations.")]
        public bool mirrorLocomotion;

        public bool useTurnOnSpot = true;

        [Range(0, 180)]
        public float PatrolAngleThreshold = 65;

        [Range(0, 180)]
        public float ChaseAngleThreshold = 150;
    }

    /// <summary>
    /// The Zombie Idle Behaviour
    /// </summary>
    [System.Serializable]
    public class ZombieIdleBehaviour
    {
        [Range(0.1f, 10f)]
        [Tooltip("How fast you want to transition to Current State")]
        public float idleTransitionDuration = 5f;

        [Range(0f, 1f)]
        public float idleAnimation = 0f;

        //Main State Manager
        uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr,NavMeshAgent nav,  Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;

            //set Idle Transitions
            _animator.SetFloat("idleAnimation", idleAnimation);
        }

        /// <summary>
        /// This will set the nav mesh agent speed to Zero
        /// and make the animator go to Idle Animation
        /// </summary>
        public void Idle()
        {
            //making sure the agent is STOPPED!
            _uzAIAgent.isStopped = true;

            //set speed to 0
            _stateManager.Locomotion.walkAnimation = Mathf.Lerp(_stateManager.Locomotion.walkAnimation, 0, Time.deltaTime * idleTransitionDuration);

            //set animation to Idle
            _animator.SetFloat("Vertical", _stateManager.Locomotion.walkAnimation);

            //disable root motion
            _animator.applyRootMotion = false;

            //clear chase target
            _stateManager.chaseBehaviour.currentChasingTargetTag = "";
            _stateManager.chaseBehaviour.targetPosition = null;

            //if our timer is less then the Patrol Delay
            if (_stateManager._globalDelayTimer < _stateManager.patrolBehaviour.PatrolDelay)
                _stateManager._globalDelayTimer += Time.deltaTime; // increment
            else
                return; // exit

            //Debug.Log("Inside Idle State");
            
        }

    }

    /// <summary>
    /// The Zombie patrol Behaviour
    /// </summary>
    [System.Serializable]
    public class ZombiePatrolBehaviour
    {
        [Tooltip("Assign the Patrolling Path of this Agent")]
        public WaypointsPath PatrolPath;

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
        uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;
        }

        /// <summary>
        /// Patrol Behaviour
        /// </summary>
        public void Patrol()
        {
            //if we are not in patrolling state
            if (_stateManager.currentZombieState != ZombieStates.Patrolling)
                return; // exit

            //if there's no patrol path
            if (PatrolPath == null)
                return;

            //if there's no patrol point
            if (PatrolPath.waypoints.Count < 1)
                return; // exit

            //if we are on an off mesh
            //if (_stateManager.offmeshBehaviour.isTraversing)
            //    return;

            //else we patrol
            //.....................

            //Debug.Log("Patrolling");



            //if we have a path
            if (_uzAIAgent.hasPath)
            {
                _uzAIAgent.isStopped = false;

                //we only calculate Angle
                //only if we are not Turning
                if (!_animator.GetBool("ZombieIsTurning") && !_stateManager.offmeshBehaviour.isTraversing) {

                    float Angle = GetAngle(_stateManager.transform.forward, _uzAIAgent.desiredVelocity, _stateManager.transform.up);

                    if ((Angle > _stateManager.Locomotion.PatrolAngleThreshold || Angle < -_stateManager.Locomotion.PatrolAngleThreshold) && _stateManager.Locomotion.useTurnOnSpot)
                    {
                        int val = Mathf.Sign(Angle) > 0 ? 1 : -1;
                        _animator.SetInteger("ZombieTurn", val);

                        // Debug.Log(Angle);

                    }
                    else
                    {
                    _stateManager.Locomotion.walkAnimation = Mathf.Lerp(_stateManager.Locomotion.walkAnimation, PatrolAnimation, Time.deltaTime * 5);
                    _animator.SetFloat("Vertical", _stateManager.Locomotion.walkAnimation);
                    }

                }
                

            }

            //if the remaining distance is less then the stopping distance
            //i.e we have reched the Destination
            if (_uzAIAgent.remainingDistance <= _uzAIAgent.stoppingDistance || _uzAIAgent.pathStatus != NavMeshPathStatus.PathComplete
                /*|| !_uzAIAgent.hasPath*/ || _uzAIAgent.isPathStale || resetPath)
            {
                //if we don't have a path or the velocity is 0
                if (!_uzAIAgent.hasPath || _uzAIAgent.velocity.sqrMagnitude == 0 || resetPath)
                {
                    if (_stateManager._globalDelayTimer >= PatrolDelay || resetPath)
                    {
                        //we set the current waypoint as the next waypoint
                        currentWaypoint = nextWaypoint;
                        _uzAIAgent.SetDestination(PatrolPath.waypoints[currentWaypoint].position);

                        //increment next waypoint accordingly
                        if (nextWaypoint < PatrolPath.waypoints.Count - 1)
                            nextWaypoint++;
                        else
                            nextWaypoint = 0;

                        resetPath = false;

                       // Debug.Log("Patrol Point : " + currentWaypoint);

                    }
                    //else we wait for the timer to reach the delay
                    else
                    {
                        //now increment 'this frame'
                        _stateManager._globalDelayTimer += Time.deltaTime;

                    }

                }
               
            }


        }

        float GetAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
        {
            if (toVector == Vector3.zero)
                return 0;

            float Angle = Vector3.Angle(fromVector, toVector);
            Vector3 normal = Vector3.Cross(fromVector, toVector);
            Angle *= Mathf.Sign(Vector3.Dot(normal, upVector));

            return Angle;
        }

    }

    [System.Serializable]
    public class ZombieChaseTargetBehaviour
    {
        [Tooltip("What is the current Target 'Tag' we are chasing?")]
        public string currentChasingTargetTag = "";
        [Tooltip("Target's position where we are heading towards.")]
        public Transform targetPosition;
        [Tooltip("Chase speed used to Blend in the Animation States.")]
        [Range(1f,2f)]
        public float chaseAnimation = 2f;
        [Tooltip("If true, Zombie will look towards the target he's Chasing.")]
        public bool useHeadTrack = true;

        public Transform lastPlayerPosition;

        //Main State Manager
        uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;

            lastPlayerPosition = new GameObject().transform;
            lastPlayerPosition.name = "Last Known Position";
            lastPlayerPosition.gameObject.layer = 11;
            lastPlayerPosition.gameObject.AddComponent<SphereCollider>().isTrigger = true;
            lastPlayerPosition.gameObject.tag = "Destination";

            //disable lpp
            lastPlayerPosition.gameObject.SetActive(false);
        }

        /// <summary>
        /// Based on the priority values,
        /// we check whether we can chase this target or not!
        /// </summary>
        /// <param name="t"></param>
        public void CanChaseThisTarget(Transform t)
        {
            //if t is the same target we are chasing
            if (t.tag == currentChasingTargetTag)
                return;

            //See if we are not chasing anything as of now...
            if (string.IsNullOrEmpty(currentChasingTargetTag))
            {
                //we make exception with food target 
                if (t.tag == _stateManager.eatingBehaviour.FoodTag)
                {
                    //make sure food source is available
                    //And Zombie is Hungry
                    if (t.GetComponent<uzAIZombieFood>()._availability == uzAIZombieFood.FoodAvailability.Available && 
                        _stateManager.eatingBehaviour.currentHunger < _stateManager.eatingBehaviour.Hunger)
                    {
                        Debug.Log("Started chasing FOOD");

                        // We get the transform
                        targetPosition = t;

                        // Change State
                        _stateManager.currentZombieState = ZombieStates.Chasing;
                        // Set Tag
                        currentChasingTargetTag = t.tag;

                        //set this as our current food source
                        _stateManager.eatingBehaviour.currentFoodSource = t;

                        //set food as un available
                        t.GetComponent<uzAIZombieFood>().ToggleFoodAvailability(uzAIZombieFood.FoodAvailability.UnAvailable);

                    }
                }
                else if (t.gameObject.layer != LayerMask.NameToLayer("Food"))
                {

                    // We get the transform
                    targetPosition = t;

                    // Change State
                    _stateManager.currentZombieState = ZombieStates.Chasing;
                    // Set Tag
                    currentChasingTargetTag = t.tag;

                    //if we are eating, we exit it
                    if (_stateManager.eatingBehaviour.isEating)
                        _stateManager.eatingBehaviour.ExitEatBehaviour();

                }
                
            }
            //Else we see if the current target we are chasing has lower priority then the one we just encountered
            else 
            {
                int currentTargetPriority = 0;
                int nextTargetPriority = 0;

                foreach(TargetsToDetect target in _stateManager.sightBehaviour.targetsToDetect)
                {
                    //we get the current target's priority
                    if (currentChasingTargetTag == target.targetTag)
                        currentTargetPriority = target.targetDetectionPriority;

                    //we get the next target priority
                    if (t.tag == target.targetTag)
                            nextTargetPriority = target.targetDetectionPriority;
                }

                //now we compare
                if(currentTargetPriority > nextTargetPriority)
                {
                    //if the target we are chasing currently, is of higher priority
                    //we simply return
                    Debug.Log("Can't chase : " + t.tag + " because of low priority then : " + currentChasingTargetTag);
                    return;
                }
                else
                {
                    //we make exception with food target 
                    if(t.tag == _stateManager.eatingBehaviour.FoodTag)
                    {
                        //make sure food source is available
                        //And Zombie is Hungry
                        if (t.GetComponent<uzAIZombieFood>()._availability == uzAIZombieFood.FoodAvailability.Available &&
                        _stateManager.eatingBehaviour.currentHunger < _stateManager.eatingBehaviour.Hunger)
                        {

                            Debug.Log("Started chasing FOOD");
                            
                            //else, we start chasing the new target we just encountered
                            targetPosition = t;

                            // Change State
                            _stateManager.currentZombieState = ZombieStates.Chasing;

                            // Set Tag
                            currentChasingTargetTag = t.tag;

                            //set this as current zombie feeding this food
                            t.GetComponent<uzAIZombieFood>().ToggleFoodAvailability(uzAIZombieFood.FoodAvailability.UnAvailable);

                            //set this as our current food source
                            _stateManager.eatingBehaviour.currentFoodSource = t;

                        }
                        else
                        {
                            Debug.Log("Food is Un Available");

                        }
                    }
                    else if(t.gameObject.layer != LayerMask.NameToLayer("Food"))
                    {
                        Debug.Log("Started chasing : " + t.tag + " instead of : " + currentChasingTargetTag);

                        //else, we start chasing the new target we just encountered
                        targetPosition = t;

                        // Change State
                        _stateManager.currentZombieState = ZombieStates.Chasing;
                        // Set Tag
                        currentChasingTargetTag = t.tag;

                        //if we are eating, we exit it
                        if (_stateManager.eatingBehaviour.isEating)
                            _stateManager.eatingBehaviour.ExitEatBehaviour();
                    }
                    
                }
            }

        }

        /// <summary>
        /// Simply chases the Target Position
        /// </summary>
        public void Chase()
        {
            if (!_uzAIAgent)
                return;

            ////if we are on an off mesh
            //if (_stateManager.offmeshBehaviour.isTraversing)
            //    return;

            //if we got hit
            if (_stateManager.currentZombieState == ZombieStates.Hit)
                return;

            //if we are not attacking
            if (_stateManager.currentZombieState == ZombieStates.Attacking)
                return;

            //if there's no target position
            if (!targetPosition)
                return;

            //if we are not already chasing the current target
            if (_uzAIAgent.destination != targetPosition.position)
            {
                //set it
                _uzAIAgent.SetDestination(targetPosition.position);
            }

            //if target position is Last known pos
            //and we still have food transform left 
            if(targetPosition == lastPlayerPosition && _stateManager.eatingBehaviour.currentFoodSource != null)
            {
                //make sure we have cleared the food thing
                _stateManager.eatingBehaviour.ExitEatBehaviour();
            }

            //make sure agent isn't stopped
            _uzAIAgent.isStopped = false;

            //we only calculate Angle
            //only if we are not Turning
            if (!_animator.GetBool("ZombieIsTurning") && !_stateManager.offmeshBehaviour.isTraversing)
            {
                float Angle = GetAngle(_stateManager.transform.forward, _uzAIAgent.desiredVelocity, _stateManager.transform.up);

                if ((Angle > _stateManager.Locomotion.ChaseAngleThreshold || Angle < -_stateManager.Locomotion.ChaseAngleThreshold) && _stateManager.Locomotion.useTurnOnSpot)
                {
                    int val = Mathf.Sign(Angle) > 0 ? 1 : -1;
                    _animator.SetInteger("ZombieTurn", val);

                   // Debug.Log(Angle);
                }
                else
                {
                    //lerp speed
                    _stateManager.Locomotion.walkAnimation = Mathf.Lerp(_stateManager.Locomotion.walkAnimation, chaseAnimation, Time.deltaTime * 5);

                    //set locomotion
                    _animator.SetFloat("Vertical", _stateManager.Locomotion.walkAnimation);
                }
            }
        }

        float GetAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
        {
            if (toVector == Vector3.zero)
                return 0;

            float Angle = Vector3.Angle(fromVector, toVector);
            Vector3 normal = Vector3.Cross(fromVector, toVector);
            Angle *= Mathf.Sign(Vector3.Dot(normal, upVector));

            return Angle;
        }
    }

    [System.Serializable]
    public class ZombieSightBehaviour
    {
        [Tooltip("Define all the tags you want to detect as a Threat")]
        public List<TargetsToDetect> targetsToDetect = new List<TargetsToDetect>();

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

        //[Tooltip("The more this Deviation is, the more farther the NPC go from the Player's 'actual' Last Know Position.")]
        //[Range(0.1f,10f)]
        //public float hearingDeviation = 1f;

        //[HideInInspector]
        //Currently visible targets
        public List<Transform> visibleTargets = new List<Transform>();

        [HideInInspector]
        public uzAIZombieStateManager _stateManager;


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
            t.gameObject.layer = 12;
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

                //if it's already added in the list
                //if (visibleTargets.Contains(target))
                  //  return; // exit

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
                        //start chasing
                        _stateManager.chaseBehaviour.CanChaseThisTarget(target.transform);
                    }
                    //if there is an obstacle in between
                    else {
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

        public Vector3 RandomNavmeshPoint(Vector3 fromPos, float randomRange, int layerMask)  
        {
            Vector3 randomDirection = Random.insideUnitSphere * randomRange;

            randomDirection += fromPos;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randomDirection, out navHit, randomRange, layerMask);

            return navHit.position;
        }

    }

    [System.Serializable]
    public class ZombieAttackBehaviour
    {
        [Header("Attack Properties")]
        public Manoeuvre.ManoeuvreFPSController Player;
        public Manoeuvre.DynamicBarricades Barricade;
        public Manoeuvre.ShooterAIStateManager ShooterAI;
        
        [Tooltip("Define from how far this agent can Attack.")]
        [Range(1.5f, 5f)]
        public float AttackDistance = 2f;

        [Tooltip("Attack Angle of this Agent, It is Recommended that you change it in Editor for better results.")]
        [Range(0f, 360f)]
        public float Angle;

        [Tooltip("How fast you want to check for Threats.")]
        public float SearchIterationTime = 0.25f;

        [Tooltip("What is the Target Layer Mask.")]
        public LayerMask targetMask = 9;
        [Tooltip("What is the Obstacles Layer Mask.")]
        public LayerMask obstacleMask = 10;


        [Tooltip("Wait between 2 consecutive attacks.")]
        [Range(0f, 5f)]
        public float AttackDelay = 1f;

        [Range(1, 15)]
        public int maxDamage = 10;
        [Range(1, 15)]
        public int minDamage = 5;

        //Main State Manager
        [HideInInspector]
        public uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        bool isAttacking;
        public bool hasAttacked;
        public bool canAttack;
        public bool isAttackingBarricades;
        float _timer;

        //[HideInInspector]
        //Currently visible targets
        public List<Transform> visibleTargets = new List<Transform>();

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;
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
                //we are only interested in 'Player' and 'Barricades'
                if (targetsInViewRadius[i].tag == "Player" || targetsInViewRadius[i].tag == "Barricades" || targetsInViewRadius[i].tag == "ShooterAI")
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
                            //we add it in our visible targets list
                            visibleTargets.Add(target);

                            //assign player
                            if (target.GetComponent<Manoeuvre.ManoeuvreFPSController>())
                            {
                                Player = target.GetComponent<Manoeuvre.ManoeuvreFPSController>();
                                //start Attacking
                                _stateManager.currentZombieState = ZombieStates.Attacking;
                                //clear chase target of chase behaviour
                                _stateManager.chaseBehaviour.currentChasingTargetTag = "";
                                _stateManager.chaseBehaviour.targetPosition = null;
                                
                            }else if (target.GetComponent<Manoeuvre.ShooterAIStateManager>())
                            {
                                if (target.GetComponent<Manoeuvre.ShooterAIStateManager>().Health.Health > 0)
                                {
                                    ShooterAI = target.GetComponent<Manoeuvre.ShooterAIStateManager>();
                                    //start Attacking
                                    _stateManager.currentZombieState = ZombieStates.Attacking;
                                    //clear chase target of chase behaviour
                                    _stateManager.chaseBehaviour.currentChasingTargetTag = "";
                                    _stateManager.chaseBehaviour.targetPosition = null;
                                }
                              
                            }
                            else
                            {
                                if (!target.GetComponent<Manoeuvre.DynamicBarricades>().allDequed)
                                {
                                    Barricade = target.GetComponent<Manoeuvre.DynamicBarricades>();
                                    //start Attacking
                                    _stateManager.currentZombieState = ZombieStates.Attacking;
                                    //clear chase target of chase behaviour
                                    _stateManager.chaseBehaviour.currentChasingTargetTag = "";
                                    _stateManager.chaseBehaviour.targetPosition = null;

                                    //we disable everything if attacking barricades
                                    isAttackingBarricades = true;

                                    //enable under attack
                                    Barricade.Zombie = _stateManager;
                                }
                                else

                                { //clear chase target of chase behaviour
                                    _stateManager.chaseBehaviour.currentChasingTargetTag = "";
                                    _stateManager.chaseBehaviour.targetPosition = null;

                                    _stateManager.currentZombieState = ZombieStates.Patrolling;

                                    if (Barricade)
                                        Barricade.Zombie = null;
                                    isAttackingBarricades = false;
                                    Barricade = null;

                                }
                            }
                            
                        }
                        //if there is an obstacle in between
                        else
                        {
                            _stateManager.currentZombieState = ZombieStates.Chasing;
                        }
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

        /// <summary>
        /// Complex Attack Mech
        /// </summary>
        public void Attack()
        {
            if (!_uzAIAgent)
                return;

            //look at Target
            if(Player && Barricade == null && ShooterAI == null)
                _stateManager.transform.LookAt(Player.transform.position);
            else if(Barricade)
                _stateManager.transform.LookAt(Barricade.transform.position);
            else if(ShooterAI)
                _stateManager.transform.LookAt(ShooterAI.transform.position);

            //stop nav agent
            _uzAIAgent.isStopped = true;

            //go to Idle
            _stateManager.Locomotion.walkAnimation = Mathf.Lerp(_stateManager.Locomotion.walkAnimation, 0, Time.deltaTime * 5);
            _animator.SetFloat("Vertical", _stateManager.Locomotion.walkAnimation);

            //see if we are attacking
            isAttacking = _animator.GetBool("isAttacking");

            //if we are not attacking
            if(!isAttacking)
            {
                //see if the delay has been made
                if(_timer >= AttackDelay)
                {
                    //randomly set mirror attack
                    if (Random.value < 0.5f)
                        _animator.SetBool("AttackMirror", true);
                    else
                        _animator.SetBool("AttackMirror", false);

                    //set attack trigger in Animator
                    _animator.SetTrigger("Attack");

                    //reset timer
                    _timer = 0;

                    //reset has Attack flag
                    hasAttacked = false;

                }
                else
                {
                    canAttack = false;
                    //delay
                    _timer += Time.deltaTime;

                }

            }

            //after triggering the animation
            //now checking can we attack
            if (canAttack)
            {
                //ONLY IF we haven't attacked already
                if (!hasAttacked)
                {
                    //Attack Player
                    if (Player && Barricade == null && ShooterAI == null)
                    {

                        if (Player.Health.currentHealth > 0)
                            Player.Health.OnDamage(Random.Range(minDamage, maxDamage));
                        else
                            //just shake player camera
                            Player.TakeDamageEffect();

                    }
                    //Destroy Each Barricade
                    else if (Barricade)
                    {
                        if (!Barricade.allDequed)
                            Barricade.OnDamage();

                    }
                    //Attack ShooterAI
                    else if (ShooterAI)
                    {
                        if (ShooterAI.Health.Health > 0)
                        {
                            //set it inside shooter ai
                            if (!ShooterAI.AttackBehaviour.Zombie)
                                ShooterAI.AttackBehaviour.Zombie = _stateManager;

                            ShooterAI.Health.onDamage(Random.Range(minDamage, maxDamage));
                        }
                        else
                            ShooterAI = null;
                       
                    }

                    //coordinate attack sfx with animation
                    if (_stateManager.ZombieSFX._attackSFX.Count > 0)
                        _stateManager.ZombieSFX.PlaySFX(_stateManager.ZombieSFX._attackSFX[Random.Range(0, _stateManager.ZombieSFX._attackSFX.Count)]);

                    //enable flag to say that we have attacked at this frame
                    hasAttacked = true;
                }
            }

        }
    }

    [System.Serializable]
    public class ZombieEatingBehaviour
    {
        public string FoodTag = "Food";
        public ParticleSystem EatingPfx;
        //We will Disable the current Food Source
        //for belly filled duration
        public Transform currentFoodSource;

        [Range(0,100)]
        public float Hunger = 100;
        [Range(0,100)]
        public float currentHunger;
        [Range(0,10)]
        public float depletionRate = 3f;
        [Range(0, 10)]
        public float replenishRate = 6f;
        [Range(0, 100)]
        public float bellyFilledDuration = 15f;
        float _timer;

        public bool isEating;
        public bool canEat;

        //Main State Manager
        [HideInInspector]
        public uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;
        ParticleSystem Pfx;
        bool cacheHeadTrack;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;

            currentHunger = Hunger;

            cacheHeadTrack = _stateManager.chaseBehaviour.useHeadTrack;

            //Set Particle inside this bone and reset its transform
            if (EatingPfx != null)
            {
                //Get Head bone Transform
                Transform headBone = _animator.GetBoneTransform(HumanBodyBones.Head);

                Pfx = GameObject.Instantiate(EatingPfx) as ParticleSystem;
                Pfx.Stop();
                Pfx.transform.SetParent(headBone);
                Pfx.transform.localPosition = Vector3.zero;
                Pfx.transform.localEulerAngles = Vector3.zero;
            }

        }

        public IEnumerator DepleteHunger()
        {
            while (true)
            {
                //we proceed only if we are not eating
                if (!isEating)
                {
                    //Debug.Log("Depleting");

                    //we can only deplete hunger if our belly filled duration has been completed
                    if (_timer >= bellyFilledDuration)
                    {
                        currentHunger -= (depletionRate * Time.deltaTime);

                        //we force current hunger to be 0
                        if (currentHunger <= 1)
                            currentHunger = 0;

                        //enable Can Eat
                        canEat = true;

                    }
                    else if (_timer < bellyFilledDuration)
                        _timer += Time.deltaTime;

                    //Clamp current Hunger 
                    currentHunger = Mathf.Clamp(currentHunger, 0, Hunger);

                }

                yield return null;
            }

        } 

        /// <summary>
        /// We will replenish hunger here!
        /// </summary>
        public void Eat()
        {
            //we start eating only if our current hunger < Hunger
            if (currentHunger < Hunger && canEat)
            {
                //Debug.Log("Eating");

                StartEatBehaviour();

                //as soon as we reaches Hunger - 1
                if (currentHunger >= (Hunger - 1))
                {
                    //EXIT
                    ExitEatBehaviour();
                }
            }

            //Clamp current Hunger 
            currentHunger = Mathf.Clamp(currentHunger, 0, Hunger);
        }

        void StartEatBehaviour()
        {
            currentHunger += (replenishRate * Time.deltaTime);

            //enable eating flag
            isEating = true;

            //Enter Eating Animation
            _animator.SetBool("Eating", true);

            //Stop NAv Mesh Agent
            _stateManager.Locomotion.uzAIAgent.isStopped = true;

            //disable Head Track
            if (cacheHeadTrack)
                _stateManager.chaseBehaviour.useHeadTrack = false;

            //emit particle from head bone location
            if (Pfx)
            {
                //make sure we are at correct animation state
                if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Biting Loop"))
                {
                    if (Pfx.isStopped)
                        Pfx.Play();
                }

            }
        }

        public void ExitEatBehaviour()
        {
            //Exit Eating Animation
            _animator.SetBool("Eating", false);

            //set Hunger
            currentHunger = Hunger;

            //Play NAv Mesh Agent
            _stateManager.Locomotion.uzAIAgent.isStopped = false;

            //set timer so we don't start depletion instantly
            _timer = 0;

            //we can't eat until we have a lil bit of stomach empty
            canEat = false;

            //clear chase target
            _stateManager.chaseBehaviour.currentChasingTargetTag = "";
            _stateManager.chaseBehaviour.targetPosition = null;

            //Change Player State to patrolling
            _stateManager.currentZombieState = ZombieStates.Patrolling;

            //disable is eating as Zombie's belly is now full
            isEating = false;

            //Set Food as Available back again
            if (currentFoodSource)
            {
                currentFoodSource.GetComponent<uzAIZombieFood>().ToggleFoodAvailability(uzAIZombieFood.FoodAvailability.Available);
                currentFoodSource = null;
            }

            //enable Head Track
            if (cacheHeadTrack)
                _stateManager.chaseBehaviour.useHeadTrack = true;

            //stop particle 
            if (Pfx)
                Pfx.Stop();

            Debug.Log("Belly Full");
        }

    }

    [System.Serializable]
    public class ZombieOffMeshBehaviour
    {
        public bool hasTraversed;
        public bool isTraversing;
        public string currentAnimatorBool;
        public float currentTransitDuration;
        public AnimationCurve currentOffmeshCurve;

        public List<OffmeshLinksAreas> OffMeshAreas = new List<OffmeshLinksAreas>();

        OffMeshLinkData _data;
        Vector3 _startPos;
        Vector3 _endPos;

        //Main State Manager
        [HideInInspector]
        public uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;
            
        }

        public IEnumerator TraverseOffmesh()
        {
            //make sure we have a nav mesh agent
            if (_uzAIAgent == null)
                yield return null;

            //enable is traversing flag
            isTraversing = true;

            PrepareAIForOffmesh();

            //we play animation
            _animator.SetBool(currentAnimatorBool, hasTraversed);

            float time = 0;

            while (time <= currentTransitDuration)
            {
                //make sure we have a nav mesh agent
                if (_uzAIAgent == null)
                    yield return null;

                Vector3 curveValue = currentOffmeshCurve.Evaluate(time / currentTransitDuration) * Vector3.up;

                //normal routine
                _uzAIAgent.transform.position = Vector3.Lerp(_startPos, _endPos, time / currentTransitDuration) + curveValue;

                time += Time.deltaTime;
                yield return null;
            }

            _uzAIAgent.CompleteOffMeshLink();
            hasTraversed = false;

            //we stop animation
            _animator.SetBool(currentAnimatorBool, hasTraversed);

            //disable flag
            isTraversing = false;
        }

        /// <summary>
        /// We will init this everytime before calling the TraverseOffmesh coroutine.
        /// </summary>
        void PrepareAIForOffmesh()
        {
            //we prepare off mesh data from agent
            _data = _uzAIAgent.currentOffMeshLinkData;
            _startPos = _uzAIAgent.transform.position;
            _endPos = _data.endPos + (_uzAIAgent.baseOffset * Vector3.up);
            hasTraversed = true;

            //now we see which off mesh did zombie encountered
            IdentifyOffmeshLink();

            //Debug.Log("Prepare AI For Offmesh");
        }

        void IdentifyOffmeshLink()
        {
            //we get the start and end transform
            Transform sp = _data.offMeshLink.startTransform;
            Transform ep = _data.offMeshLink.endTransform;

            //now we see the dist to find where the zombie really is
            float ZombieDistanceFromSP = Vector3.Distance(_uzAIAgent.transform.position, sp.position);
            float ZombieDistanceFromEP = Vector3.Distance(_uzAIAgent.transform.position, ep.position);

            //whichever distance is less, we will take that as our point and hence
            //get the Animator Bool we want 
            if (ZombieDistanceFromSP < ZombieDistanceFromEP)
            {
                currentAnimatorBool = _data.offMeshLink.transform.GetComponent<OffMeshLinkIdentifier>().startBoolName;

            }
            else if (ZombieDistanceFromEP < ZombieDistanceFromSP)
            {
                currentAnimatorBool = _data.offMeshLink.transform.GetComponent<OffMeshLinkIdentifier>().endBoolName;

            }
           //Now get the animation curve and transit Duration wrt to name obtained.
            for(int i = 0; i< OffMeshAreas.Count; i++)
            {
                if(OffMeshAreas[i].AnimatorBool == currentAnimatorBool)
                {
                    currentOffmeshCurve = OffMeshAreas[i].offmeshCurve;
                    currentTransitDuration = OffMeshAreas[i].TransitDuration;

                    //exit loop
                    break;
                }
            }

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
        /// <param name="_uzAIAgent"></param>
        public void DrawAIGizmos(NavMeshAgent _uzAIAgent)
        {
            if (drawPathToCurrentTarget && _uzAIAgent)
            {
                for (int i = 0; i < _uzAIAgent.path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(_uzAIAgent.path.corners[i], _uzAIAgent.path.corners[i + 1], pathGizmoColor);

                }
            }

            if (drawLineToCurrentTarget && _uzAIAgent)
            {
                Debug.DrawLine(_uzAIAgent.transform.position, _uzAIAgent.path.corners[_uzAIAgent.path.corners.Length-1], lineGizmoColor);
            }
            
        }

    }

    [System.Serializable]
    public class TargetsToDetect
    {
        [Tooltip("Zombie will detect this target as soon as it comes in range and view.")]
        public string targetTag;
        [Tooltip("If zombie is already chasing a target, it will ignore this target if " +
            "it's priority is less then the one being currently chased.")]
        [Range(1,100)]
        public int targetDetectionPriority = 1;
    }

    [System.Serializable]
    public class OffmeshLinksAreas
    {
        public string AnimatorBool = "Vault";
        public AnimationCurve offmeshCurve =  new AnimationCurve(new Keyframe[0]);
        public float TransitDuration = 1.5f;
    }

    #endregion

    public enum DeathType
    {
        Animation,
        Ragdoll
    }

    [System.Serializable]
    public class uzAIZombieHealth
    {
        [Tooltip("Assign theis Zombie's health or leave it at default.")]
        [Range(0,200)]
        public int Health = 100;

        [Tooltip("How many Hit Reaction animations are there in the Animator." +
            "Every time a Zombie is hit, a random hit reaction will be played.")]
        public int hitReactionsAvailable = 3;

        public DeathType _DeathType;

        [Tooltip("Death Animation ID you want to play.")]
        public int DeathID = 1;

        [Tooltip("How long you want to wait to transit from hit animation to other state.")]
        [Range(0.15f,5f)]
        public float _cooldownTimer = 0.85f;

        [Tooltip("if true, zombie will look at player on getting hit [ONLY if he's looking somewhere else]")]
        public bool lookAtCameraOnHit = true;

        [Tooltip("if true, zombie Mesh will fade after death.")]
        public bool fadeZombieMesh = false;

        [Tooltip("A simple Standard Shader material which will replace the current" +
            "Zombie's Mesh Material on Death.")]
        public Material faderMaterial;

        [Tooltip("All child meshes of Zombie.")]
        public List<Renderer> allRenderers = new List<Renderer>();

        [Tooltip("The fade will start after this much delay.")]
        [Range(0f,5f)]
        public float fadeDelay = 0.5f;

        [Tooltip("How fast you want to fade the body after death?")]
        [Range(0f, 5f)]
        public float fadeDuration = 0.5f;
       
        [HideInInspector]
        public int CurrentHealth;
        //[HideInInspector]
        public bool DisableMotion;
        //[HideInInspector]
        public bool cooldown;

        //float cacheSpeed = 0;
        public float _timer;
        
        [HideInInspector]
        public Healthbar healthBar;

        //Main State Manager
        uzAIZombieStateManager _stateManager;
        //Main Animator
        Animator _animator;
        //Main Nav Mesh Agent
        NavMeshAgent _uzAIAgent;

        /// <summary>
        /// Gets and sets the references of uzAI Zombie's StateManager, Nav Mesh Agent and the Animator
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="nav"></param>
        /// <param name="anim"></param>
        public void Initialize(uzAIZombieStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _uzAIAgent = nav;

            //init health
            CurrentHealth = Health;
        }

        public void onDamage(int amt)
        {
            CurrentHealth -= amt;

            if (healthBar)
                healthBar.StartCoroutine(healthBar.LerpSlider());

            if (CurrentHealth <= 0)
            {
                if(_DeathType == DeathType.Animation)
                    _stateManager.Die();
                else if(_DeathType == DeathType.Ragdoll)
                    _stateManager.PerformRagdollDeath(500,_animator.GetBoneTransform(HumanBodyBones.Chest).localPosition, 10);

            }
            else
                HitReaction();

            //Play Hurt SFX
            if (_stateManager.ZombieSFX._hurtSFX.Count > 0)
                _stateManager.ZombieSFX.PlaySFX(_stateManager.ZombieSFX._hurtSFX[Random.Range(0, _stateManager.ZombieSFX._hurtSFX.Count)]);

            //Make sure we exit eating behaviour
            if (_stateManager.eatingBehaviour.isEating)
                _stateManager.eatingBehaviour.ExitEatBehaviour();

        }

        int i = 1;
        void HitReaction()
        {
            //set trigger
            _animator.SetTrigger("HitReaction");
            //set reaction ID
            _animator.SetInteger("HitID", i);

            if(lookAtCameraOnHit)
            //look at camera
                _stateManager.transform.LookAt(Camera.main.transform);

            cooldown = false;

            //increment hitID
            if (i < hitReactionsAvailable)
                i++;
            else
                i = 1;
        }

        public void ZombieGotHit()
        {
            if (!_uzAIAgent)
                return;

            if (_stateManager.currentZombieState == ZombieStates.Die)
                return;


            DisableMotion = _animator.GetBool("DisableMotion");

            if (DisableMotion)
            {
                _uzAIAgent.isStopped = true;

                //lerp speed to 0
                _stateManager.Locomotion.walkAnimation = Mathf.Lerp(_stateManager.Locomotion.walkAnimation, 0, Time.deltaTime * 5f);
                _animator.SetFloat("Vertical", _stateManager.Locomotion.walkAnimation);
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

            while(et < fadeDuration)
            {
                c.a = Mathf.Lerp(c.a, 0, et / fadeDuration);
                foreach(Renderer r in allRenderers)
                {
                    if (r)
                        r.material.color = c;
                }

                et += Time.deltaTime;
                yield return null;
            }

            //5. Destroy Self
            GameObject.Destroy(_stateManager.gameObject);

        }

    }

    [System.Serializable]
    public class uzAIZombieSFX
    {

        public List<AudioClip> _hurtSFX = new List<AudioClip>();
        public List<AudioClip> _attackSFX = new List<AudioClip>();

        [HideInInspector]
        public Transform _uzAI;

        AudioSource _source;

        public void Initialize(Transform _t)
        {
            _uzAI = _t;

            //Add Audio Source
            _source =  _uzAI.gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Plays the Audio Clip
        /// </summary>
        /// <param name="clip"></param>
        public void PlaySFX(AudioClip clip)
        {
            if(_source)
                _source.PlayOneShot(clip);
        }

    }

}