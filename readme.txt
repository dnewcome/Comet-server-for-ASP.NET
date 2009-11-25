recovered this from www folder.. 
need to diff and see what changed

putting this under git source control

- update on this project - 
thinking more about comet and event-driven web programming now that
node.js is out.

when I first messed around with this, I was trying to just periodically
send some data down to the client. In the case of async.aspx.cs, this was
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