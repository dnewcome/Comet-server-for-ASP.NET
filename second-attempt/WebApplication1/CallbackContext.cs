using System;
using System.Web;

namespace CometServer
{
	/**
	* Simple container class to hold the callback and the 
	* http context that we are calling
	*/
	class CallbackContext
	{
		public CallbackContext( NotificationCallback in_callback, HttpContext in_context ) {
			m_callback = in_callback;
			m_context = in_context;
		}

		public NotificationCallback Callback {
			get { return m_callback; }
		} private NotificationCallback m_callback;

		public HttpContext Context {
			get { return m_context; }
		} private HttpContext m_context;
	} // class
} // namespace
