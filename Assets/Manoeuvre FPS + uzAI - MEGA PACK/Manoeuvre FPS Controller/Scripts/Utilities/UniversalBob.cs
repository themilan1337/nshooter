using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{

    [System.Serializable]
    public class UniversalBob
    {

        [Tooltip("Whether you want to have head bob effect.")]
        public bool enableBobbing = true;

        [Tooltip("The curve which will dictate the movement!")]
        public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 01.5f),
                                                            new Keyframe(1.5f, -1.5f), new Keyframe(2f, 0f));

        public BobState[] bobStates;

        float x;
        float y;
        float curveTime;

        public void Initialize()
        {

            curveTime = animationCurve[animationCurve.length - 1].time;

        }

        public Vector3 Offset(float speed, BobState bob)
        {

            x += (speed * Time.deltaTime) / bob.interval;
            y += ((speed * Time.deltaTime) / bob.interval) * speed;

            if (x > curveTime)
                x -= curveTime;

            if (y > curveTime)
                y -= curveTime;

            float xPos = animationCurve.Evaluate(x) * bob.horizontalFactor;
            float yPos = animationCurve.Evaluate(y) * bob.verticalFactor;

            return new Vector3(xPos, yPos, 0);

        }

    }

    [System.Serializable]
    public class BobState
    {
        [Tooltip("Define the name of the state specific to this bob behaviour.")]
        public string headBobStateName;
        [Tooltip("How fast you want head bob effect in Horizontal axis?")]
        public float horizontalFactor = 0.035f;
        [Tooltip("How fast you want head bob effect in Vertical axis?")]
        public float verticalFactor = 0.025f;
        [Tooltip("How fast you want overall head bob effect ?")]
        public float speed = 3.5f;
        [Tooltip("How much intervals you want in head bob effect ?")]
        public float interval = 3.5f;
    }
}