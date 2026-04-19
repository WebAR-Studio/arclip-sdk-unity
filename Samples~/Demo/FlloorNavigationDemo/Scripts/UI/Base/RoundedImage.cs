using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Base
{
    [AddComponentMenu("UI/Rounded Image")]
    public class RoundedImage : Image
    {
        [SerializeField] [Min(0f)] private float _radius = 12f;
        [SerializeField] private bool _useIndependentRadii;
        [SerializeField] private Vector4 _cornerRadii = new Vector4(12f, 12f, 12f, 12f);
        [SerializeField] [Range(1, 24)] private int _segmentsPerCorner = 12;

        public override Texture mainTexture => Texture2D.whiteTexture;

        protected override void OnPopulateMesh(VertexHelper value)
        {
            value.Clear();

            Rect valueRect = GetPixelAdjustedRect();
            if (valueRect.width <= 0f || valueRect.height <= 0f)
            {
                return;
            }

            Vector4 valueCornerRadii = GetClampedCornerRadii(valueRect);
            int valueSegmentsPerCorner = Mathf.Max(1, _segmentsPerCorner);
            List<Vector2> valuePoints = new List<Vector2>(valueSegmentsPerCorner * 4 + 4);

            AddCornerPoints(
                valuePoints,
                new Vector2(valueRect.xMin, valueRect.yMax),
                new Vector2(valueRect.xMin + valueCornerRadii.x, valueRect.yMax - valueCornerRadii.x),
                valueCornerRadii.x,
                180f,
                90f,
                valueSegmentsPerCorner);
            AddCornerPoints(
                valuePoints,
                new Vector2(valueRect.xMax, valueRect.yMax),
                new Vector2(valueRect.xMax - valueCornerRadii.y, valueRect.yMax - valueCornerRadii.y),
                valueCornerRadii.y,
                90f,
                0f,
                valueSegmentsPerCorner);
            AddCornerPoints(
                valuePoints,
                new Vector2(valueRect.xMax, valueRect.yMin),
                new Vector2(valueRect.xMax - valueCornerRadii.z, valueRect.yMin + valueCornerRadii.z),
                valueCornerRadii.z,
                0f,
                -90f,
                valueSegmentsPerCorner);
            AddCornerPoints(
                valuePoints,
                new Vector2(valueRect.xMin, valueRect.yMin),
                new Vector2(valueRect.xMin + valueCornerRadii.w, valueRect.yMin + valueCornerRadii.w),
                valueCornerRadii.w,
                -90f,
                -180f,
                valueSegmentsPerCorner);

            if (valuePoints.Count < 3)
            {
                return;
            }

            Color32 valueColor = color;
            UIVertex valueVertex = UIVertex.simpleVert;
            valueVertex.color = valueColor;
            valueVertex.position = valueRect.center;
            valueVertex.uv0 = Vector2.zero;
            value.AddVert(valueVertex);

            int valueCount = valuePoints.Count;
            for (int index = 0; index < valueCount; index++)
            {
                valueVertex.position = valuePoints[index];
                valueVertex.uv0 = Vector2.zero;
                value.AddVert(valueVertex);
            }

            for (int index = 0; index < valueCount; index++)
            {
                int valueCurrent = index + 1;
                int valueNext = index + 1 == valueCount ? 1 : index + 2;
                value.AddTriangle(0, valueCurrent, valueNext);
            }
        }

        private Vector4 GetClampedCornerRadii(Rect value)
        {
            Vector4 valueCornerRadii = _useIndependentRadii
                ? _cornerRadii
                : new Vector4(_radius, _radius, _radius, _radius);

            float valueHalfWidth = value.width * 0.5f;
            float valueHalfHeight = value.height * 0.5f;
            float valueClamp = Mathf.Min(valueHalfWidth, valueHalfHeight);

            valueCornerRadii.x = Mathf.Clamp(valueCornerRadii.x, 0f, valueClamp);
            valueCornerRadii.y = Mathf.Clamp(valueCornerRadii.y, 0f, valueClamp);
            valueCornerRadii.z = Mathf.Clamp(valueCornerRadii.z, 0f, valueClamp);
            valueCornerRadii.w = Mathf.Clamp(valueCornerRadii.w, 0f, valueClamp);

            return valueCornerRadii;
        }

        private static void AddCornerPoints(
            List<Vector2> value,
            Vector2 valueCornerPoint,
            Vector2 valueCenter,
            float valueRadius,
            float valueStartAngle,
            float valueEndAngle,
            int valueSegments)
        {
            if (valueRadius <= 0.001f)
            {
                if (!ContainsPoint(value, valueCornerPoint))
                {
                    value.Add(valueCornerPoint);
                }

                return;
            }

            for (int index = 0; index <= valueSegments; index++)
            {
                float valueLerp = (float)index / valueSegments;
                float valueAngle = Mathf.Lerp(valueStartAngle, valueEndAngle, valueLerp) * Mathf.Deg2Rad;
                Vector2 valuePoint = valueCenter + new Vector2(Mathf.Cos(valueAngle), Mathf.Sin(valueAngle)) * valueRadius;

                if (ContainsPoint(value, valuePoint))
                {
                    continue;
                }

                value.Add(valuePoint);
            }
        }

        private static bool ContainsPoint(List<Vector2> value, Vector2 valuePoint)
        {
            if (value.Count == 0)
            {
                return false;
            }

            Vector2 valueLast = value[value.Count - 1];
            return Vector2.SqrMagnitude(valueLast - valuePoint) < 0.01f;
        }

    }
}
