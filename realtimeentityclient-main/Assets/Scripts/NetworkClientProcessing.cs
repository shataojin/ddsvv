using UnityEngine;

static public class NetworkClientProcessing
{
    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        Debug.Log($"Network msg received = {msg}, from pipeline = {pipeline}");

        // 检查消息是否为空
        if (string.IsNullOrEmpty(msg))
        {
            Debug.LogError("Received empty or null message from server.");
            return;
        }

        // 判断消息是否包含分号
        if (msg.Contains(";"))
        {
            // 处理 StoredCoordinates 消息
            string[] csv = msg.Split(';'); // 用分号分隔
            if (!int.TryParse(csv[0], out int signifier) || signifier != ServerToClientSignifiers.StoredCoordinates)
            {
                Debug.LogError($"Invalid StoredCoordinates message format or signifier: {msg}");
                return;
            }

            // 遍历处理存储的坐标
            for (int i = 1; i < csv.Length; i++)
            {
                string[] coordinateData = csv[i].Split(','); // 用逗号分隔 x 和 y
                if (coordinateData.Length == 2 &&
                    float.TryParse(coordinateData[0], out float x) &&
                    float.TryParse(coordinateData[1], out float y))
                {
                    gameLogic.HandleNewCoordinate(x, y); // 调用生成圆球的方法
                }
                else
                {
                    Debug.LogWarning($"Invalid coordinate format in segment: {csv[i]}");
                }
            }

            Debug.Log("Loaded stored coordinates from server.");
        }
        else
        {
            // 处理其他消息
            string[] csv = msg.Split(','); // 用逗号分隔
            if (csv.Length < 1 || !int.TryParse(csv[0], out int signifier))
            {
                Debug.LogError($"Invalid message format or signifier: {msg}");
                return;
            }

            // 根据 signifier 处理不同的消息
            switch (signifier)
            {
                case ServerToClientSignifiers.NewCoordinate:
                    ProcessNewCoordinateMessage(csv);
                    break;

                case ServerToClientSignifiers.DeleteCoordinate:
                    ProcessDeleteCoordinateMessage(csv);
                    break;

                default:
                    Debug.LogWarning($"Unknown signifier: {signifier}");
                    break;
            }
        }
    }

    static private void ProcessNewCoordinateMessage(string[] csv)
    {
        if (csv.Length != 3 || !float.TryParse(csv[1], out float x) || !float.TryParse(csv[2], out float y))
        {
            Debug.LogError($"Invalid NewCoordinate format: {string.Join(",", csv)}");
            return;
        }

        // 处理新坐标
        gameLogic.HandleNewCoordinate(x, y);
        Debug.Log($"New coordinate received and processed: ({x}, {y})");
    }

    static private void ProcessDeleteCoordinateMessage(string[] csv)
    {
        if (csv.Length != 3 || !float.TryParse(csv[1], out float xPercent) || !float.TryParse(csv[2], out float yPercent))
        {
            Debug.LogError($"Invalid DeleteCoordinate format: {string.Join(",", csv)}");
            return;
        }

        // 调用删除逻辑
        gameLogic.HandleCoordinateDeletion(0, xPercent, yPercent);
        Debug.Log($"Processed delete request for coordinate: ({xPercent}, {yPercent})");
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
    public const int DeleteCoordinate = 1;
}

static public class ServerToClientSignifiers
{
    public const int NewCoordinate = 2;        // 用于单个新生成的坐标
    public const int DeleteCoordinate = 3;    // 用于删除坐标
    public const int StoredCoordinates = 4;   // 用于发送所有存储的坐标
}

#endregion
