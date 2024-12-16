using UnityEngine;

static public class NetworkClientProcessing
{
    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        Debug.Log($"Network msg received = {msg}, from pipeline = {pipeline}");

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.NewCoordinate)
        {
            float x = float.Parse(csv[1]);
            float y = float.Parse(csv[2]);
            gameLogic.HandleNewCoordinate(x, y);
            Debug.Log("New coordinate received and processed.");
        }
        else if (signifier == ServerToClientSignifiers.DeleteCoordinate)
        {
            float xPercent = float.Parse(csv[1]); // 解析比例坐标
            float yPercent = float.Parse(csv[2]); // 解析比例坐标

            Debug.Log($"Received delete request for coordinate ({xPercent}, {yPercent})");

            // 假设客户端 ID 为 0，因为这是客户端侧逻辑，客户端不会处理多个客户端的连接 ID。
            // 服务器应已广播消息。
            int clientConnectionID = 0;

            gameLogic.HandleCoordinateDeletion(clientConnectionID, xPercent, yPercent); // 传递参数
        }
    }

    static public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    static public void ConnectionEvent()
    {
        Debug.Log("Network Connection Event!");
    }

    static public void DisconnectionEvent()
    {
        Debug.Log("Network Disconnection Event!");
    }

    static public bool IsConnectedToServer()
    {
        return networkClient.IsConnected();
    }

    static public void ConnectToServer()
    {
        networkClient.Connect();
    }

    static public void DisconnectFromServer()
    {
        networkClient.Disconnect();
    }

    static NetworkClient networkClient;
    static GameLogic gameLogic;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }

    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }

    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int DeleteCoordinate = 2;
}

static public class ServerToClientSignifiers
{
    public const int NewCoordinate = 2;
    public const int DeleteCoordinate = 3;
}
#endregion
