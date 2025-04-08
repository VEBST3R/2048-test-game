#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CubeFacadeExtension
{
    [MenuItem("GameObject/3D Object/2048/Cube With Components", false, 10)]
    private static void CreateCubeWithComponents()
    {
        GameObject cube = new GameObject("2048 Cube");

        cube.AddComponent<CubeFacade>();

        Selection.activeObject = cube;

        if (SceneView.currentDrawingSceneView != null)
        {
            cube.transform.position = SceneView.currentDrawingSceneView.pivot;
        }

        Undo.RegisterCreatedObjectUndo(cube, "Create 2048 Cube");
    }

    [MenuItem("CONTEXT/Transform/Add 2048 Cube Components")]
    private static void AddCubeComponents(MenuCommand command)
    {
        GameObject targetObject = ((Transform)command.context).gameObject;

        if (targetObject.GetComponent<CubeFacade>() == null)
        {
            Undo.RecordObject(targetObject, "Add 2048 Cube Components");
            targetObject.AddComponent<CubeFacade>();
        }
        else
        {
            Debug.Log("This object already has CubeFacade component");
        }
    }
}
#endif
