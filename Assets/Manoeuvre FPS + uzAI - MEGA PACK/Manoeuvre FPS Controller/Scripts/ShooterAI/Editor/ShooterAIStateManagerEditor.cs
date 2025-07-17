using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Manoeuvre
{
    [CustomEditor(typeof(ShooterAIStateManager))]
    public class ShooterAIStateManagerEditor : Editor
    {
        ShooterAIStateManager _stateManager;

        SerializedObject _SO_AI;
        SerializedProperty OnDeath;

        private void OnEnable()
        {
            _stateManager = (ShooterAIStateManager)target;

            _SO_AI = new SerializedObject(_stateManager);
            OnDeath = _SO_AI.FindProperty("OnDeath");
        }

        public override void OnInspectorGUI()
        {

            DrawNewInspector();

            //draw on Death event
            EditorGUILayout.PropertyField(OnDeath);
            _SO_AI.ApplyModifiedProperties();

            //DrawDefaultInspector();
        }

         void DrawNewInspector()
        {
            EditorGUI.BeginChangeCheck();

            Texture t = Resources.Load("EditorContent/ShooterAI-icon") as Texture;

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("AI Type", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.HelpBox("Instead of changing the AI Type here, create one of desired Type from Wizard.", MessageType.Info);
            AIType _AIType = (AIType)EditorGUILayout.EnumPopup("AI Type", _stateManager._AIType);
            ManoeuvreFPSController Player = _stateManager.Player;
            if(_AIType == AIType.Companion)
            {
                Player = (ManoeuvreFPSController)EditorGUILayout.ObjectField("Player", _stateManager.Player, typeof(ManoeuvreFPSController));

                if (!Player)
                    EditorGUILayout.HelpBox("No Player Assigned!!! Please Assign Player here!!!", MessageType.Error);
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("AI State (Read Only)", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.HelpBox("This will be updated at Runtime to whatever the Shooter AI is currently doing.", MessageType.Info);
            ShooterAIStates currentShooterState = (ShooterAIStates)EditorGUILayout.EnumPopup("Current Shooter State", _stateManager.currentShooterState);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("AI Walk Animation (Read Only)", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.HelpBox("This will be updated at Runtime to Shooter AI Animator's Vertical (float) Parameter of Locomotion Blend Tree.", MessageType.Info);
            float walkAnimation = EditorGUILayout.FloatField("Walk Animation", _stateManager.walkAnimation);

            EditorGUILayout.EndVertical();

            DrawTabs();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI State");
                _stateManager._AIType = _AIType;
                _stateManager.Player = Player;
            }
        }

        void DrawTabs()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawTabCount1();
            DrawTabCount2();
            DrawTabCount3();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawSelectedTabsInspector();
        }

        void DrawTabCount1()
        {
            if (_stateManager._AIType == AIType.Companion) 
                _stateManager.tabCount1 = GUILayout.Toolbar(_stateManager.tabCount1, new string[] { "Companion", "Idle", "Chase" });
            else
                _stateManager.tabCount1 = GUILayout.Toolbar(_stateManager.tabCount1, new string[] { "Idle", "Patrol", "Chase" });

            switch (_stateManager.tabCount1)
            {
                case 00:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount3 = 55;
                    if (_stateManager._AIType == AIType.Companion)
                        _stateManager.tabName = "Companion";
                    else
                        _stateManager.tabName = "Idle";

                    break;
                case 01:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount3 = 55;
                    if (_stateManager._AIType == AIType.Companion)
                        _stateManager.tabName = "Idle";
                    else
                        _stateManager.tabName = "Patrol";

                    break;
                case 02:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount3 = 55;
                    _stateManager.tabName = "Chase";

                    break;
               
            }
        }

        void DrawTabCount2()
        {
            _stateManager.tabCount2 = GUILayout.Toolbar(_stateManager.tabCount2, new string[] { "Sight", "Attack", "Weapon" });

            switch (_stateManager.tabCount2)
            {
                case 00:
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabCount3 = 55;
                    _stateManager.tabName = "Sight";
                    break;
                case 01:
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabCount3 = 55;
                    _stateManager.tabName = "Attack";
                    break;
                case 02:
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabCount3 = 55;
                    _stateManager.tabName = "Weapon";
                    break;

            }
        }

        void DrawTabCount3()
        {
            _stateManager.tabCount3 = GUILayout.Toolbar(_stateManager.tabCount3, new string[] {"Aim IK", "Health", "Gizmos" });

            switch (_stateManager.tabCount3)
            {
                case 00:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabName = "Aim IK";
                    break;
                case 01:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabName = "Health";
                    break;
                case 02:
                    _stateManager.tabCount2 = 55;
                    _stateManager.tabCount1 = 55;
                    _stateManager.tabName = "Gizmos";
                    break;
                
            }
        }

        void DrawSelectedTabsInspector()
        {
            switch (_stateManager.tabName)
            {
                case "Companion":
                    DrawCompanionBehaviour();
                    break;

                case "Idle":
                    DrawIdleBehaviour();
                    break;

                case "Patrol":
                    DrawPatrolBehaviour();
                    break;

                case "Chase":
                    DrawChaseBehaviour();
                    break;

                case "Sight":
                    DrawSightBehaviour();
                    break;

			    case "Attack":
					DrawAttackBehaviour();
                    break;

                case "Weapon":
					DrawWeaponsBehaviour();
                    break;

                case "Aim IK":
                    DrawAimIKBehaviour();
                    break;

                case "Health":
                    DrawHealthBehaviour();
                    break;

                case "Gizmos":
                    DrawGizmosBehaviour();
                    break;

            }
        }

        void DrawCompanionBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Companion Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Stopping Distance from Player. ", EditorStyles.helpBox);
            float PlayerDistance = EditorGUILayout.FloatField("Player Distance", _stateManager.CompanionBehaviour.PlayerDistance);

            EditorGUILayout.LabelField("Locomotion Blend Tree Threshold Value for Follow Animation. ", EditorStyles.helpBox);
            float FollowAnimation = EditorGUILayout.FloatField("Follow Animation", _stateManager.CompanionBehaviour.FollowAnimation);

            EditorGUILayout.LabelField("If true, Player can cause Damage to Companion. ", EditorStyles.helpBox);
            bool AllowDamageFromPlayer = EditorGUILayout.Toggle("Allow Damage From Player", _stateManager.CompanionBehaviour.AllowDamageFromPlayer);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Companion");

                _stateManager.CompanionBehaviour.PlayerDistance = PlayerDistance;
                _stateManager.CompanionBehaviour.FollowAnimation = FollowAnimation;
                _stateManager.CompanionBehaviour.AllowDamageFromPlayer = AllowDamageFromPlayer;

            }
        }

        void DrawIdleBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Idle Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("How fast Transit from Current State to Idle State.", EditorStyles.helpBox);
            float idleTransitionDuration = EditorGUILayout.Slider("Idle Transition Duration", _stateManager.IdleBehaviour.idleTransitionDuration, 0.1f, 10f);

            EditorGUILayout.LabelField("Idle Animation value from Locomotion Blend Tree.", EditorStyles.helpBox);
            float idleAnimation = EditorGUILayout.FloatField("Idle Animation", _stateManager.IdleBehaviour.idleAnimation);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Idle");

                _stateManager.IdleBehaviour.idleTransitionDuration = idleTransitionDuration;
                _stateManager.IdleBehaviour.idleAnimation = idleAnimation;

            }
        }

        void DrawPatrolBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Patrol Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Patrol Animation value from Locomotion Blend Tree.", EditorStyles.helpBox);
            float PatrolAnimation = EditorGUILayout.FloatField("Patrol Animation", _stateManager.PatrolBehaviour.PatrolAnimation);

            EditorGUILayout.LabelField("Delay between Patrol Points.", EditorStyles.helpBox);
            float PatrolDelay = EditorGUILayout.FloatField("Patrol Delay", _stateManager.PatrolBehaviour.PatrolDelay);

            EditorGUILayout.EndVertical();

            DrawPatrolPathList();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Patrol");

                _stateManager.PatrolBehaviour.PatrolAnimation = PatrolAnimation;
                _stateManager.PatrolBehaviour.PatrolDelay = PatrolDelay;

            }
        }

        void DrawPatrolPathList()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Total Patrol Points : " + _stateManager.PatrolBehaviour.PatrolPath.Count, EditorStyles.centeredGreyMiniLabel);


            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                GameObject g = new GameObject();
                _stateManager.PatrolBehaviour.PatrolPath.Add(g.transform);

                DestroyImmediate(g);
            }

            if (GUILayout.Button("Clear"))
            {
                _stateManager.PatrolBehaviour.PatrolPath.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (_stateManager.PatrolBehaviour.PatrolPath.Count == 0)
            {
                EditorGUILayout.HelpBox("No Patrol Points in the List. AI will remain in Idle State!", MessageType.Warning);
            }

            for (int i =0; i< _stateManager.PatrolBehaviour.PatrolPath.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("Helpbox");
                Transform PatrolPath = (Transform)EditorGUILayout.ObjectField( _stateManager.PatrolBehaviour.PatrolPath[i], typeof(Transform));
                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _stateManager.PatrolBehaviour.PatrolPath.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Patrol Path List");

                    _stateManager.PatrolBehaviour.PatrolPath[i] = PatrolPath;
                }
            }

            EditorGUILayout.EndVertical();
        }

        void DrawChaseBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Chase Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Tag which AI is currently Chasing (Read Only).", EditorStyles.helpBox);
            string currentChasingTargetTag = EditorGUILayout.TextField("Current Chasing Target Tag", _stateManager.ChaseBehaviour.currentChasingTargetTag);

            EditorGUILayout.LabelField("Position of Tag which we are chasing (Read Only) .", EditorStyles.helpBox);
            Transform targetPosition = (Transform)EditorGUILayout.ObjectField("Target Position", _stateManager.ChaseBehaviour.targetPosition, typeof(Transform));

            EditorGUILayout.LabelField("Last Position of Player (Only for Enemy Type Shooter AI).", EditorStyles.helpBox);
            Transform lastPlayerPosition = (Transform)EditorGUILayout.ObjectField("Last Player Position", _stateManager.ChaseBehaviour.lastPlayerPosition, typeof(Transform));

            EditorGUILayout.LabelField("Chase Animation value from Locomotion Blend Tree.", EditorStyles.helpBox);
            float chaseAnimation = EditorGUILayout.FloatField("Chase Animation", _stateManager.ChaseBehaviour.chaseAnimation);

            EditorGUILayout.LabelField("If true, AI will look at Target while Chasing.", EditorStyles.helpBox);
            bool useHeadTrack = EditorGUILayout.Toggle("Use Head Track", _stateManager.ChaseBehaviour.useHeadTrack);

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Chase");

                _stateManager.ChaseBehaviour.chaseAnimation = chaseAnimation;
                _stateManager.ChaseBehaviour.useHeadTrack = useHeadTrack;

            }
        }

        void DrawSightBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Sight Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("How far this AI can See.", EditorStyles.helpBox);
			float Range = EditorGUILayout.Slider("Range", _stateManager.SightBehaviour.Range, 1f, 50f);

            EditorGUILayout.LabelField("Field Of View of Sight.", EditorStyles.helpBox);
            float Angle = EditorGUILayout.Slider("Angle", _stateManager.SightBehaviour.Angle, 0,360);

            EditorGUILayout.LabelField("How fast AI will Search for targets in FOV. Increase this Iteration Time for better performance on Mobile devices!", EditorStyles.helpBox);
            float SearchIterationTime = EditorGUILayout.FloatField("Search Iteration Time", _stateManager.SightBehaviour.SearchIterationTime);

            EditorGUILayout.LabelField("Targets' Layer Mask which this AI can See.", EditorStyles.helpBox);
            LayerMask targetMask = LayerMaskUtility.LayerMaskField("Target Mask", _stateManager.SightBehaviour.targetMask);

            EditorGUILayout.LabelField("Obstacles' Layer Mask which will block AI Sight.", EditorStyles.helpBox);
            LayerMask obstacleMask = LayerMaskUtility.LayerMaskField("Target Mask", _stateManager.SightBehaviour.obstacleMask);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Sight");

                _stateManager.SightBehaviour.Range = Range;
                _stateManager.SightBehaviour.Angle = Angle;
                _stateManager.SightBehaviour.SearchIterationTime = SearchIterationTime;
                _stateManager.SightBehaviour.targetMask = targetMask;
                _stateManager.SightBehaviour.obstacleMask = obstacleMask;

            }
        }

		void DrawAttackBehaviour (){
		
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginVertical("Box");

			EditorGUILayout.LabelField("Attack Behaviour", EditorStyles.centeredGreyMiniLabel);

			EditorGUILayout.BeginVertical("helpBox");

			EditorGUILayout.HelpBox("Player, Zombie, Shooter AI and Turret which this AI will be attacking. This will be updated at Runtimne and are Read Only properties.", MessageType.Info);
			ManoeuvreFPSController Player = (ManoeuvreFPSController)EditorGUILayout.ObjectField ("Player", _stateManager.AttackBehaviour.Player, typeof(ManoeuvreFPSController));
			uzAI.uzAIZombieStateManager Zombie = (uzAI.uzAIZombieStateManager)EditorGUILayout.ObjectField ("Zombie", _stateManager.AttackBehaviour.Zombie, typeof(uzAI.uzAIZombieStateManager));
			ShooterAIStateManager ShooterAI = (ShooterAIStateManager)EditorGUILayout.ObjectField ("Shooter AI", _stateManager.AttackBehaviour.ShooterAI, typeof(ShooterAIStateManager));
			Turret Turret = (Turret)EditorGUILayout.ObjectField ("Turret", _stateManager.AttackBehaviour.Turret, typeof(Turret));
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField ("From how far this AI can Shoot.", EditorStyles.helpBox);
			float AttackDistance = EditorGUILayout.Slider ("Attack Distance", _stateManager.AttackBehaviour.AttackDistance, 1, 50);

			EditorGUILayout.LabelField ("Attack FOV Angle.", EditorStyles.helpBox);
			float Angle = EditorGUILayout.Slider ("Angle", _stateManager.AttackBehaviour.Angle, 0, 360);

			EditorGUILayout.LabelField ("How fast this AI will search for the Targets to Attack.", EditorStyles.helpBox);
			float SearchIterationTime = EditorGUILayout.FloatField ("Search Iteration Time", _stateManager.AttackBehaviour.SearchIterationTime);
			SearchIterationTime = Mathf.Clamp (SearchIterationTime, 0, SearchIterationTime);

			EditorGUILayout.LabelField ("Delay between each Fire.", EditorStyles.helpBox);
			float FireDelay = EditorGUILayout.FloatField ("Fire Delay", _stateManager.AttackBehaviour.FireDelay);
			FireDelay = Mathf.Clamp (FireDelay, 0, FireDelay);

			EditorGUILayout.LabelField ("Target's Layer(s) which this AI can Shoot.", EditorStyles.helpBox);
			LayerMask targetMask = LayerMaskUtility.LayerMaskField ("Target Mask", _stateManager.AttackBehaviour.targetMask);

			EditorGUILayout.LabelField ("Target's Layer(s) which this AI can Not Shoot.", EditorStyles.helpBox);
			LayerMask obstacleMask = LayerMaskUtility.LayerMaskField ("Obstacle Mask", _stateManager.AttackBehaviour.obstacleMask);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Shooter AI Attack");
				_stateManager.AttackBehaviour.AttackDistance = AttackDistance;
				_stateManager.AttackBehaviour.Angle = Angle;
				_stateManager.AttackBehaviour.SearchIterationTime = SearchIterationTime;
				_stateManager.AttackBehaviour.FireDelay = FireDelay;
				_stateManager.AttackBehaviour.targetMask = targetMask;
				_stateManager.AttackBehaviour.obstacleMask = obstacleMask;

			}
		
		}

		void DrawWeaponsBehaviour(){
			
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginVertical("Box");

			EditorGUILayout.LabelField("Weapon Behaviour", EditorStyles.centeredGreyMiniLabel);

			EditorGUILayout.LabelField("Weapon object which this AI is holding. This will be Auto Assigned here when you create a Weapon for this AI from the Wizard.", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            Transform weaponObject = (Transform)EditorGUILayout.ObjectField ("Weapon Object", _stateManager.WeaponBehaviour.weaponObject, typeof(Transform));

            if (_stateManager.WeaponBehaviour.weaponObject) {
                if (GUILayout.Button("Select", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                {
                    Selection.activeGameObject = _stateManager.WeaponBehaviour.weaponObject.gameObject;
                }
            }
            
            EditorGUILayout.EndHorizontal();

            if(!_stateManager.WeaponBehaviour.weaponObject)
            {
                EditorGUILayout.HelpBox("Please Create A Weapon from Shooter AI Wizard",MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Muzzle Flash location, this is the child of Weapon Object.", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            Transform muzzleLocation = (Transform)EditorGUILayout.ObjectField ("Muzzle Location", _stateManager.WeaponBehaviour.muzzleLocation, typeof(Transform));

            if (_stateManager.WeaponBehaviour.muzzleLocation)
            {
                if (GUILayout.Button("Select", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                {
                    Selection.activeGameObject = _stateManager.WeaponBehaviour.muzzleLocation.gameObject;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Muzzle Flash to be emitted while shooting.", EditorStyles.helpBox);
			ParticleSystem muzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField ("Muzzle Flash", _stateManager.WeaponBehaviour.muzzleFlash, typeof(ParticleSystem));

			EditorGUILayout.LabelField("Ammo / Clip Count of the Weapon.", EditorStyles.helpBox);
			int Ammo = EditorGUILayout.IntField ("Ammo", _stateManager.WeaponBehaviour.Ammo);
			Ammo = Mathf.Clamp (Ammo, 01, Ammo);

			EditorGUILayout.LabelField("Fire Sound FX.", EditorStyles.helpBox);
			AudioClip FireSound = (AudioClip)EditorGUILayout.ObjectField ("Fire Sound", _stateManager.WeaponBehaviour.FireSound, typeof(AudioClip));

			EditorGUILayout.LabelField("Reload Sound FX.", EditorStyles.helpBox);
			AudioClip ReloadSound = (AudioClip)EditorGUILayout.ObjectField ("Reload Sound", _stateManager.WeaponBehaviour.ReloadSound, typeof(AudioClip));

			EditorGUILayout.LabelField("Max Damage this Weapon can give.", EditorStyles.helpBox);
			int maxDamage = EditorGUILayout.IntSlider ("Max Damage", _stateManager.WeaponBehaviour.maxDamage,1,50);

			EditorGUILayout.LabelField("Min Damage this Weapon can give.", EditorStyles.helpBox);
			int minDamage = EditorGUILayout.IntSlider ("Min Damage", _stateManager.WeaponBehaviour.minDamage,1,50);

			EditorGUILayout.EndVertical();

            DrawWeaponHitProperties();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Shooter AI Weapopn Behaviour");
				_stateManager.WeaponBehaviour.weaponObject = weaponObject;
				_stateManager.WeaponBehaviour.muzzleLocation = muzzleLocation;
				_stateManager.WeaponBehaviour.muzzleFlash = muzzleFlash;
				_stateManager.WeaponBehaviour.Ammo = Ammo;
				_stateManager.WeaponBehaviour.FireSound = FireSound;
				_stateManager.WeaponBehaviour.ReloadSound = ReloadSound;
				_stateManager.WeaponBehaviour.maxDamage = maxDamage;
				_stateManager.WeaponBehaviour.minDamage = minDamage;
			}
		}

        void DrawWeaponHitProperties()
        {

            EditorGUILayout.HelpBox("When Shooter AI will shoot below defined tags, corresponding Hit Effect and Hit Sound will be heard.", MessageType.Info);

            for(int i = 0; i< _stateManager.WeaponBehaviour.HitParticle.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField(_stateManager.WeaponBehaviour.HitParticle[i].hitTag, EditorStyles.centeredGreyMiniLabel);

                EditorGUILayout.Space();

                GameObject hitParticle = (GameObject)EditorGUILayout.ObjectField("Hit Particle", _stateManager.WeaponBehaviour.HitParticle[i].hitParticle, typeof(GameObject));

                AudioClip hitAudioClip = (AudioClip)EditorGUILayout.ObjectField("Hit Audio Clip", _stateManager.WeaponBehaviour.HitParticle[i].hitAudioClip[0], typeof(AudioClip));

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Shooter AI Weapon Hit Properties");

                    _stateManager.WeaponBehaviour.HitParticle[i].hitParticle = hitParticle;
                    _stateManager.WeaponBehaviour.HitParticle[i].hitAudioClip[0] = hitAudioClip;
                }
            }

            
        }

        void DrawAimIKBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Aim IK Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.HelpBox("If true, AI Will enter debug mode, hence, he won't be causing any damage to any object.", MessageType.Info);

            bool DebugAimIK = EditorGUILayout.Toggle("Debug Aim IK", _stateManager.AimIK.DebugAimIK);

            EditorGUILayout.BeginVertical("Helpbox");

            EditorGUILayout.LabelField("Spine bone IK Transform which will be generated at Runtime.", EditorStyles.helpBox);
            Transform SpineTransform = EditorGUILayout.ObjectField("Spine Transform", _stateManager.AimIK.SpineTransform, typeof(Transform)) as Transform;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");
            //if we are playing game
            if (EditorApplication.isPlaying)
            {
                if (_stateManager.AimIK.DebugAimIK)
                {
                    if (GUILayout.Button("COPY Spine IK offsets"))
                    {
                        //COPY POS 
                        PlayerPrefs.SetFloat("Aim Spine Offset_X", _stateManager.AimIK.AimSpineOffset_X);
                        PlayerPrefs.SetFloat("Aim Spine Offset_Y", _stateManager.AimIK.AimSpineOffset_Y);
                        PlayerPrefs.SetFloat("Aim Spine Offset_Z", _stateManager.AimIK.AimSpineOffset_Z);
                    }
                }
                
            }
            else
            {
                if (GUILayout.Button("PASTE Spine IK offsets"))
                {
                    //PASTE POS
                    _stateManager.AimIK.AimSpineOffset_X = PlayerPrefs.GetFloat("Aim Spine Offset_X");
                    _stateManager.AimIK.AimSpineOffset_Y = PlayerPrefs.GetFloat("Aim Spine Offset_Y");
                    _stateManager.AimIK.AimSpineOffset_Z = PlayerPrefs.GetFloat("Aim Spine Offset_Z");

                }
            }

            
            float AimSpineOffset_X = EditorGUILayout.FloatField("Aim Spine Offset_X", _stateManager.AimIK.AimSpineOffset_X);
            float AimSpineOffset_Y = EditorGUILayout.FloatField("Aim Spine Offset_Y", _stateManager.AimIK.AimSpineOffset_Y);
            float AimSpineOffset_Z = EditorGUILayout.FloatField("Aim Spine Offset_Z", _stateManager.AimIK.AimSpineOffset_Z);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Left hand IK. Sets the position and rotation of Left Hand.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            Transform LeftHandIK = EditorGUILayout.ObjectField("Left Hand IK", _stateManager.AimIK.LeftHandIK, typeof(Transform)) as Transform;

            if(GUILayout.Button("Select", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                Selection.activeGameObject = _stateManager.AimIK.LeftHandIK.gameObject;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.HelpBox("Left hand Aim IK will only come into play if AI is Aiming OR Shooting.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            Transform LeftHandAimIK = EditorGUILayout.ObjectField("Left Hand Aim IK", _stateManager.AimIK.LeftHandAimIK, typeof(Transform)) as Transform;

            if (GUILayout.Button("Select", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                Selection.activeGameObject = _stateManager.AimIK.LeftHandAimIK.gameObject;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Aim IK");
                _stateManager.AimIK.DebugAimIK = DebugAimIK;
                _stateManager.AimIK.AimSpineOffset_X = AimSpineOffset_X;
                _stateManager.AimIK.AimSpineOffset_Y = AimSpineOffset_Y;
                _stateManager.AimIK.AimSpineOffset_Z = AimSpineOffset_Z;
                _stateManager.AimIK.LeftHandIK = LeftHandIK;
                _stateManager.AimIK.LeftHandAimIK = LeftHandAimIK;

            }
        }

        void DrawHealthBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Health Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Shooter AI Total Health.", EditorStyles.helpBox);
            int Health = EditorGUILayout.IntSlider("Health", _stateManager.Health.Health, 1, 200);

            EditorGUILayout.LabelField("How many Hit Reactions are Available in the Animator.", EditorStyles.helpBox);
            int hitReactionsAvailable = EditorGUILayout.IntField("Available Hit Reactions", _stateManager.Health.hitReactionsAvailable);

            EditorGUILayout.LabelField("Out of all the present Death Animations, which is to be used from the Animator.", EditorStyles.helpBox);
            int DeathID = EditorGUILayout.IntField("Death ID", _stateManager.Health.DeathID);

            EditorGUILayout.LabelField("After being Hit, how long wait before going to next State.", EditorStyles.helpBox);
            float _cooldownTimer = EditorGUILayout.Slider("Cooldown Timer", _stateManager.Health._cooldownTimer, 0.1f, 5f);

            EditorGUILayout.LabelField("After being Hit, AI will look at the object which caused him Damage.", EditorStyles.helpBox);
            bool lookAtCameraOnHit = EditorGUILayout.Toggle("Look At On Hit", _stateManager.Health.lookAtCameraOnHit);

            EditorGUILayout.EndVertical();

            DrawFadeMeshProperties();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Health");

                _stateManager.Health.Health = Health;
                _stateManager.Health.hitReactionsAvailable = hitReactionsAvailable;
                _stateManager.Health.DeathID = DeathID;
                _stateManager.Health._cooldownTimer = _cooldownTimer;
                _stateManager.Health.lookAtCameraOnHit = lookAtCameraOnHit;
            }
        }

        void DrawFadeMeshProperties()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Cool Fade mesh effect to fade the mesh of AI (BETA).", EditorStyles.helpBox);
            bool FadeMesh = EditorGUILayout.Toggle("Fade Mesh", _stateManager.Health.FadeMesh);

            Material faderMaterial = _stateManager.Health.faderMaterial;
            float fadeDelay = _stateManager.Health.fadeDelay;
            float fadeDuration = _stateManager.Health.fadeDuration;

            if (FadeMesh) {

                EditorGUILayout.LabelField("Fader Material whose properties will be swapped with the mesh renderer's material Properties", EditorStyles.helpBox);
                faderMaterial = EditorGUILayout.ObjectField("Fader Material", _stateManager.Health.faderMaterial, typeof(Material)) as Material;

                EditorGUILayout.LabelField("How long wait before fading the Mesh.", EditorStyles.helpBox);
                fadeDelay = EditorGUILayout.Slider("Fade Delay", _stateManager.Health.fadeDelay, 0.1f, 10f);

                EditorGUILayout.LabelField("Total Fade Duration", EditorStyles.helpBox);
                fadeDuration = EditorGUILayout.Slider("Fade Duration", _stateManager.Health.fadeDuration, 0.1f, 10f);

                DrawFadeMeshRenderersList();
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Fade Mesh Properties");
                _stateManager.Health.FadeMesh = FadeMesh;
                _stateManager.Health.faderMaterial = faderMaterial;
                _stateManager.Health.fadeDelay = fadeDelay;
                _stateManager.Health.fadeDuration = fadeDuration;
            }
        }

        void DrawFadeMeshRenderersList()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                _stateManager.Health.allRenderers.Add(new Renderer());
            }

            if (GUILayout.Button("Clear"))
            {
                _stateManager.Health.allRenderers.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (_stateManager.Health.allRenderers.Count == 0)
            {
                EditorGUILayout.HelpBox("Please Add the mesh renderers of the Shooter AI Model", MessageType.Warning);
            }

            for (int i =0; i < _stateManager.Health.allRenderers.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                Renderer r = EditorGUILayout.ObjectField(_stateManager.Health.allRenderers[i], typeof(Renderer)) as Renderer;

                if(GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _stateManager.Health.allRenderers.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Shoooter AI Fade Mesh Renderers");

                    _stateManager.Health.allRenderers[i] = r;
                }
            }

        }

        void DrawGizmosBehaviour()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Gizmos Behaviour", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.LabelField("Draw the nav mesh path from the agent pos to current target position.", EditorStyles.helpBox);
            bool drawPathToCurrentTarget = EditorGUILayout.Toggle("Draw Path", _stateManager.DrawGizmosBehaviour.drawPathToCurrentTarget);
            Color pathGizmoColor = _stateManager.DrawGizmosBehaviour.pathGizmoColor;
            if (drawPathToCurrentTarget)
                pathGizmoColor = EditorGUILayout.ColorField("Path Color", _stateManager.DrawGizmosBehaviour.pathGizmoColor);

            EditorGUILayout.LabelField("Draw the line from the agent pos to current target position.", EditorStyles.helpBox);
            bool drawLineToCurrentTarget = EditorGUILayout.Toggle("Draw Path", _stateManager.DrawGizmosBehaviour.drawLineToCurrentTarget);
            Color lineGizmoColor = _stateManager.DrawGizmosBehaviour.lineGizmoColor;
            if (drawLineToCurrentTarget)
                lineGizmoColor = EditorGUILayout.ColorField("Path Color", _stateManager.DrawGizmosBehaviour.lineGizmoColor);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shooter AI Gizmos");
                _stateManager.DrawGizmosBehaviour.drawPathToCurrentTarget = drawPathToCurrentTarget;
                _stateManager.DrawGizmosBehaviour.pathGizmoColor = pathGizmoColor;
                _stateManager.DrawGizmosBehaviour.drawLineToCurrentTarget = drawLineToCurrentTarget;
                _stateManager.DrawGizmosBehaviour.lineGizmoColor = lineGizmoColor;
            }
        }

        #region SCENE GUI

        private void OnSceneGUI()
        {

            DrawSightGizmos();

            DrawAttackGizmos();
        }

        void DrawSightGizmos()
        {
            ShooterAISightBehaviour fov = _stateManager.SightBehaviour;

            if (fov._stateManager == null)
                fov._stateManager = _stateManager;

            Handles.color = Color.cyan;
            Handles.DrawWireArc(_stateManager.transform.position, Vector3.up, Vector3.forward, 360, fov.Range);

            Color c = Color.cyan;
            c.a = 0.35f;
            Handles.color = c;
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, fov.Angle / 2, fov.Range);
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, -fov.Angle / 2, fov.Range);

            Vector3 AngleA = fov.DirFromAngle(-fov.Angle / 2, false);
            Vector3 AngleB = fov.DirFromAngle(fov.Angle / 2, false);

            Handles.color = Color.blue;
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleA * fov.Range);
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleB * fov.Range);

            foreach (Transform t in fov.visibleTargets)
            {
                Handles.DrawLine(_stateManager.transform.position, t.position);
            }
        }

        void DrawAttackGizmos()
        {
            ShooterAIAttackBehaviour attack = _stateManager.AttackBehaviour;

            if (attack._stateManager == null)
                attack._stateManager = _stateManager;

            Handles.color = Color.red;
            Handles.DrawWireArc(_stateManager.transform.position, Vector3.up, Vector3.forward, 360, attack.AttackDistance);

            Color c = Color.red;
            c.a = 0.35f;
            Handles.color = c;
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, attack.Angle / 2, attack.AttackDistance);
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, -attack.Angle / 2, attack.AttackDistance);

            Vector3 AngleA = attack.DirFromAngle(-attack.Angle / 2, false);
            Vector3 AngleB = attack.DirFromAngle(attack.Angle / 2, false);

            Handles.color = Color.green;
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleA * attack.AttackDistance);
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleB * attack.AttackDistance);

            foreach (Transform t in attack.visibleTargets)
            {
                Handles.DrawLine(_stateManager.transform.position, t.position);
            }
        }

        #endregion
    }
}