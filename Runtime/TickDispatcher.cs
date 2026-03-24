using System;
using UnityTickDispatcher.Internal;

namespace UnityTickDispatcher
{
    public sealed class TickDispatcher : ITickDispatcher
    {
        internal static readonly ActionDisposable NoopDisposable = new ActionDisposable(null);

        private readonly TickProcessing _fixedProcessing;
        private readonly TickProcessing _updateProcessing;
        private readonly TickProcessing _lastUpdateProcessing;
        private readonly TickProcessing _lateUpdateProcessing;
        private readonly TickProcessing _lastLateUpdateProcessing;

        public TickDispatcher(LoopTiming loopTiming)
        {
            var tickPool = new TickPool();
            _fixedProcessing = loopTiming.Contains(LoopTiming.FixedUpdate) ? new TickProcessing(tickPool) : null;
            _updateProcessing = loopTiming.Contains(LoopTiming.Update) ? new TickProcessing(tickPool) : null;
            _lastUpdateProcessing = loopTiming.Contains(LoopTiming.LastUpdate) ? new TickProcessing(tickPool) : null;
            _lateUpdateProcessing = loopTiming.Contains(LoopTiming.LateUpdate) ? new TickProcessing(tickPool) : null;
            _lastLateUpdateProcessing = loopTiming.Contains(LoopTiming.LastLateUpdate) ? new TickProcessing(tickPool) : null;
        }

        public void Start()
        {
        }

        public void FixedTick()
        {
            _fixedProcessing?.Run();
        }

        public void Tick()
        {
            _updateProcessing?.Run();
        }

        public void PostTick()
        {
            _lastUpdateProcessing?.Run();
        }

        public void LateTick()
        {
            _lateUpdateProcessing?.Run();
        }

        public void PostLateTick()
        {
            _lastLateUpdateProcessing?.Run();
        }

        public void Optimize()
        {
            _fixedProcessing?.Optimize();
            _updateProcessing?.Optimize();
            _lastUpdateProcessing?.Optimize();
            _lateUpdateProcessing?.Optimize();
            _lastLateUpdateProcessing?.Optimize();
        }

        public IDisposable Subscribe(Action action, LoopTiming loopTiming = LoopTiming.Update)
        {
            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _fixedProcessing,
                LoopTiming.Update => _updateProcessing,
                LoopTiming.LastUpdate => _lastUpdateProcessing,
                LoopTiming.LateUpdate => _lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _lastLateUpdateProcessing,

                _ => null
            };

            return processing?.Add(action) ?? NoopDisposable;
        }

        public TickHandle SubscribeAsHandle(Action action, LoopTiming loopTiming = LoopTiming.Update)
        {
            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _fixedProcessing,
                LoopTiming.Update => _updateProcessing,
                LoopTiming.LastUpdate => _lastUpdateProcessing,
                LoopTiming.LateUpdate => _lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _lastLateUpdateProcessing,

                _ => null
            };

            return processing?.AddHandle(action) ?? default;
        }

        public void SubscribeAsHandle(Action action, ref TickHandle handle, LoopTiming loopTiming = LoopTiming.Update)
        {
            handle.Dispose();

            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _fixedProcessing,
                LoopTiming.Update => _updateProcessing,
                LoopTiming.LastUpdate => _lastUpdateProcessing,
                LoopTiming.LateUpdate => _lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _lastLateUpdateProcessing,

                _ => null
            };

            handle = processing?.AddHandle(action) ?? default;
        }
    }
}