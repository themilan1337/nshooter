using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class SimpleRotation : MonoBehaviour
    {

        public float rotationSpeed = 3;
        

        // Use this for initialization
        void Start()
        {
            StartCoroutine(StartRotation());
        }

        IEnumerator StartRotation()
        {
            while (true)
            {
                Vector3 newRot = new Vector3(transform.rotation.eulerAngles.x ,
                    transform.rotation.eulerAngles.y + rotationSpeed,
                    transform.rotation.eulerAngles.z );

                transform.eulerAngles = newRot;

                yield return null;

            }
        }
    }
}