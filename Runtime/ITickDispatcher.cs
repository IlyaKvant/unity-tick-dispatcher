using System;
#if VCONTAINER_SUPPORT
using VContainer.Unity;
#endif

namespace UnityTickDispatcher
{
    public interface ITickDispatcher
        #if VCONTAINER_SUPPORT
        : IStartable, IFixedTickable, ITickable, ILateTickable, IPostTickable, IPostLateTickable
    #endif
    {
        public IDisposable Subscribe(Action action, LoopTiming loopTiming = LoopTiming.Update);
        public TickHandle SubscribeAsHandle(Action action, LoopTiming loopTiming = LoopTiming.Update);
        public void SubscribeAsHandle(Action action, ref TickHandle handle, LoopTiming loopTiming = LoopTiming.Update);
        public void Optimize();
    }
}