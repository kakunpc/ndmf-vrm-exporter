// NVE_HAS_AVATAR_OPTIMIZER
// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if NVE_HAS_AVATAR_OPTIMIZER
using Anatawa12.AvatarOptimizer.API;
#endif

#if NVE_HAS_NDMF
#endif
namespace com.github.hkrn
{
#if NVE_HAS_AVATAR_OPTIMIZER
    namespace aao
    {
        [ComponentInformation(typeof(NdmfVrmExporterComponent))]
        internal sealed class NdmfVrmExporterComponentInformation : ComponentInformation<NdmfVrmExporterComponent>
        {
            protected override void CollectMutations(NdmfVrmExporterComponent component,
                ComponentMutationsCollector collector)
            {
                var blendShapeNameToSmr = new Dictionary<string, SkinnedMeshRenderer>();
                VrmExpressionProperty.RetrieveAllBlendShapes(component.transform, (name, smr, props) =>
                {
                    if (props.TryGetValue(name, out var foundSmr))
                    {
                        if (smr != foundSmr)
                        {
                            Debug.LogWarning($"Different SMR found: left={smr}, right={foundSmr}");
                        }
                    }
                    else
                    {
                        props.Add(name, smr);
                    }
                }, ref blendShapeNameToSmr);
                var allExpressionUsedBlendShapeNames = new List<string?>();
                allExpressionUsedBlendShapeNames.AddRange(component.expressionPresetHappyBlendShape.BlendShapeNames);
                allExpressionUsedBlendShapeNames.AddRange(component.expressionPresetAngryBlendShape.BlendShapeNames);
                allExpressionUsedBlendShapeNames.AddRange(component.expressionPresetSadBlendShape.BlendShapeNames);
                allExpressionUsedBlendShapeNames.AddRange(component.expressionPresetRelaxedBlendShape.BlendShapeNames);
                allExpressionUsedBlendShapeNames.AddRange(component.expressionPresetSurprisedBlendShape
                    .BlendShapeNames);
                foreach (var property in component.expressionCustomBlendShapes)
                {
                    allExpressionUsedBlendShapeNames.AddRange(property.BlendShapeNames);
                }

                // 探した
                var blink = FindBlendShape(component.gameObject, "Blink");
                if (blink.Item2 != null) allExpressionUsedBlendShapeNames.Add(blink.Item1);
                var blinkLeft = FindBlendShape(component.gameObject, "BlinkL");
                if (blinkLeft.Item2 != null) allExpressionUsedBlendShapeNames.Add(blinkLeft.Item1);
                var blinkRight = FindBlendShape(component.gameObject, "BlinkR");
                if (blinkRight.Item2 != null) allExpressionUsedBlendShapeNames.Add(blinkRight.Item1);

                var collectorProperties = new Dictionary<SkinnedMeshRenderer, IList<string>>();
                foreach (var name in allExpressionUsedBlendShapeNames)
                {
                    if (string.IsNullOrEmpty(name) || !blendShapeNameToSmr.TryGetValue(name!, out var smr))
                    {
                        continue;
                    }

                    if (collectorProperties.TryGetValue(smr, out var names))
                    {
                        names.Add($"blendShape.{name}");
                    }
                    else
                    {
                        collectorProperties.Add(smr, new List<string>());
                    }
                }

                foreach (var (smr, names) in collectorProperties)
                {
                    collector.ModifyProperties(smr, names);
                }
            }


            private (string, SkinnedMeshRenderer?) FindBlendShape(GameObject root, string blendShapeName)
            {
                // skinned mesh listを取得
                var targetShapeName = blendShapeName
                    .ToLower()
                    .Replace("_", "")
                    .Replace(" ", "")
                    .Replace("-", "");

                var skinnedMeshs = root.GetComponentsInChildren<SkinnedMeshRenderer>();
                // それぞれのスキンメッシュを調べる
                foreach (var skinnedMesh in skinnedMeshs)
                {
                    // スキンメッシュのブレンドシェイプインデックスを取得
                    var blendShapeCount = skinnedMesh.sharedMesh.blendShapeCount;
                    for (var i = 0; i < blendShapeCount; ++i)
                    {
                        var blendShapeNameLower = skinnedMesh.sharedMesh.GetBlendShapeName(i)
                            .ToLower()
                            .Replace("_", "")
                            .Replace(" ", "")
                            .Replace("-", "");
                        if (blendShapeNameLower.IndexOf(targetShapeName, StringComparison.Ordinal) < 0)
                        {
                            continue;
                        }

                        return (skinnedMesh.sharedMesh.GetBlendShapeName(i), skinnedMesh);
                    }
                }

                return (string.Empty, null);
            }

            protected override void CollectDependency(NdmfVrmExporterComponent component,
                ComponentDependencyCollector collector)
            {
                collector.MarkEntrypoint();
            }
        }
    }
#endif // NVE_HAS_AVATAR_OPTIMIZER
}
#endif // NVE_HAS_NDMF

