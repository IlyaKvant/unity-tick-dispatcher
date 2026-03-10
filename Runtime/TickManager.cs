using System;
using UnityEngine;

namespace UnityTickDispatcher
{
    public sealed class TickManager : MonoBehaviour
    {
        [SerializeField] private PlayerLoopTiming loopTiming = PlayerLoopTiming.FixedUpdate | PlayerLoopTiming.Update;

        private static readonly ActionDisposable NoopDisposable = new ActionDisposable(null);
        private static bool IsInitialized => _instance != null;
        private static TickManager _instance;

        private TickPool _tickPool;
        private TickProcessing _fixedProcessing;
        private TickProcessing _updateProcessing;
        private TickProcessing _lateUpdateProcessing;
        private TickProcessing _lastUpdateProcessing;
        private TickProcessing _lastLateUpdateProcessing;
        private PlayerLoopTiming _loopTiming;

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
            _loopTiming = loopTiming;

            _tickPool = new TickPool();
            _fixedProcessing = FastContains(_loopTiming, PlayerLoopTiming.FixedUpdate) ? new TickProcessing(_tickPool) : null;
            _updateProcessing = FastContains(_loopTiming, PlayerLoopTiming.Update) ? new TickProcessing(_tickPool) : null;
            _lastUpdateProcessing = FastContains(_loopTiming, PlayerLoopTiming.LastUpdate) ? new TickProcessing(_tickPool) : null;
            _lateUpdateProcessing = FastContains(_loopTiming, PlayerLoopTiming.LateUpdate) ? new TickProcessing(_tickPool) : null;
            _lastLateUpdateProcessing = FastContains(_loopTiming, PlayerLoopTiming.LastLateUpdate) ? new TickProcessing(_tickPool) : null;
        }

        private void FixedUpdate()
        {
            _fixedProcessing?.Run();
        }

        private void Update()
        {
            _updateProcessing?.Run();
            _lastUpdateProcessing?.Run();
        }

        private void LateUpdate()
        {
            _lateUpdateProcessing?.Run();
            _lastLateUpdateProcessing?.Run();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // TODO clear local fields
                _instance = null;
            }
        }

        public void Optimize()
        {
            _fixedProcessing?.Optimize();
            _updateProcessing?.Optimize();
            _lastUpdateProcessing?.Optimize();
            _lateUpdateProcessing?.Optimize();
            _lastLateUpdateProcessing?.Optimize();
        }

        public static IDisposable Subscribe(Action action, PlayerLoopTiming loopTiming = PlayerLoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return NoopDisposable;
            }

            var processing = loopTiming switch
            {
                PlayerLoopTiming.FixedUpdate => _instance._fixedProcessing,
                PlayerLoopTiming.Update => _instance._updateProcessing,
                PlayerLoopTiming.LastUpdate => _instance._lastUpdateProcessing,
                PlayerLoopTiming.LateUpdate => _instance._lateUpdateProcessing,
                PlayerLoopTiming.LastLateUpdate => _instance._lastLateUpdateProcessing,

                _ => null
            };

            return processing == null ? NoopDisposable : processing.Add(action);
        }

        public static TickHandle SubscribeAsHandle(Action action, PlayerLoopTiming loopTiming = PlayerLoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return default;
            }

            var processing = loopTiming switch
            {
                PlayerLoopTiming.FixedUpdate => _instance._fixedProcessing,
                PlayerLoopTiming.Update => _instance._updateProcessing,
                PlayerLoopTiming.LastUpdate => _instance._lastUpdateProcessing,
                PlayerLoopTiming.LateUpdate => _instance._lateUpdateProcessing,
                PlayerLoopTiming.LastLateUpdate => _instance._lastLateUpdateProcessing,

                _ => null
            };

            return processing == null ? default : processing.AddHandle(action);
        }

        private bool FastContains(PlayerLoopTiming whole, PlayerLoopTiming part) => (whole & part) == part;
    }
}