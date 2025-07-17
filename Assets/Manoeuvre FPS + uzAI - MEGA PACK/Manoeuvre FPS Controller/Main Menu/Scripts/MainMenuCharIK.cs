using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manoeuvre
{
    public class MainMenuCharIK : MonoBehaviour
    {
        public Transform LeftHandIK;
        public float degreesPerSecond = 90;
        public bool enableIK;

        Animator anim;
        bool canRotate;
        Quaternion _rot;

        float mainRotationDelta;
        float LookatWeight;

        private void OnEnable()
        {
            if (_rot.eulerAngles == Vector3.zero)
                _rot = transform.rotation;

            canRotate = true;
        }

        // Use this for initialization
        void Start()
        {
            anim = GetComponent<Animator>();

            mainRotationDelta = transform.rotation.eulerAngles.y;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!LeftHandIK)
                return;

            if (!enableIK)
                return;

            anim.SetLookAtPosition(Camera.main.transform.position);

            if (mainRotationDelta < _rot.eulerAngles.y + 60 && mainRotationDelta > _rot.eulerAngles.y - 60)
                LookatWeight = Mathf.MoveTowards(LookatWeight, 1, 0.025f);
            else
                LookatWeight = Mathf.MoveTowards(LookatWeight, 0.25f, 0.025f);

            anim.SetLookAtWeight(LookatWeight);

            anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandIK.rotation);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        }

        private void OnMouseDrag()
        {
            if (!canRotate)
                return;

            SmoothRotate();
        }

        private void OnMouseDown()
        {
            CancelInvoke("RotateBackToOriginal");
        }

        private void OnMouseUp()
        {
            Invoke("RotateBackToOriginal", 2f);
        }

        void SmoothRotate()
        {
            float rotX = Input.GetAxis("Mouse X") * degreesPerSecond * Mathf.Deg2Rad;

            transform.Rotate(Vector3.up, -rotX);

            mainRotationDelta = transform.eulerAngles.y;
        }

        public void RotateBackToOriginal()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(LerpRotation());
        }

        IEnumerator LerpRotation()
        {
            float et = 0;
            canRotate = false;

            while (et < 1)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, _rot, et / 1);

                et += Time.deltaTime;
                yield return null;
            }

            mainRotationDelta = _rot.eulerAngles.y;

            canRotate = true;
        }

    }
}