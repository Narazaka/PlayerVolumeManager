using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager
{
    [AddComponentMenu("PlayerVolumeManager/PlayerVolumeGroup (Area)")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerVolumeGroupArea : PlayerVolumeGroup
    {
        [SerializeField] Collider[] _targets;
        [SerializeField] bool _isStatic = true;

        bool canUseBoundsForCollision;
        Bounds staticAllBounds;
        bool[] effectives;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_isStatic)
            {
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
            if (canUseBoundsForCollision)
            {
                return true;
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
            var effectiveCount = 0;
            Collider firstCollider = null;
            for (var i = 0; i < len; i++)
            {
                var target = _targets[i];
                if (target != null && target.enabled && target.gameObject.activeInHierarchy)
                {
                    if (effectiveCount == 0)
                    {
                        staticAllBounds = target.bounds;
                        firstCollider = target;
                    }
                    else
                    {
                        staticAllBounds.Encapsulate(target.bounds);
                    }
                    effectiveCount++;
                    effectives[i] = true;
                }
            }

            canUseBoundsForCollision =
                effectiveCount == 1
                && firstCollider.GetType() == typeof(BoxCollider)
                && AxisAllignedChecker.IsRotationAxisAligned2(firstCollider.transform);
        }
    }
}
