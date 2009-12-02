using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Threading;

namespace CometServer
{
	/**
	 * Custom IAsyncResult implementation - this is mostly a 
	 * dummy object, the only thing we care about is the AsyncState
	 */
	public class AsyncOperation : IAsyncResult
	{
		bool IAsyncResult.IsCompleted {
			get { return m_completed; }
		} private bool m_completed;

		WaitHandle IAsyncResult.AsyncWaitHandle {
			get { return null; }
		}

		Object IAsyncResult.AsyncState {
			get { return m_state; }
		} private Object m_state;

		bool IAsyncResult.CompletedSynchronously {
			get { return false; }
		}

		public HttpContext Context {
			get { return m_context; }
		} private HttpContext m_context;

		private AsyncCallback m_callback;

		public AsyncOperation( AsyncCallback in_callback, HttpContext in_context, Object in_state ) {
			m_callback = in_callback;
			m_context = in_context;
			m_state = in_state;
			m_completed = false;
		}

	} // class
} // namespace
