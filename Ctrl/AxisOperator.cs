using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;
namespace RuntimeGizmos
{
    public class AxisOperator
    {
        //These are the same as the unity editor hotkeys
        public KeyCode SetMoveType = KeyCode.W;
        public KeyCode SetRotateType = KeyCode.E;
        public KeyCode SetScaleType = KeyCode.R;
        public KeyCode SetSpaceToggle = KeyCode.X;
        private Protocal protocal;
        private AxisSetting setting;
        private MonoBehaviour holder;
        private Transform transform { get { return protocal.myCamera.transform; } }
        public AxisOperator(MonoBehaviour holder,Protocal protocal, AxisSetting setting)
        {
            this.holder = holder;
            this.protocal = protocal;
            this.setting = setting;
        }
        public void Update()
        {
            SetSpaceAndType();
            SelectAxis();
            GetTarget();
            if (protocal.target == null) return;
            TransformSelected();
        }
        public void LateUpdate()
        {
            if (protocal.target == null) return;
            //We run this in lateupdate since coroutines run after update and we want our gizmos to have the updated protocal.target transform position after TransformSelected()
            SetAxisInfo();
        }

        float ClosestDistanceFromMouseToLines(List<Vector3> lines)
        {
            Ray mouseRay =protocal.myCamera.ScreenPointToRay(Input.mousePosition);

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

        void SetAxisInfo()
        {
            float size = setting.handleLength * protocal.GetDistanceMultiplier();
            protocal.axisInfo.Set(protocal.target, size, protocal.space);

            if (protocal.isTransforming && protocal.type == TransformType.Scale)
            {
                if (protocal.selectedAxis == Axis.Any) protocal.axisInfo.Set(protocal.target, size + protocal.totalScaleAmount, protocal.space);
                if (protocal.selectedAxis == Axis.X) protocal.axisInfo.xAxisEnd += (protocal.axisInfo.xDirection * protocal.totalScaleAmount);
                if (protocal.selectedAxis == Axis.Y) protocal.axisInfo.yAxisEnd += (protocal.axisInfo.yDirection * protocal.totalScaleAmount);
                if (protocal.selectedAxis == Axis.Z) protocal.axisInfo.zAxisEnd += (protocal.axisInfo.zDirection * protocal.totalScaleAmount);
            }
        }

  
        void TransformSelected()
        {
            if (protocal.selectedAxis != Axis.None && Input.GetMouseButtonDown(0))
            {
                holder. StartCoroutine(TransformSelected(protocal.type));
            }
        }

        IEnumerator TransformSelected(TransformType type)
        {
            protocal.isTransforming = true;
            protocal.totalScaleAmount = 0;
            protocal. totalRotationAmount = Quaternion.identity;

            Vector3 originalTargetPosition = protocal.target.position;
            Vector3 planeNormal = (protocal.myCamera. transform.position - protocal.target.position).normalized;
            Vector3 axis = GetSelectedAxisDirection();
            Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
            Vector3 previousMousePosition = Vector3.zero;

            while (!Input.GetMouseButtonUp(0))
            {
                Ray mouseRay = protocal.myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePosition = GeometryUtil.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalTargetPosition, planeNormal);

                if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
                {
                    if (protocal.type == TransformType.Move)
                    {
                        float moveAmount = GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * setting. moveSpeedMultiplier;
                        protocal.target.Translate(axis * moveAmount, Space.World);
                    }

                    if (protocal.type == TransformType.Scale)
                    {
                        Vector3 projected = (protocal.selectedAxis == Axis.Any) ? transform.right : projectedAxis;
                        float scaleAmount = GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projected) * setting.scaleSpeedMultiplier;

                        //WARNING - There is a bug in unity 5.4 and 5.5 that causes InverseTransformDirection to be affected by scale which will break negative scaling. Not tested, but updating to 5.4.2 should fix it - https://issuetracker.unity3d.com/issues/transformdirection-and-inversetransformdirection-operations-are-affected-by-scale
                        Vector3 localAxis = (protocal.space == TransformSpace.Local && protocal.selectedAxis != Axis.Any) ? protocal.target.InverseTransformDirection(axis) : axis;

                        if (protocal.selectedAxis == Axis.Any) protocal.target.localScale += (GeometryUtil.Abs(protocal.target.localScale.normalized) * scaleAmount);
                        else protocal.target.localScale += (localAxis * scaleAmount);

                        protocal.totalScaleAmount += scaleAmount;
                    }

                    if (protocal.type == TransformType.Rotate)
                    {
                        if (protocal.selectedAxis == Axis.Any)
                        {
                            Vector3 rotation = transform.TransformDirection(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0));
                            protocal.target.Rotate(rotation * setting.allRotateSpeedMultiplier, Space.World);
                            protocal.totalRotationAmount *= Quaternion.Euler(rotation * setting.allRotateSpeedMultiplier);
                        }
                        else
                        {
                            Vector3 projected = (protocal.selectedAxis == Axis.Any || GeometryUtil.IsParallel(axis, planeNormal)) ? planeNormal : Vector3.Cross(axis, planeNormal);
                            float rotateAmount = (GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projected) *setting.rotateSpeedMultiplier) /protocal. GetDistanceMultiplier();
                            protocal.target.Rotate(axis, rotateAmount, Space.World);
                            protocal.totalRotationAmount *= Quaternion.Euler(axis * rotateAmount);
                        }
                    }
                }

                previousMousePosition = mousePosition;

                yield return null;
            }

            protocal.totalRotationAmount = Quaternion.identity;
            protocal.totalScaleAmount = 0;
            protocal.isTransforming = false;
        }

        Vector3 GetSelectedAxisDirection()
        {
            if (protocal.selectedAxis != Axis.None)
            {
                if (protocal.selectedAxis == Axis.X) return protocal.axisInfo.xDirection;
                if (protocal.selectedAxis == Axis.Y) return protocal.axisInfo.yDirection;
                if (protocal.selectedAxis == Axis.Z) return protocal.axisInfo.zDirection;
                if (protocal.selectedAxis == Axis.Any) return Vector3.one;
            }
            return Vector3.zero;
        }

        public void SetSpaceAndType()
        {
            if (Input.GetKeyDown(SetMoveType)) protocal.type = TransformType.Move;
            else if (Input.GetKeyDown(SetRotateType)) protocal.type = TransformType.Rotate;
            else if (Input.GetKeyDown(SetScaleType)) protocal.type = TransformType.Scale;

            if (Input.GetKeyDown(SetSpaceToggle))
            {
                if (protocal.space == TransformSpace.Global) protocal.space = TransformSpace.Local;
                else if (protocal.space == TransformSpace.Local) protocal.space = TransformSpace.Global;
            }

            if (protocal.type == TransformType.Scale) protocal.space = TransformSpace.Local; //Only support local scale
        }
        public void SelectAxis()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            protocal.selectedAxis = Axis.None;

            float xClosestDistance = float.MaxValue;
            float yClosestDistance = float.MaxValue;
            float zClosestDistance = float.MaxValue;
            float allClosestDistance = float.MaxValue;
            float minSelectedDistanceCheck = setting.minSelectedDistanceCheck * protocal. GetDistanceMultiplier();

            if (protocal.type == TransformType.Move || protocal.type == TransformType.Scale)
            {
                protocal.selectedLinesBuffer.Clear();
                protocal.selectedLinesBuffer.Add(protocal.handleLines);
                if (protocal.type == TransformType.Move) protocal.selectedLinesBuffer.Add(protocal.handleTriangles);
                else if (protocal.type == TransformType.Scale) protocal.selectedLinesBuffer.Add(protocal.handleSquares);

                xClosestDistance = ClosestDistanceFromMouseToLines(protocal.selectedLinesBuffer.x);
                yClosestDistance = ClosestDistanceFromMouseToLines(protocal.selectedLinesBuffer.y);
                zClosestDistance = ClosestDistanceFromMouseToLines(protocal.selectedLinesBuffer.z);
                allClosestDistance = ClosestDistanceFromMouseToLines(protocal.selectedLinesBuffer.all);
            }
            else if (protocal.type == TransformType.Rotate)
            {
                xClosestDistance = ClosestDistanceFromMouseToLines(protocal.circlesLines.x);
                yClosestDistance = ClosestDistanceFromMouseToLines(protocal.circlesLines.y);
                zClosestDistance = ClosestDistanceFromMouseToLines(protocal.circlesLines.z);
                allClosestDistance = ClosestDistanceFromMouseToLines(protocal.circlesLines.all);
            }

            if (protocal.type == TransformType.Scale && allClosestDistance <= minSelectedDistanceCheck) protocal.selectedAxis = Axis.Any;
            else if (xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) protocal.selectedAxis = Axis.X;
            else if (yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) protocal.selectedAxis = Axis.Y;
            else if (zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) protocal.selectedAxis = Axis.Z;
            else if (protocal.type == TransformType.Rotate && protocal.target != null)
            {
                Ray mouseRay = protocal.myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePlaneHit = GeometryUtil.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, protocal.target.position, (transform.position - protocal.target.position).normalized);
                if ((protocal.target.position - mousePlaneHit).sqrMagnitude <= Mathf.Pow((setting.handleLength * protocal.GetDistanceMultiplier()), 2)) protocal.selectedAxis = Axis.Any;
            }
        }
        public void GetTarget()
        {
            if (protocal.selectedAxis == Axis.None && Input.GetMouseButtonDown(0))
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(protocal.myCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
                {
                    protocal.target = hitInfo.transform;
                }
                else
                {
                    protocal.target = null;
                }
            }
        }

    }
}