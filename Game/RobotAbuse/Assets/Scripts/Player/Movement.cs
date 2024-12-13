using UnityEngine;

namespace RobotAbuse
{
    //Movement calculations for the player that have been extracted for testing.
    public class MovementController
    {
        public bool IsMoving;
        Vector3 currentInputVector;
        Vector3 smoothInputVelocity;

        float vertRot;
        float verticalLookVelocity;//empty value for SmoothDamp
        public UnityEngine.Vector3 Calculate(Transform transform, Vector3 moveInput, float speed)
        {
            var input = new Vector3();
            input += transform.forward * moveInput.y;
            input += transform.right * moveInput.x;
            input += transform.up * moveInput.z;
            input.Normalize();
            currentInputVector = Vector3.SmoothDamp(currentInputVector, input * speed, ref smoothInputVelocity, .1f);

            return currentInputVector;
        }

        public float CalculateVerticalRotation(Vector2 lookInput, float currentVerticalRotation, float lookSensitivity, float clampRange)
        {
            currentVerticalRotation -= lookInput.y * lookSensitivity;
            var vertRot= Mathf.Clamp(currentVerticalRotation, -clampRange, clampRange);

            return vertRot;
        }
    }
}

