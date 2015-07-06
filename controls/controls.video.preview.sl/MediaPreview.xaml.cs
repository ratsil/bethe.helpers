using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace controls.video.preview.sl
{
	public partial class MediaPreview : UserControl
	{
		private bool _bPaused;
		private MediaStreamSource _cMSS;
		private DispatcherTimer _cDispatcherTimer;

		public MediaPreview()
		{
			InitializeComponent();

			MouseMove += MediaPreview_MouseMove;
			MouseLeave += MediaPreview_MouseLeave;
			_cDispatcherTimer = new DispatcherTimer();
			_cDispatcherTimer.Interval = TimeSpan.FromMilliseconds(40);
			_cDispatcherTimer.Tick += new EventHandler(_cDispatcherTimer_Tick);
		}

		public void Init(string sFile)
		{
			Init(sFile, 0);
		}
		public void Init(string sFile, ulong nFramesQty)
		{
			_cMSS = new MediaStreamSource(sFile, nFramesQty);
			_bPaused = true;
			MediaLoad();
		}
		void _cDispatcherTimer_Tick(object sender, EventArgs e)
		{
			_ui_sldFrames.Tag = true;
			_ui_sldFrames.Value = _ui_me.Position.TotalMilliseconds / 40;
			_ui_nudFrames.Value = _ui_sldFrames.Value;
			_ui_sldFrames.Tag = null;
			_ui_pbBuffered.Value = _cMSS.nFramesBuffered;
		}
		void MediaLoad()
		{
			_ui_me.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(_ui_me_MediaFailed);
			_ui_me.MediaOpened += new RoutedEventHandler(_ui_me_MediaOpened);
			_ui_me.AutoPlay = false;
			_ui_me.SetSource(_cMSS);// new Uri();
		}

		void _ui_me_MediaOpened(object sender, RoutedEventArgs e)
		{
			_ui_sldFrames.Minimum = 0;
			_ui_sldFrames.Maximum = _ui_me.NaturalDuration.TimeSpan.TotalMilliseconds / 40;
			_ui_nudFrames.Minimum = _ui_sldFrames.Minimum;
			_ui_nudFrames.Maximum = _ui_sldFrames.Maximum;
			_ui_sldVolume.Minimum = 0;
			_ui_sldVolume.Maximum = 100;
			_ui_sldVolume.Value =  _ui_me.Volume * _ui_sldVolume.Maximum;
			_ui_pbBuffered.Minimum = 0;
			_ui_pbBuffered.Maximum = _cMSS.nFramesQty;
			_cDispatcherTimer.Start();
		}

		void _ui_me_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
		}

		private void _ui_btnPlay_Click(object sender, RoutedEventArgs e)
		{
			if (_bPaused)
			{
				_ui_me.Play();
				_ui_iPlay.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/controls.video.preview.sl;component/images/pause.png", UriKind.Relative));
			}
			else
			{
				_ui_me.Pause();
				//_ui_me.Stop();
				//_ui_me.Position = TimeSpan.FromMilliseconds(_ui_nudFrames.Value * 40);
				_ui_iPlay.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/controls.video.preview.sl;component/images/play.png", UriKind.Relative));
			}
			_bPaused = !_bPaused;
		}

		private void _ui_btnFullScreen_Click(object sender, RoutedEventArgs e)
		{
		}
		private void _ui_sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_ui_me.Volume = e.NewValue / _ui_sldVolume.Maximum;
		}
		private void MediaPreview_MouseMove(object sender, MouseEventArgs e)
		{
			if (1 > _ui_grdControls.Opacity)
				_ui_grdControls.Opacity = 0.4;
		}
		private void MediaPreview_MouseLeave(object sender, MouseEventArgs e)
		{
			_ui_grdControls.Opacity = 0;
		}
		private void _ui_grdControls_MouseEnter(object sender, MouseEventArgs e)
		{
			_ui_grdControls.Opacity = 1;
		}
		private void _ui_grdControls_MouseLeave(object sender, MouseEventArgs e)
		{
			_ui_grdControls.Opacity = 0.4;
		}
		private void _ui_me_Loaded(object sender, RoutedEventArgs e)
		{
		}
		private void _ui_sldFrames_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (null == _ui_sldFrames.Tag)
			{
				_cDispatcherTimer.Stop();
				_ui_me.Position = TimeSpan.FromMilliseconds(_ui_sldFrames.Value * 40);
				_ui_nudFrames.Value = _ui_sldFrames.Value;
				_cDispatcherTimer.Start();
			}
		}
		private void _ui_nudFrames_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (null == _ui_sldFrames.Tag)
			{
				_cDispatcherTimer.Stop();
				_ui_me.Position = TimeSpan.FromMilliseconds(_ui_nudFrames.Value * 40);
				_ui_sldFrames.Value = _ui_nudFrames.Value;
				_cDispatcherTimer.Start();
			}
		}
		private void _ui_btnVolume_Click(object sender, RoutedEventArgs e)
		{
			if (_ui_sldVolume.Visibility == System.Windows.Visibility.Visible)
				_ui_sldVolume.Visibility = System.Windows.Visibility.Collapsed;
			else
				_ui_sldVolume.Visibility = System.Windows.Visibility.Visible;
		}
		private void _ui_sldVolume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_ui_sldVolume.Visibility = System.Windows.Visibility.Collapsed;
		}
	}
}
