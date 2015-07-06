using System;
using System.Collections;
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
using System.Collections.ObjectModel;

using helpers.sl;
using g = globalization;

namespace controls.sl
{
	public partial class HaulierControl : UserControl
	{
		public enum PanelTypes
		{
			list = 1,
			tree
		}
		public class SelectionChangingEventsArgs : SelectionChangedEventArgs
		{
			public bool bCancel;

			public SelectionChangingEventsArgs(IList aItemsRemoved, IList aItemsAdded)
				: base(aItemsRemoved ?? new List<object>(), aItemsAdded ?? new List<object>())
			{
				bCancel = false;
			}
		}

		#region dependency properties
		static public readonly DependencyProperty ePanelTypeProperty = DependencyProperty.Register("ePanelType", typeof(PanelTypes), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnPanelTypeChanged)));
		static public readonly DependencyProperty sDisplayMemberPathProperty = DependencyProperty.Register("sDisplayMemberPath", typeof(string), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnDisplayMemberPathChanged)));
		static public readonly DependencyProperty aItemsSourceProperty = DependencyProperty.Register("aItemsSource", typeof(IEnumerable), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourceChanged)));
		static public readonly DependencyProperty aItemsSelectedProperty = DependencyProperty.Register("aItemsSource", typeof(IEnumerable), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnItemsSelectedChanged)));
		static public readonly DependencyProperty oLeftCaptionProperty = DependencyProperty.Register("oLeftCaption", typeof(object), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnLeftCaptionChanged)));
		static public readonly DependencyProperty oRightCaptionProperty = DependencyProperty.Register("oRightCaption", typeof(object), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnRightCaptionChanged)));
		static public readonly DependencyProperty bSearchProperty = DependencyProperty.Register("bSearch", typeof(bool), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnSearchChanged)));
		static public readonly DependencyProperty bSearchButtonAddProperty = DependencyProperty.Register("bSearchButtonAdd", typeof(bool), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnSearchButtonAddChanged)));
        static public readonly DependencyProperty cItemTemplateProperty = DependencyProperty.Register("cItemTemplate", typeof(DataTemplate), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnItemTemplateChanged)));
        static public readonly DependencyProperty cItemContainerStyleProperty = DependencyProperty.Register("cItemContainerStyle", typeof(Style), typeof(HaulierControl), new PropertyMetadata(new PropertyChangedCallback(OnItemContainerStyleChanged)));
		private static void OnPanelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessPanelType();
		}
		private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessDisplayMemberPath();
		}
		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessItemsSource();
		}
		private static void OnItemsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessItemsSelected();
		}
		private static void OnLeftCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessLeftCaption();
		}
		private static void OnRightCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessRightCaption();
		}
		private static void OnSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessSearch();
		}
		private static void OnSearchButtonAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessSearchButtonAdd();
		}
		private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessItemTemplate();
		}
		private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((HaulierControl)d).ProcessItemContainerStyle();
		}
		#endregion

		public event EventHandler<SelectionChangingEventsArgs> SelectionChanging;
		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		public string sDisplayMemberPath
		{
			get
			{
				return (string)GetValue(sDisplayMemberPathProperty);
			}
			set
			{
				SetValue(sDisplayMemberPathProperty, value);
			}
		}
		public IEnumerable aItemsSource
		{
			get
			{
				return (IEnumerable)GetValue(aItemsSourceProperty);
			}
			set
			{
				SetValue(aItemsSourceProperty, value);
			}
		}
		public IEnumerable aItemsSelected
		{
			get
			{
				return (IEnumerable)GetValue(aItemsSelectedProperty);
			}
			set
			{
				SetValue(aItemsSelectedProperty, value);
			}
		}

		public PanelTypes ePanelType
		{
			get
			{
				return (PanelTypes)GetValue(ePanelTypeProperty);
			}
			set
			{
				SetValue(ePanelTypeProperty, value);
			}
		}
		public Style cItemContainerStyle
		{
			get
			{
				return (Style)GetValue(cItemContainerStyleProperty);
			}
			set
			{
				SetValue(cItemContainerStyleProperty, value);
			}
		}
		public DataTemplate cItemTemplate
		{
			get
			{
				return (DataTemplate)GetValue(cItemTemplateProperty);
			}
			set
			{
				SetValue(cItemTemplateProperty, value);
			}
		}
		public object oLeftCaption
		{
			get
			{
				return GetValue(oLeftCaptionProperty);
			}
			set
			{
				SetValue(oLeftCaptionProperty, value);
			}
		}
		public bool bSearch
		{
			get
			{
				return (bool)GetValue(bSearchProperty);
			}
			set
			{
				SetValue(bSearchProperty, value);
			}
		}
		public bool bSearchButtonAdd
		{
			get
			{
				return (bool)GetValue(bSearchButtonAddProperty);
			}
			set
			{
				SetValue(bSearchButtonAddProperty, value);
			}
		}

		public object oRightCaption
		{
			get
			{
				return GetValue(oRightCaptionProperty);
			}
			set
			{
				SetValue(oRightCaptionProperty, value);
			}
		}

		public delegate void ItemAddDelegate(string sText);
		public ItemAddDelegate ItemAdd;

		private ItemsControl _ui_icLeft;
		private ItemsControl _ui_icRight;

		private bool _bMark;

		public HaulierControl()
		{
			InitializeComponent();
            oLeftCaption = g.Controls.sNoticeHaulierControl1;
            oRightCaption = g.Controls.sNoticeHaulierControl2;

			_bMark = true;
			bSearchButtonAdd = true;

			_ui_icLeft = _ui_lbLeft;
			_ui_icRight = _ui_lbRight;

			_ui_Search.sCaption = "";
			_ui_Search.sDisplayMemberPath = "";
			_ui_Search.nGap2nd = 0;
			_ui_Search.DataContext = _ui_icLeft;
			_ui_Search._ui_btnAdd.Content = " + ";
			_ui_Search.ItemAdd = new SearchControl.ItemAddDelegate(_ui_btnAdd_Click);
		}

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!bSearch)
				_ui_Search.Visibility = Visibility.Collapsed;

            if (null == _ui_icRight.ItemsSource)
				_ui_icRight.ItemsSource = new List<object>();
			if (null == _ui_icRight.Tag)
				_ui_icRight.Tag = _ui_icRight.ItemsSource;
            MarkRight();
        }
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_ui_tcLeft = (TabControl)GetTemplateChild("_ui_tcLeft");
			_ui_icLeft = _ui_lbLeft = (ListBox)GetTemplateChild("_ui_lbLeft");
			_ui_tvLeft = (TreeView)GetTemplateChild("_ui_tvLeft");
			_ui_tcRight = (TabControl)GetTemplateChild("_ui_tcRight");
			_ui_icRight = _ui_lbRight = (ListBox)GetTemplateChild("_ui_lbRight");
			_ui_tvRight = (TreeView)GetTemplateChild("_ui_tvRight");
			_ui_Search = (SearchControl)GetTemplateChild("_ui_Search");
			ProcessPanelType();
			ProcessDisplayMemberPath();
			ProcessItemsSource();
			ProcessItemsSelected();
			ProcessLeftCaption();
			ProcessRightCaption();
			ProcessSearch();
			ProcessSearchButtonAdd();
			ProcessItemContainerStyle();
		}

		private void ProcessPanelType()
		{
			if (null == _ui_lbLeft || null == _ui_lbRight || null == _ui_tvLeft || null == _ui_tvRight)
				return;
			switch (ePanelType)
			{
				case PanelTypes.list:
					_ui_dlbLeft.Visibility = _ui_dlbRight.Visibility = Visibility.Visible;
					_ui_dtvLeft.Visibility = _ui_dtvRight.Visibility = Visibility.Collapsed;
					_ui_Search.DataContext = _ui_icLeft = _ui_lbLeft;
					_ui_icRight = _ui_lbRight;
					break;
				case PanelTypes.tree:
					_ui_dlbLeft.Visibility = _ui_dlbRight.Visibility = Visibility.Collapsed;
					_ui_dtvLeft.Visibility = _ui_dtvRight.Visibility = Visibility.Visible;
					_ui_Search.DataContext = _ui_icLeft = _ui_tvLeft;
					_ui_icRight = _ui_tvRight;
					break;
			}
			_ui_Search.DataContext = _ui_icLeft;
			if (null != _ui_icLeft)
				_ui_icLeft.RemoveHandler(MouseLeftButtonDownEvent, (MouseButtonEventHandler)ItemsControl_MouseLeftButtonDown);
			if (null != _ui_icRight)
				_ui_icRight.RemoveHandler(MouseLeftButtonDownEvent, (MouseButtonEventHandler)ItemsControl_MouseLeftButtonDown);
			_ui_icLeft.AddHandler(MouseLeftButtonDownEvent, (MouseButtonEventHandler)ItemsControl_MouseLeftButtonDown, true);
			_ui_icRight.AddHandler(MouseLeftButtonDownEvent, (MouseButtonEventHandler)ItemsControl_MouseLeftButtonDown, true);
			ProcessItemTemplate();
		}
		private void ProcessDisplayMemberPath()
		{
			if (null == _ui_icLeft || null == _ui_icRight)
				return;
			_ui_icLeft.DisplayMemberPath = _ui_icRight.DisplayMemberPath = _ui_Search.sDisplayMemberPath = sDisplayMemberPath;
		}
		private void ProcessItemsSource()
		{
			if (null == _ui_icLeft || null == _ui_icRight)
				return;
			List<object> aItems = new List<object>((IEnumerable<object>)aItemsSource);
			if (null != aItemsSelected)
			{
				List<object> aSelected = new List<object>(((IEnumerable<object>)aItemsSelected).Where(o => aItems.Contains(o)));
				aItems = aItems.Where(o => !aSelected.Contains(o)).ToList();
				_ui_icRight.ItemsSource = aSelected.ToList();
			}
			_ui_icLeft.ItemsSource = _ui_Search.aItemsSourceInitial = aItems.ToList();
			_ui_icRight.Tag = null;
		}
		private void ProcessItemsSelected()
		{
			if (null == _ui_icLeft || null == _ui_icRight)
				return;
			if (null != aItemsSource)
			{
				List<object> aItems = new List<object>((IEnumerable<object>)aItemsSource);
				List<object> aSelected = new List<object>(((IEnumerable<object>)aItemsSelected).Where(o => aItems.Contains(o)));
				aItems = aItems.Where(o => !aSelected.Contains(o)).ToList();
				_ui_icLeft.ItemsSource = _ui_Search.aItemsSourceInitial = aItems;
				_ui_icRight.ItemsSource = aSelected;
				_ui_Search.Search();
			}
			else
				aItemsSource = aItemsSelected;
			if (_bMark)
			{
				_ui_icRight.Tag = aItemsSelected;
				MarkRight();
			}
		}
		private void ProcessLeftCaption()
		{
			if (null == _ui_tcLeft || null == _ui_tcLeft.SelectedItem)
				return;
			((TabItem)_ui_tcLeft.SelectedItem).Header = oLeftCaption;
		}
		private void ProcessRightCaption()
		{
			if (null == _ui_tcRight || null == _ui_tcRight.SelectedItem)
				return;
			((TabItem)_ui_tcRight.SelectedItem).Header = oRightCaption;
		}
		private void ProcessSearch()
		{
			if (null == _ui_Search)
				return;
			_ui_Search.Visibility = (bSearch ? Visibility.Visible : Visibility.Collapsed);
		}
		private void ProcessSearchButtonAdd()
		{
			if (null == _ui_Search || null == _ui_Search._ui_btnAdd)
				return;
			_ui_Search._ui_btnAdd.Visibility = (bSearchButtonAdd ? Visibility.Visible : Visibility.Collapsed);
		}
		private void ProcessItemTemplate()
		{
			if (null == _ui_icRight || null == _ui_icLeft)
				return;
			_ui_icRight.ItemTemplate = _ui_icLeft.ItemTemplate = cItemTemplate;
		}
		private void ProcessItemContainerStyle()
		{
			Style cStyle = cItemContainerStyle;
			if (null != _ui_lbRight)
				_ui_lbRight.ItemContainerStyle = cStyle;
			if (null != _ui_lbLeft)
				_ui_lbLeft.ItemContainerStyle = cStyle;
			if (null != _ui_tvRight)
				_ui_tvRight.ItemContainerStyle = cStyle;
			if (null != _ui_tvLeft)
				_ui_tvLeft.ItemContainerStyle = cStyle;
		}

		private void DragDropTarget_Drop(object sender, Microsoft.Windows.DragEventArgs e)
		{
			try
			{
				if (e.Data.GetDataPresent(typeof(ItemDragEventArgs)))
				{
					ItemDragEventArgs cItemDragEventArgs = (ItemDragEventArgs)e.Data.GetData(typeof(ItemDragEventArgs));
					SelectionCollection aSelectionCollection = (SelectionCollection)cItemDragEventArgs.Data;
					object oItemSelected = aSelectionCollection.Select(o => o.Item).FirstOrDefault();
					if (null != oItemSelected && Microsoft.Windows.DragDropEffects.Move == cItemDragEventArgs.Effects)
					{
						if (_ui_lbLeft == cItemDragEventArgs.DragSource && _ui_dlbRight == sender)
							SelectionChange(oItemSelected, true);
						else if (_ui_tvLeft == cItemDragEventArgs.DragSource && _ui_dtvRight == sender)
							SelectionChange(oItemSelected, true);
						else if (_ui_lbRight == cItemDragEventArgs.DragSource && _ui_dlbLeft == sender)
							SelectionChange(oItemSelected, false);
						else if (_ui_tvRight == cItemDragEventArgs.DragSource && _ui_dtvLeft == sender)
							SelectionChange(oItemSelected, false);
					}
				}
			}
			catch (Exception ex)
			{
				(new controls.childs.sl.MsgBox()).ShowError(ex);
			}
		}
		private void ItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (2 > e.ClickCount)
				return;
			System.Windows.DependencyObject ui = (System.Windows.DependencyObject)e.OriginalSource;
			bool bItemClicked = false;
			while (null != ui)
			{
				if (PanelTypes.list == ePanelType)
				{
					if (bItemClicked)
					{
						if (_ui_lbLeft == ui)
						{
							SelectionChange(_ui_lbLeft.SelectedItem, true);
							return;
						}
						else if (_ui_lbRight == ui)
						{
							SelectionChange(_ui_lbRight.SelectedItem, false);
							return;
						}
					}
					else if (ui is ListBoxItem)
						bItemClicked = true;
				}
				else
				{
					if (bItemClicked)
					{
						if (_ui_tvLeft == ui)
						{
							SelectionChange(_ui_tvLeft.SelectedItem, true);
							return;
						}
						else if (_ui_tvRight == ui)
						{
							SelectionChange(_ui_tvRight.SelectedItem, false);
							return;
						}
					}
					else if (ui is TreeViewItem)
						bItemClicked = true;
				}
				ui = System.Windows.Media.VisualTreeHelper.GetParent(ui);
			}
		}
		private void SelectionChange(object cItem, bool bSelected)
		{
			if (null != cItem)
			{
				List<object> aItems;
				if (null != SelectionChanging)
				{
					aItems = new List<object>();
					aItems.Add(cItem);
					SelectionChangingEventsArgs e;
					if(bSelected)
						e = new SelectionChangingEventsArgs(null, aItems);
					else
						e = new SelectionChangingEventsArgs(aItems, null);
					SelectionChanging(this, e);
					if (e.bCancel)
						return;
				}
				if (null != aItemsSelected)
					aItems = ((IEnumerable<object>)aItemsSelected).ToList();
				else
					aItems = new List<object>();
				if(bSelected)
					aItems.Add(cItem);
				else
					aItems.Remove(cItem);
				_bMark = false;
				aItemsSelected = aItems;
				_bMark = true;
				MarkRight();
			}
		}
        private void MarkRight()
        {
			if (null == _ui_icRight.Tag || null == _ui_icRight.ItemsSource)
				return;
			IEnumerable<object> aItemsSource = (IEnumerable<object>)_ui_icRight.ItemsSource;
			IEnumerable<object> aItemsInitial = (IEnumerable<object>)_ui_icRight.Tag;
			if (aItemsInitial.Count() == aItemsSource.Count() && aItemsSource.Count(o => ((IEnumerable<object>)_ui_icRight.Tag).Contains(o)) == aItemsSource.Count())
				_ui_icRight.Background = controls.sl.Coloring.Notifications.cTextBoxActive;
            else
				_ui_icRight.Background = controls.sl.Coloring.Notifications.cTextBoxChanged;
		}
		private void _ui_btnAdd_Click(string sText)
		{
			if (null != ItemAdd)
			{
				sText = sText.ToLower().Trim();
				_ui_Search.Clear();
				ItemAdd(sText);
			}
		}
	}
}