using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
namespace RuntimeGizmos
{
    [System.Serializable]
    public class AxisSetting
    {
       public float handleLength = .25f;
       public float triangleSize = .03f;
       public float boxSize = .01f;
       public int circleDetail = 40;
       public float minSelectedDistanceCheck = .04f;
       public float moveSpeedMultiplier = 1f;
       public float scaleSpeedMultiplier = 1f;
       public float rotateSpeedMultiplier = 200f;
       public float allRotateSpeedMultiplier = 20f;
       public EnableState enableState;
    }
}