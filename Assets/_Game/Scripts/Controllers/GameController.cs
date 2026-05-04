using UnityEngine;

public class GameController : Singleton<GameController>
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0; // Tắt đồng bộ dọc của Unity để mở khóa giới hạn FPS
        Application.targetFrameRate = 60; // Ép game chạy đúng 60 FPS (hoặc 30 FPS nếu máy quá yếu)
    }
}
