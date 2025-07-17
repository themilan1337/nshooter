using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uzAI
{
    public class SimpleCameraRotation : MonoBehaviour
    {
        public float rotationSpeed = 3;

        void Update()
        {

            float rotation = rotationSpeed * Input.GetAxis("Mouse X");

            Vector3 newRot = new Vector3(transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + rotation,
                transform.rotation.eulerAngles.z);

            transform.eulerAngles = newRot;
        }
    }
}