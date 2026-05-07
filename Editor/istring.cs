using UnityEngine;

namespace Narazaka.VRChat.PlayerVolumeManager.Editor
{
#pragma warning disable IDE1006
    public class istring
#pragma warning restore IDE1006
    {
        public string en;
        public string ja;
        public string enTooltip;
        public string jaTooltip;

        public istring(string en, string ja)
        {
            this.en = en;
            this.ja = ja;
        }

        public istring(string en, string ja, string enTooltip, string jaTooltip)
        {
            this.en = en;
            this.ja = ja;
            this.enTooltip = enTooltip;
            this.jaTooltip = jaTooltip;
        }

        public GUIContent GUIContent
        {
            get
            {
                var tip = Language.Current == Language.Ja ? jaTooltip : enTooltip;
                return tip != null ? new GUIContent(this, tip) : new GUIContent(this);
            }
        }

        public static implicit operator string(istring data) => Language.Current == Language.Ja ? data.ja : data.en;
    }
}
