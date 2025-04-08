#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Додає контекстне меню та розширення для CubeFacade
/// </summary>
public static class CubeFacadeExtension
{
    [MenuItem("GameObject/3D Object/2048/Cube With Components", false, 10)]
    private static void CreateCubeWithComponents()
    {
        // Створюємо новий GameObject
        GameObject cube = new GameObject("2048 Cube");

        // Додаємо необхідні компоненти
        cube.AddComponent<CubeFacade>();

        // Встановлюємо GameObject як активний вибраний об'єкт
        Selection.activeObject = cube;

        // Розміщуємо об'єкт на позиції 0,0,0 якщо не використовується інструмент створення у сцені
        if (SceneView.currentDrawingSceneView != null)
        {
            cube.transform.position = SceneView.currentDrawingSceneView.pivot;
        }

        Undo.RegisterCreatedObjectUndo(cube, "Create 2048 Cube");
    }

    // Додаємо меню-елемент для додавання всіх компонентів до існуючого GameObject
    [MenuItem("CONTEXT/Transform/Add 2048 Cube Components")]
    private static void AddCubeComponents(MenuCommand command)
    {
        GameObject targetObject = ((Transform)command.context).gameObject;

        // Перевіряємо, чи об'єкт вже має CubeFacade
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
