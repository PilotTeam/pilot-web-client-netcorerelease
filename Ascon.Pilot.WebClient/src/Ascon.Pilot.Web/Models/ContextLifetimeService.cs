using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ascon.Pilot.Web.Models
{
    public interface IContextLifetimeListener
    {
        void OnTimeIsUp(Guid contextId);
    }

    public interface IContextLifetimeService : IDisposable
    {
        void Register(Guid contextId);
        void Unregister(Guid contextId);
        void Renewal(Guid contextId);
        void SetContextLifetimeListener(IContextLifetimeListener lifetimeListener);
    }

    class ContextLifetimeService : IContextLifetimeService
    {
        private readonly Timer _timer;
        private readonly TimeSpan _renewalTime;// = TimeSpan.FromMinutes(20);
        private readonly Dictionary<Guid, long> _contextTable = new Dictionary<Guid, long>();
        private readonly int _timerDueTime = (int) TimeSpan.FromMinutes(1).TotalMilliseconds;
        private IContextLifetimeListener _lifetimeListener;

        public ContextLifetimeService()
        {
            _timer = new Timer(TimerTick, null, _timerDueTime, _timerDueTime);
            _renewalTime = TimeSpan.FromMinutes(ApplicationConst.InactiveSessionTimeout);
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();

            lock (_contextTable)
            {
                foreach (var context in _contextTable)
                {
                    _lifetimeListener.OnTimeIsUp(context.Key);
                }
                _contextTable.Clear();
            }
        }

        public void Register(Guid contextId)
        {
            lock (_contextTable)
            {
                _contextTable[contextId] = DateTime.UtcNow.Ticks;
            }
        }

        public void Unregister(Guid contextId)
        {
            lock (_contextTable)
            {
                _contextTable.Remove(contextId);
            }
        }

        public void Renewal(Guid contextId)
        {
            lock (_contextTable)
            {
                _contextTable[contextId] = DateTime.UtcNow.Ticks;
            }
        }

        public void SetContextLifetimeListener(IContextLifetimeListener lifetimeListener)
        {
            _lifetimeListener = lifetimeListener;
        }

        private void TimerTick(object state)
        {
            lock (_contextTable)
            {
                foreach (var (id, timespan) in _contextTable.ToArray())
                {
                    var current = DateTime.UtcNow.Ticks;
                    if (current - timespan < _renewalTime.Ticks)
                        continue;

                    _contextTable.Remove(id);
                    _lifetimeListener.OnTimeIsUp(id);

                }
            }
        }
    }
}
