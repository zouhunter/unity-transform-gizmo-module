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
        private Protocal protocal;
        private AxisSetting setting;
        private MonoBehaviour holder;
        private Transform target { get { return protocal.target; } set { protocal.target = value; } }
        private Axis selectedAxis { get { return protocal.selectedAxis; } set { protocal.selectedAxis = value; } }
        public bool isTransforming { get { return protocal.isTransforming; }set { protocal.isTransforming = value; } }
        public float totalScaleAmount { get { return protocal.totalScaleAmount; } set { protocal. totalScaleAmount = value; } }
        public Quaternion totalRotationAmount { get { return protocal.totalRotationAmount; } set { protocal.totalRotationAmount = value; } }
        public Camera myCamera { get { return protocal.myCamera; } }
        public TransformSpace space { get { return protocal.space; } set { protocal.space = value; } }
        public TransformType type { get { return protocal.type; } set { protocal.type = value; } }
        private System.Func<float> GetDistanceMultiplier { get { return protocal.GetDistanceMultiplier; } }

        public AxisOperator(MonoBehaviour holder,Protocal protocal, AxisSetting setting)
        {
            this.holder = holder;
            this.protocal = protocal;
            this.setting = setting;
        }

        public void Update()
        {
            if (target == null) return;
            TransformSelected();
        }
        public void LateUpdate()
        {
            if (target == null) return;
            //We run this in lateupdate since coroutines run after update and we want our gizmos to have the updated target transform position after TransformSelected()
            SetAxisInfo();
        }

        void SetAxisInfo()
        {
            float size = setting.handleLength * GetDistanceMultiplier();
           protocal.axisInfo.Set(target, size, space);

            if (isTransforming && type == TransformType.Scale)
            {
                if (selectedAxis == Axis.Any) protocal.axisInfo.Set(target, size + totalScaleAmount, space);
                if (selectedAxis == Axis.X) protocal.axisInfo.xAxisEnd += (protocal.axisInfo.xDirection * totalScaleAmount);
                if (selectedAxis == Axis.Y) protocal.axisInfo.yAxisEnd += (protocal.axisInfo.yDirection * totalScaleAmount);
                if (selectedAxis == Axis.Z) protocal.axisInfo.zAxisEnd += (protocal.axisInfo.zDirection * totalScaleAmount);
            }
        }
        void TransformSelected()
        {
            if (selectedAxis != Axis.None && Input.GetMouseButtonDown(0))
            {
                holder.StartCoroutine(TransformSelected(type));
            }
        }
        IEnumerator TransformSelected(TransformType type)
        {
            isTransforming = true;
            totalScaleAmount = 0;
             totalRotationAmount = Quaternion.identity;

            Vector3 originalTargetPosition = target.position;
            Vector3 planeNormal = (myCamera. transform.position - target.position).normalized;
            Vector3 axis = GetSelectedAxisDirection();
            Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
            Vector3 previousMousePosition = Vector3.zero;

            while (!Input.GetMouseButtonUp(0))
            {
                Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
                Vector3 mousePosition = GeometryUtil.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalTargetPosition, planeNormal);

                if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
                {
                    if (type == TransformType.Move)
                    {
                        float moveAmount = GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * setting. moveSpeedMultiplier;
                        target.Translate(axis * moveAmount, Space.World);
                    }

                    if (type == TransformType.Scale)
                    {
                        Vector3 projected = (selectedAxis == Axis.Any) ? myCamera. transform.right : projectedAxis;
                        float scaleAmount = GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projected) * setting.scaleSpeedMultiplier;

                        //WARNING - There is a bug in unity 5.4 and 5.5 that causes InverseTransformDirection to be affected by scale which will break negative scaling. Not tested, but updating to 5.4.2 should fix it - https://issuetracker.unity3d.com/issues/transformdirection-and-inversetransformdirection-operations-are-affected-by-scale
                        Vector3 localAxis = (space == TransformSpace.Local && selectedAxis != Axis.Any) ? target.InverseTransformDirection(axis) : axis;

                        if (selectedAxis == Axis.Any) target.localScale += (GeometryUtil.Abs(target.localScale.normalized) * scaleAmount);
                        else target.localScale += (localAxis * scaleAmount);

                        totalScaleAmount += scaleAmount;
                    }

                    if (type == TransformType.Rotate)
                    {
                        if (selectedAxis == Axis.Any)
                        {
                            Vector3 rotation = myCamera.transform.TransformDirection(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0));
                            target.Rotate(rotation * setting.allRotateSpeedMultiplier, Space.World);
                            totalRotationAmount *= Quaternion.Euler(rotation * setting.allRotateSpeedMultiplier);
                        }
                        else
                        {
                            Vector3 projected = (selectedAxis == Axis.Any || GeometryUtil.IsParallel(axis, planeNormal)) ? planeNormal : Vector3.Cross(axis, planeNormal);
                            float rotateAmount = (GeometryUtil.MagnitudeInDirection(mousePosition - previousMousePosition, projected) *setting.rotateSpeedMultiplier) / GetDistanceMultiplier();
                            target.Rotate(axis, rotateAmount, Space.World);
                            totalRotationAmount *= Quaternion.Euler(axis * rotateAmount);
                        }
                    }
                }

                previousMousePosition = mousePosition;

                yield return null;
            }

            totalRotationAmount = Quaternion.identity;
            totalScaleAmount = 0;
            isTransforming = false;
        }
        Vector3 GetSelectedAxisDirection()
        {
            if (selectedAxis != Axis.None)
            {
                if (selectedAxis == Axis.X) return protocal.axisInfo.xDirection;
                if (selectedAxis == Axis.Y) return protocal.axisInfo.yDirection;
                if (selectedAxis == Axis.Z) return protocal.axisInfo.zDirection;
                if (selectedAxis == Axis.Any) return Vector3.one;
            }
            return Vector3.zero;
        }

    }
}