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

namespace controls.sl
{
	public partial class HaulierDialog : ChildWindow
	{
		public string Caption
		{
			get
			{
				return _ui_txtCaption.Text;
			}
			set
			{
				_ui_txtCaption.Text = value;
			}
		}
		public HaulierControl HaulierControl
		{
			get
			{
				return _ui_hlr;
			}
		}
		public HaulierControl.ItemAddDelegate PersonAdd
		{
			set
			{
				_ui_hlr.ItemAdd = value;
			}
		}
		public Button AcceptButton
		{
			get
			{
				return _ui_btnAccept;
			}
		}
		public Button CancelButton
		{
			get
			{
				return _ui_btnCancel;
			}
		}

		public HaulierDialog()
		{
			InitializeComponent();
            Title = globalization.Common.sName;
			this.DialogResult = false;
		}

		private void _ui_btnAccept_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		private void _ui_btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}

