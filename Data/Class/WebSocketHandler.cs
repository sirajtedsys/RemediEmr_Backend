using Newtonsoft.Json;
using RemediEmr.Class;
using RemediEmr.Repositry;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using static JwtService;

namespace RemediEmr.Data.Class
{
    //public class WebSocketHandler
    //{
    //    private readonly TvTokenRepositry _tokenService;
    //    private static List<WebSocket> _connectedClients = new List<WebSocket>();

    //    public WebSocketHandler(TvTokenRepositry tokenService)
    //    {
    //        _tokenService = tokenService;
    //    }
    //    public async Task HandleWebSockeaatAsyvcnc(WebSocket webSocket)
    //    {
    //        // Handle incoming WebSocket messages here.
    //        // For example, listen for messages from the client and send responses.
    //        var buffer = new byte[1024 * 4];
    //        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    //        while (!result.CloseStatus.HasValue)
    //        {
    //            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
    //            // Handle the message or perform the required operation.

    //            // Send back a response to the client
    //            var responseMessage = Encoding.UTF8.GetBytes("Your response here");
    //            await webSocket.SendAsync(new ArraySegment<byte>(responseMessage), WebSocketMessageType.Text, true, CancellationToken.None);

    //            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    //        }

    //        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    //    }

    //    private static readonly ConcurrentDictionary<string, WebSocket> _clients = new(); // Store deviceId -> WebSocket

    //    public static async Task AddClient(string deviceId, WebSocket socket)
    //    {
    //        _clients[deviceId] = socket; // Store WebSocket connection with Device ID
    //    }

    //    public static async Task NotifyClients(List<TokenInfo> tokens)
    //    {
    //        var json = JsonConvert.SerializeObject(tokens);
    //        var buffer = Encoding.UTF8.GetBytes(json);

    //        foreach (var client in _clients.Values)
    //        {
    //            if (client.State == WebSocketState.Open)
    //            {
    //                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    //            }
    //        }
    //    }

    //    public static string[] GetConnectedDeviceIds()
    //    {
    //        return _clients.Keys.ToArray(); // Return all active device IDs
    //    }

    //    public async Task HandleWebSocketAsync(WebSocket webSocket, string deviceId)
    //    {
    //        Console.WriteLine($"Handling WebSocket for DeviceId: {deviceId}");
    //        AddClient(deviceId, webSocket);

    //        // Add client to list when connected
    //        lock (_connectedClients)
    //        {
    //            _connectedClients.Add(webSocket);
    //        }

    //        var buffer = new byte[1024 * 4]; // Buffer size (4 KB)

    //        try
    //        {
    //            while (webSocket.State == WebSocketState.Open)
    //            {
    //                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    //                if (result.MessageType == WebSocketMessageType.Text)
    //                {
    //                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
    //                    Console.WriteLine($"Received: {message}");

    //                    try
    //                    {
    //                        WebSocketRequest request;

    //                        // Fix double-encoded JSON
    //                        if (message.StartsWith("\"") && message.EndsWith("\""))
    //                        {
    //                            Console.WriteLine("Detected double-encoded JSON, fixing...");
    //                            message = JsonConvert.DeserializeObject<string>(message);
    //                        }

    //                        request = JsonConvert.DeserializeObject<WebSocketRequest>(message);

    //                        if (request != null && !string.IsNullOrEmpty(request.DeviceId))
    //                        {
    //                            Console.WriteLine($"DeviceId received: {request.DeviceId}");

    //                            var tokens = await _tokenService.GetTokenDetails(request.DeviceId);
    //                            string response = JsonConvert.SerializeObject(tokens);
    //                            var bytes = Encoding.UTF8.GetBytes(response);

    //                            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    //                        }
    //                        else
    //                        {
    //                            Console.WriteLine("Invalid request: Missing or empty DeviceId.");
    //                        }
    //                    }
    //                    catch (JsonException ex)
    //                    {
    //                        Console.WriteLine($"Error deserializing message: {ex.Message}");
    //                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid JSON", CancellationToken.None);
    //                    }
    //                }
    //                else if (result.MessageType == WebSocketMessageType.Close)
    //                {
    //                    break; // Exit loop on close
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"WebSocket error: {ex.Message}");
    //        }
    //        finally
    //        {
    //            // Remove client when disconnected
    //            lock (_connectedClients)
    //            {
    //                _connectedClients.Remove(webSocket);
    //            }

    //            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    //        }
    //    }

    //    public static async Task NotifyClients(object updatedToken)
    //    {
    //        string response = JsonConvert.SerializeObject(updatedToken);
    //        var bytes = Encoding.UTF8.GetBytes(response);

    //        foreach (var client in _connectedClients)
    //        {
    //            if (client.State == WebSocketState.Open)
    //            {
    //                await client.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    //            }
    //        }
    //    }


    //}

    public class WebSocketHandler
    {
        private readonly TvTokenRepositry _tokenService;
        private static readonly ConcurrentDictionary<string, WebSocket> _clients = new();

        public WebSocketHandler(TvTokenRepositry tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, string deviceId)
        {
            Console.WriteLine($"[WebSocketHandler] Device {deviceId} connected.");

            _clients[deviceId] = webSocket; // Add WebSocket connection

            try
            {
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[WebSocketHandler] Received: {message}");

                        if (message.StartsWith("\"") && message.EndsWith("\"")) // Fix double-encoded JSON
                            message = JsonConvert.DeserializeObject<string>(message);

                        var request = JsonConvert.DeserializeObject<WebSocketRequest>(message);

                        if (request != null && !string.IsNullOrEmpty(request.DeviceId))
                        {
                            var tokens = await _tokenService.GetTokenDetails(request.DeviceId);
                            await SendData(webSocket, tokens);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break; // Exit loop on close
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketHandler] Error: {ex}");
            }
            finally
            {
                _clients.TryRemove(deviceId, out _);
                Console.WriteLine($"[WebSocketHandler] Device {deviceId} disconnected.");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }

        public static string[] GetConnectedDeviceIds()
        {
            return _clients.Keys.ToArray();
        }

        public static async Task NotifyClients(List<TokenInfo> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                Console.WriteLine("No tokens to send, skipping update.");
                return;
            }

            var json = JsonConvert.SerializeObject(tokens);
            var buffer = Encoding.UTF8.GetBytes(json);

            foreach (var client in _clients.Values)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        private static async Task SendData(WebSocket client, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }


    public class WebSocketHandlerDoctorToken
    {
        private readonly DoctorTvTokenRepositry _tokenService;
        private static readonly ConcurrentDictionary<string, WebSocket> _clients = new();

        public WebSocketHandlerDoctorToken(DoctorTvTokenRepositry tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, string deviceId)
        {
            Console.WriteLine($"[WebSocketHandler] Device {deviceId} connected.");

            _clients[deviceId] = webSocket; // Add WebSocket connection

            try
            {
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"[WebSocketHandler] Received: {message}");

                        if (message.StartsWith("\"") && message.EndsWith("\"")) // Fix double-encoded JSON
                            message = JsonConvert.DeserializeObject<string>(message);

                        var request = JsonConvert.DeserializeObject<WebSocketRequest>(message);

                        if (request != null && !string.IsNullOrEmpty(request.DeviceId))
                        {
                            var tokens = await _tokenService.GetDoctorRoomInfoDataTableAsync(request.DeviceId);
                            await SendData(webSocket, tokens);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break; // Exit loop on close
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketHandler] Error: {ex}");
            }
            finally
            {
                _clients.TryRemove(deviceId, out _);
                Console.WriteLine($"[WebSocketHandler] Device {deviceId} disconnected.");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }

        public static string[] GetConnectedDeviceIds()
        {
            return _clients.Keys.ToArray();
        }

        public static async Task NotifyClients(List<DoctorTokenInFo> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                Console.WriteLine("No tokens to send, skipping update.");
                return;
            }

            var json = JsonConvert.SerializeObject(tokens);
            var buffer = Encoding.UTF8.GetBytes(json);

            foreach (var client in _clients.Values)
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }


        private static async Task SendData(WebSocket client, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var buffer = Encoding.UTF8.GetBytes(json);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }


    public class WebSocketRequest
    {
        public string DeviceId { get; set; }
    }
}
