using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/PlayerVolumeGroup (Switch)")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerVolumeGroupSwitch : PlayerVolumeGroup
    {
        [SerializeField] bool _assignWhenInteract;
        [SerializeField] bool _unassignWhenInteract;

        [SerializeField] bool _assignWhenPickup;
        [SerializeField] bool _unassignWhenDrop;

        [SerializeField] bool _assignWhenPickupUseDown;
        [SerializeField] bool _unassignWhenPickupUseDown;
        [SerializeField] bool _unassignWhenPickupUseUp;

        [UdonSynced] ushort _assignedPlayerId;

        public int assignedPlayerId => _assignedPlayerId;

        public override bool _ContainsPlayer(VRCPlayerApi player)
        {
            return base._ContainsPlayer(player) && _assignedPlayerId == player.playerId;
        }

        [PublicAPI]
        public void _AssignPlayer(int playerId)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(_DoAssignPlayer), (ushort)playerId);
        }

        [PublicAPI]
        public void _UnassignPlayer()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(_DoAssignPlayer), (ushort)0);
        }

        [PublicAPI]
        public void _ToggleAssignPlayer(int playerId)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(_DoToggleAssignPlayer), (ushort)playerId);
        }

        [NetworkCallable]
        public void _DoAssignPlayer(ushort playerId)
        {
            if (Networking.IsOwner(gameObject))
            {
                _assignedPlayerId = (ushort)playerId;
                RequestSerialization();
            }
        }

        [NetworkCallable]
        public void _DoToggleAssignPlayer(ushort playerId)
        {
            if (Networking.IsOwner(gameObject))
            {
                if (_assignedPlayerId == playerId)
                {
                    _assignedPlayerId = 0;
                }
                else
                {
                    _assignedPlayerId = (ushort)playerId;
                }
                RequestSerialization();
            }
        }

        public override void Interact()
        {
            if (_assignWhenInteract && _unassignWhenInteract)
            {
                _ToggleAssignPlayer(Networking.LocalPlayer.playerId);
            }
            else if (_assignWhenInteract)
            {
                _AssignPlayer(Networking.LocalPlayer.playerId);
            }
            else if (_unassignWhenInteract)
            {
                _UnassignPlayer();
            }
        }

        public override void OnPickup()
        {
            if (_assignWhenPickup)
            {
                _AssignPlayer(Networking.LocalPlayer.playerId);
            }
        }

        public override void OnDrop()
        {
            if (_unassignWhenDrop)
            {
                _UnassignPlayer();
            }
        }

        public override void OnPickupUseDown()
        {
            if (_assignWhenPickupUseDown && _unassignWhenPickupUseDown)
            {
                _ToggleAssignPlayer(Networking.LocalPlayer.playerId);
            }
            else if (_assignWhenPickupUseDown)
            {
                _AssignPlayer(Networking.LocalPlayer.playerId);
            }
            else if (_unassignWhenPickupUseDown)
            {
                _UnassignPlayer();
            }
        }

        public override void OnPickupUseUp()
        {
            if (_unassignWhenPickupUseUp)
            {
                _UnassignPlayer();
            }
        }
    }
}
