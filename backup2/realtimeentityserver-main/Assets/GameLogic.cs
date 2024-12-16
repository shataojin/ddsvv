using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private List<Vector2> storedCoordinates = new List<Vector2>(); // 存储生成的坐标
    private bool isGeneratingCoordinates = false; // 标志是否开始生成坐标

    void Start()
    {
        NetworkServerProcessing.SetGameLogic(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            NetworkServerProcessing.SendMessageToClient("2,Hello client's world, sincerely your network server", 0, TransportPipeline.ReliableAndInOrder);
        }
    }

    IEnumerator GenerateRandomCoordinates()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            // 如果未开始生成或没有客户端连接，则跳过
            if (!isGeneratingCoordinates)
                continue;

            // 生成比例坐标 (0.0 到 1.0)
            float x = Random.Range(0.0f, 1.0f);
            float y = Random.Range(0.0f, 1.0f);
            Vector2 newCoordinate = new Vector2(x, y);

            // 添加到存储列表
            storedCoordinates.Add(newCoordinate);

            // 广播比例坐标到客户端
            string coordinateMessage = $"{ServerToClientSignifiers.NewCoordinate},{x},{y}";
            NetworkServerProcessing.BroadcastMessageToAllClients(coordinateMessage, TransportPipeline.ReliableAndInOrder);

            // 显示当前存储的坐标数量
            Debug.Log($"Generated new coordinate ({x}, {y}). Total stored coordinates: {storedCoordinates.Count}");
        }
    }

    public void HandleFirstClientConnected()
    {
        if (!isGeneratingCoordinates)
        {
            isGeneratingCoordinates = true;
            StartCoroutine(GenerateRandomCoordinates());
            Debug.Log("First client connected. Starting coordinate generation.");
        }
    }

    public void HandleCoordinateDeletion(int clientId, float x, float y)
    {
        Vector2 targetCoordinate = new Vector2(x, y);
        float tolerance = 0.1f; // 容差值

        // 查找与目标坐标接近的存储坐标
        Vector2? foundCoordinate = null;
        foreach (var storedCoordinate in storedCoordinates)
        {
            if (Vector2.Distance(storedCoordinate, targetCoordinate) <= tolerance)
            {
                foundCoordinate = storedCoordinate;
                break;
            }
        }

        if (foundCoordinate.HasValue)
        {
            // 从列表中移除找到的坐标
            storedCoordinates.Remove(foundCoordinate.Value);
            Debug.Log($"Removed coordinate: {foundCoordinate.Value} from stored list. Remaining coordinates: {storedCoordinates.Count}");
        }
        else
        {
            Debug.LogWarning($"Coordinate ({x}, {y}) not found in stored list.");
        }

        // 广播删除消息到其他客户端
        string deletionMessage = $"{ServerToClientSignifiers.DeleteCoordinate},{x},{y}";
        NetworkServerProcessing.BroadcastMessageToAllClientsExcept(clientId, deletionMessage, TransportPipeline.ReliableAndInOrder);

        Debug.Log($"Broadcasting deletion request from client {clientId} for coordinate ({x}, {y})");
    }

}
