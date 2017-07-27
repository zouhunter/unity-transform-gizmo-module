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

        private Protocal protocal { get; set; }
        private AxisSetting setting { get; set; }
        public AxisDrawer(Protocal protocal, AxisSetting setting)
        {
            this.protocal = protocal;
            this.setting = setting;
            lineMaterial = new Material(Shader.Find("Custom/Lines"));
        }
        public void LateUpdate()
        {
            if (protocal.target == null) return;
            SetLines();
        }

        public void OnPostRender()
        { 
            lineMaterial.SetPass(0);

            Color xColor = (protocal.selectedAxis == Axis.X) ? selectedColor : this.xColor;
            Color yColor = (protocal.selectedAxis == Axis.Y) ? selectedColor : this.yColor;
            Color zColor = (protocal.selectedAxis == Axis.Z) ? selectedColor : this.zColor;
            Color allColor = (protocal.selectedAxis == Axis.Any) ? selectedColor : this.allColor;

            DrawLines(protocal.handleLines.x, xColor);
            DrawLines(protocal.handleLines.y, yColor);
            DrawLines(protocal.handleLines.z, zColor);

            DrawTriangles(protocal.handleTriangles.x, xColor);
            DrawTriangles(protocal.handleTriangles.y, yColor);
            DrawTriangles(protocal.handleTriangles.z, zColor);

            DrawSquares(protocal.handleSquares.x, xColor);
            DrawSquares(protocal.handleSquares.y, yColor);
            DrawSquares(protocal.handleSquares.z, zColor);
            DrawSquares(protocal.handleSquares.all, allColor);

            AxisVectors rotationAxisVector = protocal.circlesLines;
            if (protocal.isTransforming && protocal.space == TransformSpace.Global && protocal.type == TransformType.Rotate)
            {
                rotationAxisVector = protocal.drawCurrentCirclesLines;

                AxisInfo axisInfo = new AxisInfo();
                axisInfo.xDirection = protocal. totalRotationAmount * Vector3.right;
                axisInfo.yDirection = protocal.totalRotationAmount * Vector3.up;
                axisInfo.zDirection = protocal.totalRotationAmount * Vector3.forward;
                SetCircles(axisInfo, protocal.drawCurrentCirclesLines);
            }

            DrawCircles(rotationAxisVector.x, xColor);
            DrawCircles(rotationAxisVector.y, yColor);
            DrawCircles(rotationAxisVector.z, zColor);
            DrawCircles(rotationAxisVector.all, allColor);
        }

        void SetLines()
        {
            SetHandleLines();
            SetHandleTriangles();
            SetHandleSquares();
            SetCircles(protocal.axisInfo, protocal.circlesLines);
        }

        void SetHandleLines()
        {
            protocal.handleLines.Clear();

            if (protocal.type == TransformType.Move || protocal.type == TransformType.Scale)
            {
                protocal.handleLines.x.Add(protocal.target.position);
                protocal.handleLines.x.Add(protocal.axisInfo.xAxisEnd);
                protocal.handleLines.y.Add(protocal.target.position);
                protocal.handleLines.y.Add(protocal.axisInfo.yAxisEnd);
                protocal.handleLines.z.Add(protocal.target.position);
                protocal.handleLines.z.Add(protocal.axisInfo.zAxisEnd);
            }
        }

        void SetHandleTriangles()
        {
            protocal.handleTriangles.Clear();

            if (protocal.type == TransformType.Move)
            {
                float triangleLength =setting.triangleSize * protocal.GetDistanceMultiplier();
                AddTriangles(protocal.axisInfo.xAxisEnd, protocal.axisInfo.xDirection, protocal.axisInfo.yDirection, protocal.axisInfo.zDirection, triangleLength, protocal.handleTriangles.x);
                AddTriangles(protocal.axisInfo.yAxisEnd, protocal.axisInfo.yDirection, protocal.axisInfo.xDirection, protocal.axisInfo.zDirection, triangleLength, protocal.handleTriangles.y);
                AddTriangles(protocal.axisInfo.zAxisEnd, protocal.axisInfo.zDirection, protocal.axisInfo.yDirection, protocal.axisInfo.xDirection, triangleLength, protocal.handleTriangles.z);
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
            protocal.handleSquares.Clear();

            if (protocal.type == TransformType.Scale)
            {
                float boxLength =setting. boxSize * protocal. GetDistanceMultiplier();
                AddSquares(protocal.axisInfo.xAxisEnd, protocal.axisInfo.xDirection, protocal.axisInfo.yDirection, protocal.axisInfo.zDirection, boxLength, protocal.handleSquares.x);
                AddSquares(protocal.axisInfo.yAxisEnd, protocal.axisInfo.yDirection, protocal.axisInfo.xDirection, protocal.axisInfo.zDirection, boxLength, protocal.handleSquares.y);
                AddSquares(protocal.axisInfo.zAxisEnd, protocal.axisInfo.zDirection, protocal.axisInfo.xDirection, protocal.axisInfo.yDirection, boxLength, protocal.handleSquares.z);
                AddSquares(protocal.target.position - (protocal.axisInfo.xDirection * boxLength), protocal.axisInfo.xDirection, protocal.axisInfo.yDirection, protocal.axisInfo.zDirection, boxLength, protocal.handleSquares.all);
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

            if (protocal. type == TransformType.Rotate)
            {
                float circleLength =setting. handleLength * protocal.GetDistanceMultiplier();
                AddCircle(protocal.target.position, axisInfo.xDirection, circleLength, axisVectors.x);
                AddCircle(protocal.target.position, axisInfo.yDirection, circleLength, axisVectors.y);
                AddCircle(protocal.target.position, axisInfo.zDirection, circleLength, axisVectors.z);
                AddCircle(protocal.target.position, (protocal.target.position - protocal.myCamera.transform.position).normalized, circleLength, axisVectors.all, false);
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
            float multiplier = 360f /setting. circleDetail;

            Plane plane = new Plane((protocal.myCamera.transform.position - protocal.target.position).normalized, protocal.target.position);

            for (var i = 0; i <setting.circleDetail + 1; i++)
            {
                nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);

                if (!depthTest || plane.GetSide(lastPoint))
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