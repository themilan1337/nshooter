using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manoeuvre
{
    public class TouchCameraInput : MonoBehaviour
    {
        public Vector3 InputVector;

        public static TouchCameraInput Instance;

        // Use this for initialization
        void Awake()
        {
            Instance = this;

           
        }

        private void Start()
        {
            //enable using mobile inputs flag
            ManoeuvreFPSController.Instance.Inputs.usingMobileInputs = true;

        }

        private void Update()
        {

            if (Input.touchCount > 0 )
            {
                Touch touch = new Touch();
                InputVector = Vector3.zero;

                if (Input.GetTouch(0).position.x > Screen.width / 2)
                {
                    touch = Input.GetTouch(0);
                    InputVector = new Vector3(touch.deltaPosition.y, touch.deltaPosition.x, 0);

                }
                else if (Input.GetTouch(1).position.x > Screen.width / 2)
                {
                    touch = Input.GetTouch(1);
                    InputVector = new Vector3(touch.deltaPosition.y, touch.deltaPosition.x, 0);

                }

            }
            else
                InputVector = Vector3.zero;

        }

    }
}