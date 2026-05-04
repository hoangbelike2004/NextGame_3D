using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPFontReplacer : EditorWindow
{
    // Biến để kéo thả Font vào
    private TMP_FontAsset newFont;

    // Đường dẫn mặc định như bạn yêu cầu
    private string targetFolder = "Assets/Resources/UI";

    // Tạo menu trên thanh công cụ của Unity
    [MenuItem("Tools/Đổi Font TMP Hàng Loạt")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontReplacer>("Đổi Font TMP");
    }

    private void OnGUI()
    {
        GUILayout.Label("CÔNG CỤ ĐỔI FONT TEXTMESHPRO", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Ô để kéo thả Font Asset
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Kéo Font Mới Vào Đây:", newFont, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();

        // Ô điền đường dẫn (cho phép bạn linh hoạt đổi thư mục nếu muốn)
        targetFolder = EditorGUILayout.TextField("Thư mục chứa Prefab:", targetFolder);

        EditorGUILayout.Space();

        // Nút bấm thực thi
        if (GUILayout.Button("BẮT ĐẦU ĐỔI FONT", GUILayout.Height(40)))
        {
            ReplaceFontsInPrefabs();
        }
    }

    private void ReplaceFontsInPrefabs()
    {
        // 1. Kiểm tra đầu vào
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Bạn chưa kéo Font Asset mới vào tool!", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy thư mục: {targetFolder}. Vui lòng kiểm tra lại đường dẫn.", "OK");
            return;
        }

        // 2. Tìm tất cả các file Prefab trong thư mục chỉ định
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { targetFolder });

        if (prefabGUIDs.Length == 0)
        {
            EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy Prefab nào trong thư mục này.", "OK");
            return;
        }

        int changedPrefabsCount = 0;
        int changedTextsCount = 0;

        // 3. Vòng lặp duyệt qua từng Prefab
        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);

            // Hiện thanh Progress Bar cho chuyên nghiệp
            EditorUtility.DisplayProgressBar("Đang xử lý...", $"Đang đổi font cho: {path}", (float)i / prefabGUIDs.Length);

            // Load nội dung của Prefab lên bộ nhớ đệm (không cần đưa ra Scene)
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);

            // Tìm TẤT CẢ các component TMP_Text (bao gồm cả con cháu bị ẩn)
            // TMP_Text là class cha của cả TextMeshProUGUI và TextMeshPro
            TMP_Text[] textComponents = prefabContents.GetComponentsInChildren<TMP_Text>(true);

            bool isModified = false;

            foreach (TMP_Text txt in textComponents)
            {
                // Chỉ đổi nếu font hiện tại khác với font mới
                if (txt.font != newFont)
                {
                    txt.font = newFont;
                    isModified = true;
                    changedTextsCount++;
                }
            }

            // 4. Nếu có sự thay đổi thì lưu Prefab lại
            if (isModified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                changedPrefabsCount++;
            }

            // Giải phóng bộ nhớ đệm
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        // Tắt thanh Progress Bar
        EditorUtility.ClearProgressBar();

        // Refresh lại AssetDatabase để Unity nhận thay đổi
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Báo cáo kết quả
        EditorUtility.DisplayDialog("Thành công!",
            $"Đã đổi thành công {changedTextsCount} component Text trên {changedPrefabsCount} Prefabs.",
            "Tuyệt vời");
    }
}