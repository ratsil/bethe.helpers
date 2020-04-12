#if XNA
using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using helpers;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SD = System.Drawing;
using SDI = System.Drawing.Imaging;           // отладка
using System.Diagnostics;


namespace BTL.Device
{
	public class Display : Device
	{
		class Logger : helpers.Logger
		{
			public Logger()
				: base("display", "device_", true)
			{ }
		}
		private GraphicsAdapter _cDevice;
		private XNARender _cXNARender;
		uint nBTLVideoQueueLengthPrevious = 0, nVideoQueueLengthPrevious = 0;
		System.Threading.Thread _cThreadXNA;

		new static public Device[] BoardsGet()
		{
			(new Logger()).WriteDebug3("boards:get:in");
			List<Device> aRetVal = new List<Device>();
			if (null != GraphicsAdapter.Adapters)
			{
				for (int nIndx = 0; GraphicsAdapter.Adapters.Count > nIndx; nIndx++)
					aRetVal.Add(new Display(GraphicsAdapter.Adapters[nIndx]));
			}
			(new Logger()).WriteDebug4("boards:get:device:out");
			return aRetVal.ToArray();
		}

		private Display(GraphicsAdapter cDevice)
		{
			(new Logger()).WriteDebug3("device:constructor:in");
			//_cFrameEmpty = null;
			//IDeckLink cDevice = BoardGet();
			try
			{
				_cDevice = cDevice;
                stArea = new Area(0, 0, (ushort)Preferences.cXNA.nWidth, (ushort)Preferences.cXNA.nHeight);
                _nFPS = Preferences.nFPS;
				_nFrameDuration = (ushort)(1000 / _nFPS);
            }
			catch (Exception ex)
			{
				(new Logger()).WriteError(ex);
				throw;
			}
			(new Logger()).WriteDebug4("device:constructor:out");
		}
		~Display()
		{
		}
        internal override ReverseChannelsDo ReverseChannels
        {
            get
            {
                return XNAReverseChannels;
            }
        }
        override public void TurnOn()
		{
			_cThreadXNA = new System.Threading.Thread(WorkerXNA);
			_cThreadXNA.Priority = System.Threading.ThreadPriority.AboveNormal;
			_cThreadXNA.Start();
			System.Threading.ThreadPool.QueueUserWorkItem(WorkerQueue);
			base.TurnOn();
		}
		private void WorkerXNA(object cState)
		{
			_cXNARender = new XNARender(_cDevice);
			_cXNARender.TurnOn(_stArea.nWidth, _stArea.nHeight);
			_cXNARender.Run();
		}
		private void WorkerQueue(object cState)
		{
			while(null == _cXNARender || !_cXNARender.bReady);
			int nSleepDuration = Preferences.nFrameDuration * 2;
			while(true)
			{
				if (Preferences.nQueueDeviceLength > _cXNARender._aqDisplayQueue.Count && !_bNeedToAddFrame)
					_bNeedToAddFrame = true;
				System.Threading.Thread.Sleep(nSleepDuration);
				continue;
			}
		}
		override protected Frame.Video FrameBufferPrepare()
		{
			Frame.Video cRetVal = new Frame.Video();
			cRetVal.oFrameBytes = new byte[_stArea.nWidth * _stArea.nHeight * 4];  //UNDONE    * 4   //1b
			return cRetVal;
		}
		//Stopwatch _GlobalMeter;
		//int _nGlobalIndex = 0;
		//long _nGlobalTexture = 0;
		//long _nTextureMin = long.MaxValue, _nTextureMax = 0;
		bool nStarted = false;
		Frame.Video _cFrameVideoPrevious = null;
        private void XNAReverseChannels(byte[] aFrameBytes)
        {
            byte nT;
            for (int nI = 0; nI < aFrameBytes.Length; nI += 4)
            {
                nT = aFrameBytes[nI];
                aFrameBytes[nI] = aFrameBytes[nI + 2];
                aFrameBytes[nI + 2] = nT;
            }
        }
		override protected bool FrameSchedule()
		{
			Frame.Video cFrameVideo;
			_dtLastTimeFrameScheduleCalled = DateTime.Now;
            int nLogCount = 0;
#region video
			if (NextFrameAttached)
			{
				try
				{
					while (Preferences.nQueueDeviceLength > _cXNARender._aqDisplayQueue.Count)
					{

						if (null == (cFrameVideo = VideoFrameGet()) || null == cFrameVideo.aFrameBytes)
						{
							(new Logger()).WriteDebug("got null instead of frame");
							break;
						}
						lock (_cXNARender._aqDisplayQueue)
						{
                            //_GlobalMeter = Stopwatch.StartNew();
                            _cXNARender._aqDisplayQueue.Enqueue(_cXNARender.TextureCreate(cFrameVideo.aFrameBytes, _stArea.nWidth, _stArea.nHeight)); //UNDONE
                            if (null != _cFrameVideoPrevious)
                                FrameBufferReleased(_cFrameVideoPrevious);
							_cFrameVideoPrevious = cFrameVideo;


							//_GlobalMeter.Stop();
							//_nGlobalTexture += _GlobalMeter.ElapsedMilliseconds;
							//if (_GlobalMeter.ElapsedMilliseconds < _nTextureMin) _nTextureMin = _GlobalMeter.ElapsedMilliseconds;
							//if (_GlobalMeter.ElapsedMilliseconds > _nTextureMax) _nTextureMax = _GlobalMeter.ElapsedMilliseconds;

							//if (_nGlobalIndex == 100)
							//{
							//    (new Logger()).WriteNotice("!!!!!!!!!!! 100 TextureCreate = " + _nGlobalTexture / 100 + " ms  (" + _nTextureMin + "," + _nTextureMax + ")]");
							//    _nGlobalIndex = -1;
							//    _nGlobalTexture = 0;
							//    _nTextureMin = long.MaxValue;
							//    _nTextureMax = 0;
							//}
							//_nGlobalIndex++;



						}
						AudioFrameGet();
						break;
					}
					if (!nStarted && _cXNARender._aqDisplayQueue.Count > 0)
						nStarted = true;
                    if (nStarted)
                    {
                        nLogCount++;
                        if (_cXNARender._aqDisplayQueue.Count < Preferences.nQueueDeviceLength / 2)
                            (new Logger()).WriteNotice("device queue is less than " + Preferences.nQueueDeviceLength / 2 + "    (" + _cXNARender._aqDisplayQueue.Count + ") dev buffer:" + base._nBufferFrameCount + " internal buffer:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")");
                        else if (nLogCount > 200)
                        {
                            nLogCount = 0;
                            (new Logger()).WriteNotice("device queue:(" + _cXNARender._aqDisplayQueue.Count + ") dev buffer:" + base._nBufferFrameCount + " internal buffer:(" + _aq__PROBA__AudioFrames.Count + ", " + _aq__PROBA__VideoFrames.Count + ")");
                        }
                    }
				}
				catch (Exception ex)
				{
					(new Logger()).WriteError(ex);
				}
			}
#endregion
			return true;
		}
	}
	public class XNARender : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager _cGraphicsDeviceManager;
		GraphicsAdapter _cGraphicsAdapter;
		SpriteBatch _cSpriteBatch;
		Texture2D _cTexture2D;
		Rectangle _stTextureSize;
		public bool bReady = false;

		public XNARender(GraphicsAdapter cGraphicsAdapter)
		{
			_cGraphicsAdapter = cGraphicsAdapter;
			_cTexture2D = null;
			_aqDisplayQueue = new Queue<Texture2D>();
			_aqFrames = new Queue<Texture2D>();
		}
		~XNARender()
		{
			try
			{
				if (null != _cTexture2D)
					_cTexture2D.Dispose();
				while (0 < _aqDisplayQueue.Count)
					_aqDisplayQueue.Dequeue().Dispose();
				while (0 < _aqFrames.Count)
					_aqFrames.Dequeue().Dispose();
			}
			catch (Exception ex)
			{
				(new Logger()).WriteError(ex);
			}
		}

		void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
		{
			e.GraphicsDeviceInformation.Adapter = _cGraphicsAdapter;
		}

		public void TurnOn(int nWidth, int nHeight)
		{
			IsFixedTimeStep = false;
			//TargetElapsedTime = TimeSpan.FromMilliseconds(Preferences.nFrameDuration);
			_cBlendState = new BlendState();
			_cBlendState.AlphaBlendFunction = BlendFunction.Add;
			_cBlendState.ColorDestinationBlend = Blend.SourceAlpha;
			
			Window.AllowUserResizing = false;
			_cGraphicsDeviceManager = new GraphicsDeviceManager(this);
			_cGraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
			_cGraphicsDeviceManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
			_cGraphicsDeviceManager.DeviceCreated += new EventHandler<EventArgs>(_cGraphicsDeviceManager_DeviceCreated);
			_cGraphicsDeviceManager.PreferredBackBufferWidth = nWidth; // _cGraphicsAdapter.CurrentDisplayMode.Width;
			_cGraphicsDeviceManager.PreferredBackBufferHeight = nHeight; //_cGraphicsAdapter.CurrentDisplayMode.Height;
            _cGraphicsDeviceManager.IsFullScreen = Preferences.cXNA.bFullScreen;
            _stTextureSize = new Rectangle(0, 0, nWidth, nHeight);
			_stVector2Zero = Vector2.Zero;

			//int style = helpers.WinAPI.GetWindowLong(Window.Handle, helpers.WinAPI.GWL_STYLE);
			//helpers.WinAPI.SetWindowLong(Window.Handle, helpers.WinAPI.GWL_STYLE, (style & ~helpers.WinAPI.WS_CAPTION));
			//helpers.WinAPI.SetWindowPos(Window.Handle, new IntPtr(-1), 0, 0, 0, 0, helpers.WinAPI.SWP_NOMOVE | helpers.WinAPI.SWP_NOSIZE | helpers.WinAPI.SWP_SHOWWINDOW);
		}

		void _cGraphicsDeviceManager_DeviceCreated(object sender, EventArgs e)
		{
			lock(_aqFrames)
			{
				while (Preferences.nQueueDeviceLength + 2 > _aqFrames.Count)
					_aqFrames.Enqueue(new Texture2D(GraphicsDevice, _cGraphicsDeviceManager.PreferredBackBufferWidth, _cGraphicsDeviceManager.PreferredBackBufferHeight, false, SurfaceFormat.Color)); //1b
			}
			_cSpriteBatch = new SpriteBatch(GraphicsDevice);
			bReady = true;
		}
		//int _nGlobalIndex = 0;
		//long _nGlobalByte = 0;
		//long _nGlobalSet = 0;
		//long _nByteMin = long.MaxValue, _nByteMax = 0;
		//long _nSetMin = long.MaxValue, _nSetMax = 0;
		//Stopwatch _GlobalMeter;
		public Queue<Texture2D> _aqDisplayQueue;
		public Queue<Texture2D> _aqFrames;
		private BlendState _cBlendState;
		private Vector2 _stVector2Zero;
		static public int nElseQty = 0;
		public Texture2D TextureCreate(byte[] aBytes, int nWidth, int nHeight)
		{
			Texture2D cRetVal;
			if (0 < _aqFrames.Count)
			{
				lock (_aqFrames)
					cRetVal = _aqFrames.Dequeue();
			}
			else
			{
				cRetVal = new Texture2D(GraphicsDevice, nWidth, nHeight, false, SurfaceFormat.Color);  //1b
				nElseQty++;
			}
			
			//_GlobalMeter = Stopwatch.StartNew();
			cRetVal.SetData(aBytes);
			//_GlobalMeter.Stop();
			//_nGlobalSet += _GlobalMeter.ElapsedMilliseconds;
			//if (_GlobalMeter.ElapsedMilliseconds < _nSetMin) _nSetMin = _GlobalMeter.ElapsedMilliseconds;
			//if (_GlobalMeter.ElapsedMilliseconds > _nSetMax) _nSetMax = _GlobalMeter.ElapsedMilliseconds;
			//if (_nGlobalIndex == 100)
			//{
			//    (new Logger()).WriteNotice("!!!!!!!!!!! 100 frames middle delay [in bytes[] making = " + _nGlobalByte / 100 + " ms  (" + _nByteMin + "," + _nByteMax + ")] [in SetData(bytes) = " + _nGlobalSet / 100 + " ms    (" + _nSetMin + "," + _nSetMax + ")]");
			//    _nGlobalIndex = -1;
			//    _nGlobalByte = 0;
			//    _nGlobalSet = 0;
			//    _nByteMin = long.MaxValue;
			//    _nByteMax = 0;
			//    _nSetMin = long.MaxValue;
			//    _nSetMax = 0;
			//}
			//_nGlobalIndex++;

			return cRetVal;
		}
		public void FrameNext(Texture2D cTexture2D)
		{
			//lock (_cSpriteBatch)
			//{
			if (null != _cTexture2D)
			{
				lock (_aqFrames)
					_aqFrames.Enqueue(_cTexture2D);
			}
			_cTexture2D = cTexture2D;
			//if (_stTextureSize.Width != _cTexture2D.Width)
			//    _stTextureSize.Width = _cTexture2D.Width;
			//if (_stTextureSize.Height != _cTexture2D.Height)
			//    _stTextureSize.Height = _cTexture2D.Height;
			//}
		}
		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		int nIndx = 0;
		int nIndx2 = 0;
		protected override void Draw(GameTime gameTime)
		{
			//if (nIndx / 1.5F >= nIndx2)
				if (0 < _aqDisplayQueue.Count)
				{
					Texture2D cTexture2D;
					lock (_aqDisplayQueue)
						cTexture2D = _aqDisplayQueue.Dequeue();
					FrameNext(cTexture2D);
					//nIndx2++;
				}
			//nIndx++;
			//lock (_cSpriteBatch)
			//{
				if (null != _cTexture2D)
				{
                //GraphicsDevice.Clear(Color.Black);
                //GraphicsDevice.Clear(Color.White);  //1b
                _cSpriteBatch.Begin();//SpriteSortMode.Texture, _cBlendState);
                if (Preferences.cXNA.bPromter)
                    _cSpriteBatch.Draw(_cTexture2D, _stTextureSize, null, Color.White, 0, _stVector2Zero, SpriteEffects.FlipHorizontally, 0);
                else
                    _cSpriteBatch.Draw(_cTexture2D, _stTextureSize, Color.White);

                _cSpriteBatch.End();
				}
				else
					GraphicsDevice.Clear(Color.Black);
				//}
			base.Draw(gameTime);
		}
	}
}
#endif