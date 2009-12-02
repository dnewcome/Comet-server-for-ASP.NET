This project is an extremely simple and naive implementation of a Comet-like
event registration and notification mechanism that is implemented using ASP.NET's
IHttpAsyncHandler interface.

The ASP.NET Comet server consists of two main parts: the client registration
handler Register.ashx, and the notification page Notify.aspx. The notification
page isn't really designed for use in any programmatic way, but rather as a 
sample implementation that calls the notification methods on the handler class.

There are several drawbacks with using ASP.NET for tracking client registrations.
If the worker process is recycled, we lose all client registration information.
Ideally, we would be able to persist this information in another way, but
we ultimately need to keep some persistent process in order to track sockets
and/or HttpContexts. Another is that we have to be careful about concurrent access
to the client registration data. I've put some very crude locking in place that
is probably very ineffecient.

In my testing, I was able to get over 16,000 clients registered using the 
commandline test client. In order to achieve this, you will need to be using
Server 2003 or Server 2008 with some tweaks to machine.config etc. The limit
seems to be in the way that .net handles socket buffers, but I'm not entirely
sure. Some sources indicate that we should be able to get 70,000+ Tcp connections
with Windows, but perhaps it is not possible using .net (using multiple 
ApplicationPools doesn't seem to make any difference on the limit).

when I first messed around with this idea, I was trying to just periodically
send some data down to the client. These old experiments can be found in the 
`old-files' folder.  In the case of async.aspx.cs, things were
very crude. I was just registering a callback to fire every once in a while,
sending some data.  I hadn't set up any way for the server to actually do a
notification. All it did was take some page data and send the data very slowly
without tying up a thread in the meantime.  I think that I had hundreds of 
requests pending when testing this out.

The second attempt was using the threadpool instead of using the implicit
async page attributes on the aspx page. This way I should be able to control
things better, and not have to keep registering a new callback in the callback
handler, basically 'trampolining' things.

I tried this out under mono and Microsoft both. Under IIS and under xsp2 
web server under mono.  I know that both worked, but I don't think that we 
get the benefit of socket pooling under linux.

- dan 11/24/2009