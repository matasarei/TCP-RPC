using AustinHarris.JsonRpc;
using System.IO;
using System;
using log4net;

namespace TcpRpc
{
    class RequestProcessor : JsonRpcService
    {
        private readonly AsyncCallback requestHandler;

        public RequestProcessor(ILog logger)
        {
            requestHandler = new AsyncCallback(state =>
            {
                var async = (JsonRpcStateAsync)state;
                var writer = (StreamWriter)async.AsyncState;

                try
                {
                    writer.WriteLine(async.Result);
                    writer.FlushAsync();
                } catch (Exception exception)
                {
                    logger.Error("Unable to write data. ", exception);
                    writer.Close();
                }
            });
        }

        public void HandleRequest(StreamWriter writer, string jsonRpcRequest)
        {
            var async = new JsonRpcStateAsync(requestHandler, writer) 
            {
                JsonRpc = jsonRpcRequest
            };

            JsonRpcProcessor.Process(async);
        }

        /// <summary>
        /// Hello World! {"method":"hello","id":1,"params":["World"]}
        /// </summary>
        /// <returns>Ready to serialization object</returns>
        [JsonRpcMethod("hello")]
        public object GetLoadTeachFull(string name)
        {
            return new
            {
                message = string.Format("Hello {0}!", name)
            };
        }
    }
}