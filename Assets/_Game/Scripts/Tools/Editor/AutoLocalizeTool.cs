using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.Localization.Components;
using UnityEditor.Events;
using UnityEngine.Events;

public class AutoLocalizeTool : EditorWindow
{
    // ==========================================
    // 1. NÚT QUÉT SCENE HIỆN TẠI
    // ==========================================
    [MenuItem("Tools/1. Gắn Localize cho Text (Trong Scene)")]
    public static void AutoAddLocalizeScene()
    {
        // Code mới chuẩn Unity hiện tại
        var objects = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        int countAdded = 0;

        foreach (var txt in objects)
        {
            if (txt.GetComponent<LocalizeStringEvent>() != null) continue;

            LocalizeStringEvent localizeEvent = txt.gameObject.AddComponent<LocalizeStringEvent>();
            UnityAction<string> action = System.Delegate.CreateDelegate(typeof(UnityAction<string>), txt, "set_text") as UnityAction<string>;
            UnityEventTools.AddPersistentListener(localizeEvent.OnUpdateString, action);

            countAdded++;
            EditorUtility.SetDirty(txt.gameObject);
        }

        Debug.Log($"<color=green>[SCENE] Hoàn tất! Đã thêm thành công cho {countAdded} Text.</color>");
    }

    // ==========================================
    // 2. NÚT QUÉT PREFAB (CHỈ TRONG RESOURCES/UI)
    // ==========================================
    [MenuItem("Tools/2. Gắn Localize cho Text (Chỉ trong Resources/UI)")]
    public static void AutoAddLocalizePrefabsInUI()
    {
        // Đường dẫn thư mục cần quét (Bắt buộc phải bắt đầu bằng chữ "Assets/...")
        string folderPath = "Assets/Resources/UI";
        string[] searchFolders = new string[] { folderPath };

        // Kiểm tra an toàn: Xem thư mục có tồn tại không trước khi quét
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"<color=red>Lỗi: Không tìm thấy thư mục {folderPath}! Bạn hãy kiểm tra lại xem viết hoa/thường đã chuẩn chưa nhé.</color>");
            return;
        }

        // Chỉ tìm Prefab nằm trong thư mục đã chỉ định
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", searchFolders);

        int prefabsModified = 0;
        int totalComponentsAdded = 0;

        EditorUtility.DisplayProgressBar("Đang quét Prefab UI", "Vui lòng chờ...", 0f);

        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
            EditorUtility.DisplayProgressBar("Đang quét Prefab UI", path, (float)i / prefabGUIDs.Length);

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
            bool isModified = false;

            TextMeshProUGUI[] allTextsInPrefab = prefabContents.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var txt in allTextsInPrefab)
            {
                if (txt.GetComponent<LocalizeStringEvent>() != null) continue;

                LocalizeStringEvent localizeEvent = txt.gameObject.AddComponent<LocalizeStringEvent>();
                UnityAction<string> action = System.Delegate.CreateDelegate(typeof(UnityAction<string>), txt, "set_text") as UnityAction<string>;
                UnityEventTools.AddPersistentListener(localizeEvent.OnUpdateString, action);

                isModified = true;
                totalComponentsAdded++;
            }

            if (isModified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                prefabsModified++;
            }

            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        EditorUtility.ClearProgressBar();

        Debug.Log($"<color=cyan>[PREFAB UI] Hoàn tất cực nhanh! Đã can thiệp {prefabsModified} Prefab, gắn {totalComponentsAdded} Component.</color>");
    }
}