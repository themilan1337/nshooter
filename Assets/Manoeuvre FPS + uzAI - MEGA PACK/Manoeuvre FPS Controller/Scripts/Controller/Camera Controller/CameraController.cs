using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{

    public class CameraController : MonoBehaviour
    {
        [Header("-- Common Properties --")]
        public bool hideCursor;
        public float lookSensitivity = 5;
        public float lookSmoth = 0.1f;
        public Camera weaponCamera;

        public Vector2 MinMaxAngle = new Vector2(65, -65);

        [Header("-- Camera Headbob Properties --")]
        public UniversalBob _cameraHeadBob ;

        float yRot;
        float xRot;

        float currentYRot;
        float currentXRot;

        float yRotVelocity;
        float xRotVelocity;

        //ManoeuvreFPSController fpsController;

        Vector3 camPos;
        
        CharacterController charController;

        // Use this for initialization
        void Start()
        {
            camPos = transform.localPosition;
            charController = GetComponentInParent<CharacterController>();
            //fpsController = GetComponentInParent<ManoeuvreFPSController>();
            _cameraHeadBob.Initialize();

            //toggle cursor
            ToggleCursor(hideCursor);
        }

        public void ToggleCursor(bool hide)
        {

            if (hide)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;

            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

            }
        }

        // Update is called once per frame
        void Update()
        {

            //perform head bob
            Headbob(gc_StateManager.Instance.currentPlayerState);

            //if Inventory is open
            if (Inventory.Instance.inventoryIsOpen)
                return;

            //Taking camera rotational input based on our platform
            if (ManoeuvreFPSController.Instance.Inputs.usingMobileInputs)
            {
                //if using Touch Input
                yRot += TouchCameraInput.Instance.InputVector.y * (lookSensitivity / 10);
                xRot -= TouchCameraInput.Instance.InputVector.x * (lookSensitivity / 10);

            }
            else
            {
                //if using mouse
                yRot += Input.GetAxis("Mouse X") * lookSensitivity;
                xRot -= Input.GetAxis("Mouse Y") * lookSensitivity;
                
            }

            //clamp angles
            xRot = Mathf.Clamp(xRot, MinMaxAngle.x, MinMaxAngle.y);

            //apply to our current rotations
            currentXRot = Mathf.SmoothDamp(currentXRot, xRot, ref xRotVelocity, lookSmoth);
            currentYRot = Mathf.SmoothDamp(currentYRot, yRot, ref yRotVelocity, lookSmoth);

            //finally set this to our transform i.e camera
            transform.rotation = Quaternion.Euler(currentXRot, currentYRot, 0);

            //making sure weapon cam also rotates the same
            weaponCamera.transform.rotation = Quaternion.Euler(currentXRot, currentYRot, 0);

           

        }

        /// <summary>
        /// Shakes the Camera Rotation
        /// </summary>
        /// <param name="shakeDuration"></param>
        /// <param name="shakeAmount"></param>
        /// <param name="decreaseFactor"></param>
        /// <returns></returns>
        public IEnumerator ShakeCamera(float shakeDuration, float shakeAmount = 0.2f, float decreaseFactor = 0.3f)
        {
            Vector3 originalRot = transform.eulerAngles;
            float currentShakeDuration = shakeDuration;
            while (currentShakeDuration > 0)
            {
                transform.eulerAngles +=  Random.insideUnitSphere * shakeAmount;
                currentShakeDuration -= Time.deltaTime * decreaseFactor;
                yield return null;
            }
           
        }

        /// <summary>
        /// Do head bobbing in the camera
        /// </summary>
        void Headbob(PlayerStates state)
        {
            //we won't do any headbob calculations if timescale is tweaked
            if (Time.timeScale < 0.9f)
                return;

            //return if the head bob is disabled
            if (!_cameraHeadBob.enableBobbing)
                return;

            //loop through all the bob states
            foreach (BobState bob in _cameraHeadBob.bobStates)
            {
                //identify the correct bob state
                if (bob.headBobStateName == state.ToString())
                    //now do bobbing
                    transform.localPosition = camPos + _cameraHeadBob.Offset(bob.speed, bob);
            }

            //make special case for idle since the controller speed at idle is 0
            if(state == PlayerStates.Idle)
            {
                //loop through all the bob states
                foreach (BobState bob in _cameraHeadBob.bobStates)
                {
                    //identify the correct bob state
                    if (bob.headBobStateName == state.ToString())
                        //now do bobbing
                        transform.localPosition = camPos + _cameraHeadBob.Offset(1, bob);
                }
            }

        }
    }

    
}