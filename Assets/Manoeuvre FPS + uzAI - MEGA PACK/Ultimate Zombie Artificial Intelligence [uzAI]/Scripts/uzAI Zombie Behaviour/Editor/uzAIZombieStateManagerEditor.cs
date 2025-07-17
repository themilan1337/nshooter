using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uzAI
{
    [CustomEditor(typeof(uzAIZombieStateManager))]
    [CanEditMultipleObjects]
    public class uzAIZombieStateManagerEditor : Editor
    {

        uzAIZombieStateManager _stateManager;
        //bool openCloseToggle;

        bool hintsToggle;

        private void OnEnable()
        {
            _stateManager = (uzAIZombieStateManager)target;

        }

        public override void OnInspectorGUI()
        {
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            // texture
            Texture t = (Texture)Resources.Load("EditorContent/uzAI-icon");

            GUILayout.Box(t, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            Texture t1 = (Texture)(hintsToggle ? Resources.Load("EditorContent/hints_c-icon") : Resources.Load("EditorContent/hints_bw-icon"));
            hintsToggle = GUILayout.Toggle(hintsToggle, t1, "Button");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawZombieState();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //string openCloseToggleVal = openCloseToggle ? "Collapse" : "Expand";

            //openCloseToggle = GUILayout.Toggle(openCloseToggle, openCloseToggleVal, EditorStyles.toolbarButton);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //if (!openCloseToggle)
            //    return;

            //1. Locomotion
            DrawLocomotionProperties();

            //2. Idle Behaviour
            DrawIdleProperties();

            //3. Patrol Behaviour
            DrawPatrolProperties();

            //4. Enemy Sight Behaviour
            DrawEnemySightProperties();

            //5. Chase Behaviour
            DrawChaseProperties();

            //6. Attack Behaviour
            DrawAttackProperties();

            //7. Zombie Eating
            DrawEatingProperties();

            //. Offmesh 
            DrawOffmeshProperties();

            //9. Zombie Health 
            DrawHealthProperties();

            //10. Zombie SFX
            DrawZombieSFXProperties();

            //11. Gizmos
            DrawZombiePathGizmos();

            //DrawDefaultInspector();

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
        }

        void DrawZombieState()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.LabelField("Read Only", EditorStyles.centeredGreyMiniLabel);

            if (hintsToggle)
                EditorGUILayout.HelpBox("This fetches and shows the Current Zombie State from the State Machine.\n" +
                    "Currently there are following Behaviours, there will be added more based on User Requests : \n" +
                    "> Idle \n" +
                    "> Patrol \n" +
                    "> Chase \n" +
                    "> Hit \n" +
                    "> Attack \n" +
                    "> Die", MessageType.Info);

            _stateManager.currentZombieState = (ZombieStates)EditorGUILayout.EnumPopup("Zombie State", _stateManager.currentZombieState);

            EditorGUILayout.EndVertical();
        }

        void DrawLocomotionProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.locomotionToggle ? Resources.Load("EditorContent/locomotion_c-icon") : Resources.Load("EditorContent/locomotion_bw-icon"));
            bool locomotionToggle = GUILayout.Toggle(_stateManager.locomotionToggle, t1, "Button");

            float walkAnimation = _stateManager.Locomotion.walkAnimation;
            bool mirrorLocomotion = _stateManager.Locomotion.mirrorLocomotion;
            bool useTurnOnSpot = _stateManager.Locomotion.useTurnOnSpot;
            float PatrolAngleThreshold = _stateManager.Locomotion.PatrolAngleThreshold;
            float ChaseAngleThreshold = _stateManager.Locomotion.ChaseAngleThreshold;

            if (locomotionToggle)
            {
                EditorGUILayout.BeginVertical("box");

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Walk Animation is for debug purposes. This actually is the 'Verical' value in the Animator at which zombie will be moving. \n" +
                        "Please see the Animator > Locomotion and see their the Threshold Value. \n" +
                        "In this way you can add as many motion fields as you want for walk animations and just simply set the Threshold Value.", MessageType.Info);

                walkAnimation = EditorGUILayout.FloatField("Walk Animation", _stateManager.Locomotion.walkAnimation);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Mirror Locomotion is very simple yet awesome feature. \n" +
                    "With this bool checked whole walk animation will be mirrored. \n" +
                    "Now you can check this bool in one zombie and uncheck in another. \n " +
                    "Hence, even with the exactly same walk animation, 2 zombies will have different looking walk cycle.", MessageType.Info);

                mirrorLocomotion = EditorGUILayout.Toggle("Mirror Locomotion", _stateManager.Locomotion.mirrorLocomotion);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Zombie will use the Turning Animations whenever the Turn Angle is > then Angle Threshold", MessageType.Info);

                useTurnOnSpot = EditorGUILayout.Toggle("Use TurnOnSpot", _stateManager.Locomotion.useTurnOnSpot);

                if(useTurnOnSpot)
                {
                    if (hintsToggle)
                        EditorGUILayout.HelpBox("Angle Threshold is the Minimum Angle Zombie can move without using Turning Animations while Patrolling. ", MessageType.Info);

                    PatrolAngleThreshold = EditorGUILayout.Slider("Patrol Angle Threshold", _stateManager.Locomotion.PatrolAngleThreshold, 0, 180);

                    if (hintsToggle)
                        EditorGUILayout.HelpBox("Angle Threshold is the Minimum Angle Zombie can move without using Turning Animations while Chasing. ", MessageType.Info);

                    ChaseAngleThreshold = EditorGUILayout.Slider("Chase Angle Threshold", _stateManager.Locomotion.ChaseAngleThreshold, 0, 180);
                }

                //clamp vals
                walkAnimation = Mathf.Clamp(walkAnimation, 0, walkAnimation);
                EditorGUILayout.EndVertical();

            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzAILocomotion");

                _stateManager.locomotionToggle = locomotionToggle;

                _stateManager.Locomotion.walkAnimation = walkAnimation;
                _stateManager.Locomotion.mirrorLocomotion = mirrorLocomotion;
                _stateManager.Locomotion.useTurnOnSpot = useTurnOnSpot;
                _stateManager.Locomotion.PatrolAngleThreshold = PatrolAngleThreshold;
                _stateManager.Locomotion.ChaseAngleThreshold = ChaseAngleThreshold;

            }
        }

        void DrawIdleProperties()
        {

            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.idleToggle ? Resources.Load("EditorContent/idle_c-icon") : Resources.Load("EditorContent/idle_bw-icon"));
            bool idleToggle = GUILayout.Toggle(_stateManager.idleToggle, t1, "Button");

            float idleTransitionDuration = _stateManager.idleBehaviour.idleTransitionDuration;
            float idleAnimation = _stateManager.idleBehaviour.idleAnimation;

            if (idleToggle)
            {
                EditorGUILayout.BeginVertical("box");

                if (hintsToggle)
                    EditorGUILayout.HelpBox("The longer the idle transition duration is, the longer zombie will take from current 'Vertical' value i.e walk speed to the 0 i.e the Idle Speed. ", MessageType.Info);

                idleTransitionDuration = EditorGUILayout.Slider("Idle Transition Duration", _stateManager.idleBehaviour.idleTransitionDuration, 0.1f, 10f);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Please see the Animator > Locomotion Blend Tree, there is a separate Idle Animation's Blend Tree at 0 motion field.\n" +
                        "Whatever value you will set here in Idle Animation below, that will be the value of the Animator's 'IdleAnimation' parameter. \n" +
                        "In this way, you can have infinite motion fields for Idle Animation and in every zombie just change this value below and all will have respective Idle Animation Playing.\n" +
                        "Hence, NO NEED to create separate Animators for each zombie just to have different animation. ", MessageType.Info);

                idleAnimation = EditorGUILayout.FloatField("Idle Animation", _stateManager.idleBehaviour.idleAnimation);

                //clamp vals
                idleAnimation = Mathf.Clamp(idleAnimation, 0, idleAnimation);

                EditorGUILayout.EndVertical();

            }


            if (EditorGUI.EndChangeCheck())
            {

                Undo.RecordObject(target, "uzAI Idle");

                _stateManager.idleToggle = idleToggle;
                _stateManager.idleBehaviour.idleTransitionDuration = idleTransitionDuration;
                _stateManager.idleBehaviour.idleAnimation = idleAnimation;

            }

        }

        void DrawPatrolProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.patrolToggle ? Resources.Load("EditorContent/patrol_c-icon") : Resources.Load("EditorContent/patrol_bw-icon"));
            bool patrolToggle = GUILayout.Toggle(_stateManager.patrolToggle, t1, "Button");

            WaypointsPath PatrolPath = _stateManager.patrolBehaviour.PatrolPath;
            float PatrolAnimation = _stateManager.patrolBehaviour.PatrolAnimation;
            float PatrolDelay = _stateManager.patrolBehaviour.PatrolDelay;

            if (patrolToggle)
            {
                EditorGUILayout.BeginVertical("box");

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Drag n Drop the Waypoints Path that you have created from the uzAI Wizard in this field if it's not already. \n" +
                        "This Waypoint route will dictate the movement of the zombie if it' NOT chasing a target.", MessageType.Info);

                PatrolPath = (WaypointsPath)EditorGUILayout.ObjectField("Patrol Path", _stateManager.patrolBehaviour.PatrolPath, typeof(WaypointsPath));

                if (PatrolPath == null)
                    EditorGUILayout.HelpBox("No Path Added in the field. Please Add one if you want this zombie to Patrol.\n" +
                        "If there will none added, it will remain stationery and only move when a target comes in its sight.", MessageType.Error);
                else
                {
                    if (PatrolPath.waypoints.Count < 1)
                        EditorGUILayout.HelpBox("No Waypoints in the Patrol Path you have selected .", MessageType.Error);
                    else
                        EditorGUILayout.LabelField("Total Waypoints : " + PatrolPath.waypoints.Count, EditorStyles.centeredGreyMiniLabel);

                    if (PatrolPath.waypoints.Count > 0)
                    {

                        if (hintsToggle)
                            EditorGUILayout.HelpBox("Similar to the Idle Animation above, " +
                                "the 'Vertical' float parameter of the Animator will be lerped to the below Patrol Animation value and will play the " +
                                "respective Walk animation for the Patrol Behaviour. \n" +
                                "In this way, you can add infinite walk animations in a SINGLE ANIMATOR and just simply change the Patrol Animation value below " +
                                "and all the zombies will have different Animation Set. \n" +
                                "Hence, NO NEED to have multiple animators for multiple animations!", MessageType.Info);

                        PatrolAnimation = EditorGUILayout.FloatField("Patrol Animation", _stateManager.patrolBehaviour.PatrolAnimation);

                        if (hintsToggle)
                            EditorGUILayout.HelpBox("Delay in moving from current waypoint to the next waypoint.", MessageType.Info);

                        PatrolDelay = EditorGUILayout.Slider("Patrol Delay", _stateManager.patrolBehaviour.PatrolDelay, 0.1f, 10f);


                        PatrolAnimation = Mathf.Clamp(PatrolAnimation, 0, PatrolAnimation);
                    }

                }

                EditorGUILayout.EndVertical();

            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzAI Patrol");
                _stateManager.patrolToggle = patrolToggle;

                _stateManager.patrolBehaviour.PatrolPath = PatrolPath;
                _stateManager.patrolBehaviour.PatrolAnimation = PatrolAnimation;
                _stateManager.patrolBehaviour.PatrolDelay = PatrolDelay;
            }
        }

        void DrawEnemySightProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.sightToggle ? Resources.Load("EditorContent/sight_c-icon") : Resources.Load("EditorContent/sight_bw-icon"));
            bool sightToggle = GUILayout.Toggle(_stateManager.sightToggle, t1, "Button");

            float Range = _stateManager.sightBehaviour.Range;
            float Angle = _stateManager.sightBehaviour.Angle;
            float SearchIterationTime = _stateManager.sightBehaviour.SearchIterationTime;
            LayerMask targetMask = _stateManager.sightBehaviour.targetMask;
            LayerMask obstacleMask = _stateManager.sightBehaviour.obstacleMask;

            if (sightToggle)
            {
                EditorGUILayout.BeginVertical("Box");
                if (hintsToggle)
                    EditorGUILayout.HelpBox("Yellow circle determines how far this Zombie can see.", MessageType.Info);

                Range = EditorGUILayout.Slider("Sight Range", _stateManager.sightBehaviour.Range, 1, 25);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Yellow Arc determines the angle / Field of View of Zombie's sight.", MessageType.Info);

                Angle = EditorGUILayout.Slider("Sight Angle", _stateManager.sightBehaviour.Angle, 0, 360);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Zombie will be searching for the targets in every : " + SearchIterationTime + " seconds.\n" +
                        "Which means, lowering this duration will result in faster iterations and hence, more realistic behaviour but costs performance and vice versa. \n" +
                        "If you are targetting mobile devices, increase this search iteration time for low performance.", MessageType.Info);

                SearchIterationTime = EditorGUILayout.Slider("Search Iteration Time", _stateManager.sightBehaviour.SearchIterationTime, 0.01f, 2f);

                EditorGUILayout.Space();

                if (hintsToggle)
                    EditorGUILayout.HelpBox("All the layermasks which zombie can see. We have a specific layermask named 'Target' for this purpose only.", MessageType.Info);

                targetMask = LayerMaskUtility.LayerMaskField("Target Mask", _stateManager.sightBehaviour.targetMask);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("All the layermasks which zombie can't see through. We have a specific layermask named 'Obstacle' for this purpose only.", MessageType.Info);

                obstacleMask = LayerMaskUtility.LayerMaskField("Obstacle Mask", _stateManager.sightBehaviour.obstacleMask);

                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


                DrawEnemySightTargetList();


            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai Sight");

                _stateManager.sightToggle = sightToggle;

                _stateManager.sightBehaviour.Range = Range;
                _stateManager.sightBehaviour.Angle = Angle;
                _stateManager.sightBehaviour.SearchIterationTime = SearchIterationTime;
                _stateManager.sightBehaviour.targetMask = targetMask;
                _stateManager.sightBehaviour.obstacleMask = obstacleMask;
            }
        }

        void DrawEnemySightTargetList()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Define the Targets to Detect .", MessageType.Info);

            EditorGUILayout.LabelField("Total Targets : " + _stateManager.sightBehaviour.targetsToDetect.Count, EditorStyles.centeredGreyMiniLabel);

            if (hintsToggle)
                EditorGUILayout.HelpBox("Add Button will add a new Target Tag in the List.\n" +
                    "Clear Button will REMOVE ALL the present Target Tags from the List.", MessageType.Info);

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                _stateManager.sightBehaviour.targetsToDetect.Add(new TargetsToDetect());
            }

            if (GUILayout.Button("Clear"))
            {
                _stateManager.sightBehaviour.targetsToDetect.Clear();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (hintsToggle)
            {
                EditorGUILayout.HelpBox("Target Tag : It is the tag which zombie will start chasing as soon as it comes in above defined Sight Range and Sight Angle.", MessageType.Info);
                EditorGUILayout.HelpBox("Detection Priority : Zombie will chase this Target Tag only if the Current Target tag which zombie is chasing (if any) has LOWER " +
                    "Detection Priority than this Target Tag.\n" +
                    "This is just a basic Priority check which will run in whenever zombie encounters a new target in its range and Angle.", MessageType.Info);

            }

            for (int i = 0; i < _stateManager.sightBehaviour.targetsToDetect.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                if (i > 0)
                    EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal("Box");
                string targetTag = EditorGUILayout.TextField("Target Tag", _stateManager.sightBehaviour.targetsToDetect[i].targetTag);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                {
                    _stateManager.sightBehaviour.targetsToDetect.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                int targetDetectionPriority = EditorGUILayout.IntSlider("Detection Priority", _stateManager.sightBehaviour.targetsToDetect[i].targetDetectionPriority, 1, 100);

                EditorGUILayout.EndVertical();


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "uzai sight target list");
                    _stateManager.sightBehaviour.targetsToDetect[i].targetTag = targetTag;
                    _stateManager.sightBehaviour.targetsToDetect[i].targetDetectionPriority = targetDetectionPriority;
                }
            }

            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai sight targets");

            }
        }

        void DrawChaseProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.chaseToggle ? Resources.Load("EditorContent/chase_c-icon") : Resources.Load("EditorContent/chase_bw-icon"));
            bool chaseToggle = GUILayout.Toggle(_stateManager.chaseToggle, t1, "Button");

            Transform targetPosition = _stateManager.chaseBehaviour.targetPosition;
            float chaseAnimation = _stateManager.chaseBehaviour.chaseAnimation;
            bool useHeadTrack = _stateManager.chaseBehaviour.useHeadTrack;
            Transform lastPlayerPosition = _stateManager.chaseBehaviour.lastPlayerPosition;

            if (chaseToggle)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Read Only Properties", EditorStyles.centeredGreyMiniLabel);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("This property set is dedicated to Read Only properties. These include : \n\n" +
                        "Chasing Tag : This property represents the tag which zombie is currently chasing. \n\n" +
                        "Target Position : This property is the exact position of the Transform which zombie is chasing. \n\n" +
                        "Last Player Position : This property is exclusively for Player Transform. Once the player is spotted, the Last Player Position Trigger is moved to that position. \n" +
                        "Once the player reaches out of sight, this trigger will stay at the position of the player where it was last seen by zombie. \n" +
                        "This LPP creates a very realistic effect when player runs behind a wall or runs out of sight in any other way.", MessageType.Info);

                EditorGUILayout.TextField("Chasing Tag", _stateManager.chaseBehaviour.currentChasingTargetTag);
                targetPosition = (Transform)EditorGUILayout.ObjectField("Target Position", _stateManager.chaseBehaviour.targetPosition, typeof(Transform));
                lastPlayerPosition = (Transform)EditorGUILayout.ObjectField("Last Player Position", _stateManager.chaseBehaviour.lastPlayerPosition, typeof(Transform));

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                if (hintsToggle)
                    EditorGUILayout.HelpBox("Chase Animation is exactly similar to Idle Animation and Patrol Animation that you" +
                        " have defined above. \n" +
                        "This again, simply determines the value of 'Vertical' float parameter of the Locomotion Blend Tree. \n" +
                        "In this way you can have infinite animations for single behaviour of chasing and just simply assign each zombie different chase animation value and they all will be having different animations BUT same Animator Controller.", MessageType.Info);

                chaseAnimation = EditorGUILayout.FloatField("Chase Animation", _stateManager.chaseBehaviour.chaseAnimation);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Use Head Track IK is the toggle whether Zombie looks at the target while chasing or not.", MessageType.Info);

                useHeadTrack = EditorGUILayout.Toggle("Use Head Track IK", _stateManager.chaseBehaviour.useHeadTrack);

                //clamp vals
                chaseAnimation = Mathf.Clamp(chaseAnimation, 0, chaseAnimation);
                EditorGUILayout.EndVertical();

            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai chase");

                _stateManager.chaseToggle = chaseToggle;

                _stateManager.chaseBehaviour.chaseAnimation = chaseAnimation;
                _stateManager.chaseBehaviour.useHeadTrack = useHeadTrack;
            }
        }

        void DrawAttackProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.attackToggle ? Resources.Load("EditorContent/attack_c-icon") : Resources.Load("EditorContent/attack_bw-icon"));
            bool attackToggle = GUILayout.Toggle(_stateManager.attackToggle, t1, "Button");

            Manoeuvre.ManoeuvreFPSController Player = _stateManager.attackBehaviour.Player;
            Manoeuvre.DynamicBarricades Barricade = _stateManager.attackBehaviour.Barricade;
            float AttackDistance = _stateManager.attackBehaviour.AttackDistance;
            float Angle = _stateManager.attackBehaviour.Angle;
            float SearchIterationTime = _stateManager.attackBehaviour.SearchIterationTime;
            LayerMask targetMask = _stateManager.attackBehaviour.targetMask;
            LayerMask obstacleMask = _stateManager.attackBehaviour.obstacleMask;
            float AttackDelay = _stateManager.attackBehaviour.AttackDelay;
            int maxDamage = _stateManager.attackBehaviour.maxDamage;
            int minDamage = _stateManager.attackBehaviour.minDamage;

            if (attackToggle)
            {
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Auto Assigned", EditorStyles.centeredGreyMiniLabel);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("This is the Manoeuvre FPS Object Field. \n" +
                        "As soon as it comes in Attack Distance (defined below) of the zombie, it's reference will be taken here and damage (also define below) will be given to him while attacking.", MessageType.Info);
                Player = (Manoeuvre.ManoeuvreFPSController)EditorGUILayout.ObjectField("Player", _stateManager.attackBehaviour.Player, typeof(Manoeuvre.ManoeuvreFPSController));

                if (hintsToggle)
                    EditorGUILayout.HelpBox("The Dynamic Barricade which needs to be destroyed in order to move forward.", MessageType.Info);
                Barricade = (Manoeuvre.DynamicBarricades)EditorGUILayout.ObjectField("Barricade", _stateManager.attackBehaviour.Barricade, typeof(Manoeuvre.DynamicBarricades));


                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");


                if (hintsToggle)
                    EditorGUILayout.HelpBox("Similar to the Sight Range in Enemy Sight Properties, Attack Distance is the RED Circle around the zombie and it determines from how far zombie can Attack. ", MessageType.Info);
                AttackDistance = EditorGUILayout.Slider("Attack Distance", _stateManager.attackBehaviour.AttackDistance, 1.5f, 5f);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("The Attack Angle is the Red Arc around the Zombie. If the 'Player' is inside this Arc / Angle then only Zombie can attack it otherwise, zombie will keep on chasing.", MessageType.Info);
                Angle = EditorGUILayout.Slider("Attack Angle", _stateManager.attackBehaviour.Angle, 0f, 360f);

                EditorGUILayout.Space();

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Zombie will be searching for the targets to ATTACK in every : " + SearchIterationTime + " seconds.\n" +
                        "Which means, lowering this duration will result in faster iterations and hence, more realistic behaviour but costs performance and vice versa. \n" +
                        "If you are targetting mobile devices, increase this search iteration time for low performance.", MessageType.Info);
                SearchIterationTime = EditorGUILayout.Slider("Search Iteration Time", _stateManager.attackBehaviour.SearchIterationTime, 0.01f, 2f);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("The Attack Delay is the delay between 2 consecutive attacks. If your Attack animation is too fast, increase this delay value.", MessageType.Info);

                AttackDelay = EditorGUILayout.Slider("Attack Delay", _stateManager.attackBehaviour.AttackDelay, 0f, 2f);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("A random value between Max and Min Damage will be given to the Player.", MessageType.Info);

                maxDamage = EditorGUILayout.IntSlider("Max Damage", _stateManager.attackBehaviour.maxDamage, 0, 25);
                minDamage = EditorGUILayout.IntSlider("Min Damage", _stateManager.attackBehaviour.minDamage, 0, 25);

                EditorGUILayout.Space();

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Layer mask of the Player target, by default it is Target.", MessageType.Info);

                targetMask = LayerMaskUtility.LayerMaskField("Target Mask", _stateManager.attackBehaviour.targetMask);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Layer mask of the Obstacles to avoid, by default it is Obstacles.", MessageType.Info);

                obstacleMask = LayerMaskUtility.LayerMaskField("Obstacle Mask", _stateManager.attackBehaviour.obstacleMask);

                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai attack");

                _stateManager.attackToggle = attackToggle;

                _stateManager.attackBehaviour.Player = Player;
                _stateManager.attackBehaviour.AttackDistance = AttackDistance;
                _stateManager.attackBehaviour.Angle = Angle;
                _stateManager.attackBehaviour.SearchIterationTime = SearchIterationTime;
                _stateManager.attackBehaviour.targetMask = targetMask;
                _stateManager.attackBehaviour.obstacleMask = obstacleMask;
                _stateManager.attackBehaviour.AttackDelay = AttackDelay;
                _stateManager.attackBehaviour.maxDamage = maxDamage;
                _stateManager.attackBehaviour.minDamage = minDamage;
            }
        }

        void DrawEatingProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.eatingToggle ? Resources.Load("EditorContent/eating_c-icon") : Resources.Load("EditorContent/eating_bw-icon"));
            bool eatingToggle = GUILayout.Toggle(_stateManager.eatingToggle, t1, "Button");

            Transform currentFoodSource = _stateManager.eatingBehaviour.currentFoodSource;
            string FoodTag = _stateManager.eatingBehaviour.FoodTag;
            ParticleSystem EatingPfx = _stateManager.eatingBehaviour.EatingPfx;
            float Hunger = _stateManager.eatingBehaviour.Hunger;
            float currentHunger = _stateManager.eatingBehaviour.currentHunger;
            float depletionRate = _stateManager.eatingBehaviour.depletionRate;
            float replenishRate = _stateManager.eatingBehaviour.replenishRate;
            float bellyFilledDuration = _stateManager.eatingBehaviour.bellyFilledDuration;

            if (eatingToggle)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Read Only", EditorStyles.centeredGreyMiniLabel);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("This is the Current Food Source Transform postion which will be updated as soon as Zombie sees a Food.", MessageType.Info);

                currentFoodSource = EditorGUILayout.ObjectField("Current Food Source", _stateManager.eatingBehaviour.currentFoodSource, typeof(Transform)) as Transform;
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");

                if (hintsToggle)
                    EditorGUILayout.HelpBox("The Tag with which all the Food Sources will be tagged. By default it is 'Food'.", MessageType.Info);
                FoodTag = EditorGUILayout.TextField("Food Tag", _stateManager.eatingBehaviour.FoodTag);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("This Particle FX will be emitted from the 'Mouth' of the Zombie while he is eating. Blood Pfx or a similar one will look great.", MessageType.Info);
                EatingPfx = EditorGUILayout.ObjectField("Eating Pfx", _stateManager.eatingBehaviour.EatingPfx, typeof(ParticleSystem)) as ParticleSystem;

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");


                if (hintsToggle)
                    EditorGUILayout.HelpBox("Overall Hunger is the Total Capacity of how much this Zombie can eat.", MessageType.Info);
                Hunger = EditorGUILayout.Slider("Overall Hunger", _stateManager.eatingBehaviour.Hunger, 0, 100);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Current Hunger shows currently how much belly is filled of the Zombie.", MessageType.Info);
                currentHunger = EditorGUILayout.Slider("Current Hunger", _stateManager.eatingBehaviour.currentHunger, 0, 100);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("How fast Zombie becomes Hungry again when his Belly Filled Duration has been reached.", MessageType.Info);
                depletionRate = EditorGUILayout.Slider("Depletion Rate", _stateManager.eatingBehaviour.depletionRate, 0, 10);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("How fast Zombie Feeds from the Food Source one he starts eating i.e quickly filling belly and moving on..", MessageType.Info);
                replenishRate = EditorGUILayout.Slider("Replenish Rate", _stateManager.eatingBehaviour.replenishRate, 0, 10);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Duration for how long Zombie won't feel any hunger once Belly is full, i.e Current Hunger = Overall Hunger, and hence, ignore all food sources.", MessageType.Info);
                bellyFilledDuration = EditorGUILayout.Slider("Belly Filled Duration", _stateManager.eatingBehaviour.bellyFilledDuration, 0, 100);

                EditorGUILayout.EndVertical();

            }

            if (EditorGUI.EndChangeCheck()){

                Undo.RecordObject(target, "Zombie Eating Behaviour");

                _stateManager.eatingToggle = eatingToggle;

                _stateManager.eatingBehaviour.FoodTag = FoodTag;
                _stateManager.eatingBehaviour.EatingPfx = EatingPfx;
                _stateManager.eatingBehaviour.Hunger = Hunger;
                _stateManager.eatingBehaviour.currentHunger = currentHunger;
                _stateManager.eatingBehaviour.depletionRate = depletionRate;
                _stateManager.eatingBehaviour.replenishRate = replenishRate;
                _stateManager.eatingBehaviour.bellyFilledDuration = bellyFilledDuration;

            }

        }

        void DrawOffmeshProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.offmeshToggle ? Resources.Load("EditorContent/offmesh_c-icon") : Resources.Load("EditorContent/offmesh_bw-icon"));
            bool offmeshToggle = GUILayout.Toggle(_stateManager.offmeshToggle, t1, "Button");

            if (offmeshToggle)
            {
                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Total Offmesh Areas : " + _stateManager.offmeshBehaviour.OffMeshAreas.Count, EditorStyles.centeredGreyMiniLabel);

                DrawOffmeshAreasList();

                EditorGUILayout.EndVertical();
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zombie Offmesh");
                _stateManager.offmeshToggle = offmeshToggle;

            }
        }

        void DrawOffmeshAreasList()
        {

            EditorGUILayout.BeginVertical("Helpbox");

            EditorGUILayout.BeginHorizontal();

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Add"))
            {
                _stateManager.offmeshBehaviour.OffMeshAreas.Add(new OffmeshLinksAreas());
            }

            if (GUILayout.Button("Clear"))
            {
                _stateManager.offmeshBehaviour.OffMeshAreas.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (hintsToggle)
            {
                EditorGUILayout.HelpBox("Animator Bool : It is the exact boolean Parameter in the Animator which will enable the animation which has to be played on this off mesh Link. \n" +
                    "It is also the exact same name which is written on the Offmesh Link Identifier.", MessageType.Info);
                EditorGUILayout.HelpBox("Offmesh Curve : It is the Curve which the Zombie will follow.", MessageType.Info);
                EditorGUILayout.HelpBox("Transit Duration : Total Duration of this Offmesh Link Traversal.", MessageType.Info);

            }

            for (int i =0; i< _stateManager.offmeshBehaviour.OffMeshAreas.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.BeginHorizontal();
                string AnimatorBool = EditorGUILayout.TextField("Animator Bool", _stateManager.offmeshBehaviour.OffMeshAreas[i].AnimatorBool);

                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(35)))
                {
                    _stateManager.offmeshBehaviour.OffMeshAreas.RemoveAt(i);
                    break;
                } 
                EditorGUILayout.EndHorizontal();

                AnimationCurve offmeshCurve = new AnimationCurve(new Keyframe[0]);
                if (_stateManager.offmeshBehaviour.OffMeshAreas[i].offmeshCurve != null)
                     offmeshCurve = EditorGUILayout.CurveField("Offmesh Curve", _stateManager.offmeshBehaviour.OffMeshAreas[i].offmeshCurve);

                float TransitDuration = EditorGUILayout.Slider("Transit Duration", _stateManager.offmeshBehaviour.OffMeshAreas[i].TransitDuration, 0.1f, 5f);

                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Off mesh list");

                    _stateManager.offmeshBehaviour.OffMeshAreas[i].AnimatorBool = AnimatorBool;
                    _stateManager.offmeshBehaviour.OffMeshAreas[i].offmeshCurve = offmeshCurve;
                    _stateManager.offmeshBehaviour.OffMeshAreas[i].TransitDuration = TransitDuration;
                }
            }

            EditorGUILayout.EndVertical();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        }

        void DrawHealthProperties()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.healthToggle ? Resources.Load("EditorContent/health_c-icon") : Resources.Load("EditorContent/health_bw-icon"));
            bool healthToggle = GUILayout.Toggle(_stateManager.healthToggle, t1, "Button");

            int Health = _stateManager.ZombieHealthStats.Health;
            DeathType _DeathType = _stateManager.ZombieHealthStats._DeathType;
            int hitReactionsAvailable = _stateManager.ZombieHealthStats.hitReactionsAvailable;
            int DeathID = _stateManager.ZombieHealthStats.DeathID;
            float _cooldownTimer = _stateManager.ZombieHealthStats._cooldownTimer;
            bool lookAtCameraOnHit = _stateManager.ZombieHealthStats.lookAtCameraOnHit;
            bool fadeZombieMesh = _stateManager.ZombieHealthStats.fadeZombieMesh;
            Material faderMaterial = _stateManager.ZombieHealthStats.faderMaterial;
            List<Renderer> allRenderers = _stateManager.ZombieHealthStats.allRenderers;
            float fadeDelay = _stateManager.ZombieHealthStats.fadeDelay;
            float fadeDuration = _stateManager.ZombieHealthStats.fadeDuration;

            if (healthToggle)
            {
                EditorGUILayout.BeginVertical("Box");

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Current Health of this Zombie", MessageType.Info);

                Health = EditorGUILayout.IntSlider("Health", _stateManager.ZombieHealthStats.Health, 01, 200);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Just Define how many hit reactions animations you have for this zombie.", MessageType.Info);

                hitReactionsAvailable = EditorGUILayout.IntField("Hit Reactions Available", _stateManager.ZombieHealthStats.hitReactionsAvailable);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Do you want this Zombie to Die via Animation or you want to use Ragdoll.", MessageType.Info);

                _DeathType = (DeathType) EditorGUILayout.EnumPopup("Death Type",  _stateManager.ZombieHealthStats._DeathType);

                if(_DeathType == DeathType.Animation)
                {
                    if (hintsToggle)
                        EditorGUILayout.HelpBox("Death Animation ID you want to play.", MessageType.Info);

                    DeathID = EditorGUILayout.IntField("Death ID", _stateManager.ZombieHealthStats.DeathID);

                }

                if (hintsToggle)
                    EditorGUILayout.HelpBox("Cooldown Timer is time delay for which zombie gets stunned / stopped after being hit..", MessageType.Info);

                _cooldownTimer = EditorGUILayout.Slider("Cooldown Timer", _stateManager.ZombieHealthStats._cooldownTimer, 0.15f, 2f);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("If true, zombie will look at player on being hit. If player is in his range, he will start chasing / attacking him.", MessageType.Info);

                lookAtCameraOnHit = EditorGUILayout.Toggle("Look At Camera On Hit", _stateManager.ZombieHealthStats.lookAtCameraOnHit);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Fade Zombie Mesh on Death", EditorStyles.centeredGreyMiniLabel);

                if (hintsToggle)
                    EditorGUILayout.HelpBox("An awesome fade effect. This will fade zombie's mesh after its health reaches zero. ", MessageType.Info);

                fadeZombieMesh = EditorGUILayout.Toggle("Fade Zombie Mesh", _stateManager.ZombieHealthStats.fadeZombieMesh);

                if (fadeZombieMesh)
                {
                    if (hintsToggle)
                        EditorGUILayout.HelpBox("This is a fader material we are using to fade zombie's mesh.", MessageType.Info);

                    faderMaterial = (Material)EditorGUILayout.ObjectField("Fader Material", _stateManager.ZombieHealthStats.faderMaterial, typeof(Material));

                    if (hintsToggle)
                        EditorGUILayout.HelpBox("Fade Delay is After Death, how long the fade should wait before starting.", MessageType.Info);

                    fadeDelay = EditorGUILayout.Slider("Fade Delay", _stateManager.ZombieHealthStats.fadeDelay, 0, 5);

                    if (hintsToggle)
                        EditorGUILayout.HelpBox("Duration of the Fade Effect.", MessageType.Info);

                    fadeDuration = EditorGUILayout.Slider("Fade Duration", _stateManager.ZombieHealthStats.fadeDuration, 0.1f, 2f);

                    DrawMeshRenderersProperties();
                    EditorGUILayout.EndVertical();

                } else
                    EditorGUILayout.EndVertical();


                //clamp vals
                hitReactionsAvailable = Mathf.Clamp(hitReactionsAvailable, 0, hitReactionsAvailable);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai Health");

                _stateManager.healthToggle = healthToggle;

                _stateManager.ZombieHealthStats.Health = Health;
                _stateManager.ZombieHealthStats.hitReactionsAvailable = hitReactionsAvailable;
                _stateManager.ZombieHealthStats._DeathType = _DeathType;
                _stateManager.ZombieHealthStats.DeathID = DeathID;
                _stateManager.ZombieHealthStats._cooldownTimer = _cooldownTimer;
                _stateManager.ZombieHealthStats.lookAtCameraOnHit = lookAtCameraOnHit;
                _stateManager.ZombieHealthStats.fadeZombieMesh = fadeZombieMesh;
                _stateManager.ZombieHealthStats.faderMaterial = faderMaterial;
                _stateManager.ZombieHealthStats.allRenderers = allRenderers;
                _stateManager.ZombieHealthStats.fadeDelay = fadeDelay;
                _stateManager.ZombieHealthStats.fadeDuration = fadeDuration;
            }

        }

        void DrawMeshRenderersProperties()
        {

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (hintsToggle)
                EditorGUILayout.HelpBox("If your zombie is having a complex geometry i.e it contains several meshes, just add them one by one below.", MessageType.Info);

            EditorGUILayout.LabelField("Total Renderers to Fade : " + _stateManager.ZombieHealthStats.allRenderers.Count, EditorStyles.centeredGreyMiniLabel);

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Add Mesh Renderer"))
            {
                _stateManager.ZombieHealthStats.allRenderers.Add(new Renderer());
            }

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < _stateManager.ZombieHealthStats.allRenderers.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("box");

                Renderer meshR = (Renderer)EditorGUILayout.ObjectField(_stateManager.ZombieHealthStats.allRenderers[i], typeof(Renderer));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                {
                    _stateManager.ZombieHealthStats.allRenderers.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "uzai fade mesh");
                    _stateManager.ZombieHealthStats.allRenderers[i] = meshR;
                }

            }

        }

        void DrawZombieSFXProperties()
        {

            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.zSFXToggle ? Resources.Load("EditorContent/sounds_c-icon") : Resources.Load("EditorContent/sounds_bw-icon"));
            bool zSFXToggle = GUILayout.Toggle(_stateManager.zSFXToggle, t1, "Button");

            if (zSFXToggle)
            {
                //attack sfx
                DrawAttackSFX();

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                //hurt sfx
                DrawHurtSFX();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zombie SFX");
                _stateManager.zSFXToggle = zSFXToggle;

            }

        }

        void DrawAttackSFX(){

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add SFX For Attack SFX", MessageType.Info);

            if (hintsToggle)
                EditorGUILayout.HelpBox("A random sound will be played from below EXACTLY at the normalized time you have defined for" +
                    " attack animation's Attack Event to happen in Zombie Attack Behaviour SMB.", MessageType.Info);

            EditorGUILayout.LabelField("Total SFX : " + _stateManager.ZombieSFX._attackSFX.Count, EditorStyles.centeredGreyMiniLabel);

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Add"))
            {
                AudioClip newAC = null;
                _stateManager.ZombieSFX._attackSFX.Add(newAC);
            }

            for(int i = 0; i < _stateManager.ZombieSFX._attackSFX.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("box");

                AudioClip _ac = (AudioClip)EditorGUILayout.ObjectField(_stateManager.ZombieSFX._attackSFX[i], typeof(AudioClip));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                {
                    _stateManager.ZombieSFX._attackSFX.RemoveAt(i);
                    break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "uzAI SFX");
                    _stateManager.ZombieSFX._attackSFX[i] = _ac;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            

        }

        void DrawHurtSFX()
        {
            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.HelpBox("Add SFX For Hurt SFX", MessageType.Info);

            if (hintsToggle)
                EditorGUILayout.HelpBox("A random sound will be played from below as soon as Zombie recieves a damage in its" +
                    " OnDamage() event.", MessageType.Info);

            EditorGUILayout.LabelField("Total SFX : " + _stateManager.ZombieSFX._hurtSFX.Count, EditorStyles.centeredGreyMiniLabel);

            GUI.skin.button.alignment = TextAnchor.MiddleCenter;

            if (GUILayout.Button("Add"))
            {
                AudioClip newAC = null;
                _stateManager.ZombieSFX._hurtSFX.Add(newAC);
            }

            for (int i = 0; i < _stateManager.ZombieSFX._hurtSFX.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal("box");

                AudioClip _ac = (AudioClip)EditorGUILayout.ObjectField(_stateManager.ZombieSFX._hurtSFX[i], typeof(AudioClip));

                if (GUILayout.Button("X", EditorStyles.miniButtonRight))
                {
                    _stateManager.ZombieSFX._hurtSFX.RemoveAt(i);
                    break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "uzAI SFX");
                    _stateManager.ZombieSFX._hurtSFX[i] = _ac;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

        }

        void DrawZombiePathGizmos()
        {
            EditorGUI.BeginChangeCheck();

            //will be replaced later with texture
            Texture t1 = (Texture)(_stateManager.gizmoToggle ? Resources.Load("EditorContent/gizmo_c-icon") : Resources.Load("EditorContent/gizmo_bw-icon"));
            bool gizmoToggle = GUILayout.Toggle(_stateManager.gizmoToggle, t1, "Button");

            bool drawLineToCurrentTarget = _stateManager.drawGizmos.drawLineToCurrentTarget;
            Color lineGizmoColor = _stateManager.drawGizmos.lineGizmoColor;

            bool drawPathToCurrentTarget = _stateManager.drawGizmos.drawPathToCurrentTarget;
            Color pathGizmoColor = _stateManager.drawGizmos.pathGizmoColor;

            if (gizmoToggle)
            {
                EditorGUILayout.BeginVertical("Box");
                if (hintsToggle)
                    EditorGUILayout.HelpBox("Draws a straight line towards the current target from the zombie at RUNTIME.", MessageType.Info);

                drawLineToCurrentTarget = EditorGUILayout.Toggle("Draw Line To Current Target", _stateManager.drawGizmos.drawLineToCurrentTarget);

                if (drawLineToCurrentTarget)
                    lineGizmoColor = EditorGUILayout.ColorField("Line Gizmo Color", _stateManager.drawGizmos.lineGizmoColor);

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("Box");
                if (hintsToggle)
                    EditorGUILayout.HelpBox("Draws the full corners path towards the current target from the zombie at RUNTIME.", MessageType.Info);

                drawPathToCurrentTarget = EditorGUILayout.Toggle("Draw Path To Current Target", _stateManager.drawGizmos.drawPathToCurrentTarget);

                if (drawPathToCurrentTarget)
                    pathGizmoColor = EditorGUILayout.ColorField("Path Gizmo Color", _stateManager.drawGizmos.pathGizmoColor);

                EditorGUILayout.EndVertical();

            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "uzai gizmo");

                _stateManager.gizmoToggle = gizmoToggle;
                _stateManager.drawGizmos.drawLineToCurrentTarget = drawLineToCurrentTarget;
                _stateManager.drawGizmos.lineGizmoColor = lineGizmoColor;
                _stateManager.drawGizmos.drawPathToCurrentTarget = drawPathToCurrentTarget;
                _stateManager.drawGizmos.pathGizmoColor = pathGizmoColor;

            }
        }

        //SCENE GUI

        private void OnSceneGUI()
        {

            DrawSightGizmos();

            DrawAttackGizmos();
        }

        void DrawSightGizmos()
        {
            ZombieSightBehaviour fov = _stateManager.sightBehaviour;

            if (fov._stateManager == null)
                fov._stateManager = _stateManager;

            Handles.color = Color.yellow;
            Handles.DrawWireArc(_stateManager.transform.position, Vector3.up, Vector3.forward, 360, fov.Range);

            Color c = Color.yellow;
            c.a = 0.35f;
            Handles.color = c;
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, fov.Angle / 2, fov.Range);
            Handles.DrawSolidArc(_stateManager.transform.position, _stateManager.transform.up, _stateManager.transform.forward, -fov.Angle / 2, fov.Range);

            Vector3 AngleA = fov.DirFromAngle(-fov.Angle / 2, false);
            Vector3 AngleB = fov.DirFromAngle(fov.Angle / 2, false);

            Handles.color = Color.red;
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleA * fov.Range);
            Handles.DrawLine(_stateManager.transform.position, _stateManager.transform.position + AngleB * fov.Range);

            foreach (Transform t in fov.visibleTargets)
            {
                Handles.DrawLine(_stateManager.transform.position, t.position);
            }
        }

        void DrawAttackGizmos()
        {
            ZombieAttackBehaviour attack = _stateManager.attackBehaviour;

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

    }
}