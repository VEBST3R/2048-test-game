#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ScriptableObjectMenu : MonoBehaviour
{
    [MenuItem("2048/Create/Movement Settings")]
    public static void CreateCubeMovementSettings()
    {
        CubeMovementSettings asset = ScriptableObject.CreateInstance<CubeMovementSettings>();

        string path = "Assets/CubeMovementSettings.asset";

        if (System.IO.File.Exists(path))
        {
            int count = 1;
            while (System.IO.File.Exists(path))
            {
                path = $"Assets/CubeMovementSettings_{count}.asset";
                count++;
            }
        }

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log("Created Cube Movement Settings at " + path);
    }
}
#endif