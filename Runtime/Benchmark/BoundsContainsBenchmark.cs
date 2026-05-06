using System.Drawing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.PlayerVolumeManager.Benchmark
{
    public class BoundsContainsBenchmark : Benchmark
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
                    playerPosition == target.ClosestPoint(playerPosition)
                    )
                {
                    a = true;
                }
            }
        }

        Bounds[] bounds;
        protected override void _Prepare2()
        {
            var len = _targets.Length;
            bounds = new Bounds[len];
            for (var i = 0; i < len; i++)
            {
                bounds[i] = _targets[i].bounds;
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
                    bounds[i].Contains(playerPosition)
                    )
                {
                    a = true;
                }
            }
        }
    }
}
