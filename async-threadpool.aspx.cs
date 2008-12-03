using System;
using System.Web;
using System.Web.UI;
using System.Threading;

public partial class AsyncThreadpool : Page
{
	private WaitOrTimerCallback m_wtCallback;
	private delegate void DoAsyncDelegate();
	private DoAsyncDelegate m_doAsyncDelegate;
	
	public void Page_Load( Object sender, EventArgs e )
	{
		Response.Write("page load entered\n");
		

		TimeSpan timeOut = new TimeSpan( 1, 0, 0, 0, 0 );
		this.AsyncTimeout = timeOut;
		
		m_doAsyncDelegate = new DoAsyncDelegate( DoAsync );
		
	
		for( int i=0; i < 1000; i++ )
		{ 
			Response.Write("<span></span>");
		}
		Response.Flush();
		m_wtCallback = new WaitOrTimerCallback( Callback );
		AutoResetEvent are = new AutoResetEvent( false );
		ThreadPool.RegisterWaitForSingleObject( are, m_wtCallback, null, 1000, false );
	
			RegisterAsyncTask( 
			new PageAsyncTask(
	   new BeginEventHandler( this.BeginLoad ),
	   new EndEventHandler( this.EndLoad ),
	   null, null, false )
		);

	}
	
	private void Callback( Object state, bool timedout )
	{
		Response.Write( "Callback" );
		Response.Flush();
	}
	
	private IAsyncResult BeginLoad( Object src, EventArgs args, AsyncCallback cb, Object state )
	{
		// need to return dummy IAsyncResult or find a way to make a fake async call, so that
		// we suspend the current execution without creating another thread.  For now we just
		// use a threadpool thread since we are lazy.. 
		Response.Write("calling DoAsync\n");
		return m_doAsyncDelegate.BeginInvoke( cb, state );	
	}

	private void EndLoad( IAsyncResult ar )
	{
		m_doAsyncDelegate.EndInvoke( ar );		
	}

	private void DoAsync()
	{
		Response.Write("sleeping\n");
		System.Threading.Thread.Sleep( Timeout.Infinite );
	}
}