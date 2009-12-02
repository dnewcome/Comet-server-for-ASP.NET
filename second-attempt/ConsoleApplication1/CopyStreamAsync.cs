using System;
using System.IO;

namespace CometServer
{
	/**
	* Copy one stream to another, performing async reads on the input
	*	output stream is written synchronously
	*	Both streams remain open on return.
	*	The Copy method kicks off the first read and sets up the
	*	callback for handling the data and any subsequent reads.
	*/
	class CopyStreamAsync
	{
		public static void Copy( Stream in_sourceStream, Stream out_destStream ) {
			Streams streams = new Streams( in_sourceStream, out_destStream );

			IAsyncResult ar = in_sourceStream.BeginRead(
				streams.Buffer, 
				0, 
				streams.BufferSize, 
				new AsyncCallback( CopyStreamCallback ), 
				streams
			);
		}

		/**
		 * Callback function that does all the work
		 */
		private static void CopyStreamCallback( IAsyncResult in_result ) {
			Streams streams = ( Streams )in_result.AsyncState;
			int bytesRead = streams.InputStream.EndRead( in_result );

			if( bytesRead > 0 ) {
				streams.OutputStream.Write( streams.Buffer, 0, bytesRead );

				// set up the next read
				IAsyncResult ar = streams.InputStream.BeginRead(
					streams.Buffer, 
					0, 
					streams.BufferSize, 
					new AsyncCallback( CopyStreamCallback ), 
					streams
				);
			}
		}
	} // class

	/**
	 * small container to hold a pair of streams to be copied. We need this 
	 * since both streams are needed in the async callback, and the only
	 * place we can store them is IAsyncResult.AsyncState. We also
	 * need to keep the buffer here, as it is the only context that
	 * is passed to the callback.
	 */
	class Streams
	{
		public Streams( Stream in_inputStream, Stream in_outputStream ) {
			InputStream = in_inputStream;
			OutputStream = in_outputStream;
			Buffer = new Byte[ BufferSize ];
		}

		public Stream InputStream;
		public Stream OutputStream;
		public int BufferSize = 256;
		public Byte[] Buffer;
	} // class

} // namespace