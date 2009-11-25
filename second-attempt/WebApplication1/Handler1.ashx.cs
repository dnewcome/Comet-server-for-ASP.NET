using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace WebApplication1
{

	class Handler1 : IHttpAsyncHandler
	{
		/*
		 * Handlers that are 'reusable' are able to be used for more than one request. I 
		 * don't think that this guarantees that one persistent object will be kept for all
		 * requests, it merely means that the class must be thread safe in order to be used
		 * as a reusable request handler
		 */
		public bool IsReusable { 
			get { return false; } 
		}

		private static List<string> m_keysFlaggedForDeletion;

		public delegate void MyCallback( string message, string key );

		public static Dictionary<String, CallbackContext> callbacks;

		/*
		 * Type initializer for handler - set up the callbacks data structure here
		 */
		static Handler1() {
			System.Diagnostics.Trace.WriteLine( "Running Type initializer for Handler1" );	
			callbacks = new Dictionary<String, CallbackContext>();
			m_keysFlaggedForDeletion = new List<string>();
		}

		/**
		 * invoke the callbacks - this is a static method
		 * I was thinking about how this could be abused in a multithreaded 
		 * environment, so doing a pretty pessimistic lock here
		 */
		public static void CallAllConnectedClients( string in_message ) {
			// get a burly, coarse-grained lock here
			lock( typeof( Handler1 ) ) {
				foreach( string key in Handler1.callbacks.Keys ) {
					// todo: use threadpool to invoke 
					Handler1.callbacks[ key ].Callback.Invoke( in_message, key );
				}
			}
		}

		/*
		 * Since we can't delete unused items during the iteration when we are 
		 * performing the callbacks, we need another method to do it after the fact
		 * 
		 * note that it is a hack for this to be static. we are asking for issues with
		 * thread safety here
		 */
		public static void DeleteDisconnectedClients() {
			foreach( string key in m_keysFlaggedForDeletion ) {
				callbacks.Remove( key );
			}
			m_keysFlaggedForDeletion.Clear();
			System.Diagnostics.Trace.WriteLine( "There are " + callbacks.Count + " callbacks registered after purging" );
		}

		// constructor
		public Handler1() {
			System.Diagnostics.Trace.WriteLine( "Instantiating handler instance" );
		}

		/**
		 * write to output so that most browsers will show text without having completed the request
		 */
		private void flush( HttpContext in_context ) {
			for( int i=0; i < 1000; i++ ) {
				in_context.Response.Write( "<span></span>" );
			}
			in_context.Response.Flush();
		}

		~Handler1() {
			System.Diagnostics.Trace.WriteLine( "Finalizing handler instance" );
		}

		/**
		 * this is the method that we want to fire periodically from another thread
		 */
		public void Callback( string in_message, string in_key ) {
			CallbackContext context = callbacks[ in_key ];

			System.Diagnostics.Trace.WriteLine( "calling callback with message " + in_message );

		
			// note that IsClientConnected doesn't get set until there has been a failure
			// so the first time we try to write to disconnected response will result in first chance exception
			// that we can't catch here for some reason
			if( context.Context.Response.IsClientConnected ) {
				context.Context.Response.Write( "<p>CallBack: Thread " + Thread.CurrentThread.ManagedThreadId + " " + in_message + "</p>" );
				flush( context.Context );
			}
			else {
				m_keysFlaggedForDeletion.Add( in_key );
			}
		}

		/*
		 * BeginProcessRequest gets called automatically when the request comes in
		 * We don't have to handle things in ProcessRequest/PageLoad and then set 
		 * everything up manually using RegisterAsyncTask like we do with an async
		 * aspx page
		 */
		public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, Object extraData ) {
			context.Response.Write( "<p>BeginProcessReqeust: Thread " + Thread.CurrentThread.ManagedThreadId + "</p>");
			AsyncOperation async = new AsyncOperation( cb, context, extraData );
			CallbackContext cbx = new CallbackContext( new MyCallback( Callback ), context ); 
			callbacks.Add( Guid.NewGuid().ToString(), cbx );
			return async;
		}

		/*
		 * EndProcessRequest gets called automatically as part of the Begin/End pair. 
		 * We return the IAsyncResult in Begin, which in our case is a custom 
		 * AsyncOperation, so our downcast is valid here
		 */
		public void EndProcessRequest( IAsyncResult result ) {
			AsyncOperation op = ( AsyncOperation )result;
			op.Context.Response.Write( "<p>EndProcessRequest: Thread " + Thread.CurrentThread.ManagedThreadId + "</p>" );
			System.Diagnostics.Trace.Write( "EndProcessReqeust reached" );
		}

		/*
		 * ProcessRequest must be on the interface of IHttpAsyncHandler - note sure why
		 * though, since it never gets called by the framework
		 */
		public void ProcessRequest( HttpContext context ) {
			throw new InvalidOperationException();
		}

	} // class

	/**
	 * Custom IAsyncResult implementation
	 */
	class AsyncOperation : IAsyncResult
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

	/**
	* Class to hold the callback and the http context that we are calling
	*/
	class CallbackContext
	{
		public CallbackContext( Handler1.MyCallback in_callback, HttpContext in_context ) {
			m_callback = in_callback;
			m_context = in_context;
		}

		public Handler1.MyCallback Callback {
			get { return m_callback; }
		} private Handler1.MyCallback m_callback;

		public HttpContext Context {
			get { return m_context; }
		} private HttpContext m_context;
	} // class

} // namespace WebApplication1