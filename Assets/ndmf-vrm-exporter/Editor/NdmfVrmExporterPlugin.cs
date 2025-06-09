// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using UnityEditor;

#if NVE_HAS_VRCHAT_AVATAR_SDK
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
#endif // NVE_HAS_VRCHAT_AVATAR_SDK

#if NVE_HAS_NDMF
using nadena.dev.ndmf;
#endif

namespace com.github.hkrn
{
    internal sealed class NdmfVrmExporterPlugin : Plugin<NdmfVrmExporterPlugin>
    {
        public override string QualifiedName => NdmfVrmExporter.PackageJson.Name;

        public override string DisplayName => "NDMF VRM Exporter";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("net.rs64.tex-trans-tool")
                .Run("VrmExportPlugin", ctx =>
                {
                    if (!ctx.AvatarRootObject.TryGetComponent<NdmfVrmExporterComponent>(out var component))
                    {
                        Debug.LogWarning(Translator._("component.runtime.error.validation.not-attached"));
                        return;
                    }

                    if (!component.enabled)
                    {
                        Debug.Log(Translator._("component.runtime.error.validation..not-enabled"));
                        return;
                    }

                    if (!component.HasAuthor)
                    {
                        Debug.LogWarning(Translator._("component.runtime.error.validation.author"));
                        return;
                    }

                    if (!component.HasLicenseUrl)
                    {
                        Debug.LogWarning(Translator._("component.runtime.error.validation.license-url"));
                        return;
                    }

                    var corrupted = new List<SkinnedMeshRenderer>();
                    CheckAllSkinnedMeshRenderers(ctx.AvatarRootTransform, ref corrupted);
                    if (corrupted.Count > 0)
                    {
                        ErrorReport.ReportError(Translator.Instance, ErrorSeverity.NonFatal,
                            "component.runtime.error.validation.smr", corrupted);
                        return;
                    }

                    var ro = ctx.AvatarRootObject;

                    // ブレンドシェイプの重みを一時的に保存してリセット（破綻防止）
                    var blendShapeWeightBackups = new List<(SkinnedMeshRenderer smr, float[] weights)>();
                    var allSkinnedMeshRenderers = ro.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in allSkinnedMeshRenderers)
                    {
                        if (smr.sharedMesh == null) continue;

                        var blendShapeCount = smr.sharedMesh.blendShapeCount;
                        var originalWeights = new float[blendShapeCount];

                        for (var i = 0; i < blendShapeCount; i++)
                        {
                            originalWeights[i] = smr.GetBlendShapeWeight(i);
                            smr.SetBlendShapeWeight(i, 0f);
                        }

                        blendShapeWeightBackups.Add((smr, originalWeights));
                    }

                    string basePath = null;
                    try
                    {
                        // サムネイルがない場合はサムネイルを撮影して適応する

                        if (component.thumbnail == null)
                        {
                            // thumbnail がない場合は、撮影する
                            var thumbnail = CreateAvatarThumbnail(ro.GetComponent<VRC_AvatarDescriptor>());
                            component.thumbnail = thumbnail;
                        }

                        basePath = AssetPathUtils.GetOutputPath(ro);
                        Directory.CreateDirectory(basePath);
                        using var stream = new MemoryStream();
                        using var exporter = new NdmfVrmExporter(ro, ctx.AssetSaver);
                        var json = exporter.Export(stream);
                        var bytes = stream.GetBuffer();
                        File.WriteAllBytes($"{basePath}.vrm", bytes);
                        if (component.enableGenerateJsonFile)
                        {
                            File.WriteAllText($"{basePath}.json", json);
                        }
                    }
                    finally
                    {
                        // ブレンドシェイプの重みを復元
                        foreach (var (smr, weights) in blendShapeWeightBackups)
                        {
                            for (var i = 0; i < weights.Length; i++)
                            {
                                smr.SetBlendShapeWeight(i, weights[i]);
                            }
                        }
                    }

                    if (!component.deleteTemporaryObjects)
                    {
                        return;
                    }

                    foreach (var item in new[] { AssetPathUtils.GetTempPath(ro), basePath })
                    {
                        try
                        {
                            var info = new DirectoryInfo(item);
                            info.Delete(true);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            /* this is ignorable */
                        }
                    }

                    AssetDatabase.Refresh();
                });
        }

        private Texture2D CreateAvatarThumbnail(VRC_AvatarDescriptor avatar)
        {
            // カメラを作りアバターを撮影する
            var camera = new GameObject("Camera");
            var rt = RenderTexture.GetTemporary(512, 512, 24);
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

            List<GameObject> hideAvatars = new List<GameObject>();

            try
            {
                // 見つけたアバターを非表示にする
                var avatars = Object.FindObjectsOfType<VRCAvatarDescriptor>();
                foreach (var otherAvatar in avatars)
                {
                    if (!otherAvatar.gameObject.activeSelf || avatar == otherAvatar) continue;
                    hideAvatars.Add(otherAvatar.gameObject);
                    otherAvatar.gameObject.SetActive(false);
                }

                var cam = camera.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Color;
                cam.backgroundColor = Color.white;
                cam.orthographic = true;
                cam.orthographicSize = 0.2f;

                cam.targetTexture = rt;

                var p = camera.transform.position;
                var avatarPos = avatar.transform.position;
                p.x = avatarPos.x;
                p.y = avatar.ViewPosition.y;
                p.z = avatarPos.z + 10;
                camera.transform.position = p;
                camera.transform.rotation = Quaternion.Euler(0, 180, 0);
                cam.Render();

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
            }
            finally
            {
                RenderTexture.active = null;
                Object.DestroyImmediate(camera);
                RenderTexture.ReleaseTemporary(rt);

                // もとに戻す
                foreach (var otherAvatar in hideAvatars)
                {
                    otherAvatar.SetActive(true);
                }
            }

            return tex;
        }

        private static void CheckAllSkinnedMeshRenderers(Transform parent, ref List<SkinnedMeshRenderer> corrupted)
        {
            foreach (Transform child in parent)
            {
                if (!child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (child.gameObject.TryGetComponent<SkinnedMeshRenderer>(out var smr) && smr.sharedMesh &&
                    (smr.bones.Any(bone => !bone) || IsSharedMeshCorrupted(smr)))
                {
                    corrupted.Add(smr);
                }

                CheckAllSkinnedMeshRenderers(child, ref corrupted);
            }
        }

        private static bool IsSharedMeshCorrupted(SkinnedMeshRenderer smr)
        {
            var mesh = smr.sharedMesh;
            var numPositions = mesh.boneWeights.Length;
            var numSubMeshes = mesh.subMeshCount;
            var indexMapping = new Dictionary<uint, uint>();
            for (var meshIndex = 0; meshIndex < numSubMeshes; meshIndex++)
            {
                var indices = mesh.GetIndices(meshIndex).Select(index => (uint)index).ToArray();
                var indexSet = new HashSet<uint>(indices);
                indexMapping.Clear();
                for (uint i = 0; i < numPositions; i++)
                {
                    if (!indexSet.Contains(i))
                    {
                        continue;
                    }

                    var newIndex = (uint)indexMapping.Count;
                    indexMapping.Add(i, newIndex);
                }

                if (indices.Any(index => !indexMapping.TryGetValue(index, out _)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif // NVE_HAS_NDMF
