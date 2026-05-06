using System.Drawing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager.Benchmark
{
    public class BoxColliderContainsBenchmark : Benchmark
    {
        [SerializeField] BoxCollider[] _targets;

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


        Matrix4x4[] worldToLocals;
        Bounds[] bounds;
        protected override void _Prepare2()
        {
            var len = _targets.Length;
            worldToLocals = new Matrix4x4[len];
            bounds = new Bounds[len];
            for (var i = 0; i < len; i++)
            {
                var target = _targets[i];
                worldToLocals[i] = target.transform.worldToLocalMatrix;
                bounds[i] = new Bounds(target.center, target.size);
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
                    && bounds[i].Contains(worldToLocals[i].MultiplyPoint3x4(playerPosition))
                    )
                {
                    a = true;
                }
            }
        }
    }
}
