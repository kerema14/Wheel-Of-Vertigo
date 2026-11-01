using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WheelOfVertigo.UI.Common;

namespace WheelOfVertigo.EditorTools
{
    /// <summary>
    /// Checks the project's UI against the team's UI rules, for the open scenes and every prefab under _Project/Prefabs:
    /// "ui_" names, no needless Raycast Target / Maskable, sliced sprites, no stretched images,
    /// no inspector OnClick events and no Animator on a root transform.
    /// </summary>
    public static class UIRulesValidator
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs";
        private const string NamePrefix = "ui_";
        private const float AspectTolerance = 0.02f;

        [MenuItem("Tools/Wheel Of Vertigo/Validate UI")]
        private static void Validate() => Run(fix: false);

        [MenuItem("Tools/Wheel Of Vertigo/Fix Raycast Target And Maskable")]
        private static void Fix() => Run(fix: true);

        private static void Run(bool fix)
        {
            int issues = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach (GameObject root in scene.GetRootGameObjects())
                    issues += CheckHierarchy(root, fix, isPrefabAsset: false);

                if (fix) EditorSceneManager.MarkSceneDirty(scene);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                issues += CheckHierarchy(contents, fix, isPrefabAsset: true);
                if (fix) PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);
            }

            if (issues == 0)
                Debug.Log("UI validation passed.");
            else
                Debug.LogWarning($"UI validation found {issues} issue(s){(fix ? " (raycast and maskable issues were fixed)" : string.Empty)}.");
        }

        // A UI prefab asset opened on its own has no parent Canvas, so every RectTransform in it counts as UI.
        // Maskable is only checked in scenes: a prefab can't know whether it will end up under a mask.
        private static int CheckHierarchy(GameObject root, bool fix, bool isPrefabAsset)
        {
            int issues = 0;

            if (root.TryGetComponent(out Animator _) && root.transform is RectTransform)
                issues += Report(root, "has an Animator on its root transform. Move it to a child.");

            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (!isPrefabAsset && rect.GetComponentInParent<Canvas>(true) == null) continue;

                if (!rect.name.StartsWith(NamePrefix))
                    issues += Report(rect.gameObject, $"name should start with \"{NamePrefix}\".");

                if (rect.TryGetComponent(out Graphic graphic))
                    issues += CheckGraphic(graphic, fix, checkMaskable: !isPrefabAsset);

                if (rect.TryGetComponent(out Button button) && button.onClick.GetPersistentEventCount() > 0)
                    issues += Report(rect.gameObject, "has OnClick events set in the Inspector. Add listeners from code instead.");
            }

            return issues;
        }

        private static int CheckGraphic(Graphic graphic, bool fix, bool checkMaskable)
        {
            int issues = 0;

            if (graphic.raycastTarget && !NeedsRaycast(graphic))
            {
                issues += Report(graphic.gameObject, "has Raycast Target on but nothing clickable uses it.");
                if (fix) SetRaycastTarget(graphic, false);
            }

            if (checkMaskable && graphic is MaskableGraphic maskable && maskable.maskable && !IsUnderMask(graphic.transform))
            {
                issues += Report(graphic.gameObject, "is Maskable but no mask above it.");
                if (fix) SetMaskable(maskable, false);
            }

            if (graphic is Image image && image.sprite != null)
                issues += CheckImage(image);

            return issues;
        }

        private static int CheckImage(Image image)
        {
            bool hasBorder = image.sprite.border != Vector4.zero;

            if (hasBorder && image.type == Image.Type.Simple)
                return Report(image.gameObject, "uses a 9-sliced sprite as Simple. Set Image Type to Sliced.");

            if (image.type != Image.Type.Simple || image.preserveAspect) return 0;

            Rect rect = image.rectTransform.rect;
            Vector2 spriteSize = image.sprite.rect.size;
            if (rect.height <= 0f || spriteSize.y <= 0f) return 0;

            float rectAspect = rect.width / rect.height;
            float spriteAspect = spriteSize.x / spriteSize.y;
            return Mathf.Abs(rectAspect / spriteAspect - 1f) > AspectTolerance
                ? Report(image.gameObject, "is stretched. Use Preserve Aspect, a sliced sprite or the sprite's aspect.")
                : 0;
        }

        // Only a Selectable's target graphic, a graphic marked as an intentional blocker, or a RaycastArea
        // (which exists only to be hit) needs raycasts.
        private static bool NeedsRaycast(Graphic graphic)
        {
            if (graphic is RaycastArea || graphic.TryGetComponent(out RaycastBlocker _)) return true;
            return graphic.TryGetComponent(out Selectable selectable) && selectable.targetGraphic == graphic;
        }

        private static bool IsUnderMask(Transform transform)
        {
            for (Transform current = transform.parent; current != null; current = current.parent)
                if (current.TryGetComponent(out RectMask2D _) || current.TryGetComponent(out Mask _))
                    return true;
            return false;
        }

        private static void SetRaycastTarget(Graphic graphic, bool value)
        {
            Undo.RecordObject(graphic, "Fix Raycast Target");
            graphic.raycastTarget = value;
            PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
        }

        private static void SetMaskable(MaskableGraphic graphic, bool value)
        {
            Undo.RecordObject(graphic, "Fix Maskable");
            graphic.maskable = value;
            PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
        }

        private static int Report(GameObject gameObject, string message)
        {
            Debug.LogWarning($"[UI] {GetPath(gameObject.transform)} {message}", gameObject);
            return 1;
        }

        private static string GetPath(Transform transform)
        {
            var names = new List<string>();
            for (Transform current = transform; current != null; current = current.parent)
                names.Add(current.name);
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
