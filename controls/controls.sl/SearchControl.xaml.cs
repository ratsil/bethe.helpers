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
using System.Windows.Data;

using helpers.extensions;

namespace controls.sl
{
    public partial class SearchControl : UserControl
    {
        #region dependency properties
        static public readonly DependencyProperty sDisplayMemberPathProperty = DependencyProperty.Register("sDisplayMemberPath", typeof(string), typeof(SearchControl), new PropertyMetadata(OnDisplayMemberPathChanged));
        static public readonly DependencyProperty aItemsSourceInitialProperty = DependencyProperty.Register("aItemsSourceInitial", typeof(IEnumerable), typeof(SearchControl), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourceInitialChanged)));
        static public readonly DependencyProperty aItemsSourceProperty = DependencyProperty.Register("aItemsSource", typeof(IEnumerable), typeof(SearchControl), new PropertyMetadata(new PropertyChangedCallback(OnItemsSourceChanged)));
        static public readonly DependencyProperty bButtonAddProperty = DependencyProperty.Register("bButtonAdd", typeof(bool), typeof(SearchControl), new PropertyMetadata(new PropertyChangedCallback(OnButtonAddChanged)));
        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchControl)d).ProcessDisplayMemberPath();
        }
        private static void OnItemsSourceInitialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchControl)d).ProcessItemsSourceInitial();
        }
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchControl)d).ProcessItemsSource();
        }
        private static void OnButtonAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchControl)d).ProcessButtonAdd();
        }
        #endregion

        public delegate void ItemAddDelegate(string sText);
        public delegate object ItemSelectedGetDelegate();
        public delegate void ItemSelectedSetDelegate(object oItem);

        private ItemAddDelegate _bItemAdd;
        private Dictionary<string, string> _ahTransliteration, _ahTransliterationInvert;
        private Dictionary<string, string> _ahWrongKeyboard, _ahWrongKeyboardInvert;
        private bool _bThisLostFocus = false;
        private System.Windows.Threading.DispatcherTimer _cTimerForLostFocus;
        private System.Reflection.PropertyInfo _cPropertyTarget;
		private List<System.Reflection.PropertyInfo> _aPropertyTargets;

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
        public IEnumerable aItemsSourceInitial
        {
            get
            {
                return (IEnumerable)GetValue(aItemsSourceInitialProperty);
            }
            set
            {
                if (null != value && value == aItemsSource)
                    value = value.ToList();
                SetValue(aItemsSourceInitialProperty, value);
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
                if (null != value && value == aItemsSourceInitial)
                    value = value.ToList();
                SetValue(aItemsSourceProperty, value);
            }
        }
        public bool bButtonAdd
        {
            get
            {
                return (bool)GetValue(bButtonAddProperty);
            }
            set
            {
                SetValue(bButtonAddProperty, value);
            }
        }

        public ItemAddDelegate ItemAdd
        {
            set
            {
                if (null == value)
                    _ui_btnAdd.Visibility = Visibility.Collapsed;
                else
                    _ui_btnAdd.Visibility = Visibility.Visible;
                _bItemAdd = value;
            }
        }
        public ItemSelectedGetDelegate ItemSelectedGet;
        public ItemSelectedSetDelegate ItemSelectedSet;
        public double AddButtonWidth
        {
            set { _ui_btnAdd.Width = value; }
        }
        public double nGap2nd
        {
            set
            {
                _ui_tbName.Margin = new Thickness(0, 0, value, 0);
            }
        }
        public int nMaxItemsInListOrTable;
		public string[] aAdditionalSearchFields;

		public string sCaption
        {
            get
            {
                return _ui_tbWhatToFind.Text;
            }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    _ui_tbWhatToFind.Visibility = Visibility.Collapsed;
                    _ui_tbName.Margin = new Thickness(0, 0, 0, 0);
                    _ui_tbWhatToFind.Text = "";
                }
                else
                    _ui_tbWhatToFind.Text = value;
            }
        }
        public string sText
        {
            get
            {
                return _ui_tbName.Text;
            }
        }

        public SearchControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            nMaxItemsInListOrTable = int.MaxValue;

            _ahWrongKeyboard = GetWrongKeyDictionary(false);
            _ahWrongKeyboardInvert = GetWrongKeyDictionary(true);
            _ahTransliteration = GetTransliterationDictionary(false);
            _ahTransliterationInvert = GetTransliterationDictionary(true);

            _cTimerForLostFocus = new System.Windows.Threading.DispatcherTimer();
            _cTimerForLostFocus.Tick += new EventHandler(LoosingFocus);
            _cTimerForLostFocus.Interval = new TimeSpan(0, 0, 0, 0, 300);  // период проверки статуса темплейта

            bButtonAdd = true;
        }

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			DataContextUpdateInitial();
		}
		public void DataContextUpdateInitial()
		{
			if (null != DataContext)   // && DataContext is ItemsControl
			{
				DataContextUpdate();
				aItemsSourceInitial = aItemsSource;
			}
		}
		public void DataContextUpdate()
		{
			if (null != DataContext)   // && DataContext is ItemsControl
			{
				if (sDisplayMemberPath.IsNullOrEmpty())
					SetBinding(sDisplayMemberPathProperty, new Binding("DisplayMemberPath") { Mode = BindingMode.OneWay });
				if (null == aItemsSource)
					SetBinding(aItemsSourceProperty, new Binding("ItemsSource") { Mode = BindingMode.TwoWay });
			}
		}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _ui_btnAdd = (Button)GetTemplateChild("_ui_btnAdd");
            ProcessDisplayMemberPath();
            ProcessItemsSourceInitial();
            ProcessItemsSource();
            ProcessButtonAdd();
        }

		private void ProcessDisplayMemberPath()
		{
			//if (null != DataContext)
			//SetBinding(sDisplayMemberPathProperty, new Binding("DisplayMemberPath") { Mode = BindingMode.TwoWay });
		}
		private void ProcessItemsSourceInitial()
		{
			Search();
		}
		private void ProcessItemsSource()
		{
		}
		private void ProcessButtonAdd()
		{
			if (null == _ui_btnAdd)
				return;
			_ui_btnAdd.Visibility = (bButtonAdd ? Visibility.Visible : Visibility.Collapsed);
		}

        #region ui
        private void _ui_btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (null != _bItemAdd)
                _bItemAdd(_ui_tbName.Text);
        }

		private void _ui_tbName_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search();
		}
		private object[] GetMaxQtyOfItems(System.Collections.IEnumerable aArray)
		{
			int ni = 0;
			List<object> aRetVal = new List<object>();
			foreach (object oo in aArray)
			{
				if (ni > nMaxItemsInListOrTable)
					break;
				if (!aRetVal.Contains(oo))
				{
					aRetVal.Add(oo);
					ni++;
				}
			}
			return aRetVal.ToArray();
		}
		private void _ui_btnAdd_GotFocus(object sender, RoutedEventArgs e)
		{
			_bThisLostFocus = false;
		}
		private void _ui_tbName_LostFocus(object sender, RoutedEventArgs e)
		{
			_bThisLostFocus = true;
			_cTimerForLostFocus.Start();
		}
		private void LoosingFocus(object s, EventArgs args)
		{
			if (_bThisLostFocus)
				_ui_btnAdd.IsEnabled = false;
			_bThisLostFocus = false;
			_cTimerForLostFocus.Stop();
		}
		private void _ui_tbName_GotFocus(object sender, RoutedEventArgs e)
		{
			if (null != _cPropertyTarget && null != aItemsSource && 0 < _ui_tbName.Text.Length && 1 > ((IEnumerable<object>)aItemsSource).Count(o => _cPropertyTarget.GetValue(o, null).ToString().ToLower().Trim().Equals(_ui_tbName.Text)))
				_ui_btnAdd.IsEnabled = true;
		}
		#endregion

        public void Clear()
        {
            _ui_tbName.Text = "";
        }

        public void Search()
        {
            if (null == aItemsSourceInitial)
                return;
            List<object> aResult = new List<object>();
            object oItemSelected = null;
            if (null != ItemSelectedGet)
                oItemSelected = ItemSelectedGet();
            object cItem = (aItemsSource = aItemsSourceInitial).ToList().FirstOrDefault();
			_aPropertyTargets = new List<System.Reflection.PropertyInfo>();
			if (null != cItem && null != sDisplayMemberPath)
			{
				_cPropertyTarget = (System.Reflection.PropertyInfo)cItem.GetType().GetMember(sDisplayMemberPath)[0];
				_aPropertyTargets.Add(_cPropertyTarget);
			}
			if (!aAdditionalSearchFields.IsNullOrEmpty())
			{
				foreach (string sS in aAdditionalSearchFields)
					_aPropertyTargets.Add((System.Reflection.PropertyInfo)cItem.GetType().GetMember(sS)[0]);
			}
            string sNewName = "";
            if (0 < _ui_tbName.Text.Length)
            {
                #region анализ строки поиска
                _ui_btnAdd.IsEnabled = true;
                sNewName = _ui_tbName.Text.ToLower().Trim();
                string sTranslitNewName = "";
                string sWrongNewName = "";
                string sValue = null;
                string sWrong = null;

                foreach (char sChar in sNewName)
                {
                    sWrong = sValue = sChar.ToString();
                    if (char.IsLetter(sChar))
                    {
                        if (_ahTransliteration.ContainsKey(sValue))
                        {
                            sValue = _ahTransliteration[sValue];
                        }
                        else if (_ahTransliterationInvert.ContainsKey(sValue))
                        {
                            sValue = _ahTransliterationInvert[sValue];
                        }
                    }
                    if (_ahWrongKeyboard.ContainsKey(sWrong))
                    {
                        sWrong = _ahWrongKeyboard[sWrong];
                    }
                    else if (_ahWrongKeyboardInvert.ContainsKey(sWrong))
                    {
                        sWrong = _ahWrongKeyboardInvert[sWrong];
                    }
                    sTranslitNewName += sValue;
                    sWrongNewName += sWrong;
                }
                #endregion

                aResult.AddRange(ItemsFind(sNewName));
                if (sNewName != sTranslitNewName)
                    aResult.AddRange(ItemsFind(sTranslitNewName));
                if (sNewName != sWrongNewName && sTranslitNewName != sWrongNewName)
                    aResult.AddRange(ItemsFind(sWrongNewName));

                aItemsSource = GetMaxQtyOfItems(aResult);
            }
            else
            {
                _ui_btnAdd.IsEnabled = false;
                aItemsSource = GetMaxQtyOfItems(aItemsSource);
            }
            if (null != _cPropertyTarget && 0 < aResult.Count(o => _cPropertyTarget.GetValue(o, null).ToString().ToLower().Trim().Equals(sNewName)))
                _ui_btnAdd.IsEnabled = false;
            if (null != ItemSelectedSet)
                ItemSelectedSet(oItemSelected);
        }
        private List<object> ItemsFind(string sPattern)
        {
            List<object> aRetVal = new List<object>();
            List<object> aStartedWith = new List<object>();
			List<object> aAdditional = new List<object>();
			List<object> aAdditionalStartWidth = new List<object>();
			sPattern = sPattern.ToLower();
            string sValue;
            foreach (object o in aItemsSource)
            {
				for (int nI=0;nI< _aPropertyTargets.Count;nI++)
                {
                    sValue = _aPropertyTargets[nI].GetValue(o, null) == null ? "" : _aPropertyTargets[nI].GetValue(o, null).ToString().ToLower().Trim();
                    if (sValue.Contains(sPattern))
					{
						if (nI == 0)
						{
							if (sValue.StartsWith(sPattern))
								aStartedWith.Add(o);
							else
								aRetVal.Add(o);
						}
						else
						{
							if (sValue.StartsWith(sPattern))
								aAdditionalStartWidth.Add(o);
							else
								aAdditional.Add(o);
						}
						break;
					}
				}
            }
			aRetVal.InsertRange(0, aAdditionalStartWidth);
			aRetVal.InsertRange(0, aStartedWith);
			aRetVal.AddRange(aAdditional);
            return aRetVal;
        }
        private Dictionary<string, string> GetTransliterationDictionary(bool bInvert)
        {
            Dictionary<string, string> sRetVal = new Dictionary<string, string>();
            string[] as1 = { "a", "b", "v", "g", "d", "e", "zh", "z", "i", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "c", "ch", "sh", "w", "y", "yu", "ya" };
            string s2 = "абвгдежзиклмнопрстуфхцчшщыюя";

            if (bInvert)
            {
                for (int i = 0; s2.Length > i; i++)
                    sRetVal.Add(as1[i], s2.Substring(i, 1));
            }
            else
            {
                for (int i = 0; s2.Length > i; i++)
                    sRetVal.Add(s2.Substring(i, 1), as1[i]);
            }

            return sRetVal;
        }
        private Dictionary<string, string> GetWrongKeyDictionary(bool bInvert)
        {
            Dictionary<string, string> sRetVal = new Dictionary<string, string>();
            string s1 = "qwertyuiop[]asdfghjkl;'zxcvbnm,.`";
            string s2 = "йцукенгшщзхъфывапролджэячсмитьбюё";
            if (bInvert)
            {
                string s3 = s1;
                s1 = s2;
                s2 = s3;
            }
            for (int i = 0; s1.Length > i; i++)
            {
                sRetVal.Add(s1.Substring(i, 1), s2.Substring(i, 1));
            }

            return sRetVal;
        }
    }
}
