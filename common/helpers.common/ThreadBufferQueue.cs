using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpers
{
	public class ThreadBufferQueue<T>
    {
		private Queue<T>[] _aQueues;
		private uint[] _aBufferLength;
		private bool _bWaitOnEnqueue; // waits if length = max
        private bool _bWaitOnDequeue; // waits if length = 0
		private object _oSyncRoot;

		public uint nCount
		{
			get
			{
				return CountGet(0);
			}
		}
		public object oSyncRoot
		{
			get
			{
				return _oSyncRoot;
			}
		}

        public System.Collections.IEnumerator GetEnumerator()
        {
            return GetEnumerator(0);
        }
        public System.Collections.IEnumerator GetEnumerator(ushort nQueueIndx)
        {
            lock (_oSyncRoot)
            {
                if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
                    throw new Exception("incorrect queue index"); //TODO LANG
                return _aQueues[nQueueIndx].GetEnumerator();
            }
        }
        public void Clear()
        {
            Clear(0);
        }
        public void Clear(ushort nQueueIndx)
        {
            lock (_oSyncRoot)
            {
                if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
                    throw new Exception("incorrect queue index"); //TODO LANG
                _aQueues[nQueueIndx].Clear();
            }
        }

        public ThreadBufferQueue(uint[] aBufferLengths, bool bWaitOnEnqueue, bool bWaitOnDequeue)
		{
			if (null == aBufferLengths || 1 > aBufferLengths.Length)
				throw new Exception("array of lengths can't be null or empty"); //TODO LANG
			_oSyncRoot = new object();
			_aQueues = new Queue<T>[aBufferLengths.Length];
			for (int nIndx = 0; _aQueues.Length > nIndx; nIndx++)
				_aQueues[nIndx] = new Queue<T>();
			_aBufferLength = aBufferLengths;
			_bWaitOnEnqueue = bWaitOnEnqueue;
			_bWaitOnDequeue = bWaitOnDequeue;
		}
		public ThreadBufferQueue(uint[] aBufferLength, bool bWaitOnEnqueue)
			: this(aBufferLength, bWaitOnEnqueue, true)
		{ }

		public ThreadBufferQueue(uint[] aBufferLength)
			: this(aBufferLength, true, true)
		{ }

		public ThreadBufferQueue(uint nBufferLength, bool bWaitOnEnqueue, bool bWaitOnDequeue)
			: this(new uint[] { nBufferLength }, bWaitOnEnqueue, bWaitOnDequeue)
		{
		}
		public ThreadBufferQueue(uint nBufferLength, bool bWaitOnEnqueue)
			: this(nBufferLength, bWaitOnEnqueue, true)
		{ }

        public ThreadBufferQueue(uint nBufferLength)
            : this(nBufferLength, true, true)
        { }

        public ThreadBufferQueue(bool bWaitOnEnqueue, bool bWaitOnDequeue)
            : this(0, bWaitOnEnqueue, bWaitOnDequeue)
        { }
        public ThreadBufferQueue()
            : this(0, true, true)
        { }

        public void Enqueue(ushort nQueueIndx, T item)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG
				_aQueues[nQueueIndx].Enqueue(item);

                while (ReadyToSleepOnEnqueue(nQueueIndx))
					System.Threading.Monitor.Wait(_oSyncRoot);

                System.Threading.Monitor.PulseAll(_oSyncRoot);
            }
        }
		public T Peek(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG
				if (_bWaitOnDequeue)
				{
					while (1 > _aQueues[nQueueIndx].Count)
						System.Threading.Monitor.Wait(_oSyncRoot);
				}
				System.Threading.Monitor.PulseAll(_oSyncRoot);
				return _aQueues[nQueueIndx].Peek();
			}
		}
		//public T GetLast(ushort nQueueIndx)
		//{
		//	lock (_oSyncRoot)
		//	{
		//	}
		//}
		//public T GetLast()
		//{
		//	return Last(0);
		//}
		public void EnqueueFirst(ushort nQueueIndx, T item)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG

				if (_aQueues[nQueueIndx].Count > 0)
				{
					Queue<T> ahQ = new Queue<T>();
					ahQ.Enqueue(item);
					while (_aQueues[nQueueIndx].Count > 0)
						ahQ.Enqueue(_aQueues[nQueueIndx].Dequeue());
					_aQueues[nQueueIndx] = ahQ;
				}
				else
					_aQueues[nQueueIndx].Enqueue(item);

				while (ReadyToSleepOnEnqueue(nQueueIndx))
					System.Threading.Monitor.Wait(_oSyncRoot);

				System.Threading.Monitor.PulseAll(_oSyncRoot);
			}
		}
		private bool ReadyToSleepOnEnqueue(ushort nQueueIndx)
        {
            if (_bWaitOnEnqueue && _aBufferLength[nQueueIndx] > 0)
            {
				if (_aQueues[nQueueIndx].Count >= _aBufferLength[nQueueIndx])
				{
                    if (_aBufferLength.Length == 1)
                        return true;

                    bool bReadyToSleep = true;
					while (true)
					{
						for (int nIndx = 0; _aBufferLength.Length > nIndx; nIndx++)
						{
							if (_aQueues[nIndx].Count < _aBufferLength[nIndx])
							{
								bReadyToSleep = false;
								break;
							}
						}
						return bReadyToSleep;
					}
				}
			}
			return false;
		}
		public void EnqueueFirst(T item)
		{
			EnqueueFirst(0, item);
		}
		public T FirstOrDefault(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG

				return _aQueues[nQueueIndx].FirstOrDefault(o => !EqualityComparer<T>.Default.Equals(o, default(T)));
			}
		}
		public T FirstOrDefault()
		{
			return FirstOrDefault(0);
        }
        public void Remove(ushort nQueueIndx, T oItem)
        {
            lock (_oSyncRoot)
            {
                if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
                    throw new Exception("incorrect queue index"); //TODO LANG
                if (_aQueues[nQueueIndx].Contains(oItem))
                {
                    Queue<T> ahQ = new Queue<T>();
                    T oCurrent;
                    while (_aQueues[nQueueIndx].Count > 0)
                    {
                        oCurrent = _aQueues[nQueueIndx].Dequeue();
                        if (!EqualityComparer<T>.Default.Equals(oItem, oCurrent))
                            ahQ.Enqueue(oCurrent);
                    }
                    _aQueues[nQueueIndx] = ahQ;
                }
            }
        }
        public void Remove(T item)
        {
            Remove(0, item);
        }
        public void RemoveBeginningDefaults(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG
				T cT;
				while (true)
				{
					cT = _aQueues[nQueueIndx].Peek();
					if (EqualityComparer<T>.Default.Equals(cT, default(T)))
						_aQueues[nQueueIndx].Dequeue();
					else
						return;
				}
			}
		}
		public void RemoveBeginningDefaults()
		{
			RemoveBeginningDefaults(0);
		}
		public T Dequeue(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
				T oItem = Peek(nQueueIndx);
				_aQueues[nQueueIndx].Dequeue();
				return oItem;
			}
		}
		public void BufferLengthSet(uint nBufferLength)
		{
			BufferLengthSet(0, nBufferLength);
		}
		public void BufferLengthSet(ushort nQueueIndx, uint nBufferLength)
		{
			lock (_oSyncRoot)
				_aBufferLength[nQueueIndx] = nBufferLength;
		}
		public uint CountGet()
		{
			return CountGet(0);
		}
		public uint CountGet(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG
				if (0 > _aQueues[nQueueIndx].Count)
					return 0;
				return (uint)_aQueues[nQueueIndx].Count;
			}
		}
		public bool Contains(ushort nQueueIndx, T oItem)
		{
			lock (_oSyncRoot)
			{
				if (_aQueues.Length <= nQueueIndx || null == _aQueues[nQueueIndx])
					throw new Exception("incorrect queue index"); //TODO LANG
				if (0 >= _aQueues[nQueueIndx].Count)
					return false;
				return _aQueues[nQueueIndx].Contains(oItem);
			}
		}

		public void Enqueue(T item)
		{
			Enqueue(0, item);
		}
		public T Peek()
		{
            return Peek(0);
		}
		public T Dequeue()
		{
			return Dequeue(0);
		}
		public bool Contains(T oItem)
		{
			return Contains(0, oItem);
		}
	}
}
