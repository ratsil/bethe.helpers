using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace captcha
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:captcha runat=server />")]
	public class captcha : WebControl
	{
		public bool IsValid(string sCode)
		{
			bool bRetVal = false;
			try
			{
				if (null != Context.Session)
				{
					if (
						null != Context.Session["captcha_expire:" + ID]
						&& DateTime.Now < (DateTime)Context.Session["captcha_expire:" + ID]
						&& null != Context.Session["captcha_code:" + ID]
						&& (sCode + Context.Session.SessionID).GetHashCode() == (int)Context.Session["captcha_code:" + ID]
					)
						bRetVal = true;
					Context.Session["captcha_expire:" + ID] = null;
					Context.Session["captcha_code:" + ID] = null;
				}
			}
            catch { }
            return bRetVal;
		}

		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		protected override void RenderContents(HtmlTextWriter writer)
		{
			string sUID = Math.Abs(Context.Session.GetHashCode()+ID.GetHashCode()).ToString();
			string sTimer = "window.setTimeout('captcha_" + sUID + "_reload()', " + (nTimeToLive * 1000) + ");";
			string sContent = "<img id=\"_ui_imgCaptcha" + sUID + "\" src=\"captcha.image.ashx?name=" + ID + "\" onclick=\"captcha_" + sUID + "_reload();\" style=\"cursor:pointer;\" />";
			sContent += "<script type=\"text/javascript\">";
			sContent += "function captcha_" + sUID + "_reload(){";
			sContent +=		"var ui_img = document.getElementById('_ui_imgCaptcha" + sUID + "');";
			sContent +=		"if(null == ui_img)";
			sContent +=			"return;";
			sContent +=		"ui_img.src = 'captcha.image.ashx?' + (new Date()).getTime() + '&name=" + ID + "';";
			sContent +=		sTimer;
			sContent += "}";
			sContent += sTimer;
			sContent += "</script>";
			writer.Write(sContent);
			base.RenderContents(writer);
		}
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue(60)]
		[Localizable(true)]
		public ushort nTimeToLive
		{
		    get
		    {
				if(null != ViewState["nTimeToLive"])
					return (ushort)ViewState["nTimeToLive"];
				return 60;
		    }

		    set
		    {
		        ViewState["nTimeToLive"] = value;
		    }
		}

		//protected override void RenderContents(HtmlTextWriter output)
		//{
		//    output.Write(Text);
		//}
	}
}