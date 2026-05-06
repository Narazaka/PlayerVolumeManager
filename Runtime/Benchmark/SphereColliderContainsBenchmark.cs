using System.Drawing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager.Benchmark
{
    public class SphereColliderContainsBenchmark : Benchmark
    {
        [SerializeField] SphereCollider[] _targets;

        protected override void _RunNeutral()
        {
            Vector3 playerPosition;
            for (var i = 0; i < _count; i += 5)
            {
                playerPosition = Random.insideUnitSphere * 10f;
                EmptyFunc(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                EmptyFunc(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                EmptyFunc(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                EmptyFunc(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                EmptyFunc(playerPosition);
            }
        }
        void EmptyFunc(Vector3 t) { }

        protected override void _Run1()
        {
            Vector3 playerPosition;
            for (var i = 0; i < _count; i += 5)
            {
                playerPosition = Random.insideUnitSphere * 10f;
                Func1(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func1(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func1(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func1(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func1(playerPosition);
            }
        }

        protected override void _Run2()
        {
            Vector3 playerPosition;
            for (var i = 0; i < _count; i += 5)
            {
                playerPosition = Random.insideUnitSphere * 10f;
                Func2(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func2(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func2(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func2(playerPosition);
                playerPosition = Random.insideUnitSphere * 10f;
                Func2(playerPosition);
            }
        }

        void Func1(Vector3 playerPosition)
        {
            var a = false;
            foreach (var target in _targets)
            {
                if (
                    target.bounds.Contains(playerPosition)
                    && playerPosition == target.ClosestPoint(playerPosition)
                    )
                {
                    a = true;
                }
            }
        }


        Vector3[] centers;
        float[] sqrRadiuses;
        protected override void _Prepare2()
        {
            var len = _targets.Length;
            centers = new Vector3[len];
            sqrRadiuses = new float[len];
            for (var i = 0; i < len; i++)
            {
                var target = _targets[i];
                centers[i] = target.transform.TransformPoint(target.center);
                var scale = target.transform.lossyScale;
                var radius = target.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
                sqrRadiuses[i] = radius * radius;
            }
        }

        void Func2(Vector3 playerPosition)
        {
            var a = false;
            var len = _targets.Length;
            for (var i = 0; i < len; i++)
            {
                var target = _targets[i];
                if (
                    target.bounds.Contains(playerPosition)
                    && (playerPosition - centers[i]).sqrMagnitude <= sqrRadiuses[i]
                    )
                {
                    a = true;
                }
            }
        }
    }
}
