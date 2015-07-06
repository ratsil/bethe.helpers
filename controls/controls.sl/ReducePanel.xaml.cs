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
using helpers.extensions;

using g = globalization;

namespace controls.sl
{
    public partial class ReducePanel : ContentControl
    {
        public event EventHandler IsOpenChanged;
        static public readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ReducePanel), new PropertyMetadata(new PropertyChangedCallback(OnTitleChanged)));
        static public readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ReducePanel), new PropertyMetadata(new PropertyChangedCallback(OnIsOpenChanged)));

        private object _cOpenSymbol;
        private object _cCloseSymbol;

        internal Button _ui_btnOpenClose;
        internal TextBlock _ui_txtTitle;
        internal Border _ui_brdContent;
        internal DockPanel _ui_dpHeader;
        internal ContentControl _ui_cntContent;
        internal Rectangle _ui_rectOpenClose;

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
            set
            {
                bool bOldValue = (bool)GetValue(IsOpenProperty);
                SetValue(IsOpenProperty, value);
                if (value != bOldValue && null != IsOpenChanged)
                    IsOpenChanged(this, value ? new EventArgs() : null);
            }
        }

        public ReducePanel()
        {
            DefaultStyleKey = GetType();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _ui_txtTitle = (TextBlock)GetTemplateChild("_ui_txtTitle");
            if (null != (_ui_btnOpenClose = (Button)GetTemplateChild("_ui_btnOpenClose")))
                _ui_btnOpenClose.Click += new RoutedEventHandler(_ui_btnOpenClose_Click);
            _ui_rectOpenClose = (Rectangle)GetTemplateChild("_ui_rectOpenClose");
            _ui_brdContent = (Border)GetTemplateChild("_ui_brdContent");
            _ui_cntContent = (ContentControl)GetTemplateChild("_ui_cntContent");

            if (null != (_ui_dpHeader = (DockPanel)GetTemplateChild("_ui_dpHeader")))
                _ui_dpHeader.MouseLeftButtonDown += new MouseButtonEventHandler(_ui_dpHeader_MouseLeftButtonDown);


            //            Image cImg = new Image();
            ImageBrush cIB = new ImageBrush();
            cIB.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("/controls.sl;component/Images/rp_open.png", UriKind.Relative));
            cIB.Stretch = Stretch.None;
            _cOpenSymbol = cIB;               // = cImg;
            cIB = new ImageBrush();
            cIB.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("/controls.sl;component/Images/rp_close.png", UriKind.Relative));
            cIB.Stretch = Stretch.None;
            _cCloseSymbol = cIB;              //= cImg;
            if(Title.IsNullOrEmpty())
                Title = g.Common.sName;
            ProcessTitle();
            ProcessIsOpen();
        }

        private void ProcessTitle()
        {
            if (null == _ui_txtTitle)
                return;
            _ui_txtTitle.Text = Title;
        }
        private void ProcessIsOpen()
        {
            if (null == _ui_brdContent)
                return;
            if (IsOpen)
            {
                _ui_brdContent.Visibility = Visibility.Visible;
                //_ui_btnOpenClose.Content = _cCloseSymbol;
                _ui_rectOpenClose.Fill = (ImageBrush)_cCloseSymbol;
            }
            else
            {
                _ui_brdContent.Visibility = Visibility.Collapsed;
                //_ui_btnOpenClose.Content = _cOpenSymbol;
                _ui_rectOpenClose.Fill = (ImageBrush)_cOpenSymbol;
            }
        }
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReducePanel ui_rp = (ReducePanel)d;
            ui_rp.ProcessTitle();
        }
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReducePanel ui_rp = (ReducePanel)d;
            ui_rp.ProcessIsOpen();
        }

        private void _ui_btnOpenClose_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = (Visibility.Collapsed == _ui_brdContent.Visibility);
        }
        private void _ui_dpHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _ui_btnOpenClose_Click(null, null);
        }
    }
}
