using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class WeaponMelee : MonoBehaviour
    {

        public string WeaponName;

        [HideInInspector]
        public int Weapon_ID;

        //[HideInInspector]
        public MeleeAttackProperties _MeleeAttack;
        //[HideInInspector]
        public MeleeAttackAnimationProperties _MeleeAnimations;
        //[HideInInspector]
        public MeleeAttackFOVProperties _MeleeAttackRange;
        [HideInInspector]
        public MeleeHitParticleFX _MeleeHitParticles;

        [HideInInspector]
        public RecoilSpring recoilProperties = new RecoilSpring();

        [HideInInspector]
        public ManoeuvreFPSController _controller;

        //editor vars
        [HideInInspector]
        public int tabCount;

        // Use this for initialization
        IEnumerator Start()
        {
            //wait for first frame
            yield return new WaitForEndOfFrame();

            //get controller reference
            _controller = ManoeuvreFPSController.Instance;

            //Initialize Attack Properties
            _MeleeAttack.Initialize();
            _MeleeAttackRange.Initialize(_controller);
            _MeleeAnimations.Initialize(this);
            recoilProperties.Initialize(transform, null, GetComponent<WeaponProceduralManoeuvre>());
            
        }

        // Update is called once per frame
        void Update()
        {
            //make sure we don't proceed if there's no controller
            if (!_controller)
                return;

            //if this weapon is not animated
            //no need to do anything
            if (!_MeleeAnimations.isAnimated)
                return;

            //Start Searching for targets
            _MeleeAttackRange.SearchAttackTargets();

            //make sure weapon is lerping back to original rotation
            recoilProperties.returnToRot_Melee();

            //check every frame can we attack
            _MeleeAttack.canWeAttack();

            //if there's shoot input and we can attack
            if (_controller.Inputs.shootInput && _MeleeAttack.canAttack)
            {
                //Attack
                Attack();
            }

        }

        void Attack()
        {
            //reset everything
            _MeleeAttack.attackTimer = 0;
            _MeleeAttack.canAttack = false;

            //Play Attack Animation
            _MeleeAnimations.PlayAttackAnimation();

            //Start Apply Damage Coroutine
            StartCoroutine(PrepareForAttack());

        }

        IEnumerator PrepareForAttack()
        {
            float et = 0;
            while (et < _MeleeAnimations.currentClipLength)
            {
                
                //as soon as we cross the attack start time
                if(et >= _MeleeAnimations.currentAttackStartTime)
                {
                    //see if we haven't already attacked
                    if (_MeleeAttack.hasAttacked == false)
                        //We finally apply damage
                        ApplyDamage();
                }

                et += Time.deltaTime;
                yield return null;
            }
        }

        void ApplyDamage()
        {
            //enable has attacked flag
            _MeleeAttack.hasAttacked = true;

            //we don't bother to proceed if we don't have any targets to attack to
            if (_MeleeAttackRange.visibleTargets.Count < 1)
                return;

            //Apply Damage to Each Target
            for(int i =0; i< _MeleeAttackRange.visibleTargets.Count; i++)
            {
                //calculate random damage each time
                int damage = Random.Range(_MeleeAttack.maxDamage, _MeleeAttack.minDamage);

                Transform target = _MeleeAttackRange.visibleTargets[i];

                //Apply Damage to Zombie
                if (target.tag == "uzAIZombie")
                {
                    target.GetComponent<uzAI.uzAIZombieStateManager>().ZombieHealthStats.onDamage(damage);

                    //spawn particle
                    _MeleeHitParticles.SpawnParticleAtTarget(target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head), target.tag);
                }

                if(target.tag == "Destructible")
                {
                    target.GetComponent<DestructibleProps>().OnDamage(damage);

                    //spawn particle
                    _MeleeHitParticles.SpawnParticleAtTarget(target, target.tag);

                }

                if (target.tag == "ShooterAI")
                {
                    target.GetComponent<ShooterAIStateManager>().Health.onDamage(damage);

                    //spawn particle
                    _MeleeHitParticles.SpawnParticleAtTarget(target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head), target.tag);

                }

            }

        }

    }

    [System.Serializable]
    public class MeleeAttackProperties
    {
        public int maxDamage = 15;
        public int minDamage = 5;

        [HideInInspector]
        public float attackDelay = 2f;
        [HideInInspector]
        public float attackTimer;
        [HideInInspector]
        public bool canAttack;
        [HideInInspector]
        public bool hasAttacked;

        public void Initialize()
        {
            //making sure we can attack as soon as we equip this for first time
            attackTimer = attackDelay;
            canAttack = true;
        }

        /// <summary>
        /// Checks every frame can we attack
        /// </summary>
        public void canWeAttack()
        {

            if (ManoeuvreFPSController.Instance.Inputs.inventoryInput)
            {
                canAttack = false;
                return;
            }

            if (attackTimer < attackDelay)
            {
                attackTimer += Time.deltaTime;
                canAttack = false;
                
            }
            else
            {
                // reset bools
                canAttack = true;
                hasAttacked = false;
            }

            //also check if we are not currently equipping this weapon
            if (gc_AmmoManager.Instance.isEquipping)
                canAttack = false;
        }

    }

    [System.Serializable]
    public class MeleeAttackFOVProperties
    {
        public LayerMask hitMask;
        public LayerMask obstacleMask;
        [Range(0,15)]
        public float AttackDistance = 3f;
        [Range(0,360)]
        public float Angle = 35f;

        public List<string> hitTags = new List<string>();

        //[HideInInspector]
        public List<Transform> visibleTargets = new List<Transform>();

        ManoeuvreFPSController _controller;

        [HideInInspector]
        public Camera _mainCamera;

        public void Initialize(ManoeuvreFPSController _c)
        {
            _controller = _c;
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Search the visible Targets as soon as they enter in our Vision Angle
        /// </summary>
        public void SearchAttackTargets()
        {
            //We get all the colliders in our Range
            Collider[] targetsInViewRadius = Physics.OverlapSphere(_controller.transform.position , AttackDistance, hitMask);
            //Debug.Log("targetsInViewRadius : " + targetsInViewRadius.Length);

            //we loop through all the above registered colliders
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                //get the target
                Transform target = targetsInViewRadius[i].transform;
                //Debug.Log(target.name);

                //check is this target the one we need to attack with melee weapon
                bool isTarget = CheckAsTarget(target.tag);

                //if it's not
                if (!isTarget)
                    return; //exit

                //we only proceed if this target is not already added
                if (!visibleTargets.Contains(target))
                {
                    //we get the direction
                    Vector3 dirToTarget = (target.position - _controller.transform.position).normalized;

                    //if the angle between Target and the direction is less then the view angle
                    if (Vector3.Angle(_mainCamera.transform.forward, dirToTarget) < Angle / 2)
                    {
                        //we see the distance of this target
                        //from our position
                        float distToTarget = Vector3.Distance(_controller.transform.position, target.position);

                        //if there's no obstacle in between
                        if (!Physics.Raycast(_mainCamera.transform.position, dirToTarget, distToTarget, obstacleMask))
                        {
                            //we add it in our visible targets list
                            visibleTargets.Add(target);
                        }

                    }

                }
                
            }

            //Clear all previously added targets
            visibleTargets.Clear();
        }

        bool CheckAsTarget(string _targetTag)
        {
            //Debug.Log(_targetTag);
            for (int i = 0; i < hitTags.Count; i++)
            {
                if (hitTags[i] == _targetTag)
                {
                    return true;
                    
                }
            }

            return false;

        }

        /// <summary>
        /// This is only used in the editor to make the arc
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="isAngleGlobal"></param>
        /// <returns></returns>
        public Vector3 DirFromAngle(float angle, bool isAngleGlobal, Camera _mainCam)
        {

            if (_mainCam)
            {
                if (!isAngleGlobal)
                {
                    angle += _mainCam.transform.eulerAngles.y;
                }

                float retAngle = angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(retAngle), 0, Mathf.Cos(retAngle));

            }

            return Vector3.zero;
        }

    }

    [System.Serializable]
    public class MeleeAttackAnimationProperties
    {
        public Animation _weaponAnimation;
        public bool playRandom = true;
        [HideInInspector]
        public float currentClipLength = 0;
        [HideInInspector]
        public float currentAttackStartTime = 0;
        [HideInInspector]
        public AnimationClip currentClipPlayed;

        public List<AttackAnimationClassification> _MeleeAnimations = new List<AttackAnimationClassification>();

        [HideInInspector]
        public bool isAnimated;

        int lastPlayedAnimation = 0;
        WeaponMelee _weapon;

        public void Initialize(WeaponMelee _wm)
        {
            _weapon = _wm;

            if (_MeleeAnimations.Count > 0)
                isAnimated = true;
            else
                isAnimated = false;
        }

        /// <summary>
        /// Plays an attack Animation
        /// </summary>
        public void PlayAttackAnimation()
        {
            //make sure last animation is not being played currently
            if (_weaponAnimation.IsPlaying(_MeleeAnimations[lastPlayedAnimation].animationClip)) 
                return;

            //if we have to play random animation each time
            if(playRandom)
            {
                int i = 0;
                i = Random.Range(0, _MeleeAnimations.Count);

                lastPlayedAnimation = i;

                Play(lastPlayedAnimation);

            }
            //else we play each animation in sequence
            else
            {
                if(lastPlayedAnimation < _MeleeAnimations.Count - 1)
                {
                    Play(lastPlayedAnimation);

                    lastPlayedAnimation++;
                }
                else if(lastPlayedAnimation == _MeleeAnimations.Count-1)
                {
                    Play(lastPlayedAnimation);

                    lastPlayedAnimation = 0;
                }
            }
                
        }

        void Play(int index)
        {
            _weaponAnimation.CrossFadeQueued(_MeleeAnimations[index].animationClip, 0, QueueMode.PlayNow);

            //Play AudioClip
            _MeleeAnimations[index].PlayAttackSound(_weapon.transform);

            //set current attack start time
            currentAttackStartTime = _MeleeAnimations[index].attackStartTime;

            //set current clip
            currentClipPlayed = _weaponAnimation.GetClip(_MeleeAnimations[index].animationClip);

            //set clip length 
            currentClipLength = _weaponAnimation.GetClip(_MeleeAnimations[index].animationClip).length;

            //set attack Delay
            _weapon._MeleeAttack.attackDelay = currentClipLength;

        }

    }

    [System.Serializable]
    public class AttackAnimationClassification
    {
        public string animationClip;
        public AudioClip attackSound;
        public float attackStartTime = 0.15f;

        public void PlayAttackSound(Transform weaponTransform)
        {

            if (attackSound)
                AudioSource.PlayClipAtPoint(attackSound, weaponTransform.position);
            else
                Debug.Log("No audio clip assigned for the animation" + animationClip);
        }
    }

    [System.Serializable]
    public class MeleeHitParticleFX
    {
        public List<HitParticle> ParticleFX = new List<HitParticle>();

        /// <summary>
        /// We spawn particle inside our tag
        /// </summary>
        /// <param name="target"></param>
        public void SpawnParticleAtTarget(Transform target, string Tag)
        {
            //see inside all particles
            for(int i =0; i< ParticleFX.Count; i++)
            {
                //find the one we need
                if(ParticleFX[i].Tag == Tag)
                {
                    //spawn pfx
                    ParticleSystem _pfx = GameObject.Instantiate(ParticleFX[i]._particle) as ParticleSystem;

                    //orient it correctly
                    _pfx.transform.SetParent(target);
                    _pfx.transform.localPosition = Vector3.zero;
                    _pfx.transform.localEulerAngles = Vector3.zero;

                    //play
                    _pfx.Play();
                }
            }
        }
    }

    [System.Serializable]
    public class HitParticle
    {
        public string Tag;
        public ParticleSystem _particle;

    }
}