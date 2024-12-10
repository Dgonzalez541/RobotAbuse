using UnityEngine;

namespace RobotAbuse
{
    //The movement controller contains the core logic for moving and rotating the charachter. This has been decoupled from Static classes like Time, in order to be properly tested.
    public class MovementController
    {
        public bool IsMoving;
        public Vector3 Calculate(Transform transform, Vector3 moveInput, float speed, float time)
        {
            var input = new Vector3();
            input += transform.forward * moveInput.y;
            input += transform.right * moveInput.x;
            input += transform.up * moveInput.z;
            input = Vector3.ClampMagnitude(input, 1f);
            input = input * speed * time;
            return input;
        }

        public float CalculateVerticalRotation(Vector2 lookInput, float currentVerticalRotation, float lookSensitivity, float clampRange)
        {
            currentVerticalRotation -= lookInput.y * lookSensitivity;
            currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, -clampRange, clampRange);
            return currentVerticalRotation;
        }
    }
}

