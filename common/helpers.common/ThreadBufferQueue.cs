using System;
using System.Collections.Generic;
using System.Text;

namespace helpers
{
	public class ThreadBufferQueue<T>
	{
		private Queue<T>[] _aQueues;
		private uint[] _aBufferLength;
		private bool _bWaitOnEnqueue;
		private bool _bWaitOnDequeue;
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



		public ThreadBufferQueue(uint[] aBufferLengths, bool bWaitOnEnqueue, bool bWaitOnDequeue)
		{
			if (null == aBufferLengths || 1 > aBufferLengths.Length)
				throw new Exception("array of lengths can't ne null or empty"); //TODO LANG
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
				if (_bWaitOnEnqueue)
				{
					if (_aQueues[nQueueIndx].Count >= _aBufferLength[nQueueIndx])
					{
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
							if (bReadyToSleep)
								System.Threading.Monitor.Wait(_oSyncRoot);
							else
								break;
						}
					}
				}
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
		public T Dequeue(ushort nQueueIndx)
		{
			lock (_oSyncRoot)
			{
                T oItem = Peek(nQueueIndx);
                _aQueues[nQueueIndx].Dequeue();
				return oItem;
			}
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
				if (0 > _aQueues[nQueueIndx].Count)
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
