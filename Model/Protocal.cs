using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

namespace RuntimeGizmos
{
    public class Protocal
    {
        public TransformSpace space = TransformSpace.Global;
        public TransformType type = TransformType.Move;

        public AxisVectors handleLines = new AxisVectors();
        public AxisVectors handleTriangles = new AxisVectors();
        public AxisVectors handleSquares = new AxisVectors();
        public AxisVectors circlesLines = new AxisVectors();
        public AxisVectors drawCurrentCirclesLines = new AxisVectors();

        public bool isTransforming;
        public float totalScaleAmount;
        public Quaternion totalRotationAmount;
        public Axis selectedAxis = Axis.None;
        public AxisInfo axisInfo;
        public Transform target;
        public Camera myCamera;
        public AxisVectors selectedLinesBuffer = new AxisVectors();

        //This helps keep the size consistent no matter how far we are from it.
        public float GetDistanceMultiplier()
        {
            if (target == null) return 0f;
            return Mathf.Max(.01f, Mathf.Abs(GeometryUtil.MagnitudeInDirection(target.position - myCamera.transform.position, myCamera.transform.forward)));
        }

    }
}
