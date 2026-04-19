using UnityEngine;
using UnityEngine.UI;

namespace NavigationDemo.UI.Base
{
    [AddComponentMenu("UI/Ring Graphic")]
    public class RingGraphic : MaskableGraphic
    {
        [SerializeField] [Min(0f)] private float _thickness = 4f;
        [SerializeField] [Range(0f, 1f)] private float _fillAmount = 1f;
        [SerializeField] [Range(8, 360)] private int _segments = 120;
        [SerializeField] private float _startAngle = -90f;
        [SerializeField] private bool _clockwise = true;

        public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public float FillAmount
        {
            get => _fillAmount;
            set
            {
                _fillAmount = Mathf.Clamp01(value);
                SetVerticesDirty();
            }
        }

        public int Segments
        {
            get => _segments;
            set
            {
                _segments = Mathf.Clamp(value, 8, 360);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper value)
        {
            value.Clear();

            Rect valueRect = GetPixelAdjustedRect();
            float valueRadius = Mathf.Min(valueRect.width, valueRect.height) * 0.5f;
            if (valueRadius <= 0.01f)
            {
                return;
            }

            float valueThickness = Mathf.Clamp(_thickness, 0f, valueRadius);
            if (valueThickness <= 0.01f)
            {
                return;
            }

            float valueFillAmount = Mathf.Clamp01(_fillAmount);
            if (valueFillAmount <= 0f)
            {
                return;
            }

            int valueSegments = Mathf.Clamp(_segments, 8, 360);
            int valueUsedSegments = Mathf.Max(1, Mathf.RoundToInt(valueSegments * valueFillAmount));
            float valueDirection = _clockwise ? -1f : 1f;
            float valueAngleStep = 360f / valueSegments * valueDirection;
            float valueOuterRadius = valueRadius;
            float valueInnerRadius = valueRadius - valueThickness;
            Vector2 valueCenter = valueRect.center;

            UIVertex valueVertex = UIVertex.simpleVert;
            valueVertex.color = color;

            for (int index = 0; index <= valueUsedSegments; index++)
            {
                float valueAngle = (_startAngle + valueAngleStep * index) * Mathf.Deg2Rad;
                Vector2 valueDirectionVector = new Vector2(Mathf.Cos(valueAngle), Mathf.Sin(valueAngle));

                Vector2 valueOuter = valueCenter + valueDirectionVector * valueOuterRadius;
                valueVertex.position = valueOuter;
                valueVertex.uv0 = Vector2.zero;
                value.AddVert(valueVertex);

                Vector2 valueInner = valueCenter + valueDirectionVector * valueInnerRadius;
                valueVertex.position = valueInner;
                valueVertex.uv0 = Vector2.zero;
                value.AddVert(valueVertex);
            }

            for (int index = 0; index < valueUsedSegments; index++)
            {
                int valueStart = index * 2;
                int valueNext = valueStart + 2;

                value.AddTriangle(valueStart, valueNext, valueStart + 1);
                value.AddTriangle(valueStart + 1, valueNext, valueNext + 1);
            }
        }
    }
}