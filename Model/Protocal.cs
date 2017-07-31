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
        //xyz方向上移动和旋转时使用的手柄绘制信息
        public AxisVectors handleLines = new AxisVectors();
        public AxisVectors handleTriangles = new AxisVectors();
        public AxisVectors handleSquares = new AxisVectors();
        public AxisVectors circlesLines = new AxisVectors();
        public AxisVectors drawCurrentCirclesLines = new AxisVectors();
        public AxisVectors selectedLinesBuffer = new AxisVectors();

        public bool isTransforming;
        public float totalScaleAmount;
        public Quaternion totalRotationAmount;
        public Axis selectedAxis = Axis.None;

        //3轴方向信息
        public AxisInfo axisInfo;
        public Transform target;
        public Camera myCamera;

        internal UnityAction<bool> onTransormingStateChanged;
        internal UnityAction<Vector3> OnPositionChanged;
        internal UnityAction<Vector3> OnLocalScaleChanged;
        internal UnityAction<Vector3> OnRotationChanged;
        internal UnityAction<Vector3, float> OnRotationChangedwithfloat;

        public Protocal(Camera camera)
        {
            this.myCamera = camera;
        }

        //This helps keep the size consistent no matter how far we are from it.
        public float DistanceMultiplier
        {
            get
            {
                if (target == null) return 0f;
                return Mathf.Max(.01f, Mathf.Abs(GeometryUtil.MagnitudeInDirection(target.position - myCamera.transform.position, myCamera.transform.forward)));
            }
        }
    }
}
