// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.github.hkrn
{
        [CustomPropertyDrawer(typeof(VrmExpressionProperty))]
    public sealed class VrmExpressionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldRect = position;
            fieldRect.height = LineHeight;

            var expressionName = property.FindPropertyRelative(nameof(VrmExpressionProperty.expressionName));
            if (!property.FindPropertyRelative(nameof(VrmExpressionProperty.isPreset)).boolValue)
            {
                expressionName.stringValue = EditorGUI.TextField(fieldRect, Translator._("component.expression.name"),
                    expressionName.stringValue);
                fieldRect.y += fieldRect.height;
            }

            var baseType = property.FindPropertyRelative(nameof(VrmExpressionProperty.baseType));
            baseType.intValue = EditorGUI.Popup(
                fieldRect,
                Translator._("component.expression.type"),
                baseType.intValue,
                new[]
                {
                    Translator._("component.expression.type.blend-shape"),
                    Translator._("component.expression.type.animation-clip")
                });
            fieldRect.y += fieldRect.height;

            switch ((VrmExpressionProperty.BaseType)baseType.intValue)
            {
                case VrmExpressionProperty.BaseType.BlendShape:
                {
                    var gameObjectProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.gameObject));
                    var gameObject = (GameObject)gameObjectProp.objectReferenceValue;
                    if (_blendShapeNames.Count == 0 && gameObject)
                    {
                        VrmExpressionProperty.RetrieveAllBlendShapes(gameObject.transform, ref _blendShapeNames);
                    }

                    var blendShapeName = property.FindPropertyRelative(nameof(VrmExpressionProperty.blendShapeName));
                    var selectedIndex = blendShapeName.stringValue != null
                        ? _blendShapeNames.IndexOf(blendShapeName.stringValue)
                        : -1;
                    var newIndex = EditorGUI.Popup(fieldRect, selectedIndex, _blendShapeNames.ToArray());
                    if (newIndex >= 0 && newIndex < _blendShapeNames.Count)
                    {
                        var targetBlendShapeName = _blendShapeNames[newIndex];
                        blendShapeName.stringValue = targetBlendShapeName;
                    }

                    break;
                }
                case VrmExpressionProperty.BaseType.AnimationClip:
                {
                    var animationClipProp =
                        property.FindPropertyRelative(nameof(VrmExpressionProperty.blendShapeAnimationClip));
                    var animationClip = (AnimationClip)animationClipProp.objectReferenceValue;
                    animationClipProp.objectReferenceValue =
                        (AnimationClip)EditorGUI.ObjectField(fieldRect, animationClip, typeof(AnimationClip), false);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            fieldRect.y += fieldRect.height;

            EditorGUI.indentLevel++;
            var optionsFoldoutProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.optionsFoldout));
            var foldout = EditorGUI.Foldout(fieldRect, optionsFoldoutProp.boolValue,
                Translator._("component.expression.property.header"));
            fieldRect.y += fieldRect.height;
            if (foldout)
            {
                var selections = new[]
                {
                    Translator._("component.expression.property.type.none"),
                    Translator._("component.expression.property.type.block"),
                    Translator._("component.expression.property.type.blend")
                };
                var overrideBlinkProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.overrideBlink));
                overrideBlinkProp.intValue = EditorGUI.Popup(fieldRect,
                    Translator._("component.expression.property.override.blink"),
                    overrideBlinkProp.intValue,
                    selections);
                fieldRect.y += fieldRect.height;
                var overrideLookAtProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.overrideLookAt));
                overrideLookAtProp.intValue = EditorGUI.Popup(fieldRect,
                    Translator._("component.expression.property.override.look-at"),
                    overrideLookAtProp.intValue, selections);
                fieldRect.y += fieldRect.height;
                var overrideMouthProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.overrideMouth));
                overrideMouthProp.intValue = EditorGUI.Popup(fieldRect,
                    Translator._("component.expression.property.override.mouth"),
                    overrideMouthProp.intValue,
                    selections);
                fieldRect.y += fieldRect.height;
                var isBinaryProp = property.FindPropertyRelative(nameof(VrmExpressionProperty.isBinary));
                isBinaryProp.boolValue =
                    EditorGUI.Toggle(fieldRect, Translator._("component.expression.property.is-binary"),
                        isBinaryProp.boolValue);
            }

            EditorGUI.indentLevel--;

            optionsFoldoutProp.boolValue = foldout;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = LineHeight * 3;
            if (!property.FindPropertyRelative(nameof(VrmExpressionProperty.isPreset)).boolValue)
            {
                height += LineHeight;
            }

            if (property.FindPropertyRelative(nameof(VrmExpressionProperty.optionsFoldout)).boolValue)
            {
                height += LineHeight * 4;
            }

            return height;
        }

        private float LineHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        private List<string> _blendShapeNames = new();
    }

}
#endif // NVE_HAS_NDMF
