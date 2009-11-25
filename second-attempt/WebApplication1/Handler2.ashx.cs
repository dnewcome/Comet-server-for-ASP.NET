using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace WebApplication1
{
	/// <summary>
	/// Summary description for $codebehindclassname$
	/// </summary>
	[WebService( Namespace="http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo=WsiProfiles.BasicProfile1_1 )]
	public class Handler2 : IHttpHandler
	{

		public void ProcessRequest( HttpContext context )
		{
			context.Response.ContentType = "text/plain";
			context.Response.Write( "Hello World" );
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}
