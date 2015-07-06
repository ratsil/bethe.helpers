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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Browser;
using helpers.extensions;

namespace helpers.sl
{
	public class common
	{
		static public void CookieSet(string sKey, string sValue)
		{
			DateTime dtExpireDate = DateTime.Now.AddYears(20);
			string sCookie = String.Format("{0}={1};expires={2}", sKey, sValue, dtExpireDate.ToString("R"));
			HtmlPage.Document.SetProperty("cookie", sCookie);
		}
		static public string CookieGet(string sKey)
		{
			string[] aCookies = HtmlPage.Document.Cookies.Split(';');
			foreach (string sCookie in aCookies)
			{
				string[] aKeyValue = sCookie.Split('=');
				if (aKeyValue.Length == 2)
				{
					if (aKeyValue[0].ToString().Trim() == sKey && aKeyValue[1] != "")
						return aKeyValue[1];
				}
			}
			return null;
		}
	}
}
