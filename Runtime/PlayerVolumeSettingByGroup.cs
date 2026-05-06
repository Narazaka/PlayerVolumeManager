using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeSettingByGroup : PlayerVolumeSetting
    {
        [SerializeField] public PlayerVolumeGroup _from;
    }
}
