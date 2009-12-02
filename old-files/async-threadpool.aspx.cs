using System;
using System.Web;
using System.Web.UI;
using System.Threading;

/**
 * This attempt at doing async pages uses the threadpool's ability to have
 * wait handles that fire periodically.  This is how we get around the fact
 * that we have to keep registering handlers if we want to have periodic 
 * events otherwise. See async.cs for details on the alternatives 
 */
public partial class AsyncThreadpool : Page
{
	// Delegate used for the timer callback 
	private WaitOrTimerCallback m_wtCallback;

	// Delegate used to invoke method that we want to call periodically
	private delegate void DoAsyncDelegate();
	private DoAsyncDelegate m_doAsyncDelegate;
	

	/**
	 * we lay the groundwork for further async calls in page load
 	 */
	public void Page_Load( Object sender, EventArgs e )
	{
		Response.Write("page load entered\n");

		// we set the timeout to something really long, so that .net
		// won't time out our request - we want long running
		TimeSpan timeOut = new TimeSpan( 1, 0, 0, 0, 0 );
		this.AsyncTimeout = timeOut;
		
		// fire up a delegate that invokes the function that we 
		// ultimately want to call
		m_doAsyncDelegate = new DoAsyncDelegate( DoAsync );
		
		// this is a hack to get some browsers to display something
		// we write some data to fill the buffer or something	
		for( int i=0; i < 1000; i++ )
		{ 
			Response.Write("<span></span>");
		}
		Response.Flush();

		// here we set up a periodic poller using threadpool. the
		// basic idea is that we have a call back that is periodically
		// visited by a threadpool thread.  This really isn't a poll,
		// and is not really done by the threadpool, we use an autoresetevent
		m_wtCallback = new WaitOrTimerCallback( Callback );
		AutoResetEvent are = new AutoResetEvent( false );
		ThreadPool.RegisterWaitForSingleObject( are, m_wtCallback, null, 1000, false );
	
		// the meat of getting this all to work revolves around the 
		// registration of handlers for begin and end
		RegisterAsyncTask( 
			new PageAsyncTask(
	   		new BeginEventHandler( this.BeginLoad ),
	  		new EndEventHandler( this.EndLoad ),
	  		null, null, false )
		);

	}

	/**
	 * this is the callback method that is called by the threadpool at the request
	 * of the autoresetevent that we stick in there. This doesn't have much to 
	 * do with the actual async page stuff, but it does the main bit of what 
	 * we are trying to accomplish
	 */
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

	/**
	 * We never actually get here since we block forever on DoAsync()
	 */
	private void EndLoad( IAsyncResult ar )
	{
		m_doAsyncDelegate.EndInvoke( ar );		
	}

	/** 
	 * this is just to satisfy the async page mechanism.. we block here
	 * indefinitely and let the threadpool call the other callback method 
	 */
	private void DoAsync()
	{
		Response.Write("sleeping\n");
		System.Threading.Thread.Sleep( Timeout.Infinite );
	}
}
