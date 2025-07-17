using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public enum AnimationType
    {
        Procedural,
        Animation
    }

    public enum WeaponType
    {
        Shooter,
        Melee,
        Throwable,
        ThrowableItem
    }

    public class WeaponProceduralManoeuvre : MonoBehaviour
    {
        public WeaponType weaponType = WeaponType.Shooter;

        public bool DebugTransform = false;

        [Header("Procedural Weapon Sway")]
        public WeaponSway _weaponSway;

        [Header("Procedural Weapon Bobbing")]
        public UniversalBob _weaponBob;
        public AnimationType _weaponBobType = AnimationType.Procedural;
        public AnimatedBob _animatedBob;

        [Header("Procedural Weapon Reloading")]
        public ProceduralReload _weaponReload;

        [Header("Procedural Weapon Equipment Sway")]
        public ProceduralEquipSway _equipSway;

        //Editor Var
        [HideInInspector]
        public int TabCount;

        // Use this for initialization
        public void Start()
        {
            _weaponBob.Initialize();

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (DebugTransform)
                return; //exit

            // if(weaponType == WeaponType.Melee && !_equipSway.isEquipSway)
            WeaponBob(gc_StateManager.Instance.currentPlayerState);

        }

        /// <summary>
        /// Do weapon bobbing 
        /// </summary>
        void WeaponBob(PlayerStates state)
        {
            switch (_weaponBobType)
            {
                //Procedural weapon bobbing
                case AnimationType.Procedural:
                    ProceduralWeaponBob(state);
                    break;

                //Animated weapon bobbing
                case AnimationType.Animation:
                    AnimatedWeaponBob(state);
                    break;
            }
           
        }

        void ProceduralWeaponBob(PlayerStates state)
        {
            if (GetComponent<WeaponShooter>())
            {
                if (!GetComponent<WeaponShooter>().ironSightProperties.canTween)
                    return;

            }

            //perform weapon sway
            Vector3 weaponPos = _weaponSway.Sway(transform);


            //return if the weapon bob is disabled
            if (!_weaponBob.enableBobbing)
                return;

            //loop through all the bob states
            foreach (BobState bob in _weaponBob.bobStates)
            {
                //identify the correct bob state
                if (bob.headBobStateName == state.ToString())
                    //now do bobbing
                    transform.localPosition = weaponPos + _weaponBob.Offset(bob.speed, bob);

            }

            //make special case for idle since the controller speed at idle is 0
            if (state == PlayerStates.Idle)
            {
                //loop through all the bob states
                foreach (BobState bob in _weaponBob.bobStates)
                {
                    //identify the correct bob state
                    if (bob.headBobStateName == state.ToString())
                        //now do bobbing
                        transform.localPosition = weaponPos + _weaponBob.Offset(1, bob);
                }

            }
        }

        void AnimatedWeaponBob(PlayerStates state)
        {
            if (_animatedBob.WeaponAnimation == null)
                return;

            ////we don't play any animation if firing or ironsight
            if (GetComponent<WeaponShooter>())
            {
                if (GetComponent<WeaponShooter>().weaponState == WeaponState.Firing || GetComponent<WeaponShooter>().weaponState == WeaponState.Reloading)
                {
                    return;
                }

                if (GetComponentInParent<ManoeuvreFPSController>().Inputs.ironsightInput)
                {

                    _animatedBob.WeaponAnimation.Play(_animatedBob.IdleAnimation);
                    return;
                }

            }

            ///we don't play animation if we are doing a melee attack
            if (GetComponent<WeaponMelee>())
            {
                if (!GetComponent<WeaponMelee>()._MeleeAttack.canAttack)
                    return;

            }

            _weaponSway.Sway(transform);

            switch (state)
            {
                //IDLE
                case PlayerStates.Idle:
                    if (string.IsNullOrEmpty(_animatedBob.IdleAnimation))
                        return;

                    //set speed
                    _animatedBob.WeaponAnimation[_animatedBob.IdleAnimation].speed = _animatedBob.IdleAnimationSpeed;

                    //play animation
                    _animatedBob.WeaponAnimation.CrossFade(_animatedBob.IdleAnimation);

                    break;

                //WALKING
                case PlayerStates.Walking:
                    if (string.IsNullOrEmpty(_animatedBob.WalkAnimation))
                        return;

                    //set speed
                    _animatedBob.WeaponAnimation[_animatedBob.WalkAnimation].speed = _animatedBob.WalkAnimationSpeed;

                    //play animation
                    _animatedBob.WeaponAnimation.CrossFade(_animatedBob.WalkAnimation);

                    break;

                //RUNNING
                case PlayerStates.Running:
                    if (string.IsNullOrEmpty(_animatedBob.RunAnimation))
                        return;

                    //set speed
                    _animatedBob.WeaponAnimation[_animatedBob.RunAnimation].speed = _animatedBob.RunAnimationSpeed;

                    //play animation
                    _animatedBob.WeaponAnimation.CrossFade(_animatedBob.RunAnimation);

                    break;

                //CROUCHING
                case PlayerStates.Crouching:
                    if (string.IsNullOrEmpty(_animatedBob.CrouchAnimation))
                        return;

                    //set speed
                    _animatedBob.WeaponAnimation[_animatedBob.CrouchAnimation].speed = _animatedBob.CrouchAnimationSpeed;

                    //play animation
                    _animatedBob.WeaponAnimation.CrossFade(_animatedBob.CrouchAnimation);

                    break;
            }
        }

        public void StartProceduralReloadManoeuvre(Transform weaponTransform)
        {
            //exit if it is a melee weapon
            if (weaponType == WeaponType.Melee)
                return;

            switch (_weaponReload._ReloadType)
            {
                case AnimationType.Procedural:
                    StartCoroutine(_weaponReload.StartProceduralReloading(weaponTransform, _weaponSway));
                    break;

                case AnimationType.Animation:
                    StartCoroutine(_weaponReload.StartAnimationReloading(weaponTransform));
                    break;
            }

        }

    }

    [System.Serializable]
    public class AnimatedBob
    {
        public Animation WeaponAnimation;

        public string IdleAnimation;
        public float IdleAnimationSpeed;

        public string WalkAnimation;
        public float WalkAnimationSpeed;

        public string RunAnimation;
        public float RunAnimationSpeed;

        public string CrouchAnimation;
        public float CrouchAnimationSpeed;

    }

    [System.Serializable]
    public class WeaponSway
    {
        public string MouseX = "Mouse X";
        public string MouseY = "Mouse Y";

        [Range(1f, 5f)]
        public float moveAmount = 3f;
        [Range(1f, 5f)]
        public float moveSpeed = 2f;

        public float moveX;
        public float moveY;
        public Vector3 defPos;
        public Vector3 newPos;

        /// <summary>
        /// Sway the weapon transform passed into this method!
        /// </summary>
        /// <param name="weaponTransform"></param>
        public Vector3 Sway(Transform weaponTransform)
        {
            //if we are not using touch camera input
            if(TouchCameraInput.Instance == null)
            {
                moveX = Input.GetAxis(MouseX) * moveAmount * Time.deltaTime;
                moveY = Input.GetAxis(MouseY) * moveAmount * Time.deltaTime;

            }
            else
            {
                //we need much lesser move amount in mobile devices
                moveX = TouchCameraInput.Instance.InputVector.y * (moveAmount/10) * Time.deltaTime;
                moveY = TouchCameraInput.Instance.InputVector.x * (moveAmount/10) * Time.deltaTime;
            }

            newPos = new Vector3(defPos.x + moveX, defPos.y + moveY, defPos.z);

            weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, newPos, Time.deltaTime * moveSpeed);

            return weaponTransform.localPosition;

        }

    }

    [System.Serializable]
    public class ProceduralReload
    {
        [Tooltip("Do you want to use the Procedural Reload Manoeuvre?")]
        public AnimationType _ReloadType = AnimationType.Procedural;
        [HideInInspector]
        public bool currentlyReloading = false;

        [Header("Position Offset while Reloading")]
        [Tooltip("Reload Position Offset - Gun will move to this Position while Reloading")]
        public Vector3 ReloadPositionOffset = new Vector3(-0.075f, -0.1f, -0.1f);

        [Header("Rotation Offset while Reloading")]
        [Tooltip("Reload Rotation Offset - Gun will rotate to this Rotation while Reloading")]
        public Vector3 ReloadRotationOffset = new Vector3(-45, 0, 30);

        [Range(0.1f, 1f)]
        public float reloadDuration = 0.75f;

        public Animation weaponAnimation;
        public string reloadAnimationName = "Reload";

        public IEnumerator StartProceduralReloading(Transform weaponTransform, WeaponSway _weaponSway)
        {
            //enable flag in Ammo Manager
            gc_AmmoManager.Instance.isReloading = true;

            Vector3 weaponPos = _weaponSway.Sway(weaponTransform);

            //Vector3 defPos = weaponTransform.localPosition;
            Quaternion defRot = weaponTransform.localRotation;

            Quaternion rotOffset = Quaternion.Euler(ReloadRotationOffset);

            currentlyReloading = true;

            float t1 = 0;

            Vector3 localPos = weaponTransform.localPosition;
            Quaternion localRot = weaponTransform.localRotation;

            //Get the gun TO POSITION Offset
            while (t1 < reloadDuration)
            {
                weaponTransform.localPosition = Vector3.Lerp(localPos, weaponPos + ReloadPositionOffset,
                                                t1 / reloadDuration);
                t1 += Time.deltaTime;

                yield return null;
            }

            //reset time
            float t2 = 0;

            //Get the gun TO ROTATION Offset
            while (t2 < (reloadDuration / 2))
            {
                weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, defRot * rotOffset,
                                                t2 / (reloadDuration / 2));
                t2 += Time.deltaTime;

                yield return null;
            }

            yield return new WaitForSeconds(reloadDuration / 2);

            //reset time
            float t3 = 0;

            //Get the gun FROM ROTATION Offset
            while (t3 < (reloadDuration / 2))
            {
                weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, localRot,
                                                t3 / (reloadDuration / 2));
                t3 += Time.deltaTime;

                yield return null;
            }

            //reset time
            float t4 = 0;

            localPos = weaponTransform.localPosition;
            
            //Get the gun FROM POSITION Offset
            while (t4 < (reloadDuration / 2))
            {
                weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, weaponPos,
                                                t4 / (reloadDuration / 2));
                t4 += Time.deltaTime;

                yield return null;
            }

            currentlyReloading = false;

            //disable flag in Ammo Manager
            gc_AmmoManager.Instance.isReloading = false;
        }

        public IEnumerator StartAnimationReloading(Transform weaponTransform)
        {
            //enable flag in Ammo Manager
            gc_AmmoManager.Instance.isReloading = true;

            currentlyReloading = true;

            //Play Animation
            weaponAnimation.CrossFade(reloadAnimationName, 0.1f);

            yield return new WaitForSeconds(weaponAnimation.GetClip(reloadAnimationName).length);

            currentlyReloading = false;

            //disable flag in Ammo Manager
            gc_AmmoManager.Instance.isReloading = false;

        }

    }

    [System.Serializable]
    public class ProceduralEquipSway
    {
        [Header("Position Offset while Un-Equipping")]
        [Tooltip("Equip Position Offset - Gun will move to this Position while Equipping")]
        public Vector3 equipPositionOffset;

        [Header("Position Offset while Un-Equipping")]
        [Tooltip("Equip Position Offset - Gun will move to this Position while Equipping")]
        public Vector3 equipRotationOffset;

        [Range(0.1f, 1f)]
        public float equipDuration = 1f;

        [HideInInspector]
        public bool isEquipSway = false;

        [HideInInspector]
        public AudioClip equipSound;
        [HideInInspector]
        public WeaponSounds weaponSoundProperties;

        Vector3 _OffsetPosition;
        Quaternion _OffsetRotation;

        public Vector3 myLocalPos;
        public Quaternion myLocalRot;

        public Vector3 GetOffsetPositionForInit(Vector3 WeaponTransform)
        {
            if(_OffsetPosition != Vector3.zero)
                return _OffsetPosition;

            myLocalPos = WeaponTransform;
            _OffsetPosition = WeaponTransform + equipPositionOffset;

            return _OffsetPosition;
        }

        public Quaternion GetOffsetRotationForInit(Vector3 WeaponTransform)
        {
            if (_OffsetRotation.eulerAngles != Vector3.zero)
                return _OffsetRotation;

            myLocalRot = Quaternion.Euler(WeaponTransform);
            _OffsetRotation = Quaternion.Euler(WeaponTransform) * Quaternion.Euler(equipRotationOffset);

            return _OffsetRotation;
        }

        public IEnumerator Un_EquipSwayCoroutine(Transform weaponTransform, WeaponSway _weaponSway)
        {
            //play equip sound
            if (equipSound)
                weaponSoundProperties.PlayAudio(equipSound, true, Vector3.zero);

            //enable equipping flag in ammo manager
            gc_AmmoManager.Instance.isEquipping = true;

            Vector3 weaponPos = weaponTransform.localPosition;
            //Vector3 weaponPos = _weaponSway.Sway(weaponTransform);

            Quaternion defRot = weaponTransform.localRotation;

            Quaternion rotOffset = Quaternion.Euler(equipRotationOffset);

            isEquipSway = true;

            float et = 0;

            //both position and rotation will be lerped together here
            //unlike procedural reload
            while(et < equipDuration)
            {
                weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, weaponPos + equipPositionOffset,
                                               et / equipDuration);

                weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, defRot * rotOffset,
                                             et / equipDuration);

                et += Time.deltaTime;

                yield return null;
            }

            isEquipSway = false;

            //disable equipping flag in ammo manager
            gc_AmmoManager.Instance.isEquipping = false;

            weaponTransform.transform.localPosition = weaponPos;

            weaponTransform.gameObject.SetActive(false);

        }

        public IEnumerator EquipSwayCoroutine(Vector3 _weaponPos, Vector3 _weaponRot, Transform weaponTransform)
        {

            //enable transform
            weaponTransform.gameObject.SetActive(true);

            //play equip sound
            if (equipSound)
                weaponSoundProperties.PlayAudio(equipSound, true, Vector3.zero);

            //enable sway flag
            isEquipSway = true;

            //enable equipping flag in ammo manager
            gc_AmmoManager.Instance.isEquipping = true;

            //TIMER
            float et = 0;

            //both position and rotation will be lerped together here
            //unlike procedural reload
            while (et < equipDuration)
            {
                //lerp pos
                weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, myLocalPos,
                                               et / equipDuration);

                //lerp rot
                weaponTransform.localRotation = Quaternion.Lerp(weaponTransform.localRotation, myLocalRot,
                                             et / equipDuration);

                et += Time.deltaTime;

                yield return null;
            }

            isEquipSway = false;

            //disable equipping flag in ammo manager
            gc_AmmoManager.Instance.isEquipping = false;

           
        }

    }
}