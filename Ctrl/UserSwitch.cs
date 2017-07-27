using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
namespace RuntimeGizmos
{
    public class UserSwitcher
    {
        public KeyCode SetMoveType = KeyCode.W;
        public KeyCode SetRotateType = KeyCode.E;
        public KeyCode SetScaleType = KeyCode.R;
        public KeyCode SetSpaceToggle = KeyCode.X;

        private Protocal protocal { get; set; }
        private AxisSetting setting { get; set; }
        private Transform target { get { return protocal.target; } set { protocal.target = value; } }
        private Axis selectedAxis { get { return protocal.selectedAxis; } set { protocal.selectedAxis = value; } }
        public AxisVectors handleLines { get { return protocal.handleLines; } }
        public AxisVectors handleTriangles { get { return protocal.handleTriangles; } }
        public AxisVectors handleSquares { get { return protocal.handleSquares; } }
        public AxisVectors circlesLines { get { return protocal.circlesLines; } }
        public AxisVectors drawCurrentCirclesLines { get { return protocal.drawCurrentCirclesLines; } }
        public AxisVectors selectedLinesBuffer { get { return protocal.selectedLinesBuffer; } }
        public bool isTransforming { get { return protocal.isTransforming; } }
        public float totalScaleAmount { get { return protocal.totalScaleAmount; } }
        public Quaternion totalRotationAmount { get { return protocal.totalRotationAmount; } }
        public Camera myCamera { get { return protocal.myCamera; } }
        public TransformSpace space { get { return protocal.space; } set { protocal.space = value; } }
        public TransformType type { get { return protocal.type; } set { protocal.type = value; } }
        private System.Func<float> GetDistanceMultiplier { get { return protocal.GetDistanceMultiplier; } }

        public UserSwitcher(Protocal protocal, AxisSetting setting)
        {
            this.protocal = protocal;
            this.setting = setting;
        }

        public void SetSpaceAndType(TransformType type, TransformSpace space)
        {
            this.type = type;
            this.space = space;
        }
        public void Update()
        {
            SetSpaceAndType();
            SelectAxis();
        }

        public void SetSpaceAndType()
        {
            if (Input.GetKeyDown(SetMoveType)) type = TransformType.Move;
            else if (Input.GetKeyDown(SetRotateType)) type = TransformType.Rotate;
            else if (Input.GetKeyDown(SetScaleType)) type = TransformType.Scale;

            if (Input.GetKeyDown(SetSpaceToggle))
            {
                if (space == TransformSpace.Global) space = TransformSpace.Local;
                else if (space == TransformSpace.Local) space = TransformSpace.Global;
            }

            if (type == TransformType.Scale) space = TransformSpace.Local; //Only support local scale
        }
     

        private void SelectAxis()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            selectedAxis = Axis.None;

            float xClosestDistance = float.MaxValue;
            float yClosestDistance = float.MaxValue;
            float zClosestDistance = float.MaxValue;
            float allClosestDistance = float.MaxValue;
            float minSelectedDistanceCheck = setting.minSelectedDistanceCheck * GetDistanceMultiplier();

            if (type == TransformType.Move || type == TransformType.Scale)
            {
                selectedLinesBuffer.Clear();
                selectedLinesBuffer.Add(handleLines);
                if (type == TransformType.Move) selectedLinesBuffer.Add(handleTriangles);
                else if (type == TransformType.Scale) selectedLinesBuffer.Add(handleSquares);

                xClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.x);
                yClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.y);
                zClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.z);
                allClosestDistance = ClosestDistanceFromMouseToLines(selectedLinesBuffer.all);
            }
            else if (type == TransformType.Rotate)
            {
                xClosestDistance = ClosestDistanceFromMouseToLines(circlesLines.x);
                yClosestDistance = ClosestDistanceFromMouseToLines(circlesLines.y);
                zClosestDistance = ClosestDistanceFromMouseToLines(circlesLines.z);
                allClosestDistance = ClosestDistanceFromMouseToLines(circlesLines.all);
            }

            if (type == TransformType.Scale && allClosestDistance <= minSelectedDistanceCheck) selectedAxis = Axis.Any;
            else if (xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) selectedAxis = Axis.X;
            else if (yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) selectedAxis = Axis.Y;
            else if (zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) selectedAxis = Axis.Z;
            else if (type == TransformType.Rotate && target != null)
            {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePlaneHit = GeometryUtil.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, target.position, (myCamera.transform.position - target.position).normalized);
                if ((target.position - mousePlaneHit).sqrMagnitude <= Mathf.Pow((setting.handleLength * GetDistanceMultiplier()), 2)) selectedAxis = Axis.Any;
            }
        }
        float ClosestDistanceFromMouseToLines(List<Vector3> lines)
        {
            Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

            float closestDistance = float.MaxValue;
            for (int i = 0; i < lines.Count; i += 2)
            {
                IntersectPoints points = GeometryUtil.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
                float distance = Vector3.Distance(points.first, points.second);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
            return closestDistance;
        }
    }
}
