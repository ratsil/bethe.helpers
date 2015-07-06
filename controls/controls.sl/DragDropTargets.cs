using System.Threading;
using System.Windows.Controls;
using swc=System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Windows;
using System.Windows.Data;
using sw=System.Windows;

namespace controls.sl
{
	internal interface IDragDropSourceEffectDefault
	{
		DragDropEffects SourceEffectDefaultGet();
	}
	public class TreeViewDragDropTarget : swc.TreeViewDragDropTarget, IDragDropSourceEffectDefault
    {
		private Timer _cTimerDragDropDelay;
		private bool _bMouseDown;
		private object _cSyncRoot;
		private DragDropKeyStates _eDragDropKeyStates;

		public ushort nDelay { get; set; }
		public DragDropEffects eSourceEffectDefault { get; set; }

		public TreeViewDragDropTarget()
        {
			_cSyncRoot = new object();
			_bMouseDown = false;
			_eDragDropKeyStates = DragDropKeyStates.None;
			nDelay = 300;
			AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
			AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);
			AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
			AddHandler(KeyUpEvent, new KeyEventHandler(OnKeyUp), true);
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			lock (_cSyncRoot)
			{
				_bMouseDown = true;
				if (_cTimerDragDropDelay != null)
					_cTimerDragDropDelay.Dispose();
				_cTimerDragDropDelay = null;
			}
		}
		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			lock (_cSyncRoot)
			{
				_bMouseDown = false;
				if (_cTimerDragDropDelay != null)
					_cTimerDragDropDelay.Dispose();
				_cTimerDragDropDelay = null;
			}
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Ctrl:
					_eDragDropKeyStates |= DragDropKeyStates.ControlKey;
					break;
				case Key.Shift:
					_eDragDropKeyStates |= DragDropKeyStates.ShiftKey;
					break;
			}
		}
		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Ctrl:
					_eDragDropKeyStates &= ~DragDropKeyStates.ControlKey;
					break;
				case Key.Shift:
					_eDragDropKeyStates &= ~DragDropKeyStates.ShiftKey;
					break;
			}
		}

        protected override void OnItemDragStarting(ItemDragEventArgs cEventArgs)
        {
            if (!_bMouseDown)
            {
                cEventArgs.Handled = true;
                return;
            }

			lock (_cSyncRoot)
            {
                if (_cTimerDragDropDelay == null)
                {
                    _cTimerDragDropDelay = new Timer
                    (
                        callback =>
                        {
							lock (_cSyncRoot)
                            {
                                if (!DragDrop.IsDragInProgress)
                                {
                                    Dispatcher.BeginInvoke(() =>
                                    {
                                        lock (_cSyncRoot)
                                        {
                                            base.OnItemDragStarting(cEventArgs);
                                        }
                                    });
                                }
                            }
                        },
                        null,
						nDelay,
                        Timeout.Infinite
                    );
                }
            }
        }
		protected override void OnDragOver(DragEventArgs args)
		{
			if (DragEventArgsUpdate(args))
				args.Handled = true;
			base.OnDragOver(args);
		}
		protected override void OnDrop(DragEventArgs args)
		{
			DragEventArgsUpdate(args);
			base.OnDrop(args);
		}
		protected override void OnItemDroppedOnTarget(ItemDragEventArgs args)
		{
			base.OnItemDroppedOnTarget(args);
		}
		public override void OnItemDroppedOnSource(DragEventArgs args)
		{
			base.OnItemDroppedOnSource(args);
		}
		protected override void OnItemDragCompleted(ItemDragEventArgs args)
		{
 			 base.OnItemDragCompleted(args);
		}
		DragDropEffects IDragDropSourceEffectDefault.SourceEffectDefaultGet()
		{
			return DragDropEffectCurrentGet();
		}

		private bool DragEventArgsUpdate(DragEventArgs args)
		{
			if (args.Data.GetDataPresent(typeof(ItemDragEventArgs)))
			{
				ItemDragEventArgs cItemDragEventArgs = (ItemDragEventArgs)args.Data.GetData(typeof(ItemDragEventArgs));
				sw.DependencyObject ui = (sw.DependencyObject)cItemDragEventArgs.DragSource;
				while (null != ui)
				{
					if (ui is IDragDropSourceEffectDefault)
					{
						args.Effects = cItemDragEventArgs.Effects = ((IDragDropSourceEffectDefault)ui).SourceEffectDefaultGet();
						return true;
					}
					ui = System.Windows.Media.VisualTreeHelper.GetParent(ui);
				}
			}
			return false;
		}
		private DragDropEffects DragDropEffectCurrentGet()
		{
			if (DragDropEffects.None == AllowedSourceEffects)
				return DragDropEffects.None;
			if (DragDropKeyStates.ControlKey == (_eDragDropKeyStates & DragDropKeyStates.ControlKey) && DragDropKeyStates.ShiftKey == (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Link == (AllowedSourceEffects & DragDropEffects.Link))
					return DragDropEffects.Link;
			if (DragDropKeyStates.ControlKey == (_eDragDropKeyStates & DragDropKeyStates.ControlKey))
				if (DragDropEffects.Copy == (AllowedSourceEffects & DragDropEffects.Copy))
					return DragDropEffects.Copy;
			if (DragDropKeyStates.ShiftKey == (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Move == (AllowedSourceEffects & DragDropEffects.Move))
					return DragDropEffects.Move;
			if (DragDropKeyStates.ControlKey != (_eDragDropKeyStates & DragDropKeyStates.ControlKey) && DragDropKeyStates.ShiftKey != (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Move == (AllowedSourceEffects & DragDropEffects.Move))
					return DragDropEffects.Move;
			return eSourceEffectDefault;
		}
	}
	public class ListBoxDragDropTarget : swc.ListBoxDragDropTarget, IDragDropSourceEffectDefault
    {
		private Timer _cTimerDragDropDelay;
        private bool _bMouseDown;
		private object _cSyncRoot;
		private DragDropKeyStates _eDragDropKeyStates;

		public ushort nDelay { get; set; }
		public DragDropEffects eSourceEffectDefault { get; set; }

		public ListBoxDragDropTarget()
        {
			_cSyncRoot = new object();
			_bMouseDown = false;
			_eDragDropKeyStates = DragDropKeyStates.None;
			nDelay = 300;

			AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
			AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);
			AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
			AddHandler(KeyUpEvent, new KeyEventHandler(OnKeyUp), true);
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			lock (_cSyncRoot)
			{
				_bMouseDown = true;
				if (_cTimerDragDropDelay != null)
					_cTimerDragDropDelay.Dispose();
				_cTimerDragDropDelay = null;
			}
		}
		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			lock (_cSyncRoot)
			{
				_bMouseDown = false;
				if (_cTimerDragDropDelay != null)
					_cTimerDragDropDelay.Dispose();
				_cTimerDragDropDelay = null;
			}
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Ctrl:
					_eDragDropKeyStates |= DragDropKeyStates.ControlKey;
					break;
				case Key.Shift:
					_eDragDropKeyStates |= DragDropKeyStates.ShiftKey;
					break;
			}
		}
		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Ctrl:
					_eDragDropKeyStates &= ~DragDropKeyStates.ControlKey;
					break;
				case Key.Shift:
					_eDragDropKeyStates &= ~DragDropKeyStates.ShiftKey;
					break;
			}
		}

        protected override void OnItemDragStarting(ItemDragEventArgs cEventArgs)
        {
			if (!_bMouseDown)
			{
				cEventArgs.Handled = true;
				return;
			}
            lock (_cSyncRoot)
            {
                if (_cTimerDragDropDelay == null)
                {
                    _cTimerDragDropDelay = new Timer
                    (
                        callback =>
                        {
                            lock (_cSyncRoot)
                            {
                                if (!DragDrop.IsDragInProgress)
                                {
                                    Dispatcher.BeginInvoke(() =>
                                    {
                                        lock (_cSyncRoot)
                                        {
											cEventArgs.Effects = DragDropEffectCurrentGet();
											base.OnItemDragStarting(cEventArgs);
                                        }
                                    });
                                }
                            }
                        },
                        null,
                        nDelay,
                        Timeout.Infinite
                    );
                }
            }
        }
		protected override void OnDragOver(DragEventArgs args)
		{
			try
			{
				if (DragEventArgsUpdate(args))
					args.Handled = true;
			}
			catch { }
			base.OnDragOver(args);
		}
		protected override void OnDrop(DragEventArgs args)
		{
			DragEventArgsUpdate(args);
			base.OnDrop(args);
		}
		DragDropEffects IDragDropSourceEffectDefault.SourceEffectDefaultGet()
		{
			return DragDropEffectCurrentGet();
		}

		private bool DragEventArgsUpdate(DragEventArgs args)
		{
			if (args.Data.GetDataPresent(typeof(ItemDragEventArgs)))
			{
				ItemDragEventArgs cItemDragEventArgs = (ItemDragEventArgs)args.Data.GetData(typeof(ItemDragEventArgs));
				sw.DependencyObject ui = (sw.DependencyObject)cItemDragEventArgs.DragSource;
				while (null != ui)
				{
					if (ui is IDragDropSourceEffectDefault)
					{
						args.Effects = cItemDragEventArgs.Effects = ((IDragDropSourceEffectDefault)ui).SourceEffectDefaultGet();
						return true;
					}
					ui = System.Windows.Media.VisualTreeHelper.GetParent(ui);
				}
			}
			return false;
		}
		private DragDropEffects DragDropEffectCurrentGet()
		{
			if (DragDropEffects.None == AllowedSourceEffects)
				return DragDropEffects.None;
			if (DragDropKeyStates.ControlKey == (_eDragDropKeyStates & DragDropKeyStates.ControlKey) && DragDropKeyStates.ShiftKey == (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Link == (AllowedSourceEffects & DragDropEffects.Link))
					return DragDropEffects.Link;
			if (DragDropKeyStates.ControlKey == (_eDragDropKeyStates & DragDropKeyStates.ControlKey))
				if (DragDropEffects.Copy == (AllowedSourceEffects & DragDropEffects.Copy))
					return DragDropEffects.Copy;
			if (DragDropKeyStates.ShiftKey == (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Move == (AllowedSourceEffects & DragDropEffects.Move))
					return DragDropEffects.Move;
			if (DragDropKeyStates.ControlKey != (_eDragDropKeyStates & DragDropKeyStates.ControlKey) && DragDropKeyStates.ShiftKey != (_eDragDropKeyStates & DragDropKeyStates.ShiftKey))
				if (DragDropEffects.Move == (AllowedSourceEffects & DragDropEffects.Move))
					return DragDropEffects.Move;
			return eSourceEffectDefault;
		}
	}
}
