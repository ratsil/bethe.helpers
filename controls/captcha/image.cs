using System;
using System.Web;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Web.SessionState;
using System.Security.Cryptography;

namespace captcha
{
	public class image : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// You will need to configure this handler in the web.config file of your 
		/// web and register it with IIS before being able to use it. For more information
		/// see the following link: http://go.microsoft.com/?linkid=8101007
		/// </summary>
		#region IHttpHandler Members

		public bool IsReusable
		{
			// Return false in case your Managed Handler cannot be reused for another request.
			// Usually this would be false in case you have some state information preserved per request.
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
            try
            {
                if (null == HttpContext.Current.Request.Params["name"])
                    throw new Exception("wrong request");
                string sCaptchaName = HttpContext.Current.Request.Params["name"];
                int nSourceMatWidth = 800, nSourceMatHeight = 600, nSourceDigitWidth = 120, nSourceDigitHeight = 120, nTargetWidth = 610, nTargetHeight = 180, nRotateSafe = 50, nDigitSpace = 7;
                string sCode = "";
                Random cRandom = new Random((DateTime.Now.Ticks.ToString() + HttpContext.Current.Session.SessionID).GetHashCode());
                context.Response.ContentType = "image/jpeg";
                Bitmap cBitmap = new Bitmap(nTargetWidth, nTargetHeight);
                Graphics cGraphics = Graphics.FromImage(cBitmap);
                cGraphics.RotateTransform(cRandom.Next(-5, 6));
                cGraphics.DrawImage(Resources.mat,
                    new Point(
                        cRandom.Next((nTargetWidth + nRotateSafe) - nSourceMatWidth, -1 * nRotateSafe),
                        cRandom.Next((nTargetHeight + nRotateSafe) - nSourceMatHeight, -1 * nRotateSafe)
                    )
                );

                int nDigit = 0;
                for (int nIndx = 0; 4 > nIndx; nIndx++)
                {
                    nDigit = cRandom.Next(0, 10);
                    sCode += nDigit;
                    cGraphics.ResetTransform();
                    cGraphics.TranslateTransform(50 + (nIndx * (nSourceDigitWidth + nDigitSpace)), nRotateSafe);
                    cGraphics.RotateTransform((float)cRandom.Next(-300, 300) / 10);
                    cGraphics.DrawImage((Bitmap)Resources.ResourceManager.GetObject("_" + nDigit), new Point(0, 0));
                }
                HttpContext.Current.Session["captcha_code:" + sCaptchaName] = (sCode + HttpContext.Current.Session.SessionID).GetHashCode();
                HttpContext.Current.Session["captcha_expire:" + sCaptchaName] = DateTime.Now.AddSeconds(60);

                ImageCodecInfo cImageCodecInfo = null;
                foreach (ImageCodecInfo cICI in ImageCodecInfo.GetImageEncoders())
                {
                    if (ImageFormat.Jpeg.Guid == cICI.FormatID)
                    {
                        cImageCodecInfo = cICI;
                        break;
                    }
                }
                if (null == cImageCodecInfo)
                    throw new Exception("can't find jpeg codec");
                EncoderParameters cEncoderParameters = new EncoderParameters(1);
                cEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 75L);
                cBitmap.GetThumbnailImage(170, 50, new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero).Save(context.Response.OutputStream, cImageCodecInfo, cEncoderParameters);
                cBitmap.Dispose();
            }
            catch { }
		}
		private bool ThumbnailCallback()
		{
			return false;
		}

		#endregion
	}
}
