using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace MistCore.Framework.Cache
{
    internal class BackgroundThreadService : IDisposable
    {
        private Thread _thread;
        private CancellationTokenSource _cancellationTokenSource;
        private Action action;

        public BackgroundThreadService(Action action)
        {
            this.action = action;
            _thread = new Thread(DoWork);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool KeepAlive()
        {
            if (_thread.IsAlive)
            {
                return false;
            }

            lock (_thread)
            {
                if (_thread.IsAlive)
                {
                    return false;
                }

                _thread = new Thread(DoWork);
                _cancellationTokenSource = new CancellationTokenSource();
                _thread.Start(_cancellationTokenSource.Token);

                return true;
            }
        }

        public void Start()
        {
            _thread.Start(_cancellationTokenSource.Token);
        }

        private void DoWork(object obj)
        {
            var cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                this.action();
                Thread.Sleep(100);
            }
        }

        public System.Threading.ThreadState ThreadState()
        {
            return _thread.ThreadState;
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _thread.Join();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }

}
