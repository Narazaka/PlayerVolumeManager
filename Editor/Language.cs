using UnityEditor;
using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
    public static class Language
    {
        public const string En = "en";
        public const string Ja = "ja";

        const string PrefKey = "Narazaka.VRChat.PlayerVolumeManager.Language";

        public static string Current
        {
            get => EditorPrefs.GetString(PrefKey, GetDefault());
            set => EditorPrefs.SetString(PrefKey, value == Ja ? Ja : En);
        }

        static string GetDefault()
        {
            return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == Ja ? Ja : En;
        }

        static readonly GUIContent _label = new GUIContent("Language");
        static readonly GUIContent[] _displayOptions =
        {
            new GUIContent("English"),
            new GUIContent("日本語"),
        };
        static readonly string[] _codes = { En, Ja };

        public static void DrawDropdownLayout()
        {
            var current = Current;
            var index = current == Ja ? 1 : 0;
            var newIndex = EditorGUILayout.Popup(_label, index, _displayOptions);
            if (newIndex != index) Current = _codes[newIndex];
        }
    }
}
