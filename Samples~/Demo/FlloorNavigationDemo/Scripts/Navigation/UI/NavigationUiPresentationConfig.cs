using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavigationDemo.Navigation.UI
{
    [CreateAssetMenu(
        fileName = "NavigationUiPresentationConfig",
        menuName = "Navigation/UI Presentation Config")]
    public class NavigationUiPresentationConfig : ScriptableObject
    {
        [Serializable]
        public class TurnDirectionPresentation
        {
            [SerializeField] private RouteTurnDirection _direction;
            [SerializeField] private Sprite _iconSprite;
            [SerializeField] private string _iconFallback = "\u2191";
            [SerializeField] private string _title = "\u0418\u0434\u0438\u0442\u0435 \u043F\u0440\u044F\u043C\u043E";

            public RouteTurnDirection Direction => _direction;
            public Sprite IconSprite => _iconSprite;
            public string IconFallback => _iconFallback;
            public string Title => _title;
        }

        [Serializable]
        public class TransitionInstructionPresentation
        {
            [SerializeField] private FloorTransitionInstruction _instruction;
            [SerializeField] private Sprite _iconSprite;
            [SerializeField] private string _iconFallback = "\u21C5";
            [SerializeField] private string _subtitle = "\u0427\u0442\u043E\u0431\u044B \u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u0442\u044C,";
            [SerializeField] private string _titleTemplate =
                "\u041F\u0435\u0440\u0435\u0439\u0434\u0438\u0442\u0435 \u043D\u0430 \u044D\u0442\u0430\u0436 {floor}";

            public FloorTransitionInstruction Instruction => _instruction;
            public Sprite IconSprite => _iconSprite;
            public string IconFallback => _iconFallback;
            public string Subtitle => _subtitle;
            public string TitleTemplate => _titleTemplate;
        }

        [Header("Direction Presets")]
        [SerializeField] private List<TurnDirectionPresentation> _turnDirectionPresentations =
            new List<TurnDirectionPresentation>();

        [Header("Floor Transition Presets")]
        [SerializeField] private List<TransitionInstructionPresentation> _transitionPresentations =
            new List<TransitionInstructionPresentation>();

        [Header("Distance And Time Format")]
        [SerializeField] [Min(0)] private int _distanceDigitsAfterDot;
        [SerializeField] [Min(0)] private int _timeDigitsAfterDot;
        [SerializeField] private string _metersSuffix = "\u043C";
        [SerializeField] private string _minutesSuffix = "\u043C\u0438\u043D";

        public void GetTurnPresentation(
            RouteTurnDirection direction,
            out Sprite iconSprite,
            out string iconFallback,
            out string title)
        {
            TurnDirectionPresentation preset = FindTurnPresentation(direction);
            if (preset != null)
            {
                iconSprite = preset.IconSprite;
                iconFallback = preset.IconFallback;
                title = preset.Title;
                return;
            }

            iconSprite = null;
            iconFallback = GetDefaultTurnIcon(direction);
            title = GetDefaultTurnTitle(direction);
        }

        public void GetTransitionPresentation(
            FloorTransitionInstruction instruction,
            int destinationFloorId,
            out Sprite iconSprite,
            out string iconFallback,
            out string subtitle,
            out string title)
        {
            TransitionInstructionPresentation preset = FindTransitionPresentation(instruction);
            if (preset != null)
            {
                iconSprite = preset.IconSprite;
                iconFallback = preset.IconFallback;
                subtitle = preset.Subtitle;
                title = ResolveFloorTemplate(preset.TitleTemplate, destinationFloorId);
                return;
            }

            iconSprite = null;
            iconFallback = GetDefaultTransitionIcon(instruction);
            subtitle = "\u0427\u0442\u043E\u0431\u044B \u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u0442\u044C,";
            title = ResolveFloorTemplate(GetDefaultTransitionTitle(instruction), destinationFloorId);
        }

        public string FormatDistance(float meters)
        {
            if (float.IsInfinity(meters))
            {
                return "\u221E";
            }

            if (meters < 1f)
            {
                return $"< 1 {_metersSuffix}";
            }

            return $"{meters.ToString($"F{_distanceDigitsAfterDot}")} {_metersSuffix}";
        }

        public string FormatTime(float seconds)
        {
            if (float.IsInfinity(seconds))
            {
                return "\u221E";
            }

            float minutes = Mathf.Max(0f, seconds) / 60f;
            if (minutes < 1f)
            {
                return $"< 1 {_minutesSuffix}";
            }

            return $"{minutes.ToString($"F{_timeDigitsAfterDot}")} {_minutesSuffix}";
        }

        private TurnDirectionPresentation FindTurnPresentation(RouteTurnDirection direction)
        {
            for (int i = 0; i < _turnDirectionPresentations.Count; i++)
            {
                TurnDirectionPresentation preset = _turnDirectionPresentations[i];
                if (preset != null && preset.Direction == direction)
                {
                    return preset;
                }
            }

            return null;
        }

        private TransitionInstructionPresentation FindTransitionPresentation(FloorTransitionInstruction instruction)
        {
            for (int i = 0; i < _transitionPresentations.Count; i++)
            {
                TransitionInstructionPresentation preset = _transitionPresentations[i];
                if (preset != null && preset.Instruction == instruction)
                {
                    return preset;
                }
            }

            return null;
        }

        private static string ResolveFloorTemplate(string template, int destinationFloorId)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            return template.Replace("{floor}", destinationFloorId.ToString());
        }

        private static string GetDefaultTurnIcon(RouteTurnDirection direction)
        {
            return direction switch
            {
                RouteTurnDirection.Right => "\u2192",
                RouteTurnDirection.Left => "\u2190",
                RouteTurnDirection.Back => "\u21B6",
                _ => "\u2191"
            };
        }

        private static string GetDefaultTurnTitle(RouteTurnDirection direction)
        {
            return direction switch
            {
                RouteTurnDirection.Right => "\u041F\u043E\u0432\u0435\u0440\u043D\u0438\u0442\u0435 \u043D\u0430\u043F\u0440\u0430\u0432\u043E",
                RouteTurnDirection.Left => "\u041F\u043E\u0432\u0435\u0440\u043D\u0438\u0442\u0435 \u043D\u0430\u043B\u0435\u0432\u043E",
                RouteTurnDirection.Back => "\u0420\u0430\u0437\u0432\u0435\u0440\u043D\u0438\u0442\u0435\u0441\u044C \u043D\u0430\u0437\u0430\u0434",
                _ => "\u0414\u0432\u0438\u0433\u0430\u0439\u0442\u0435\u0441\u044C \u043F\u0440\u044F\u043C\u043E"
            };
        }

        private static string GetDefaultTransitionIcon(FloorTransitionInstruction instruction)
        {
            return instruction switch
            {
                FloorTransitionInstruction.Up => "\u21E7",
                FloorTransitionInstruction.Down => "\u21E9",
                _ => "\u21C5"
            };
        }

        private static string GetDefaultTransitionTitle(FloorTransitionInstruction instruction)
        {
            return instruction switch
            {
                FloorTransitionInstruction.Up => "\u041F\u043E\u0434\u043D\u0438\u043C\u0438\u0442\u0435\u0441\u044C \u043D\u0430 \u044D\u0442\u0430\u0436 {floor}",
                FloorTransitionInstruction.Down => "\u0421\u043F\u0443\u0441\u0442\u0438\u0442\u0435\u0441\u044C \u043D\u0430 \u044D\u0442\u0430\u0436 {floor}",
                _ => "\u0414\u043E\u0435\u0434\u044C\u0442\u0435 \u043D\u0430 \u043B\u0438\u0444\u0442\u0435 \u0434\u043E \u044D\u0442\u0430\u0436\u0430 {floor}"
            };
        }
    }
}
