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

using System.Collections.Generic;

using System.Threading;

namespace WebApplication1
{
	/**
	 * The only purpose of this web page is to send a message to all registered listeners
	 * of the webhandler. This is accomplished by sending a message to a static method
	 */
	public partial class _Default : System.Web.UI.Page
	{
		protected void Page_Load( object sender, EventArgs e ) {
			foreach( string key in Handler1.callbacks.Keys ) {
			Response.Write( key + "<br>" );
			}
			
		}

		protected void Button1_ServerClick( object sender, EventArgs e )
		{
			// signal id

			// note that we can't use an iterator-based solution for going
			// through all callbacks because we want to remove dead ones
			// from the list internally -

			Handler1.CallAllConnectedClients( message.Value );
			Handler1.DeleteDisconnectedClients();
		}

		protected void Button2_ServerClick( object sender, EventArgs e ) {
			Handler1.CallConnectedClient( message.Value, clientID.Value );
			Handler1.DeleteDisconnectedClients();
		}
	}
}
