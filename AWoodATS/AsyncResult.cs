using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;

namespace AWoodATS
{
    public class AsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback _callback;
        private object _state;
        private ManualResetEvent _manualResetEvent;

        public AsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;
            _manualResetEvent = new ManualResetEvent(false);
        }

        public bool IsCompleted
        {
            get { return _manualResetEvent.WaitOne(0, false); }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _manualResetEvent; }
        }

        public object AsyncState
        {
            get { return _state; }
        }
        public ManualResetEvent AsyncWait
        {
            get { return _manualResetEvent; }

        }
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public void Completed()
        {
            _manualResetEvent.Set();
            if (_callback != null)
                _callback(this);
        }

        public void Dispose()
        {
            _manualResetEvent.Close();
            _manualResetEvent = null;
            _state = null;
            _callback = null;
        }
    }

    public class MeridianAsyncResult : AsyncResult
    {
         public MeridianResult Result { get; set; }
        public OperationContext CurrentOperationContext { get; private set; }

        public MeridianAsyncResult(OperationContext currentContext, AsyncCallback callback, object state) : base(callback, state)
        {
            CurrentOperationContext = currentContext;
        }
    }
}