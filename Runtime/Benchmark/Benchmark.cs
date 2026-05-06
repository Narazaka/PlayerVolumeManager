using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Diagnostics;

namespace Narazaka.VRChat.PlayerVolumeManager.Benchmark
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class Benchmark : UdonSharpBehaviour
    {
        [SerializeField] protected int _count = 10000;

        void Start()
        {
            SendCustomEventDelayedFrames(nameof(_Run), Random.Range(60, 180));
        }

        public void _Run()
        {
            _PrepareAll();

            var sw0 = new Stopwatch();
            sw0.Start();
            _RunNeutral();
            sw0.Stop();

            _Prepare1();
            var sw1 = new Stopwatch();
            sw1.Start();
            _Run1();
            sw1.Stop();
            _Clean1();

            _Prepare2();
            var sw2 = new Stopwatch();
            sw2.Start();
            _Run2();
            sw2.Stop();
            _Clean2();

            _CleanAll();

            var typeName = GetType().Name;
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}]({_count} times) completed in");
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}] N: {sw0.Elapsed.TotalMilliseconds} ms ({sw0.Elapsed.TotalMilliseconds / _count * 1000} µs per iteration)");
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}] 1: {sw1.Elapsed.TotalMilliseconds} ms ({sw1.Elapsed.TotalMilliseconds / _count * 1000} µs per iteration)");
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}] 2: {sw2.Elapsed.TotalMilliseconds} ms ({sw2.Elapsed.TotalMilliseconds / _count * 1000} µs per iteration)");
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}] 1-N: {(sw1.Elapsed - sw0.Elapsed).TotalMilliseconds} ms ({(sw1.Elapsed - sw0.Elapsed).TotalMilliseconds / _count * 1000} µs per iteration)");
            UnityEngine.Debug.Log($"[Benchmark][{name}][{typeName}] 2-N: {(sw2.Elapsed - sw0.Elapsed).TotalMilliseconds} ms ({(sw2.Elapsed - sw0.Elapsed).TotalMilliseconds / _count * 1000} µs per iteration)");
        }

        protected virtual void _PrepareAll() { }
        protected virtual void _Prepare1() { }
        protected virtual void _Prepare2() { }
        protected virtual void _Clean1() { }
        protected virtual void _Clean2() { }
        protected virtual void _CleanAll() { }

        protected abstract void _RunNeutral();

        protected abstract void _Run1();

        protected abstract void _Run2();
    }
}
