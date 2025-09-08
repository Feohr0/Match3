using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class AllFontsChanger : EditorWindow
{
    public Font newUIFont;
    public TMP_FontAsset newTMPFont;

    [MenuItem("Tools/Font Changer")]
    static void ShowWindow()
    {
        GetWindow<AllFontsChanger>("All Fonts Changer");
    }

    void OnGUI()
    {
        newUIFont = EditorGUILayout.ObjectField("UI Font", newUIFont, typeof(Font), false) as Font;
        newTMPFont = EditorGUILayout.ObjectField("TMP Font", newTMPFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;

        if (GUILayout.Button("Change Fonts in Current Scene"))
        {
            ChangeCurrentSceneFonts();
        }

        if (GUILayout.Button("Change Fonts in ALL SCENES"))
        {
            ChangeAllScenesFonts();
        }

        if (GUILayout.Button("Change Fonts in ALL PREFABS"))
        {
            ChangeAllPrefabsFonts();
        }
    }

    void ChangeCurrentSceneFonts()
    {
        // Mevcut kod - sadece aktif sahne
        Text[] uiTexts = FindObjectsOfType<Text>();
        TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>();

        ChangeFonts(uiTexts, tmpTexts);
        Debug.Log($"? Aktif sahne: {uiTexts.Length} UI Text + {tmpTexts.Length} TextMeshPro deðiþtirildi");
    }

    void ChangeAllScenesFonts()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

            Text[] uiTexts = FindObjectsOfType<Text>();
            TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>();

            ChangeFonts(uiTexts, tmpTexts);
            Debug.Log($"? {Path.GetFileName(scenePath)}: {uiTexts.Length} UI Text + {tmpTexts.Length} TextMeshPro deðiþtirildi");

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
    }

    void ChangeAllPrefabsFonts()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Text[] uiTexts = prefab.GetComponentsInChildren<Text>(true);
            TextMeshProUGUI[] tmpTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (uiTexts.Length > 0 || tmpTexts.Length > 0)
            {
                ChangeFonts(uiTexts, tmpTexts);
                EditorUtility.SetDirty(prefab);
                Debug.Log($"? {Path.GetFileName(prefabPath)}: {uiTexts.Length} UI Text + {tmpTexts.Length} TextMeshPro deðiþtirildi");
            }
        }

        AssetDatabase.SaveAssets();
    }

    void ChangeFonts(Text[] uiTexts, TextMeshProUGUI[] tmpTexts)
    {
        foreach (Text text in uiTexts)
        {
            if (newUIFont != null)
                text.font = newUIFont;
        }

        foreach (TextMeshProUGUI tmp in tmpTexts)
        {
            if (newTMPFont != null)
                tmp.font = newTMPFont;
        }
    }
}