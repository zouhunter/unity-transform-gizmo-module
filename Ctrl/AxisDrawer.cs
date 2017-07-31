using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

namespace RuntimeGizmos
{
    public class AxisDrawer
    {
        Color xColor = new Color(1, 0, 0, 0.8f);
        Color yColor = new Color(0, 1, 0, 0.8f);
        Color zColor = new Color(0, 0, 1, 0.8f);
        Color allColor = new Color(.7f, .7f, .7f, 0.8f);
        Color selectedColor = new Color(1, 1, 0, 0.8f);

        static Material lineMaterial;

        #region TogatherUseInfo
        private AxisSetting setting { get; set; }
        private Protocal protocal { get; set; }
        private Transform target { get { return protocal.target; } }
        private Axis selectedAxis { get { return protocal.selectedAxis; } }
        public AxisVectors handleLines { get { return protocal.handleLines; } }
        public AxisVectors handleTriangles { get { return protocal.handleTriangles; } }
        public AxisVectors handleSquares { get { return protocal.handleSquares; } }
        public AxisVectors circlesLines { get { return protocal.circlesLines; } }
        public AxisVectors drawCurrentCirclesLines { get { return protocal.drawCurrentCirclesLines; } }
        public AxisVectors selectedLinesBuffer { get { return protocal.selectedLinesBuffer; } }
        public bool isTransforming { get { return protocal.isTransforming; } }
        public Quaternion totalRotationAmount { get { return protocal.totalRotationAmount; } }
        public AxisInfo axisInfo { get { return protocal.axisInfo; } }
        public Camera myCamera { get { return protocal.myCamera; } }
        public TransformSpace space { get { return protocal.space; } }
        public TransformType type { get { return protocal.type; } }
        private float DistanceMultiplier { get { return protocal.DistanceMultiplier; } }
        #endregion

        public AxisDrawer(Protocal protocal, AxisSetting setting)
        {
            this.protocal = protocal;
            this.setting = setting;
            if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Custom/Lines"));
        }

        /// <summary>
        /// 在LateUpdate中执行，尝试设置绘制信息
        /// </summary>
        public void TrySetLines()
        {
            if (target == null) return;
            SetHandleLines();
            SetHandleTriangles();
            SetHandleSquares();
            SetCircles(axisInfo, circlesLines);
        }
        /// <summary>
        /// 在OnPostRender中执行，尝试绘制出可操作范围
        /// </summary>
        public void TryDrawing()
        {
            if (target == null) return;
            lineMaterial.SetPass(0);

            Color xColor = (selectedAxis == Axis.X) ? selectedColor : this.xColor;
            Color yColor = (selectedAxis == Axis.Y) ? selectedColor : this.yColor;
            Color zColor = (selectedAxis == Axis.Z) ? selectedColor : this.zColor;
            Color allColor = (selectedAxis == Axis.Any) ? selectedColor : this.allColor;

            DrawLines(handleLines.x, xColor);
            DrawLines(handleLines.y, yColor);
            DrawLines(handleLines.z, zColor);

            DrawTriangles(handleTriangles.x, xColor);
            DrawTriangles(handleTriangles.y, yColor);
            DrawTriangles(handleTriangles.z, zColor);

            DrawSquares(handleSquares.x, xColor);
            DrawSquares(handleSquares.y, yColor);
            DrawSquares(handleSquares.z, zColor);
            DrawSquares(handleSquares.all, allColor);

            AxisVectors rotationAxisVector = circlesLines;
            if (isTransforming && space == TransformSpace.Global && type == TransformType.Rotate)
            {
                rotationAxisVector = drawCurrentCirclesLines;

                AxisInfo axisInfo = new AxisInfo();
                axisInfo.xDirection = totalRotationAmount * Vector3.right;
                axisInfo.yDirection = totalRotationAmount * Vector3.up;
                axisInfo.zDirection = totalRotationAmount * Vector3.forward;
                SetCircles(axisInfo, drawCurrentCirclesLines);
            }

            DrawCircles(rotationAxisVector.x, xColor);
            DrawCircles(rotationAxisVector.y, yColor);
            DrawCircles(rotationAxisVector.z, zColor);
            DrawCircles(rotationAxisVector.all, allColor);
        }

        void SetHandleLines()
        {
            handleLines.Clear();

            if (type == TransformType.Move)
            {
                if (setting.enableState == EnableState.Normal)
                {
                    handleLines.x.Add(target.position);
                    handleLines.x.Add(axisInfo.xAxisEnd);

                    handleLines.y.Add(target.position);
                    handleLines.y.Add(axisInfo.yAxisEnd);

                    handleLines.z.Add(target.position);
                    handleLines.z.Add(axisInfo.zAxisEnd);
                }
                else if (setting.enableState == EnableState.Clamp)
                {
                    var endPosx = (target.position + axisInfo.xAxisEnd) * 0.5f;
                    var startPosx = (target.position * 2 - endPosx);
                    handleLines.x.Add(startPosx);
                    handleLines.x.Add(endPosx);
                    if (setting.enableState == EnableState.Normal)
                    {
                        handleLines.y.Add(target.position);
                        handleLines.y.Add(axisInfo.yAxisEnd);
                    }
                    var endPosz = (target.position + axisInfo.zAxisEnd) * 0.5f;
                    var startPosz = (target.position * 2 - endPosz);
                    handleLines.z.Add(startPosz);
                    handleLines.z.Add(endPosz);
                }
            }
            else if (type == TransformType.Scale && setting.enableState == EnableState.Normal)
            {
                handleLines.x.Add(target.position);
                handleLines.x.Add(axisInfo.xAxisEnd);

                handleLines.y.Add(target.position);
                handleLines.y.Add(axisInfo.yAxisEnd);

                handleLines.z.Add(target.position);
                handleLines.z.Add(axisInfo.zAxisEnd);
            }
        }

        void SetHandleTriangles()
        {
            handleTriangles.Clear();

            if (type == TransformType.Move)
            {
                float triangleLength = setting.triangleSize * DistanceMultiplier;
                if (setting.enableState == EnableState.Normal)
                {
                    AddTriangles(axisInfo.xAxisEnd, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, triangleLength, handleTriangles.x);
                    AddTriangles(axisInfo.yAxisEnd, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, triangleLength, handleTriangles.y);
                    AddTriangles(axisInfo.zAxisEnd, axisInfo.zDirection, axisInfo.yDirection, axisInfo.xDirection, triangleLength, handleTriangles.z);
                }
                else
                {
                    var endPosx = (target.position + axisInfo.xAxisEnd) * 0.5f;
                    var endPosz = (target.position + axisInfo.zAxisEnd) * 0.5f;

                    AddTriangles(endPosx, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, triangleLength, handleTriangles.x);
                    AddTriangles(endPosz, axisInfo.zDirection, axisInfo.yDirection, axisInfo.xDirection, triangleLength, handleTriangles.z);
                }
            }
        }

        void AddTriangles(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            Vector3 endPoint = axisEnd + (axisDirection * (size * 2f));
            Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size / 2f);

            resultsBuffer.Add(baseSquare.bottomLeft);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.topRight);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.bottomRight);
            resultsBuffer.Add(baseSquare.topRight);

            for (int i = 0; i < 4; i++)
            {
                resultsBuffer.Add(baseSquare[i]);
                resultsBuffer.Add(baseSquare[i + 1]);
                resultsBuffer.Add(endPoint);
            }
        }

        void SetHandleSquares()
        {
            handleSquares.Clear();

            if (type == TransformType.Scale)
            {
                float boxLength = setting.boxSize * DistanceMultiplier;
                if (setting.enableState == EnableState.Normal)
                {
                    AddSquares(axisInfo.xAxisEnd, axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxLength, handleSquares.x);
                    AddSquares(axisInfo.yAxisEnd, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, boxLength, handleSquares.y);
                    AddSquares(axisInfo.zAxisEnd, axisInfo.zDirection, axisInfo.xDirection, axisInfo.yDirection, boxLength, handleSquares.z);
                }
                AddSquares(target.position - (axisInfo.xDirection * boxLength), axisInfo.xDirection, axisInfo.yDirection, axisInfo.zDirection, boxLength, handleSquares.all);
            }
        }

        void AddSquares(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
        {
            Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size);
            Square baseSquareEnd = GetBaseSquare(axisEnd + (axisDirection * (size * 2f)), axisOtherDirection1, axisOtherDirection2, size);

            resultsBuffer.Add(baseSquare.bottomLeft);
            resultsBuffer.Add(baseSquare.topLeft);
            resultsBuffer.Add(baseSquare.bottomRight);
            resultsBuffer.Add(baseSquare.topRight);

            resultsBuffer.Add(baseSquareEnd.bottomLeft);
            resultsBuffer.Add(baseSquareEnd.topLeft);
            resultsBuffer.Add(baseSquareEnd.bottomRight);
            resultsBuffer.Add(baseSquareEnd.topRight);

            for (int i = 0; i < 4; i++)
            {
                resultsBuffer.Add(baseSquare[i]);
                resultsBuffer.Add(baseSquare[i + 1]);
                resultsBuffer.Add(baseSquareEnd[i + 1]);
                resultsBuffer.Add(baseSquareEnd[i]);
            }
        }

        Square GetBaseSquare(Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size)
        {
            Square square;
            Vector3 offsetUp = ((axisOtherDirection1 * size) + (axisOtherDirection2 * size));
            Vector3 offsetDown = ((axisOtherDirection1 * size) - (axisOtherDirection2 * size));
            //These arent really the proper directions, as in the bottomLeft isnt really at the bottom left...
            square.bottomLeft = axisEnd + offsetDown;
            square.topLeft = axisEnd + offsetUp;
            square.bottomRight = axisEnd - offsetDown;
            square.topRight = axisEnd - offsetUp;
            return square;
        }

        void SetCircles(AxisInfo axisInfo, AxisVectors axisVectors)
        {
            axisVectors.Clear();

            if (type == TransformType.Rotate)
            {
                float circleLength = setting.handleLength * DistanceMultiplier;
                if (setting.enableState == EnableState.Normal)
                {
                    AddCircle(target.position, axisInfo.xDirection, circleLength, axisVectors.x);
                    AddCircle(target.position, axisInfo.zDirection, circleLength, axisVectors.z);
                    AddCircle(target.position, (target.position - myCamera.transform.position).normalized, circleLength, axisVectors.all, false);
                }
                AddCircle(target.position, axisInfo.yDirection, circleLength, axisVectors.y);
            }
        }

        void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer, bool depthTest = true)
        {
            Vector3 up = axisDirection.normalized * size;
            Vector3 forward = Vector3.Slerp(up, -up, .5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * size;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = right.x;
            matrix[1] = right.y;
            matrix[2] = right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = forward.x;
            matrix[9] = forward.y;
            matrix[10] = forward.z;

            Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;
            float multiplier = 360f / setting.circleDetail;

            Plane plane = new Plane((myCamera.transform.position - target.position).normalized, target.position);

            for (var i = 0; i < setting.circleDetail + 1; i++)
            {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                if (!depthTest || plane.GetSide(lastPoint)|| setting.enableState == EnableState.Clamp)
                {
                    resultsBuffer.Add(lastPoint);
                    resultsBuffer.Add(nextPoint);
                }

                lastPoint = nextPoint;
            }
        }

        void DrawLines(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 2)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
            }

            GL.End();
        }

        void DrawTriangles(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 3)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
            }

            GL.End();
        }

        void DrawSquares(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.QUADS);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 4)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
                GL.Vertex(lines[i + 2]);
                GL.Vertex(lines[i + 3]);
            }

            GL.End();
        }

        void DrawCircles(List<Vector3> lines, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int i = 0; i < lines.Count; i += 2)
            {
                GL.Vertex(lines[i]);
                GL.Vertex(lines[i + 1]);
            }

            GL.End();
        }

    }
}