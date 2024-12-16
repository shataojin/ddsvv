using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkServerProcessing
{
    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log($"Network msg received = {msg}, from connection id = {clientConnectionID}, from pipeline = {pipeline}");

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ClientToServerSignifiers.DeleteCoordinate)
        {
            float x = float.Parse(csv[1]);
            float y = float.Parse(csv[2]);
            gameLogic.HandleCoordinateDeletion(clientConnectionID, x, y);
        }
    }

    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public void BroadcastMessageToAllClients(string msg, TransportPipeline pipeline)
    {
        foreach (var clientID in networkServer.idToConnectionLookup.Keys)
        {
            SendMessageToClient(msg, clientID, pipeline);
        }
    }

    static public void BroadcastMessageToAllClientsExcept(int excludedClientID, string msg, TransportPipeline pipeline)
    {
        foreach (var clientID in networkServer.idToConnectionLookup.Keys)
        {
            if (clientID != excludedClientID)
            {
                SendMessageToClient(msg, clientID, pipeline);
            }
        }
    }
    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log($"Client connected: ID = {clientConnectionID}");

        // 通知 GameLogic 启动生成逻辑（如果是第一个客户端）
        gameLogic.HandleFirstClientConnected();

        // 发送存储的坐标到新客户端
        SendStoredCoordinatesToNewClient(clientConnectionID);
    }




    static public void SendStoredCoordinatesToNewClient(int clientConnectionID)
    {
        // 从 GameLogic 获取存储的坐标
        string storedCoordinates = gameLogic.GetStoredCoordinates();

        if (!string.IsNullOrEmpty(storedCoordinates))
        {
            // 格式化消息
            string message = $"{ServerToClientSignifiers.StoredCoordinates};{storedCoordinates}";
            SendMessageToClient(message, clientConnectionID, TransportPipeline.ReliableAndInOrder);

            Debug.Log($"Sent stored coordinates to new client {clientConnectionID}: {storedCoordinates}");
        }
    }



    static public void DisconnectionEvent(int clientConnectionID)
    {
        Debug.Log($"Client disconnected: ID = {clientConnectionID}");
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static GameLogic gameLogic;

    static public void SetNetworkServer(NetworkServer server)
    {
        networkServer = server;
    }

    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }

    static public void SetGameLogic(GameLogic logic)
    {
        gameLogic = logic;
    }
    #endregion
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int DeleteCoordinate = 1;
}

static public class ServerToClientSignifiers
{
    public const int NewCoordinate = 2;        // 单个新生成的坐标
    public const int DeleteCoordinate = 3;    // 删除坐标
    public const int StoredCoordinates = 4;   // 用于发送所有存储的坐标
}

#endregion
