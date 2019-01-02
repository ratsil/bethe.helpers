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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using System.Xml.Serialization;

using helpers.extensions;
using g = globalization;
using System.ComponentModel;

namespace controls.childs.sl
{
    public partial class MsgBox : ChildWindow
    {
        public enum MsgBoxButton
        {
            OK,
            OKCancel,
            Cancel,
            OKALLCancel,
            ALL
        }
        public enum Type
        {
            Unknown,
            FileDuration
        }
		public delegate void CtrlIsInputCorrect();

		static public bool Question(string sMsg)
		{
			return (MessageBoxResult.OK == MessageBox.Show(sMsg, g.Common.sAttention, MessageBoxButton.OKCancel));
		}
		static public void Warning(string sMsg)
		{
			MessageBox.Show(sMsg, g.Common.sWarning, MessageBoxButton.OK);
		}
		static public void Error(string sMsg)
		{
			MessageBox.Show(sMsg, g.Common.sError, MessageBoxButton.OK);
		}

		private CtrlIsInputCorrect _fIsInputCorrect;
        private Type _enType = Type.Unknown;
        public MsgBoxButton enMsgResult;
		private string _sText;
        public string sText
        {
            get
			{
				return _ui_tbText.Text;
			}
            private set
			{
				_sText = value;
				_ui_tbText.Text = null == value ? "" : value;
			}
        }
		public bool bTextIsReadOnly
		{
			get { return _ui_tbText.IsReadOnly; }
			set { _ui_tbText.IsReadOnly = value; }
		}
        public DateTime dtSelectedDateTime = DateTime.MinValue;
        private bool _bShowTextBox;
        private bool _bShowDateTimePicker;
        private ListBox _cListBox;
        private string _sMsg;
        private string _sTitle;
        private MsgBoxButton _enBtn;
        public MsgBox()
            : this("Default", "Message Box", MsgBoxButton.OK) { }
        public MsgBox(string sMsg)
            : this(sMsg, "Message Box", MsgBoxButton.OK) { }
        public MsgBox(string sMsg, string sTitle, MsgBoxButton enBtn, DateTime dtMinDate, DateTime dtMaxDate, DateTime dtSelected)
            : this(sMsg, sTitle, enBtn)
        {
			_ui_tmpDateTime.ValueChanged -= _ui_tmpDateTime_ValueChanged;
			_ui_tmpDateTime.ValueChanged += new RoutedPropertyChangedEventHandler<DateTime?>(_ui_tmpDateTime_ValueChanged);
            _ui_dtpDateTime.DisplayDateStart = dtMinDate;
            _ui_dtpDateTime.DisplayDateEnd = dtMaxDate;
			_sText = null;
			if (dtSelected < dtMinDate)
			{
				_ui_dtpDateTime.SelectedDate = dtMinDate;
				_ui_tmpDateTime.Value = dtMinDate;
			}
			else
			{
				_ui_dtpDateTime.SelectedDate = dtSelected;
				_ui_tmpDateTime.Value = dtSelected;
			}	
			//_ui_tmpDateTime.Minimum = dtMinDate;
			//_ui_tmpDateTime.Maximum = dtMaxDate;
			
            _bShowDateTimePicker = true;
        }

		void _ui_tmpDateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
		{
			DateTime dtChanged = new DateTime(((DateTime)_ui_dtpDateTime.SelectedDate).Year, ((DateTime)_ui_dtpDateTime.SelectedDate).Month, ((DateTime)_ui_dtpDateTime.SelectedDate).Day,
												  ((DateTime)_ui_tmpDateTime.Value).Hour, ((DateTime)_ui_tmpDateTime.Value).Minute, ((DateTime)_ui_tmpDateTime.Value).Second);
			if (dtChanged < _ui_dtpDateTime.DisplayDateStart.Value)
				_ui_tmpDateTime.Value = _ui_dtpDateTime.DisplayDateStart.Value;
		}
        public MsgBox(Type enType)
        {
            if (enType == Type.FileDuration)
            {
                InitializeComponent();
                _sMsg = g.Controls.sNoticeMsgBox1 + ".";
				_sTitle = g.Common.sFileTimings;
                _enBtn = MsgBoxButton.OKALLCancel;
                _enType = enType;
            }
        }
        public MsgBox(string sMsg, string sTitle, MsgBoxButton enBtn)
        {
            InitializeComponent();
            //this.SizeChanged += new SizeChangedEventHandler(MsgBox_SizeChanged);
            _sMsg = sMsg;
            _sTitle = sTitle;
            _enBtn = enBtn;
        }
        void MsgBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
        protected override void OnOpened()
        {
            base.OnOpened();
            _ui_MsgBox.Width = _ui_MsgBox.ActualWidth + 30;
            this.AddHandler(Button.KeyDownEvent, new KeyEventHandler(_ui_This_KeyDown), true);
			if (null != _sText)
			{
				try
				{
					System.Windows.Browser.HtmlPage.Plugin.Focus();
				}
				catch { }
				_ui_tbText.Focus();
				_ui_tbText.SelectionStart = sText.Length;  // чтобы сразу печатать можно было
			}
		}
		protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _bShowTextBox = false;
            _cListBox = null;
			_bShowDateTimePicker = false;
			_enType = Type.Unknown;
        }
		public void ControlAdd(UIElement ui_Control, CtrlIsInputCorrect fIsInputCorrect)
		{
			if (0 == _ui_spControl.Children.Count)
			{
				_ui_spControl.Children.Add(ui_Control);
				_fIsInputCorrect = fIsInputCorrect;
			}
			else
                throw new Exception(g.Controls.sErrorMsgBox1);
		}
		public UIElement ControlGet()
		{
			if (0 < _ui_spControl.Children.Count)
				return _ui_spControl.Children[0];
			return null;
		}
        private void DoInterface()
        {
            _ui_Label.Content = _sMsg;
            this.Title = _sTitle;
            switch (_enBtn)
            {
                case MsgBoxButton.OK:
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MsgBoxButton.OKCancel:
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            _ui_spParrent.Children.Clear();
            _ui_svParrent.Visibility = Visibility.Collapsed;
            if (null != _cListBox)
            {
                _ui_svParrent.Visibility = Visibility.Visible;
                _ui_spParrent.Children.Add(_cListBox);
                //double ii = _ui_spParrent.Height;
            }
            _ui_tbText.Visibility = Visibility.Collapsed;
            if (_bShowTextBox)
            {
                _ui_tbText.Text = sText;
                _ui_tbText.Visibility = Visibility.Visible;
                _ui_tbText.Focus();
            }
            _ui_spDateTime.Visibility = Visibility.Collapsed;
            if (_bShowDateTimePicker)
            {
                _ui_spDateTime.Visibility = Visibility.Visible;
                _ui_tmpDateTime.Focus();
            }
			AllButton.Visibility = Visibility.Collapsed;
			_ui_spControl.Visibility = Visibility.Collapsed;
			if (Type.FileDuration == _enType)
			{
				AllButton.Visibility = Visibility.Visible;
				_ui_spControl.Visibility = Visibility.Visible;
			}
			_ui_lblHotKeys.Content = HotKeysGet();
        }
        string HotKeysGet()
        {
            string sRes = "";
            string sSpace = ",   ";
            string sC = "'ESC' - Cancel", sO = "'ENTER' - OK", sA = "'CTRL+A' - ALL";
            if (CancelButton.Visibility == Visibility.Visible)
                sRes += sSpace + sC;
            if (OKButton.Visibility == Visibility.Visible)
                sRes += sSpace + sO;
            if (AllButton.Visibility == Visibility.Visible)
                sRes += sSpace + sA;
			return g.Common.sPress + ": " + sRes.Substring(4);
        }
        new public void Show()
        {
            DoInterface();
            base.Show();
        }
        public void Show(string sMsg)
        {
            Show(sMsg, "Message Box", MsgBoxButton.OK);
        }
        public void ShowError()
        {
            _ui_tbText.IsReadOnly = true;
            Show(g.Common.sErrorUnknown, g.Common.sError, MsgBoxButton.OK);
        }
        public void ShowError(string sMsg)
        {
            _ui_tbText.IsReadOnly = true;
            Show(g.Common.sError, g.Common.sError, MsgBoxButton.OK, sMsg);
        }
        public void ShowError(string sMsg, ListBox cLB)
        {
            Show(sMsg, g.Common.sError, MsgBoxButton.OK, cLB);
        }
        public void ShowError(string sMessage, Exception ex)
        {
            if (null == ex)
                return;
            string sText = ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + Environment.NewLine;
            do
            {
                if (ex is FaultException)
                {
                    try
                    {
                        MessageFault cMessageFault = ((FaultException)ex).CreateMessageFault();
                        if (cMessageFault.HasDetail)
                        {
                            helpers.web.Exception cException = (helpers.web.Exception)(new XmlSerializer(typeof(helpers.web.Exception))).Deserialize(new System.IO.MemoryStream(cMessageFault.GetDetail<XElement>().Value.FromBase64().ToBytes()));
                            sText = "";
                            do
                            {
                                sText += cException.sMessage + Environment.NewLine + cException.sStackTrace + Environment.NewLine + Environment.NewLine;
                                cException = cException.cExceptionInner;
                            }
                            while (null != cException);
                        }
                    }
                    catch { }
                    break;
                }
                ex = ex.InnerException;
            }
            while (null != ex);

            _ui_tbText.IsReadOnly = true;
            Show(sMessage, g.Common.sError, MsgBoxButton.OK, sText);
        }
		public void ShowError(Exception ex)
		{
            ShowError(g.Common.sDetails.ToLower(), ex);
		}
        public void ShowWarning(string sMsg, ListBox cLB)
        {
            _cListBox = cLB;
            Show(sMsg, g.Common.sWarning, MsgBoxButton.OK, cLB);
        }
        public void ShowAttention(string sMsg, ListBox cLB)
        {
            Show(sMsg, g.Common.sAttention.ToUpper() + "!", MsgBoxButton.OK, cLB);
        }
        public void ShowAttention(string sMsg)
        {
            Show(sMsg, g.Common.sAttention.ToUpper() + "!", MsgBoxButton.OK);
        }
        public void ShowQuestion(string sMsg, ListBox cLB)
        {
            Show(sMsg, g.Common.sWarning, MsgBox.MsgBoxButton.OKCancel, cLB);
        }
        public void ShowQuestion(string sMsg, string sValue)
        {
            Show(sMsg, g.Common.sWarning, MsgBox.MsgBoxButton.OKCancel, sValue);
        }
        public void ShowQuestion(string sMsg)
        {
            Show(sMsg, g.Common.sWarning, MsgBox.MsgBoxButton.OKCancel);
        }
        public void Show(string sMsg, string sTitle, MsgBoxButton enBtn, ListBox cLB)
        {
            _cListBox = cLB;
            Show(sMsg, sTitle, enBtn);
        }
        public void Show(string sMsg, string sTitle, MsgBoxButton enBtn)
        {
            _sMsg = sMsg;
            _sTitle = sTitle;
            _enBtn = enBtn;
            this.Show();
        }
        public void Show(string sMsg, string sTitle, MsgBoxButton enBtn, string sText)
        {
            _bShowTextBox = true;
            this.sText = sText;
			_ui_tbText.IsReadOnly = false;
			this.Show(sMsg, sTitle, enBtn);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != sText)
            {
                sText = _ui_tbText.Text;
            }
            if (_bShowDateTimePicker)
            {
                dtSelectedDateTime = new DateTime(((DateTime)_ui_dtpDateTime.SelectedDate).Year, ((DateTime)_ui_dtpDateTime.SelectedDate).Month, ((DateTime)_ui_dtpDateTime.SelectedDate).Day,
                                                  ((DateTime)_ui_tmpDateTime.Value).Hour, ((DateTime)_ui_tmpDateTime.Value).Minute, ((DateTime)_ui_tmpDateTime.Value).Second);
            }
            if (IsInputCorrect())
            {
                enMsgResult = MsgBoxButton.OK;
				this.DialogResult = true;
            }
        }
        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInputCorrect())
            {
                enMsgResult = MsgBoxButton.ALL;
				this.DialogResult = true;
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            enMsgResult = MsgBoxButton.Cancel;
			this.DialogResult = false;
        }
        private void _ui_This_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && Visibility.Visible == OKButton.Visibility)
                OKButton_Click(null, null);
            if ((e.Key == Key.A) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && Visibility.Visible == AllButton.Visibility)
                AllButton_Click(null, null);
            if ((e.Key == Key.Escape) && Visibility.Visible == CancelButton.Visibility)
                CancelButton_Click(null, null);
        }
		private bool IsInputCorrect()
		{
			bool bRes = true;
			if (Type.FileDuration == _enType && null != _fIsInputCorrect)
			{
				try
				{
					_fIsInputCorrect();
				}
				catch (Exception ex)
				{
					bRes = false;
					MessageBox.Show(ex.Message, g.Common.sError, MessageBoxButton.OK);
				}
			}
			return bRes;
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			Progress _dlgProgress = new Progress();
			base.OnClosing(e);
			_dlgProgress.Show();
			_dlgProgress.Close();
		}
	}
}

