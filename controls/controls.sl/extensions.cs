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

using System.Linq;
using System.Collections.Generic;

namespace controls.extensions.sl
{
	static public class x
	{
		static public void Refresh(this MenuItem cMI)
		{
			if (cMI.IsEnabled)
				VisualStateManager.GoToState(cMI, "Normal", true);
			else
				VisualStateManager.GoToState(cMI, "Disabled", true);
		}

		static public void ColumnAdd(this DataGrid dg, string sName, string sBinding, int nDisplayIndex)
		{
			DataGridTextColumn dgtcColumn = new DataGridTextColumn();
			dgtcColumn.Header = sName;
			dgtcColumn.Binding = new System.Windows.Data.Binding(sBinding);
			dg.Columns.Add(dgtcColumn);
			dgtcColumn.DisplayIndex = nDisplayIndex;
		}
		static public void ColumnRemove(this DataGrid dg, string sName)
		{
			DataGridColumn dgcColumn;
			if (null != (dgcColumn = dg.ColumnGet(sName)))
				dg.Columns.Remove(dgcColumn);
		}
		static public void ColumnMove(this DataGrid dg, string sName, int nDisplayIndex)
		{
			DataGridColumn dgcColumn;
			if (null != (dgcColumn = dg.ColumnGet(sName)))
				dgcColumn.DisplayIndex = nDisplayIndex;
		}
		static public void ColumnResize(this DataGrid dg, string sName, DataGridLength stDGL)
		{
			DataGridColumn dgcColumn;
			if (null != (dgcColumn = dg.ColumnGet(sName)))
				dgcColumn.Width = stDGL;
		}
		static public bool ColumnExist(this DataGrid dg, string sName)
		{
			return (null != dg.ColumnGet(sName));
		}
		static private DataGridColumn ColumnGet(this DataGrid dg, string sBinding)
		{
				return dg.Columns.FirstOrDefault(o => ((DataGridTextColumn)o).Binding.Path.Path == sBinding);
		}
		private class ObjectForSort
		{
			public object o = null;
			public object oValue
			{
				get
				{
					if (null == sValue)
						return nValue;
					else
						return sValue;
				}
				set
				{
					if (typeof(int) == value.GetType())
						nValue = (int)value;
					else
						sValue = value.ToString();
				}
			}
			private string sValue = null;
			private int nValue = int.MinValue;
		}
		public static void Sort(this DataGrid dg, Type cTypeOfElement, string sFieldName, bool bBackward)
		{
			System.Reflection.PropertyInfo cHeader = (System.Reflection.PropertyInfo)cTypeOfElement.GetMember(sFieldName)[0];

			List<ObjectForSort> aOFS = new List<ObjectForSort>();
			foreach (object oOFS in dg.ItemsSource)
					aOFS.Add(new ObjectForSort() { o = oOFS, oValue = cHeader.GetValue(oOFS, null) });
			if (bBackward)
				dg.ItemsSource = aOFS.OrderByDescending(o => o.oValue).Select(o => o.o).ToList();
			else
				dg.ItemsSource = aOFS.OrderBy(o => o.oValue).Select(o => o.o).ToList();
		}
	}
}
