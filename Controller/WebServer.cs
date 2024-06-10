using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EVEAutoInvite
{
    internal class WebServer
    {
        private readonly HttpListener _listener;

        public WebServer(string listenerPrefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(listenerPrefix);
        }

        public async Task<HttpListenerRequest> StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Start();

            try
            {
                var contextTask = _listener.GetContextAsync();
                var cancelTask = new TaskCompletionSource<bool>();

                using (cancellationToken.Register(() => cancelTask.SetResult(true)))
                {
                    if (contextTask == await Task.WhenAny(contextTask, cancelTask.Task))
                    {
                        if (contextTask.IsCompleted)
                        {
                            HttpListenerContext context = contextTask.Result;

                            // Prepare the response content
                            string responseString = $"<html><body><h1>EVE-AutoFleet</h1><br><h3>You may close this tab.</h3></body></html>";
                            byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);

                            // Set the response headers
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.ContentType = "text/html";
                            context.Response.ContentLength64 = responseBytes.Length;

                            // Write the response data to the output stream asynchronously
                            await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);

                            // Close the output stream
                            context.Response.Close();

                            return context.Request;
                        }
                    }
                    else
                    {
                        _listener.Stop();
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation exception if needed
                Console.WriteLine("Operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                // Log or handle any exceptions
                Console.WriteLine($"Error processing request: {ex.Message}");
                return null;
            }
            finally
            {
                _listener.Stop();
                _listener.Close();
            }

            throw new OperationCanceledException(cancellationToken);
        }
    }
}