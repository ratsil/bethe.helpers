using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace helpers.web
{
	public class SoapFaultHandler : IHttpHandler, IRequiresSessionState
	{
		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{
			IHttpHandlerFactory fact = (IHttpHandlerFactory)Activator.CreateInstance(Type.GetType("System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"));
			IHttpHandler handler = fact.GetHandler(context, context.Request.RequestType, context.Request.Path, context.Request.PhysicalApplicationPath);
			try
			{
				handler.ProcessRequest(context);
				context.Response.StatusCode = 200;
				context.ApplicationInstance.CompleteRequest();
			}
			finally
			{
				fact.ReleaseHandler(handler);
			}
		}
	}
}
