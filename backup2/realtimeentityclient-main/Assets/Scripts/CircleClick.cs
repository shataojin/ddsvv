using UnityEngine;

public class CircleClick : MonoBehaviour
{
    void OnMouseDown()
    {
        // 获取当前气球的世界坐标
        Vector3 worldPosition = transform.position;

        // 将世界坐标转换为屏幕坐标
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // 计算比例坐标 (0.0 到 1.0)
        float xPercent = Mathf.Clamp01(screenPosition.x / Screen.width);
        float yPercent = Mathf.Clamp01(screenPosition.y / Screen.height);

        // 格式化删除请求消息为 "{signifier},{xPercent},{yPercent}"
        string message = $"{ClientToServerSignifiers.DeleteCoordinate},{xPercent},{yPercent}";

        // 发送消息到服务器
        NetworkClientProcessing.SendMessageToServer(message, TransportPipeline.ReliableAndInOrder);

        // 删除当前气球
        Destroy(gameObject);

        Debug.Log($"Sent delete message: {message}");
    }
}
