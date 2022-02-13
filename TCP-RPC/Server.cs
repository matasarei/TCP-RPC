using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System;
using log4net;

namespace TcpRpc
{
    class Server
    {
        private readonly RequestProcessor requestProcessor;
        private readonly TcpListener listener;
        private readonly ILog logger;
        private readonly int port;
        private readonly Thread serverThread;

        public Server(int port, ILog logger)
        {
            this.port = port;
            this.logger = logger;

            listener = new TcpListener(IPAddress.Any, port);
            requestProcessor = new RequestProcessor(logger);

            serverThread = new Thread(new ParameterizedThreadStart(Listen))
            {
                IsBackground = true
            };
        }

        public void Start()
        {
            logger.Info(string.Format("Starting up proxy server on port {0}...", this.port));
            listener.Start();

            logger.Info("Waiting for clients...");
            serverThread.Start();
        }

        public void Stop()
        {
            logger.Info("Stopping server...");
            serverThread.Interrupt();
            listener.Stop();
        }

        private void Listen(object state)
        {
            Thread clientThread = new Thread(new ParameterizedThreadStart(Serve))
            {
                IsBackground = true
            };

            try
            {
                clientThread.Start(listener.AcceptTcpClient());
            } catch (InvalidOperationException exception)
            {
                logger.Debug("Operation error. ", exception);

                return;
            } catch (SocketException exception)
            {
                logger.Error("Transport error. ", exception);

                return;
            }

            Listen(state);
        }

        private void Serve(object state)
        {
            var client = (TcpClient)state;

            IPAddress ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            NetworkStream stream = client.GetStream();

            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, new UTF8Encoding(false));

            logger.Info(string.Format("A client connected: {0}", ip));

            try
            {
                while (!reader.EndOfStream)
                {
                    var request = reader.ReadLine();

                    logger.Info(string.Format("Processing a request from {0}: {1}", ip, request));
                    requestProcessor.HandleRequest(writer, request);
                }
            }
            catch (Exception exception)
            {
                logger.Error("Unable to process request. ", exception);
            } 
            finally
            {
                writer.Close();
                reader.Close();
                client.Close();
            }

            logger.Info(string.Format("A client disconnected: {0}", ip));
        }
    }
}
