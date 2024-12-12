using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace RobotAbuse.Tests
{
    public class MovementTests
    {

        [Test]
        public void Move_Along_X_Axis_For_Horizontal_Input()
        {
            //Given:A gameobject at (0,0,0) with no rotation, a horizontal input to the right, a move speed of 1, and delta time (60fps).
            //Expected: A movement vector that points right with a value of .1f;
            double delta = .1;
            Assert.AreEqual(new Vector3(0.01f, 0f, 0f).ToString(), (new MovementController().Calculate(new GameObject().transform, new Vector3(1, 0, 0), 1) * 0.01666f).ToString());
        }

        [Test]
        public void Look_Up_Vertical_Axis_Input()
        {
            //Given:A looking down input vector, a current rotation, a look sensitivity, and an allowed rotation range
            //Expected:calculate a new rotation in a valid range.
            Assert.AreEqual(1f,new MovementController().CalculateVerticalRotation(new Vector2(0,-1),0,1,90));

            Assert.AreEqual(90f, new MovementController().CalculateVerticalRotation(new Vector2(0, -100), 0, 1, 90));
        }
    }
}
