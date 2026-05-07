using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/Group Setting/Player Volume Override Setting")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeOverrideSetting : PlayerVolumeSetting
    {
    }
}
