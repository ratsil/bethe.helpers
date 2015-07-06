using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace helpers.web
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public class Exception
	{
		public Exception cExceptionInner { get; set; }
		public string sType { get; set; }
		public string sMessage { get; set; }
		public string sStackTrace { get; set; }

		public Exception()
		{
			cExceptionInner = null;
			sType = null;
			sMessage = null;
			sStackTrace = null;
		}
		public Exception(System.Exception ex)
			: this()
		{
			cExceptionInner = null;
			if(null != ex.InnerException)
				cExceptionInner = new Exception(ex.InnerException);
			sType = ex.GetType().ToString();
			sMessage = ex.Message;
			sStackTrace = ex.StackTrace;
		}
	}
}
