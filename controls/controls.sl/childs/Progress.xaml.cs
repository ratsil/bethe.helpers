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

namespace controls.childs.sl
{
    public partial class Progress : ChildWindow
    {
        private string _sText;
        public string sText
        {
            get
            {
                if (null != _ui_txt)
                    _sText = _ui_txt.Text;
                return _sText;
            }
            set
            {
                _sText = value;
                if (null != _ui_txt)
                    _ui_txt.Text = value;
            }
        }
        public string sInfo
        {
            set
            {
                _ui_info.Text = value;
            }
        }
        public System.ComponentModel.AsyncCompletedEventArgs cAsyncRequestResult;

        public Progress()
        {
            InitializeComponent();
            cAsyncRequestResult = null;
            HasCloseButton = false;
            _sText = null;
        }
        public Progress(EventHandler eh)
            : this()
        {
            Closed += eh;
        }

        public void Set(double nProgress)
        {
            if (_ui_pb.IsIndeterminate)
            {
                _ui_pb.Minimum = 0;
                _ui_pb.Maximum = 100;
                _ui_pb.IsIndeterminate = false;
            }
            _ui_pb.Value = nProgress;
        }
        public void AsyncRequestCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            cAsyncRequestResult = e;
            DialogResult = true;
        }
        private void _ui_txt_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != _sText)
                _ui_txt.Text = _sText;
            else
                _sText = _ui_txt.Text;
        }
        private void ChildWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
        }
    }
}

