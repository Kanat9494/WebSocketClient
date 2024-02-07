using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketClient;


var senderId = ulong.Parse(Console.ReadLine());
var receiverId = ulong.Parse(Console.ReadLine());
ClientWebSocket clientWebSocket = new ClientWebSocket();

var buffer = new byte[1024 * 4];

string uri = $"ws://192.168.2.33:45455/api/Chats/ConnectToWS?userId={senderId}";
await clientWebSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
Message message;

WebSocketReceiveResult receiveResult;
string jsonString;

Task receiveMessageTask = new Task(async () =>
{
    byte[] data = new byte[1024 * 4];

    while (true)
    {
        receiveResult = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(data), CancellationToken.None);
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.Append(Encoding.UTF8.GetString(data, 0, receiveResult.Count));
        message = JsonSerializer.Deserialize<Message>(messageBuilder.ToString());
        Console.WriteLine($"Сообщение от пользователя: {message.SenderId} - {message.Content}");
    }
});

receiveMessageTask.Start();

SendMessage();

void SendMessage()
{
    Console.WriteLine("Введите сообщение: ");
    while (true)
    {
        string content = Console.ReadLine();
        message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content
        };
        var jsonMessage = JsonSerializer.Serialize(message);
        buffer = Encoding.UTF8.GetBytes(jsonMessage);
        Task.Run(async () =>
        {
            await clientWebSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        });
    }
}


