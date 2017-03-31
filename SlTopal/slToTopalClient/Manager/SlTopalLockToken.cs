using System;
using System.Threading;
using slToTopalClient.Exceptions;

namespace slToTopalClient.Manager
{
    public class SlTopalLockToken : IDisposable
    {
        private readonly object _lockToken;

        public TimeSpan ManagerLockTimeout { get; set; }

        public SlTopalLockToken(object lockToken)
        {
            ManagerLockTimeout = TimeSpan.FromMilliseconds(5000);

            _lockToken = lockToken;

            Lock();
        }

        public void Dispose()
        {
            UnLock();
        }

        private void Lock()
        {
            if (!Monitor.TryEnter(_lockToken, ManagerLockTimeout))
            {
                throw new SlTopalLockException();
            }
        }

        private void UnLock()
        {
            if (Monitor.IsEntered(_lockToken))
            {
                Monitor.Exit(_lockToken);
            }
        }
    }
}