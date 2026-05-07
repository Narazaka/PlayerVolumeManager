using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/Group Setting/Player Volume Setting By Group")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeSettingByGroup : PlayerVolumeSetting
    {
    }
}
