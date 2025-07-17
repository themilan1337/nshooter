using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Manoeuvre
{
    public class JoystickInput : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Range(1, 5f)]
        public float KnobRestriction = 2f;

        //Image References
        Image JoystickBackgroundImage;
        Image KnobImage;

        //Main Input Vector
        //[HideInInspector]
        public Vector3 InputVector;

        //This Instance
        public static JoystickInput Instance;

        private void Awake()
        {
            Instance = this;
            InputVector = Vector3.zero;
            JoystickBackgroundImage = GetComponent<Image>();
            KnobImage = GameObject.Find("Knob").GetComponent<Image>();

           
        }

        private void Start()
        {
            //enable using mobile inputs flag
            ManoeuvreFPSController.Instance.Inputs.usingMobileInputs = true;
        }

        public virtual void OnDrag(PointerEventData _data)
        {
            Vector2 pos = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(JoystickBackgroundImage.rectTransform, _data.position, _data.pressEventCamera, out pos)) 
            {
                pos.x = pos.x / JoystickBackgroundImage.rectTransform.sizeDelta.x;
                pos.y = pos.y / JoystickBackgroundImage.rectTransform.sizeDelta.y;

                float x = (JoystickBackgroundImage.rectTransform.pivot.x == 1) ? pos.x * 2 + 1 : pos.x * 2 - 1;
                float y = (JoystickBackgroundImage.rectTransform.pivot.y == 1) ? pos.y * 2 + 1 : pos.y * 2 - 1;

                InputVector = new Vector3(x, 0, y);

                InputVector = InputVector.magnitude > 1 ? InputVector.normalized : InputVector;

                KnobImage.rectTransform.anchoredPosition = new Vector2(InputVector.x * (JoystickBackgroundImage.rectTransform.sizeDelta.x / KnobRestriction),
                                                                         InputVector.z * (JoystickBackgroundImage.rectTransform.sizeDelta.y / KnobRestriction));
            }

        }

        public virtual void OnPointerDown(PointerEventData _data)
        {
            OnDrag(_data);

        }

        public virtual void OnPointerUp(PointerEventData _data)
        {

            InputVector = Vector3.zero;
            KnobImage.rectTransform.anchoredPosition = InputVector;

        }
    }
}