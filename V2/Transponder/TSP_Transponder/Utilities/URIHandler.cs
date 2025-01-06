
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using TSP_Transponder;
using System.Windows.Threading;

namespace IPC
{
    /// <summary>
    /// Specifies the behaviour for a URI handler.
    /// </summary>
    public interface IUriHandler
    {
        /// <summary>
        /// Handles the specified URI and returns a value indicating whether any action was taken.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool HandleUri(string uri);
    }

    /// <summary>
    /// Example implementation of a URI handler.
    /// </summary> 
    public partial class UriHandler : MarshalByRefObject, IUriHandler
    {
        private IUriHandler Handler = null;

        /// <summary>
        /// Registers the URI handler in the singular instance of the application.
        /// </summary>
        /// <returns></returns>
        public bool Register(string channelName)
        {
            try
            {
                IpcServerChannel channel = new IpcServerChannel(channelName);
                ChannelServices.RegisterChannel(channel, true);

                RemotingConfiguration.RegisterWellKnownServiceType(typeof(UriHandler), "UriHandler", WellKnownObjectMode.SingleCall);

                Console.WriteLine("Registered IPC");
                return true;
            }
            catch
            {
                Console.WriteLine("Couldn't register IPC channel.");
                Console.WriteLine("");
            }

            return false;
        }

        /// <summary>
        /// Returns the URI handler from the singular instance of the application, or null if there is no other instance.
        /// </summary>
        /// <returns></returns>
        public bool Connect(string channelName)
        {
            try
            {
                IpcClientChannel channel = new IpcClientChannel(channelName, null);
                ChannelServices.RegisterChannel(channel, true);
                string address = String.Format("ipc://{0}/UriHandler", channelName);
                Console.WriteLine(address);

                IUriHandler hdl = (IUriHandler)Activator.GetObject(typeof(IUriHandler), address);

                // need to test whether connection was established
                TextWriter.Null.WriteLine(hdl.ToString());

                Handler = hdl;
                return true;
            }
            catch (Exception ex)
            {
                if(ex.HResult == -2146233077)
                {
                    Console.WriteLine("No other instance found");
                }
                else
                {
                    Console.WriteLine("Failed to check other instances " + ex.Message);
                }
            }

            return false;
        }


        public bool IsConnected()
        {
            return Handler != null;
        }

        public void Send(string message)
        {
            Handler.HandleUri(message);
        }

        public void Receive(string message)
        {
            HandleUri(message);
        }
        /// <summary>
        /// Handles the URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool HandleUri(string uri)
        {
            Console.WriteLine("URI Received: " + uri);
            App.MW.ProcessHandler(uri);
            return true;
        }
    }
}
