using System;
using Topshelf;
using log4net;

namespace TcpRpc
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = HostFactory.Run(app =>
            {
                ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                app.Service<Server>(service =>
                {
                    service.ConstructUsing(server => new Server(7707, logger));
                    service.WhenStarted(server => server.Start());
                    service.WhenStopped(server => server.Stop());
                });

                app.RunAsLocalService();
                app.StartAutomaticallyDelayed();
                app.EnableServiceRecovery(recoveryOption =>
                {
                    recoveryOption.RestartService(0);
                });

                app.SetServiceName("TcpRpc");
                app.SetDisplayName("TCP-RPC");
                app.SetDescription("A TCP Sockets JSON-RPC server (NOT WebSockets)");
            });

            Environment.ExitCode = (int)Convert.ChangeType(service, service.GetTypeCode());
        }
    }
}
