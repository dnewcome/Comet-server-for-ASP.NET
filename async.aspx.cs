using System;
using System.Web;
using System.Web.UI;

public partial class AsyncPage : Page
{
	private int i=0;
	// try to attach another task here...
	PageAsyncTask m_asyncTask;

	private delegate void DoAsyncDelegate();

	private	DoAsyncDelegate m_doAsyncDelegate;

	public void Page_Load( Object sender, EventArgs e )
	{
		// we have to do all the usual tricks to see any output before the
		// page `renders' completely by asp.net standards.. remember to flush
		for( int i = 0; i < 100; i++ )
		{
			Response.Write( "<span></span>" );
		}

		m_doAsyncDelegate = new DoAsyncDelegate( DoAsync );

		m_asyncTask = new PageAsyncTask(
	   new BeginEventHandler( this.BeginLoad ),
	   new EndEventHandler( this.EndLoad ),
	   null, null, false
		);
		
		Response.Write("Working");
		Response.Flush();

		// can't be done in the handlers themselves.. have to be before 
		// prerender event has fired...
		// AttachHandlers();
		
		RegisterAsyncTask( m_asyncTask );

		
	}

	private void AttachHandlers()
	{
		BeginEventHandler bh = new BeginEventHandler( this.BeginLoad );
		EndEventHandler eh = new EndEventHandler( this.EndLoad );

		AddOnPreRenderCompleteAsync( bh, eh );
	}

	private IAsyncResult BeginLoad( Object src, EventArgs args, AsyncCallback cb, Object state )
	{
		Response.Write( "BeginLoad" );

		

		// note that BeginInvoke uses a threadpool thread
		return m_doAsyncDelegate.BeginInvoke( cb, state );
		
	}

	private void EndLoad( IAsyncResult ar )
	{
		m_doAsyncDelegate.EndInvoke( ar );
		Response.Write( "EndLoad" );
		
		
	}

	public void DoAsync()
	{
		// crazy hacks.. we just need to avoid infinitely
		// calling this in order to get page output...
		i++;
		if( i > 10 ) return;

		Response.Write( "DoAsync" );
		Response.Flush();

		// we have to keep creating new tasks, we can't keep reusing one ..
		RegisterAsyncTask( 
			new PageAsyncTask(
	   new BeginEventHandler( this.BeginLoad ),
	   new EndEventHandler( this.EndLoad ),
	   null, null, false )
		);
		// System.Threading.Thread.Sleep( 1000 );
	}
}
