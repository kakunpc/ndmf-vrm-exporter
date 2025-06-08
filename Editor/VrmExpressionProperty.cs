// NVE_HAS_AVATAR_OPTIMIZER
// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace com.github.hkrn
{
    [Serializable]
    public sealed class VrmExpressionProperty
    {
        internal static VrmExpressionProperty Happy => new()
        {
            expressionName = "Happy",
            isPreset = true,
        };

        internal static VrmExpressionProperty Angry => new()
        {
            expressionName = "Angry",
            isPreset = true,
        };

        internal static VrmExpressionProperty Sad => new()
        {
            expressionName = "Sad",
            isPreset = true,
        };

        internal static VrmExpressionProperty Relaxed => new()
        {
            expressionName = "Relaxed",
            isPreset = true,
        };

        internal static VrmExpressionProperty Surprised => new()
        {
            expressionName = "Surprised",
            isPreset = true,
        };

        [NotKeyable] [SerializeField] internal string? expressionName;
        [NotKeyable] [SerializeField] internal BaseType baseType;
        [NotKeyable] [SerializeField] internal GameObject? gameObject;
        [NotKeyable] [SerializeField] internal string? blendShapeName;
        [NotKeyable] [SerializeField] internal AnimationClip? blendShapeAnimationClip;
        [NotKeyable] [SerializeField] internal bool optionsFoldout;
        [NotKeyable] [SerializeField] internal vrm.core.ExpressionOverrideType overrideBlink;
        [NotKeyable] [SerializeField] internal vrm.core.ExpressionOverrideType overrideLookAt;
        [NotKeyable] [SerializeField] internal vrm.core.ExpressionOverrideType overrideMouth;
        [NotKeyable] [SerializeField] internal bool isBinary;
        [NotKeyable] [SerializeField] internal bool isPreset;

        internal const string BlendShapeNamePrefix = "blendShape.";

        internal enum BaseType
        {
            BlendShape,
            AnimationClip,
        };

        internal List<string> BlendShapeNames => baseType switch
        {
            BaseType.AnimationClip => ExtractAllBlendShapeNamesFromAnimationClip(),
            BaseType.BlendShape => new List<string?> { blendShapeName },
            _ => throw new ArgumentOutOfRangeException(),
        };

        internal string CanonicalExpressionName
        {
            get
            {
                if (!string.IsNullOrEmpty(expressionName))
                {
                    return expressionName!;
                }

                return baseType switch
                {
                    BaseType.AnimationClip => blendShapeAnimationClip!.name,
                    BaseType.BlendShape => blendShapeName!,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        }

        internal bool IsValid => baseType switch
        {
            BaseType.AnimationClip => blendShapeAnimationClip != null,
            BaseType.BlendShape => !string.IsNullOrEmpty(blendShapeName),
            _ => throw new ArgumentOutOfRangeException(),
        };

        internal void SetFromMmdExpression(string targetName)
        {
            var blendShapeNames = new List<string>();
            if (!gameObject || !gameObject!.transform)
            {
                return;
            }

            RetrieveAllBlendShapes(gameObject.transform, ref blendShapeNames);
            var index = blendShapeNames.IndexOf(targetName);
            if (index == -1)
                return;
            baseType = BaseType.BlendShape;
            blendShapeName = blendShapeNames[index];
            overrideBlink = vrm.core.ExpressionOverrideType.Block;
            overrideLookAt = vrm.core.ExpressionOverrideType.Block;
            overrideMouth = vrm.core.ExpressionOverrideType.Block;
        }

        internal static void RetrieveAllBlendShapes(Transform transform, ref List<string> blendShapeNames)
        {
            RetrieveAllBlendShapes(transform, (name, _, blendShapeNames) => { blendShapeNames.Add(name); },
                ref blendShapeNames);
        }

        internal static void RetrieveAllBlendShapes<T>(Transform transform,
            Action<string, SkinnedMeshRenderer, T> callback, ref T blendShapeNames)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (!child || child is null || !child.TryGetComponent<SkinnedMeshRenderer>(out var smr))
                    continue;
                var mesh = smr.sharedMesh;
                if (mesh)
                {
                    var numBlendShapes = mesh.blendShapeCount;
                    for (var j = 0; j < numBlendShapes; j++)
                    {
                        callback(mesh.GetBlendShapeName(j), smr, blendShapeNames);
                    }
                }

                RetrieveAllBlendShapes(child, callback, ref blendShapeNames);
            }
        }

        private List<string?> ExtractAllBlendShapeNamesFromAnimationClip()
        {
            if (!blendShapeAnimationClip)
                return new List<string?>();
            return (from binding in AnimationUtility.GetCurveBindings(blendShapeAnimationClip)
                where binding.propertyName.StartsWith(BlendShapeNamePrefix, StringComparison.Ordinal)
                let name = binding.propertyName[BlendShapeNamePrefix.Length..]
                let curve = AnimationUtility.GetEditorCurve(blendShapeAnimationClip, binding)
                from keyframe in curve.keys
                where !(keyframe.time > 0.0f) && !Mathf.Approximately(keyframe.value, 0.0f)
                select name).ToList();
        }
    }
}
#endif // NVE_HAS_NDMF
