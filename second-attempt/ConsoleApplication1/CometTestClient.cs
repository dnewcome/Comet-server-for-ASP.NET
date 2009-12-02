using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace CometServer
{
	/*
	* Console client to do massive number of concurrent connections 
	* to the comet server for testing
	*/
	class CometTestClient
	{
		static void Main( string[] args ) {
			ProcessArguments( args );
			string HttpHeaders = 
				"GET " + PATH + " HTTP/1.1\r\nHost: " + HOSTNAME + "\r\n\r\n";
			headerArray = Encoding.ASCII.GetBytes( HttpHeaders );

			Console.WriteLine( HttpHeaders );
			// note that using the threadpool results in substantially faster 
			// performance when opening all of the connections
			for( int i=0; i < ConcurrentConnections; i++ ) {
				if( USE_THREADPOOL ) {
					ThreadPool.QueueUserWorkItem( new WaitCallback( TcpConnectAsync ) );
				}
				else {
					TcpConnectAsync( null );
				}
			}
			// once we make all our connections, put the main thread to sleep
			// so that the program doesn't exit
			Thread.Sleep( Timeout.Infinite );
		}

		/**
		* Callback that initiates the Tcp connection.
		* in_context is just to satisfy the interface for delegate WaitCallback
		* note that we have to use raw tcp connection instead of HttpWebRequest since 
		* the request must complete before we can get any data via the callback
		*/
		static void TcpConnectAsync( object in_context ) {
			TcpClient client = new TcpClient( HOSTNAME, PORT );
			NetworkStream stream = client.GetStream();
			stream.Write( headerArray, 0, headerArray.Length );

			// we can't do synchronous stream copy otherwise the thread blocks on
			// last call to Stream.Read(). Console is threadsafe, so we don't need
			// to keep locks on stdout
			Stream stdout = System.Console.OpenStandardOutput();
			CopyStreamAsync.Copy( stream, stdout );
		}

		/**
		 * Helper function to deal with commandline args. Quick and dirty
		 */
		private static void ProcessArguments( string[] in_args ) {
			if( in_args.Length < 4 ) {
				Console.WriteLine( "Not enough arguments, using defaults" );
			}
			else {
				// very coarse error handling of args.
				try {
					HOSTNAME = in_args[ 0 ];
					PORT = Convert.ToInt32( in_args[ 1 ] );
					PATH = in_args[ 2 ];
					ConcurrentConnections = Convert.ToInt32( in_args[ 3 ] );
				}
				catch( Exception e ) {
					Console.WriteLine( "Error parsing arguments" );
					Console.WriteLine( e.ToString() );
					Environment.Exit( 1 );
				}
			}
		}

		// default parameters
		static string HOSTNAME = "localhost";
		static int PORT = 80;
		static string PATH = "/CometServer/Register.ashx";
		static byte[] headerArray;
		static int ConcurrentConnections = 1000;

		// USE_THREADPOOL determines whether or not we create the connections synchronously
		// or not - once response stream is opened, data is read async regardless of this
		static bool USE_THREADPOOL = false;

	} // class
} // namespace
