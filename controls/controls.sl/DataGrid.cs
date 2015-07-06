using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using swc=System.Windows.Controls;
using System.Windows.Data;

namespace controls.sl
{
	public class DataGridTextColumn : swc.DataGridTextColumn
	{
		#region dependency properties
		static public readonly DependencyProperty oBindingHeaderProperty = DependencyProperty.Register("BindingHeader", typeof(object), typeof(DataGridTextColumn), new PropertyMetadata(OnBindingHeaderChanged));
		private static void OnBindingHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((DataGridTextColumn)d).ProcessBindingHeader();
		}
		#endregion

		public object oBindingHeader
		{
			get
			{
				return (string)GetValue(oBindingHeaderProperty);
			}
			set
			{
				SetValue(oBindingHeaderProperty, value);
			}
		}
		public Application cApplication
		{
			get
			{
				return Application.Current;
			}
		}

		public DataGridTextColumn()
			: base()
		{
		}

		private void ProcessBindingHeader()
		{
			Header = oBindingHeader;
		}
	}
}
