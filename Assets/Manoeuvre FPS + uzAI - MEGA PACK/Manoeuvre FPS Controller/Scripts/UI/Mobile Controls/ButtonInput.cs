using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace Manoeuvre
{
    public class ButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
       //All Inputs
       public enum ButtonType
       {
            Run,
            Crouch,
            Jump,
            Shoot,
            Reload,
            Ironsight,
            Inventory,
            PreviousWeapon,
            NextWeapon,
            ZoomIn,
            ZoomOut
       }

        public ButtonType _ButtonType;

        ManoeuvreFPSInputs _Inputs;

        Image buttonImage;

        // Use this for initialization
        void Start()
        {
            buttonImage = GetComponent<Image>();
            //set default color
            SpriteChange(false);

            //enable using mobile inputs flag
            _Inputs = ManoeuvreFPSController.Instance.Inputs;
            _Inputs.usingMobileInputs = true;

        }

        public virtual void OnPointerDown(PointerEventData _data)
        {
            GetButton(true);

            GetButtonDown();
        }

        public virtual void OnPointerUp(PointerEventData _data)
        {
            GetButton(false);
        }

        /// <summary>
        /// Input will remain true until user helds the button
        /// </summary>
        /// <param name="inputState"></param>
        void GetButton(bool inputState)
        {
            switch (_ButtonType)
            {
                case ButtonType.Jump:
                    _Inputs.jumpInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.Shoot:
                    _Inputs.shootInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.NextWeapon:
                    _Inputs.nextWeaponInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.PreviousWeapon:
                    _Inputs.previousWeaponInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.Reload:
                    _Inputs.reloadInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.ZoomIn:
                    _Inputs.zoomInInput = inputState;
                    SpriteChange(inputState);
                    break;

                case ButtonType.ZoomOut:
                    _Inputs.zoomOutInput = inputState;
                    SpriteChange(inputState);
                    break;

            }

            
        }

        /// <summary>
        /// Input will be toggled at each press 
        /// </summary>
        void GetButtonDown()
        {
            switch (_ButtonType)
            {
                case ButtonType.Run:
                    _Inputs.runInput = !_Inputs.runInput;
                    SpriteChange(_Inputs.runInput);
                    break;

                case ButtonType.Crouch:
                    _Inputs.crouchInput = !_Inputs.crouchInput;
                    SpriteChange(_Inputs.crouchInput);
                    break;

                case ButtonType.Ironsight:
                    _Inputs.ironsightInput = !_Inputs.ironsightInput;
                    SpriteChange(_Inputs.ironsightInput);
                    break;

                case ButtonType.Inventory:
                    _Inputs.inventoryInput = !_Inputs.inventoryInput;
                    SpriteChange(_Inputs.inventoryInput);
                    break;
            }
        }

        void SpriteChange(bool inputState)
        {

            if (buttonImage == null)
                return;

            Color c = Color.white;

            if (inputState)
                c.a = 1f;
            else
                c.a = 0.5f;

            buttonImage.color = c;
        }

    }
}