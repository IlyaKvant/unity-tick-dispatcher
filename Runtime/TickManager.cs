using UnityEngine;
#if !VCONTAINER_SUPPORT
using System;
#endif

namespace UnityTickDispatcher
{
    public sealed class TickManager : MonoBehaviour
    {
        #if !VCONTAINER_SUPPORT
        [SerializeField] private LoopTiming loopTiming = LoopTiming.All;

        public static bool IsInitialized => _instance != null;

        private static TickManager _instance;

        private TickDispatcher TickDispatcher { get; set; }

        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticOnLoad()
        {
            _instance = null;
        }
        #endif

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            TickDispatcher = new TickDispatcher(loopTiming);
        }

        private void FixedUpdate()
        {
            TickDispatcher.FixedTick();
        }

        private void Update()
        {
            TickDispatcher.Tick();
            TickDispatcher.PostTick();
        }

        private void LateUpdate()
        {
            TickDispatcher.LateTick();
            TickDispatcher.PostLateTick();
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            _instance = null;
            TickDispatcher = null;
        }

        public static void Optimize()
        {
            if (!IsInitialized)
            {
                return;
            }

            _instance.TickDispatcher.Optimize();
        }

        public static IDisposable Subscribe(Action action, LoopTiming loopTiming = LoopTiming.Update)
            => IsInitialized ? _instance.TickDispatcher.Subscribe(action, loopTiming) : TickDispatcher.NoopDisposable;

        public static TickHandle SubscribeAsHandle(Action action, LoopTiming loopTiming = LoopTiming.Update)
            => IsInitialized ? _instance.TickDispatcher.SubscribeAsHandle(action, loopTiming) : default;

        public static void SubscribeAsHandle(Action action, ref TickHandle handle, LoopTiming loopTiming = LoopTiming.Update)
        {
            if (IsInitialized)
            {
                _instance.TickDispatcher.SubscribeAsHandle(action, ref handle, loopTiming);
                return;
            }

            handle.Dispose();
            handle = default;
        }
        #endif
    }
}