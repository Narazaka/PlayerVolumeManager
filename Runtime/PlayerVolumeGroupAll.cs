using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/PlayerVolumeGroup (All)")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeGroupAll : PlayerVolumeGroup
    {
        public override bool _ContainsPlayer(VRCPlayerApi player)
        {
            return true;
        }
    }
}
