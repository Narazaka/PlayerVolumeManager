using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeGroupArea : PlayerVolumeGroup
    {
        [SerializeField] Collider[] _targets;
        [SerializeField] bool _isStatic = true;

        Bounds staticAllBounds;
        bool[] effectives;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isStatic) {
                CalcAllBounds();
            }
        }

        public override bool _ContainsPlayer(VRCPlayerApi player)
        {
            var playerPosition = player.GetPosition();
            return _isStatic ? ContainsPlayerStatic(playerPosition) : ContainsPlayerDynamic(playerPosition);
        }

        bool ContainsPlayerStatic(Vector3 playerPosition)
        {
            if (!staticAllBounds.Contains(playerPosition))
            {
                return false;
            }
            var len = _targets.Length;
            for (var i = 0; i < len; i++)
            {
                var target = _targets[i];
                if (
                    effectives[i]
                    && target.bounds.Contains(playerPosition)
                    && playerPosition == target.ClosestPoint(playerPosition)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        bool ContainsPlayerDynamic(Vector3 playerPosition)
        {
            foreach (var target in _targets)
            {
                if (
                    target != null
                    && target.enabled
                    && target.gameObject.activeInHierarchy
                    && target.bounds.Contains(playerPosition)
                    && playerPosition == target.ClosestPoint(playerPosition)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        void CalcAllBounds()
        {
            var len = _targets.Length;
            effectives = new bool[len];
            var initialized = false;
            for (var i = 0; i < len; i++) {
                var target = _targets[i];
                if (target != null && target.enabled && target.gameObject.activeInHierarchy)
                {
                    if (!initialized)
                    {
                        staticAllBounds = target.bounds;
                        initialized = true;
                    }
                    else
                    {
                        staticAllBounds.Encapsulate(target.bounds);
                    }
                    effectives[i] = true;
                }
            }
        }
    }
}
