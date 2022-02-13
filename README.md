# TCP-RPC
A TCP Sockets server, C# demo project.

## How it works
The service provides a JSON PRC API accesible via TCP Socket connection (NOT WebSocket).
Can be installed and run in background on a Windows machine.

## How to connect
Connect using any TCP client such as Putty via port 7707.

## Usage example
Request:
```json
{"method":"hello","id":1,"params":["World"]}
```
Response:
```json
{"jsonrpc":"2.0","result":{"message":"Hello World!"},"id":1}
```
