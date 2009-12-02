using System;
using System.Web;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace CometServer
{
	/**
	 * The Register class holds a static collection of HttpContexts and GUIDs
	 * that are used to send data to a clients that are registered. By sending
	 * an HTTP GET to this handler, a long running connection will be established
	 * through which futher data may be sent by any external process that knows
	 * the client's GUID. Alternatively, all connected clients can be sent a 
	 * broadcast notification.
	 * 
	 * Note that we depend on the worker process not getting recycled in 
	 * order to maintain client registrations. Also, we are unable
	 * to be notified of a client disconnect until after we have unsuccessfully
	 * tried to send data. The data will be sent and a first chance exception
	 * will be raised. Following this request, the .net framework will have
	 * been notified that the Tcp connection has been closed at the remote end, 
	 * and we can flag the connection for deletion.
	 * 
	 * Removal of connections from the registration dictionary is done 
	 * separately from the notification loop since we use an Iterator over
	 * the collection, and items may not be removed without invalidating the
	 * iterator.
	 */
	class Register : IHttpAsyncHandler
	{
		/*
		 * Type initializer for handler - set up the callbacks data structure here
		 */
		static Register() {
			System.Diagnostics.Trace.WriteLine( "Running Type initializer for Register" );	
			m_callbacks = new Dictionary<String, CallbackContext>();
			m_keysFlaggedForDeletion = new List<string>();
		}

		/**
		 * Instance constructor
		 */
		public Register() {
			System.Diagnostics.Trace.WriteLine( "Instantiating handler instance" );
		}

		/*
		 * BeginProcessRequest gets called automatically when the request comes in
		 * We don't have to handle things in ProcessRequest/PageLoad and then set 
		 * everything up manually using RegisterAsyncTask like we do with an async
		 * aspx page. We ignore 
		 */
		public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback callback, Object extraData ) {
			context.Response.Write( "<p>BeginProcessReqeust: Thread " + Thread.CurrentThread.ManagedThreadId + "</p>" );
			
			// async is a dummy IAsyncResult to satisfy the method signature
			AsyncOperation async = new AsyncOperation( null, null, null );

			// Callback context holds a callback and the httpcontext.
			// In hindsight we could have done this without the callback
			// so this is a possible point of refactoring. (TODO)
			CallbackContext callbackContext = new CallbackContext( 
				new NotificationCallback( Callback ), 
				context
			);

			/**
			 * TODO: this is too coarse of a lock, should be locking 'callbacks'
			 */
			lock( typeof( Register ) ) {
				Callbacks.Add( Guid.NewGuid().ToString(), callbackContext );
			}
			return async;
		}

		// simple data type to hold call data to callback for client notification
		private class CallConnectedClientData 
		{
			public CallConnectedClientData( string in_message, string in_key ) {
				Message = in_message;
				Key = in_key;
			}
			
			public string Message;
			public string Key;
		}

		/**
		 * invoke the callbacks - this is a static method
		 * I was thinking about how this could be abused in a multithreaded 
		 * environment, so doing a pretty pessimistic lock here
		 */
		public static void CallAllConnectedClients( string in_message ) {
			// get a burly, coarse-grained lock here
			lock( typeof( Register ) ) {
				foreach( string key in Register.Callbacks.Keys ) {
					// todo: use threadpool to invoke 
					if( USE_THREADPOOL ) {
						ThreadPool.QueueUserWorkItem(
							new WaitCallback( CallAllConnectedClientsCallback ),
							new CallConnectedClientData( in_message, key )
						);
					}
					else {
						Register.Callbacks[ key ].Callback.Invoke( in_message, key );
					}
				}
			}
		}

		public static void CallAllConnectedClientsCallback( object in_context ) {
			CallConnectedClientData data = ( CallConnectedClientData )in_context;
			Register.Callbacks[ data.Key ].Callback.Invoke( data.Message, data.Key );
			DeleteDisconnectedClients();
		}

		public static void CallConnectedClient( string in_message, string in_id ) {
			Register.Callbacks[ in_id ].Callback.Invoke( in_message, in_id );
			DeleteDisconnectedClients();
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
				Callbacks.Remove( key );
			}
			m_keysFlaggedForDeletion.Clear();
			System.Diagnostics.Trace.WriteLine( "There are " + Callbacks.Count + " callbacks registered after purging" );
		}

		/**
		 * write to output so that most browsers will show text without having completed the request
		 */
		private void flush( HttpContext in_context ) {
			if( FLUSH ) {
				for( int i=0; i < 1000; i++ ) {
					in_context.Response.Write( "<span></span>" );
				}
			}
			in_context.Response.Flush();
		}

		/**
		 * callback function that writes in_message out to the response stream
		 * of client with ID in_key.
		 */
		public void Callback( string in_message, string in_key ) {
			CallbackContext context = Callbacks[ in_key ];

			System.Diagnostics.Trace.WriteLine( "calling callback with message " + in_message );
		
			/*
			 * note that IsClientConnected doesn't get set until there has been a failure
			 * so the first time we try to write to disconnected response will result in first chance exception
			 * that we can't catch here for some reason
			 */
			if( context.Context.Response.IsClientConnected ) {
				context.Context.Response.Write( "<p>CallBack: Thread " + Thread.CurrentThread.ManagedThreadId + " " + in_message + "</p>" );
				flush( context.Context );
			}
			else {
				m_keysFlaggedForDeletion.Add( in_key );
			}
		}

		/**
		 * Dictionary of connected clients. Given the ID of the client
		 * we can look up all the information needed to send it data
		 * TODO: we should wrap this up with threadsafe accessors
		 * instead of relying on callers.
		 */
		public static Dictionary<String, CallbackContext> Callbacks {
			get { return m_callbacks; }
		} private static Dictionary<String, CallbackContext> m_callbacks;

		/*
		 * list of clients that we have determined are no longer connected.
		 * We keep a separate list since we can't remove items from Callbacks
		 * while iterating over it.
		 */
		private static List<string> m_keysFlaggedForDeletion;

		/*
		 * FLUSH can be set in order to write out lots of empty html tags
		 * in order to force visible output to some web browsers.
		 */
		private const bool FLUSH = false;

		/**
		 * When calling all clients, we can do so asynchronously by
		 *	using the threadpool.
		 */
		private const bool USE_THREADPOOL = false;

		/*
		 * Handlers that are 'reusable' are able to be used for more than one request,
		 * which is something that we don't want here.
		 */
		public bool IsReusable {
			get { return false; }
		}

		/*
		 * EndProcessRequest should never get called. We keep the request alive
		 * indefinitely 
		 */
		public void EndProcessRequest( IAsyncResult result ) { }

		/*
		* ProcessRequest must be on the interface of IHttpAsyncHandler
		* but should never get called by the framework.
		*/
		public void ProcessRequest( HttpContext context ) { }

	} // class
} // namespace WebApplication1