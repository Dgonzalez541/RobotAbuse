using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RobotAbuse;

namespace RobotAbuse
{
    public class MovementTests
    {

        [Test]
        public void Move_Along_X_Axis_For_Horizontal_Input()
        {
            //Given:A gameobject at (0,0,0) with no rotation, a horizontal input to the right, a move speed of 1, and delta time.
            //Expected: A movement vector that points right with a value of .1f;
            Assert.AreEqual(new Vector3(.1f, 0f, 0f), new Movement().Calculate(new GameObject().transform, new Vector3(1, 0, 0), 1, .1f));
        }
    }
}
