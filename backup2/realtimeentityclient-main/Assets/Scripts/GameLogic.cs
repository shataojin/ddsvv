using UnityEngine;

public class GameLogic : MonoBehaviour
{
    float durationUntilNextBalloon;
    Sprite circleTexture;

    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this);
    }

    void Update()
    {
        //durationUntilNextBalloon -= Time.deltaTime;

        //if (durationUntilNextBalloon < 0)
        //{
        //    durationUntilNextBalloon = 1f;

        //    float screenPositionXPercent = Random.Range(0.0f, 1.0f);
        //    float screenPositionYPercent = Random.Range(0.0f, 1.0f);
        //    Vector2 screenPosition = new Vector2(screenPositionXPercent * Screen.width, screenPositionYPercent * Screen.height);
        //    SpawnNewBalloon(screenPosition);
        //}
    }

    public void SpawnNewBalloon(Vector2 screenPosition)
    {
        if (circleTexture == null)
            circleTexture = Resources.Load<Sprite>("Circle");

        GameObject balloon = new GameObject("Balloon");

        balloon.AddComponent<SpriteRenderer>();
        balloon.GetComponent<SpriteRenderer>().sprite = circleTexture;
        balloon.AddComponent<CircleClick>();
        balloon.AddComponent<CircleCollider2D>();

        // 设置标签
        balloon.tag = "Balloon";

        // 转换为世界坐标
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        pos.z = 0;
        balloon.transform.position = pos;

        // 打印生成气球的坐标
        Debug.Log($"Spawned balloon at world position: {pos}");
    }



    public void HandleNewCoordinate(float x, float y)
    {
        Vector2 screenPosition = new Vector2(x * Screen.width, y * Screen.height);
        SpawnNewBalloon(screenPosition);
    }

    public void HandleCoordinateDeletion(int clientId, float xPercent, float yPercent)
    {
        // 将比例坐标转换为屏幕坐标
        float screenX = xPercent * Screen.width;
        float screenY = yPercent * Screen.height;

        // 将屏幕坐标转换为世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenX, screenY, 0f));
        worldPosition.z = 0f; // 确保忽略 Z 轴

        // 查找与转换后的世界坐标最近的气球
        GameObject[] balloons = GameObject.FindGameObjectsWithTag("Balloon");
        GameObject targetBalloon = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject balloon in balloons)
        {
            float distance = Vector3.Distance(balloon.transform.position, worldPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetBalloon = balloon;
            }
        }

        // 如果找到匹配的气球，删除它
        if (targetBalloon != null && minDistance < 0.5f) // 距离阈值 0.5 可根据需要调整
        {
            Destroy(targetBalloon);
            Debug.Log($"Deleted balloon at world position: {targetBalloon.transform.position}");
        }
        else
        {
            Debug.LogWarning($"No balloon found near world position: {worldPosition}");
        }

        // 记录日志
        Debug.Log($"Handled deletion request from client {clientId} for coordinate ({xPercent}, {yPercent}) -> World Position ({worldPosition.x}, {worldPosition.y})");
    }

}
