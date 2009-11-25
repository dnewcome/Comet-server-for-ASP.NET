using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;

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
		public bool IsReusable { get { return false; } }
		public delegate void MyCallback( string message );

		public static List<MyCallback> callbacks;

		static Handler1() {
			System.Diagnostics.Trace.WriteLine( "Running Type initializer for Handler1" );
			callbacks = new List<MyCallback>();
		}

		// constructor
		public Handler1() {
			System.Diagnostics.Trace.WriteLine( "Instantiating handler" );
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
			callbacks.Add( new MyCallback( async.Callback ) );
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

	class AsyncOperation : IAsyncResult
	{
		private bool m_completed;
		private Object m_state;
		private AsyncCallback m_callback;
		private HttpContext m_context;

		private WaitOrTimerCallback m_wtCallback;


		bool IAsyncResult.IsCompleted { get { return m_completed; } }
		WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
		Object IAsyncResult.AsyncState { get { return m_state; } }
		bool IAsyncResult.CompletedSynchronously { get { return false; } }
		public HttpContext Context { get { return m_context; } }

		public AsyncOperation( AsyncCallback in_callback, HttpContext in_context, Object in_state ) {
			m_callback = in_callback;
			m_context = in_context;
			m_state = in_state;
			m_completed = false;
		}

		public void StartAsyncWork() {
			//ThreadPool.QueueUserWorkItem( new WaitCallback( StartAsyncTask ), null );
			
			/*
			m_wtCallback = new WaitOrTimerCallback( Callback );
			AutoResetEvent are = new AutoResetEvent( false );
			ThreadPool.RegisterWaitForSingleObject( are, m_wtCallback, null, 1000, false );
			*/

			
			

		}

		/**
		 * this is the method that we want to fire periodically from another thread
		 */
		public void Callback( string in_message ) {
			
			System.Diagnostics.Trace.WriteLine( "calling callback with message " + in_message );
			
			m_context.Response.Write( "<p>CallBack: Thread " + Thread.CurrentThread.ManagedThreadId + " " + in_message + "</p>" );
			flush();
		}

		private void StartAsyncTask( Object workItemState ) {
			m_context.Response.Write( "<p>StartAsyncTask: Thread " + Thread.CurrentThread.ManagedThreadId + "</p>" );
			flush();

			// not sure how essential the completed attribute is. Things seem to work without it
			// m_completed = true;

			// we need to call the callback at the end of our async task, otherwise we never complete
			// m_callback( this );
		}

		/**
		 * write to output so that most browsers will show text without having completed the request
		 */
		private void flush()
		{
			for( int i=0; i < 1000; i++ )
			{
				m_context.Response.Write( "<span></span>" );
			}
			m_context.Response.Flush();
		}


	} // class
} // namespace WebApplication1