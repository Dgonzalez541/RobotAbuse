using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotAbuse
{
    public class Movement
    {
        public Movement()
        {

        }
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
    }
}

