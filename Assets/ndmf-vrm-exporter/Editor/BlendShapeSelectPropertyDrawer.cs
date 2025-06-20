
#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace com.github.hkrn
{
    [CustomPropertyDrawer(typeof(BlendShapeSelectProperty))]
    public sealed class BlendShapeSelectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldRect = position;
            fieldRect.height = LineHeight;

            var isUse = property.FindPropertyRelative(nameof(BlendShapeSelectProperty.IsAutomatic));
            isUse.boolValue = EditorGUI.ToggleLeft(fieldRect, "自動設定", isUse.boolValue);
            fieldRect .y += fieldRect.height;
            if (!isUse.boolValue)
            {
                var blendShapeName = property.FindPropertyRelative(nameof(BlendShapeSelectProperty.BlendShapeName)).stringValue;
                // 子どものSkinnedMeshRendererを取得してPopupに表示する
                var meshRenderers = new List<SkinnedMeshRenderer>();
                if (property.serializedObject.targetObject is Component component)
                {
                    meshRenderers.AddRange(component.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());
                }

                var meshRendererNames =new List<(string,int)> { ("None",-1) };
                meshRendererNames.AddRange(meshRenderers.Select(mr => (mr.name, mr.GetInstanceID())).ToList());

                var selectObject = property.FindPropertyRelative(nameof(BlendShapeSelectProperty.MeshRenderer)).objectReferenceValue as SkinnedMeshRenderer;
                var selectedInstanceID = (selectObject?.GetInstanceID() ?? -1);
                var selectedIndex = meshRendererNames.FindIndex(mr => mr.Item2 == selectedInstanceID);
                if (selectedIndex == -1) selectedIndex = 0;
                var meshRenderer = EditorGUI.Popup(fieldRect,
                    "Skinned Mesh Renderer",
                    selectedIndex,
                    meshRendererNames.Select(x=>x.Item1).ToArray());
                if (meshRenderer == 0)
                {
                    property.FindPropertyRelative(nameof(BlendShapeSelectProperty.MeshRenderer)).objectReferenceValue = null;
                    blendShapeName = string.Empty;
                }
                else
                {
                    var meshRendererName = meshRendererNames[meshRenderer];
                    var mesh = meshRenderers.First(x => x.GetInstanceID() == meshRendererName.Item2);
                    property.FindPropertyRelative(nameof(BlendShapeSelectProperty.MeshRenderer)).objectReferenceValue =
                        mesh;

                    if (selectedInstanceID != mesh.GetInstanceID())
                    {
                        blendShapeName = string.Empty;
                        selectObject = mesh;
                    }
                }
                fieldRect.y += fieldRect.height;

                // 選択されたObjectのBlendShapeを取得してPopupに表示する
                var blendShapeNames = new List<string>();
                if (selectObject is { } skinnedMeshRenderer)
                {
                    for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        blendShapeNames.Add(skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i));
                    }
                }

                var blendShapeIndex = blendShapeNames.IndexOf(blendShapeName);
                if (blendShapeIndex == -1) blendShapeIndex = 0;
                blendShapeIndex = EditorGUI.Popup(fieldRect,
                    "Blend Shape",
                    blendShapeIndex,
                    blendShapeNames.ToArray());
                if (blendShapeIndex == 0 || blendShapeNames.Count <= blendShapeIndex)
                {
                    property.FindPropertyRelative(nameof(BlendShapeSelectProperty.BlendShapeName)).stringValue = string.Empty;
                }
                else
                {
                    property.FindPropertyRelative(nameof(BlendShapeSelectProperty.BlendShapeName)).stringValue =
                        blendShapeNames[blendShapeIndex];
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = LineHeight;
            if (!property.FindPropertyRelative(nameof(BlendShapeSelectProperty.IsAutomatic)).boolValue)
            {
                height += LineHeight * 2;
            }

            return height;
        }

        private float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    }
}
#endif
