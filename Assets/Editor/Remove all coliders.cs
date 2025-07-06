// Assets/Editor/RemoveAllColliders.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RemoveAllColliders
{
    private const string MenuPath = "Tools/Scene Cleanup/Remove All Colliders";

    [MenuItem(MenuPath)]
    private static void RemoveCollidersInActiveScene()
    {
        // Запрашиваем подтверждение, чтобы случайно не стереть коллайдеры
        if (!EditorUtility.DisplayDialog(
                "Remove All Colliders",
                "Удалить все 3D‑ и 2D‑коллайдеры в активной сцене?",
                "Удалить", "Отмена"))
        {
            return;
        }

        // Делаем операцию отменяемой одним шагом
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        // Ищем 3D коллайдеры
        Collider[] colliders3D = Object.FindObjectsOfType<Collider>(includeInactive: true);
        foreach (Collider col in colliders3D)
        {
            Undo.DestroyObjectImmediate(col);
        }

        // Ищем 2D коллайдеры
        Collider2D[] colliders2D = Object.FindObjectsOfType<Collider2D>(includeInactive: true);
        foreach (Collider2D col2D in colliders2D)
        {
            Undo.DestroyObjectImmediate(col2D);
        }

        // Завершаем группу Undo
        Undo.CollapseUndoOperations(undoGroup);

        // Помечаем сцену “грязной”, чтобы Unity предложила сохранить её
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log($"Удалено 3D коллайдеров: {colliders3D.Length}, 2D коллайдеров: {colliders2D.Length}");
    }

    // Активируем пункт меню только если в сцене действительно есть хотя бы один коллайдер
    [MenuItem(MenuPath, true)]
    private static bool ValidateRemoveColliders()
    {
        return Object.FindObjectOfType<Collider>() || Object.FindObjectOfType<Collider2D>();
    }
}
