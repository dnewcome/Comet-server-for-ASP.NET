using System;
using System.Web.UI;
using System.Threading;

namespace CometServer
{
	/**
	 * Very simple page that is used to communicate with the 
	 * registration class in order to send notifications to
	 * connected clients
	 */
	public partial class Notify : Page
	{
		/**
		 * To see what is going on, we write the guids of all
		 * registered listeners out to the page on load
		 */
		protected void Page_Load( object sender, EventArgs e ) {
			foreach( string key in Register.Callbacks.Keys ) {
				Response.Write( key + "<br>" );
			}
		}

		/**
		 * Notify all connected clients
		 */
		protected void btnNotifyAll_Click( object sender, EventArgs e ) {
			Register.CallAllConnectedClients( message.Value );
		}

		/**
		 * Notify single connected client by ID
		 */
		protected void btnNotifyOne_Click( object sender, EventArgs e ) {
			Register.CallConnectedClient( message.Value, clientID.Value );
		}
	}
}
