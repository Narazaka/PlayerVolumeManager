using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/Group Setting/Player Volume Listen Pair")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeListenPair : UdonSharpBehaviour
    {
        public PlayerVolumeGroup _group;
        public PlayerVolumeSettingByGroup _setting;
    }
}
