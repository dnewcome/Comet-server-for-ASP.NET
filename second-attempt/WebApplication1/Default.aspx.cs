using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Threading;

namespace WebApplication1
{
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load( object sender, EventArgs e )
		{

		}

		protected void Button1_ServerClick( object sender, EventArgs e )
		{
			// signal id
			foreach( Handler1.MyCallback callback in Handler1.callbacks ) {
				// todo: use threadpool to invoke 
				callback.Invoke( message.Value );
			
			}
			
		}
	}
}
