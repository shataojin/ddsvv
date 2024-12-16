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
        gameLogic.HandleFirstClientConnected();
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
    public const int DeleteCoordinate = 2;
}

static public class ServerToClientSignifiers
{
    public const int NewCoordinate = 2;
    public const int DeleteCoordinate = 3;
}
#endregion
