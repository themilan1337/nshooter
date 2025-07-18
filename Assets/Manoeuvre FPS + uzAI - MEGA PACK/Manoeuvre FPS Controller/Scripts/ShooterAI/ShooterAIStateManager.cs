using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Manoeuvre
{
    // --- НОВЫЙ КЛАСС ДЛЯ ОРГАНИЗАЦИИ ДИАЛОГОВ ---
    [System.Serializable]
    public class CharacterDialogueClips
    {
        [Tooltip("Реплики, когда AI замечает врага и начинает погоню.")]
        public List<AudioClip> onChaseClips = new List<AudioClip>();
        [Tooltip("Реплики, которые AI произносит, убив врага.")]
        public List<AudioClip> onKillClips = new List<AudioClip>();
        [Tooltip("Случайные реплики во время бездействия или патрулирования.")]
        public List<AudioClip> onIdleTauntClips = new List<AudioClip>();
        [Tooltip("Как часто (в секундах) AI может произносить реплику в состоянии бездействия. Будет выбрано случайное значение между X и Y.")]
        public Vector2 idleTauntFrequency = new Vector2(15f, 30f);
    }
    
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

        // --- ИЗМЕНЕНО: Добавлен раздел для диалогов персонажа ---
        [Header("Character Specific Dialogues")]
        public CharacterDialogueClips dialogueClips;
        private float nextIdleTauntTime = 0f;
        private bool chaseDialoguePlayed = false;

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
            // Получаем стандартные компоненты
            _animator = GetComponent<Animator>();
            _shooterAIAgent = GetComponent<NavMeshAgent>();
            
            // НЕ инициализируем аудио менеджер здесь
        }

        // Use this for initialization
        void Start()
        {
            // Инициализируем все поведения
            CompanionBehaviour.Initialize(this, _shooterAIAgent, _animator);
            IdleBehaviour.Initialize(this, _shooterAIAgent, _animator);
            PatrolBehaviour.Initialize(this, _shooterAIAgent, _animator);
            ChaseBehaviour.Initialize(this, _shooterAIAgent, _animator);
            AttackBehaviour.Initialize(this, _shooterAIAgent, _animator);
            WeaponBehaviour.Initialize(this, _animator);
            AimIK.Initialize(this);
            Health.Initialize(this, _shooterAIAgent, _animator);

            // --- ПРАВИЛЬНАЯ ИНИЦИАЛИЗАЦИЯ АУДИО ---
            // Объект _audioManager уже существует как поле, нам просто нужно вызвать его метод Initialize.
            _audioManager.Initialize(this);

            // Сброс таймера для реплик в простое
            ResetIdleTauntTimer();
            
            // Запускаем корутины
            StartCoroutine(SightBehaviour.SearchTargetsCoroutine());
            SightBehaviour._stateManager = this;
            SightBehaviour.CreateSensorTrigger();
            StartCoroutine(AttackBehaviour.SearchAttackTargetsCoroutine());
        }
        
        // --- НОВАЯ ФУНКЦИЯ для проигрывания случайного диалога из списка ---
        public void PlayRandomDialogue(List<AudioClip> clips)
        {
            if (clips != null && clips.Count > 0)
            {
                int randomIndex = Random.Range(0, clips.Count);
                AudioClip clipToPlay = clips[randomIndex];
                if (clipToPlay != null && _audioManager != null)
                {
                    _audioManager.PlayAudioClip(clipToPlay);
                }
            }
        }
        
        // --- НОВАЯ ФУНКЦИЯ для сброса таймера реплик ---
        private void ResetIdleTauntTimer()
        {
            if (dialogueClips != null)
            {
                nextIdleTauntTime = Time.time + Random.Range(dialogueClips.idleTauntFrequency.x, dialogueClips.idleTauntFrequency.y);
            }
        }

        // --- НОВАЯ ФУНКЦИЯ, вызывается когда AI убивает цель ---
        public void OnTargetKilled()
        {
            PlayRandomDialogue(dialogueClips.onKillClips);
        }


        private void OnAnimatorMove()
        {
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh)
                return;

            _animator.applyRootMotion = true;
            _shooterAIAgent.speed = (_animator.deltaPosition / Time.deltaTime).magnitude;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_animator) return;
            if (!AimIK.LeftHandIK) return;

            if ((currentShooterState == ShooterAIStates.Patrol || currentShooterState == ShooterAIStates.Idle) && _AIType == AIType.Companion)
            {
                if (Player)
                    _animator.SetLookAtPosition(Player.transform.position + Vector3.up);
                _animator.SetLookAtWeight(1);
            }

            if (!WeaponBehaviour.weaponObject) return;

            if (setHandsIK && currentShooterState != ShooterAIStates.Die && !AttackBehaviour.isReloading)
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, AimIK.LeftHandIK.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, AimIK.LeftHandIK.rotation);
                _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            }

            if (ChaseBehaviour.useHeadTrack)
            {
                if (currentShooterState == ShooterAIStates.Chase && ChaseBehaviour.targetPosition)
                {
                    _animator.SetLookAtPosition(ChaseBehaviour.targetPosition.position + Vector3.up);
                    _animator.SetLookAtWeight(1);
                }
                else if (currentShooterState == ShooterAIStates.Firing)
                {
                    if (AttackBehaviour.Player) _animator.SetLookAtPosition(AttackBehaviour.Player.transform.position + Vector3.up);
                    else if (AttackBehaviour.Zombie) _animator.SetLookAtPosition(AttackBehaviour.Zombie.transform.position + Vector3.up);
                    else if (AttackBehaviour.ShooterAI) _animator.SetLookAtPosition(AttackBehaviour.ShooterAI.transform.position + Vector3.up);
                    else if (AttackBehaviour.Turret) _animator.SetLookAtPosition(AttackBehaviour.Turret.transform.position + Vector3.up);
                    _animator.SetLookAtWeight(1);
                }
            }

            if (currentShooterState == ShooterAIStates.Firing)
            {
                _animator.SetIKPosition(AvatarIKGoal.LeftHand, AimIK.LeftHandAimIK.position);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, AimIK.LeftHandAimIK.rotation);
                _animator.SetBoneLocalRotation(HumanBodyBones.Spine, AimIK.SpineTransform.localRotation);
            }

            if (Health.DisableMotion)
            {
                AttackBehaviour.RotateSpine();
            }
        }

        private void FixedUpdate()
        {
            if (currentShooterState == ShooterAIStates.Die) return;
            Health.AIGotHit();
            if (Health.DisableMotion) return;
            if (Health.cooldown) return;
            IdentifyStates();
            SetState();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (currentShooterState == ShooterAIStates.Firing) return;
            if (other.gameObject.tag == "AwarenessTrigger" && _AIType == AIType.Enemy)
            {
                if (other.gameObject.GetComponentInParent<Transform>().tag == "Player")
                {
                    ChaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);
                    SightBehaviour.visibleTargets.Add(other.gameObject.GetComponentInParent<Manoeuvre.ManoeuvreFPSController>().transform);
                }
                else
                {
                    ChaseBehaviour.CanChaseThisTarget(other.gameObject.GetComponentInParent<Transform>().transform);
                }
            }

            if (other.gameObject.tag == "Destination")
            {
                OnDestinationReached();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            SetLastPlayerPositionVector(other.transform);
        }

        public void IdentifyStates()
        {
            if (currentShooterState == ShooterAIStates.Chase) return;
            if (SightBehaviour.visibleTargets.Count > 0) return;
            if (AttackBehaviour.Player || AttackBehaviour.Zombie) return;

            if (_AIType == AIType.Companion)
            {
                CompanionBehaviour.FollowPlayer();
            }
            else if (_AIType == AIType.Enemy)
            {
                if (_globalDelayTimer < PatrolBehaviour.PatrolDelay)
                    currentShooterState = ShooterAIStates.Idle;
                else if (_globalDelayTimer > PatrolBehaviour.PatrolDelay)
                    currentShooterState = ShooterAIStates.Patrol;
            }
        }

        public void SetState()
        {
            switch (currentShooterState)
            {
                case ShooterAIStates.Idle:
                    _animator.SetBool("isAiming", false);
                    chaseDialoguePlayed = false; // Сброс флага диалога погони
                    IdleBehaviour.Idle();
                    
                    // --- ИЗМЕНЕНО: Логика для реплик в простое ---
                    if (Time.time > nextIdleTauntTime)
                    {
                        PlayRandomDialogue(dialogueClips.onIdleTauntClips);
                        ResetIdleTauntTimer();
                    }
                    break;

                case ShooterAIStates.Patrol:
                    _animator.SetBool("isAiming", false);
                    chaseDialoguePlayed = false; // Сброс флага диалога погони
                    PatrolBehaviour.Patrol();
                    break;

                case ShooterAIStates.Chase:
                    _animator.SetBool("isAiming", true);
                    // --- ИЗМЕНЕНО: Проигрываем реплику при начале погони ---
                    if (!chaseDialoguePlayed)
                    {
                        PlayRandomDialogue(dialogueClips.onChaseClips);
                        chaseDialoguePlayed = true;
                    }
                    ChaseBehaviour.Chase();
                    break;

                case ShooterAIStates.Firing:
                    _animator.SetBool("isAiming", true);
                    AttackBehaviour.Attack();
                    break;
            }
            
            WeaponBehaviour.SetAwarenessTriggerVisibility(currentShooterState);
            DrawGizmosBehaviour.DrawAIGizmos(_shooterAIAgent);
        }

        public void SetLastPlayerPositionVector(Transform p)
        {
            if (ChaseBehaviour.lastPlayerPosition == null) return;
            if (p.tag == "Player" || p.tag == "AwarenessTrigger")
            {
                if (ChaseBehaviour.currentChasingTargetTag == "Player" || ChaseBehaviour.currentChasingTargetTag == "AwarenessTrigger")
                {
                    ChaseBehaviour.currentChasingTargetTag = "";
                    ChaseBehaviour.lastPlayerPosition.position = p.position;
                    ChaseBehaviour.targetPosition = ChaseBehaviour.lastPlayerPosition;
                    ChaseBehaviour.lastPlayerPosition.gameObject.SetActive(true);
                }
            }
        }

        public void OnDestinationReached()
        {
            Debug.Log("Destination Reached");
            if (_shooterAIAgent != null && _shooterAIAgent.isOnNavMesh)
            {
                _shooterAIAgent.isStopped = true;
            }
            _globalDelayTimer = 0;
            if (ChaseBehaviour.lastPlayerPosition != null)
            {
                ChaseBehaviour.lastPlayerPosition.gameObject.SetActive(false);
            }
            ChaseBehaviour.targetPosition = null;
            AttackBehaviour.Player = null;
            AttackBehaviour.ShooterAI = null;
            AttackBehaviour.Zombie = null;
            AttackBehaviour.Turret = null;
            PatrolBehaviour.resetPath = true;
            currentShooterState = ShooterAIStates.Idle;
        }

        public void Die()
        {
            currentShooterState = ShooterAIStates.Die;
            if (Manoeuvre.gc_PlayerDialoguesManager.Instance)
                Manoeuvre.gc_PlayerDialoguesManager.Instance.PlayDialogueClip(Manoeuvre.gc_PlayerDialoguesManager.DialogueType.Kills);

            // --- ИЗМЕНЕНО: Проигрываем звук смерти из нового слота ---
            _audioManager.PlayAudioClip(Health.DeathSound);
            
            _animator.SetBool("DeathMirror", Random.value < 0.5f);
            _animator.SetTrigger("Death");
            _animator.SetInteger("DeathID", Health.DeathID);

            if (_shooterAIAgent != null) Destroy(_shooterAIAgent);
            foreach (Collider c in GetComponentsInChildren<Collider>()) Destroy(c);
            if (GetComponent<Rigidbody>() != null) Destroy(GetComponent<Rigidbody>());
            if (ChaseBehaviour.lastPlayerPosition != null) Destroy(ChaseBehaviour.lastPlayerPosition.gameObject);
            
            Transform iconTransform = transform.Find("MinimapIcon");
            if (iconTransform != null) { /* ... icon logic ... */ }

            if (Health.FadeMesh)
            {
                foreach (Renderer r in Health.allRenderers)
                {
                    if (r == null) continue;
                    foreach (Material m in r.materials)
                    {
                        if (m == null || Health.faderMaterial == null) continue;
                        m.shader = Health.faderMaterial.shader;
                    }
                }
                StartCoroutine(Health.FadeOnDeath());
            }
            else
            {
                Destroy(this.gameObject, 5f);
            }
            OnDeath.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (AimIK.LeftHandIK)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(AimIK.LeftHandIK.position, .05f);
            }
            if (AimIK.LeftHandAimIK)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(AimIK.LeftHandAimIK.position, .05f);
            }
            if (!WeaponBehaviour.muzzleLocation) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(WeaponBehaviour.muzzleLocation.position, .05f);
        }
        
        // ... (Остальной код остается без изменений до класса ShooterAIWeaponBehaviour) ...
        // Я пропущу идентичные части для краткости, но они должны быть в твоем файле.
        // Вот измененные классы, которые важны для аудио.
    }

    [System.Serializable]
    public class ShooterAIIdleBehaviour { /* Без изменений */ 
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
        
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;
            _animator.SetFloat("Vertical", 0);
        }
        
        public void Idle()
        {
            if (_shooterAIAgent != null && _shooterAIAgent.isOnNavMesh)
            {
                _shooterAIAgent.isStopped = true;
            }
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * idleTransitionDuration);
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);
            _animator.applyRootMotion = false;
            if (_stateManager._globalDelayTimer < _stateManager.PatrolBehaviour.PatrolDelay)
                _stateManager._globalDelayTimer += Time.deltaTime;
            else
                return;
        }
    }
    [System.Serializable]
    public class ShooterAIPatrolBehaviour { /* Без изменений */
        [Tooltip("Assign the Patrolling Path of this Agent")]
        public List<Transform> PatrolPath;

        [Tooltip("This is the Patrol Speed  i.e it simply sets the movement in the Blend Tree of the Animator")]
        [Range(0.5f, 2f)]
        public float PatrolAnimation = 1f;

        [Tooltip("How much you want agent to stop before moving on to next Waypoint!")]
        [Range(0.1f, 10f)]
        public float PatrolDelay = 3f;

        public bool resetPath;
        int currentWaypoint;
        int nextWaypoint;
        ShooterAIStateManager _stateManager;
        Animator _animator;
        NavMeshAgent _shooterAIAgent;
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;

            if (_shooterAIAgent != null)
            {
                _shooterAIAgent.stoppingDistance = 1f;
            }
        }
        public void Patrol()
        {
            if (_stateManager.currentShooterState != ShooterAIStates.Patrol) return;
            if (_stateManager._AIType == AIType.Companion) { PatrolPath.Clear(); PatrolTowardsPlayer(); return; }
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh) return;
            if (PatrolPath == null) return;
            if (PatrolPath.Count < 1) return;
            if (_shooterAIAgent.hasPath)
            {
                _shooterAIAgent.isStopped = false;
                _shooterAIAgent.updatePosition = true;
                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, PatrolAnimation, Time.deltaTime * 5);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);
            }
            if (_shooterAIAgent.remainingDistance <= _shooterAIAgent.stoppingDistance || _shooterAIAgent.pathStatus != NavMeshPathStatus.PathComplete
                || _shooterAIAgent.isPathStale || resetPath)
            {
                if (!_shooterAIAgent.hasPath || _shooterAIAgent.velocity.sqrMagnitude == 0 || resetPath)
                {
                    if (_stateManager._globalDelayTimer >= PatrolDelay || resetPath)
                    {
                        if (PatrolPath.Count > nextWaypoint && PatrolPath[nextWaypoint] == null) return;
                        currentWaypoint = nextWaypoint;
                        _shooterAIAgent.SetDestination(PatrolPath[currentWaypoint].position);
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
                _stateManager._globalDelayTimer = 0;
            }
        }
        public void PatrolTowardsPlayer()
        {
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh) return;
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
    public class ShooterAIChaseBehaviour { /* Без изменений */
        public string currentChasingTargetTag = "";
        public Transform targetPosition;
        [Range(1f, 2f)]
        public float chaseAnimation = 2f;
        public bool useHeadTrack = true;
        public Transform lastPlayerPosition;
        ShooterAIStateManager _stateManager;
        Animator _animator;
        NavMeshAgent _shooterAIAgent;
        public void Initialize(ShooterAIStateManager mgr, NavMeshAgent nav, Animator anim)
        {
            _stateManager = mgr;
            _animator = anim;
            _shooterAIAgent = nav;
            lastPlayerPosition = new GameObject().transform;
            lastPlayerPosition.name = "Last Known Position";
            lastPlayerPosition.gameObject.AddComponent<SphereCollider>().isTrigger = true;
            lastPlayerPosition.gameObject.layer = LayerMask.NameToLayer("Default");
            lastPlayerPosition.gameObject.tag = "Destination";
            lastPlayerPosition.gameObject.SetActive(false);
        }
        public void CanChaseThisTarget(Transform t)
        {
            if (t.tag == currentChasingTargetTag) return;
            if (_stateManager._AIType == AIType.Companion && t.tag == "Player") return;
            if (string.IsNullOrEmpty(currentChasingTargetTag))
            {
                targetPosition = t;
                _stateManager.currentShooterState = ShooterAIStates.Chase;
                currentChasingTargetTag = t.tag;
            }
        }
        public void Chase()
        {
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh) return;
            if (_stateManager.currentShooterState == ShooterAIStates.Hit) return;
            if (_stateManager.currentShooterState == ShooterAIStates.Firing) return;
            if (!targetPosition)
            {
                _stateManager.currentShooterState = ShooterAIStates.Idle;
                return;
            }
            if (_shooterAIAgent.destination != targetPosition.position)
            {
                _shooterAIAgent.SetDestination(targetPosition.position);
            }
            _shooterAIAgent.isStopped = false;
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, chaseAnimation, Time.deltaTime * 5);
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);
        }
    }
    [System.Serializable]
    public class ShooterAISightBehaviour { /* Без изменений */
        [Header("Sight Properties")]
        [Range(1f, 25f)]
        public float Range = 10f;
        [Range(0f, 360f)]
        public float Angle;
        public float SearchIterationTime = 0.25f;
        public LayerMask targetMask;
        public LayerMask obstacleMask;
        public List<Transform> visibleTargets = new List<Transform>();
        [HideInInspector]
        public ShooterAIStateManager _stateManager;
        public void CreateSensorTrigger()
        {
            Transform t = new GameObject().transform;
            t.SetParent(_stateManager.transform);
            t.gameObject.AddComponent<SphereCollider>().radius = Range;
            t.gameObject.GetComponent<SphereCollider>().isTrigger = true;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.name = "Sensor Trigger";
            int layer = LayerMask.NameToLayer("ShooterAISensor");
            if (layer != -1)
            {
                t.gameObject.layer = layer;
            }
        }
        public IEnumerator SearchTargetsCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(SearchIterationTime);
                SearchVisibleTarget();
            }
        }
        void SearchVisibleTarget()
        {
            if (_stateManager == null) return;
            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(_stateManager.transform.position, Range, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform target = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (target.position - _stateManager.transform.position).normalized;
                if (Vector3.Angle(_stateManager.transform.forward, dirToTarget) < Angle / 2)
                {
                    float distToTarget = Vector3.Distance(_stateManager.transform.position, target.position);
                    if (!Physics.Raycast(_stateManager.transform.position, dirToTarget, distToTarget, obstacleMask))
                    {
                        if (_stateManager._AIType == AIType.Companion && target.tag == "Player") continue;
                        if(target.tag == "ShooterAI")
                        {
                            var otherAI = target.GetComponent<ShooterAIStateManager>();
                            if (otherAI != null && _stateManager._AIType == otherAI._AIType) continue;
                        }
						if (target.tag == "uzAIZombie" || target.tag == "ShooterAI" || target.tag == "Player" || target.tag == "Turret") {    
							visibleTargets.Add (target);
							_stateManager.ChaseBehaviour.CanChaseThisTarget (target.transform);
						}
                    }
                    else
                    {
                        _stateManager.SetLastPlayerPositionVector(target);
                    }
                }
            }
        }
        public Vector3 DirFromAngle(float angle, bool isAngleGlobal)
        {
            if (_stateManager)
            {
                if (!isAngleGlobal) angle += _stateManager.transform.eulerAngles.y;
                float retAngle = angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(retAngle), 0, Mathf.Cos(retAngle));
            }
            return Vector3.zero;
        }
    }
    [System.Serializable]
    public class ShooterAIAttackBehaviour { /* Без изменений */
        [Header("Attack Properties")]
        public ManoeuvreFPSController Player;
        public ShooterAIStateManager ShooterAI;
		public Turret Turret;
		public uzAI.uzAIZombieStateManager Zombie;
        [Range(1.5f, 15f)]
        public float AttackDistance = 10f;
        [Range(0f, 360f)]
        public float Angle;
        public float SearchIterationTime = 0.25f;
        public LayerMask targetMask;
        public LayerMask obstacleMask;
        [Range(0f, 5f)]
        public float FireDelay = 1f;
        [HideInInspector]
        public bool isReloading;
        [HideInInspector]
        public ShooterAIStateManager _stateManager;
        Animator _animator;
        NavMeshAgent _shooterAIAgent;
        public bool hasAttacked;
        float _timer;
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
            visibleTargets.Clear();
            Collider[] targetsInViewRadius = Physics.OverlapSphere(_stateManager.transform.position, AttackDistance, targetMask);
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
				if (targetsInViewRadius[i].tag == "Player" || targetsInViewRadius[i].tag == "uzAIZombie" || targetsInViewRadius[i].tag == "ShooterAI" || targetsInViewRadius[i].tag == "Turret")
                {
                    if (_stateManager._AIType == AIType.Companion && targetsInViewRadius[i].tag == "Player") continue;
                    Transform target = targetsInViewRadius[i].transform;
                    Vector3 dirToTarget = (target.position - _stateManager.transform.position).normalized;
                    if (Vector3.Angle(_stateManager.transform.forward, dirToTarget) < Angle / 2)
                    {
                        float distToTarget = Vector3.Distance(_stateManager.transform.position, target.position);
                        if (!Physics.Raycast(_stateManager.transform.position, dirToTarget, distToTarget, obstacleMask))
                        {
                            visibleTargets.Add(target);
							if (target.GetComponent<ManoeuvreFPSController> ()) {
								Player = target.GetComponent<ManoeuvreFPSController> ();
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;
							}
                            else if (target.GetComponent<ShooterAIStateManager> ()) {
                                var otherAI = target.GetComponent<ShooterAIStateManager>();
								if (otherAI != null && otherAI._AIType != _stateManager._AIType) {
									ShooterAI = otherAI;
									_stateManager.currentShooterState = ShooterAIStates.Firing;
									_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
									_stateManager.ChaseBehaviour.targetPosition = null;
								}
							} 
							else if (target.GetComponent<Turret> ()) {
								Turret = target.GetComponent<Turret> ();
								_stateManager.currentShooterState = ShooterAIStates.Firing;
								_stateManager.ChaseBehaviour.currentChasingTargetTag = "";
								_stateManager.ChaseBehaviour.targetPosition = null;
							}
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
            if (!_shooterAIAgent || !_shooterAIAgent.isOnNavMesh) return;
            isReloading = _animator.GetBool("isReloading");
            if (isReloading) return;
            _shooterAIAgent.isStopped = true;
            RotateSpine();
            RotateTowardsTarget();
            _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * 5);
            _animator.SetFloat("Vertical", _stateManager.walkAnimation);
            if (_timer >= FireDelay )
            {
                _timer = 0;
                hasAttacked = false;
            }
            else
            {
                _timer += Time.deltaTime;
            }
            if (!hasAttacked && CanAttack() && _stateManager.WeaponBehaviour.weaponObject)
            {
                _animator.SetTrigger("isFiring");
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
                hasAttacked = true;
            }
            else if (!hasAttacked && !CanAttack())
            {
                hasAttacked = true;
                _animator.SetTrigger("Reload");
                if (_stateManager.WeaponBehaviour.ReloadSound)
                    _stateManager._audioManager.PlayAudioClip(_stateManager.WeaponBehaviour.ReloadSound);
            }
        }
        public void RotateSpine()
        {
            Vector3 destinationRotation = Vector3.zero;
			if (Player) destinationRotation = _stateManager.transform.position - Player.transform.position;
			else if (ShooterAI) destinationRotation = _stateManager.transform.position - ShooterAI.transform.position;
			else if (Zombie) destinationRotation = _stateManager.transform.position - Zombie.transform.position;
			else if (Turret) destinationRotation = _stateManager.transform.position - Turret.transform.position;
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
                if (ShooterAI.Health.Health > 0) _stateManager.transform.LookAt(ShooterAI.transform.position);
                else ShooterAI = null;
            }
            else if (Zombie)
            {
                if (Zombie.ZombieHealthStats.CurrentHealth > 0) _stateManager.transform.LookAt(Zombie.transform.position);
                else Zombie = null;
            }
			else if (Turret)
			{
				if (Turret._turretHealth.Health > 0) _stateManager.transform.LookAt(Turret.transform.position);
				else Turret = null;
			}
        }
        bool CanAttack()
        {
            if (_stateManager.AimIK.DebugAimIK) return true;
            if(_stateManager.WeaponBehaviour.Ammo > 0) return true;
            return false;
        }
        public Vector3 DirFromAngle(float angle, bool isAngleGlobal)
        {
            if (_stateManager)
            {
                if (!isAngleGlobal) angle += _stateManager.transform.eulerAngles.y;
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
        
        // --- ИЗМЕНЕНО: Поля для аудио вынесены сюда для наглядности ---
        [Header("Weapon Sounds")]
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
            if (_cacheMuzzleFX) _cacheMuzzleFX.Play();
            _stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = _controller.transform.position - muzzleLocation.position;
            int damage = Random.Range(minDamage, maxDamage);

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _controller.transform.tag)
                {
                    if (!_stateManager.AimIK.DebugAimIK)
                    {
                        // --- ИЗМЕНЕНО: Проверка на убийство ---
                        if (_controller.Health.currentHealth > 0 && _controller.Health.currentHealth - damage <= 0)
                        {
                            _stateManager.OnTargetKilled();
                        }
                        _controller.Health.OnDamage(damage);
                    }
                    if (HitParticle.Count > 0 && HitParticle[0] != null)
                        HitParticle[0].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void ShootTarget(ShooterAIStateManager _shooterAI)
        {
            if (_cacheMuzzleFX) _cacheMuzzleFX.Play();
            _stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = (_shooterAI.transform.position + Vector3.up) - muzzleLocation.position;
            int damage = Random.Range(minDamage, maxDamage);
            
            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _shooterAI.transform.tag)
                {
                    if (!_stateManager.AimIK.DebugAimIK)
                    {
                        // --- ИЗМЕНЕНО: Проверка на убийство ---
                        if (_shooterAI.Health.Health > 0 && _shooterAI.Health.Health - damage <= 0)
                        {
                            _stateManager.OnTargetKilled();
                        }
                        _shooterAI.Health.onDamage(damage);
                    }
                    _shooterAI.ChaseBehaviour.targetPosition = _stateManager.transform;
                    _shooterAI.AttackBehaviour.ShooterAI = _stateManager;
                    if (HitParticle.Count > 2 && HitParticle[2] != null)
                        HitParticle[2].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void ShootTarget(Turret _turret)
        {
            if (_cacheMuzzleFX) _cacheMuzzleFX.Play();
            _stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = (_turret.transform.position + Vector3.up) - muzzleLocation.position;
            int damage = Random.Range(minDamage, maxDamage);

            if (Physics.Raycast(muzzleLocation.position, direction, out hit, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _turret.transform.tag)
                {
                    if (!_stateManager.AimIK.DebugAimIK)
                    {
                        // --- ИЗМЕНЕНО: Проверка на убийство ---
                        if (_turret._turretHealth.Health > 0 && _turret._turretHealth.Health - damage <= 0)
                        {
                             _stateManager.OnTargetKilled();
                        }
                        _turret._turretHealth.onDamage(damage, _stateManager.transform);
                    }
                    if (HitParticle.Count > 3 && HitParticle[3] != null)
                        HitParticle[3].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }

        public void ShootTarget(uzAI.uzAIZombieStateManager _uzAI)
        {
            if (_cacheMuzzleFX) _cacheMuzzleFX.Play();
            _stateManager._audioManager.PlayAudioClip(FireSound);

            if (muzzleLocation == null) return;
            RaycastHit hit;
            Vector3 direction = (_uzAI.transform.position + Vector3.up) - muzzleLocation.position;
            int damage = Random.Range(minDamage, maxDamage);
            
            if (Physics.Raycast(muzzleLocation.position, direction, out hit, 100, _stateManager.AttackBehaviour.targetMask))
            {
                if (hit.transform.tag == _uzAI.transform.tag)
                {
                    if (!_stateManager.AimIK.DebugAimIK)
                    {
                        // --- ИЗМЕНЕНО: Проверка на убийство ---
                        if (_uzAI.ZombieHealthStats.CurrentHealth > 0 && _uzAI.ZombieHealthStats.CurrentHealth - damage <= 0)
                        {
                            _stateManager.OnTargetKilled();
                        }
                        _uzAI.ZombieHealthStats.onDamage(damage);
                    }
                    if (HitParticle.Count > 1 && HitParticle[1] != null)
                        HitParticle[1].onHit(hit.transform, hit.point, hit.normal);
                }
            }
        }
        
        public void RefillAmmo() { Ammo = cacheAmmo; }

        void InitializeAudioTarget()
        {
            myAwarenessTrigger = new GameObject("AwarenessTrigger");
            myAwarenessTrigger.transform.SetParent(_stateManager.transform);
            myAwarenessTrigger.transform.localPosition = Vector3.zero;
            myAwarenessTrigger.SetActive(false);
            myAwarenessTrigger.AddComponent<SphereCollider>().isTrigger = true;
            int layer = LayerMask.NameToLayer("AwarenessTrigger");
            if (layer != -1) myAwarenessTrigger.layer = layer;
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
    public class ShooterAICompanionBehaviour { /* Без изменений */
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
            if (_stateManager.AttackBehaviour.Zombie || _stateManager.currentShooterState == ShooterAIStates.Die) return;
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
        public int hitReactionsAvailable = 3;
        public int DeathID = 1;
        [Range(0.15f, 5f)]
        public float _cooldownTimer = 0.85f;
        public bool lookAtCameraOnHit = true;

        // --- ИЗМЕНЕНО: Поля для аудио вынесены сюда для наглядности ---
        [Header("Health & Damage Sounds")]
        public AudioClip DeathSound;
        public List<AudioClip> HitSounds = new List<AudioClip>();

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
            if (Health <= 0) return;
            if (_stateManager._AIType == AIType.Enemy) Health -= amt;
            else if (_stateManager._AIType == AIType.Companion && _stateManager.CompanionBehaviour.AllowDamageFromPlayer) Health -= amt;

            if (healthBar) healthBar.StartLerp();
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
            // --- ИЗМЕНЕНО: Проигрывание звука получения урона ---
            if (HitSounds.Count > 0 && _stateManager._audioManager._source != null && !_stateManager._audioManager._source.isPlaying)
            {
                _stateManager._audioManager.PlayAudioClip(HitSounds[Random.Range(0, HitSounds.Count)]);
            }

            _animator.SetTrigger("HitReaction");
            _animator.SetInteger("HitID", i);
            if (lookAtCameraOnHit)
            {
                if (_stateManager.AttackBehaviour.Player) _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Player.transform.position);
                else if (_stateManager.AttackBehaviour.Zombie) _stateManager.transform.LookAt(_stateManager.AttackBehaviour.Zombie.transform.position);
                else if (_stateManager.AttackBehaviour.ShooterAI) _stateManager.transform.LookAt(_stateManager.AttackBehaviour.ShooterAI.transform.position);
            }
            cooldown = false;
            if (i < hitReactionsAvailable) i++;
            else i = 1;
        }

        public void AIGotHit()
        {
            if (_shooterAIAgent == null) return;
            if (_stateManager.currentShooterState == ShooterAIStates.Die) return;
            DisableMotion = _animator.GetBool("DisableMotion");
            if (DisableMotion)
            {
                if (_shooterAIAgent.isOnNavMesh) _shooterAIAgent.isStopped = true;
                _stateManager.walkAnimation = Mathf.Lerp(_stateManager.walkAnimation, 0, Time.deltaTime * 5f);
                _animator.SetFloat("Vertical", _stateManager.walkAnimation);
                _timer = 0;
                cooldown = true;
            }
            if (cooldown)
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
            if (_animator != null) _animator.enabled = false;
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
            if (_stateManager != null) GameObject.Destroy(_stateManager.gameObject);
        }
    }
    
    [System.Serializable]
    public class ShooterAIAimIK { /* Без изменений */
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
            
            // --- Настройки для 3D звука остаются ---
            _source.spatialBlend = 1.0f; // Делает звук 3D
            _source.minDistance = 5f;
            _source.maxDistance = 50f;
            _source.rolloffMode = AudioRolloffMode.Linear;
        }

        public void PlayAudioClip(AudioClip clip)
        {
            if (clip != null && _source != null)
            {
                // Используем PlayOneShot, чтобы реплики не прерывали друг друга, если они короткие
                // и чтобы не прерывать фоновые звуки, если они есть.
                _source.PlayOneShot(clip);
            }
        }
    }

    [System.Serializable]
    public class DrawGizmos { /* Без изменений */
        public bool drawPathToCurrentTarget = true;
        public Color pathGizmoColor = Color.cyan;
        public bool drawLineToCurrentTarget = true;
        public Color lineGizmoColor = Color.green;
        public void DrawAIGizmos(NavMeshAgent _shooterAIAgent)
        {
            if (_shooterAIAgent == null || !_shooterAIAgent.isOnNavMesh || !_shooterAIAgent.hasPath) return;
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