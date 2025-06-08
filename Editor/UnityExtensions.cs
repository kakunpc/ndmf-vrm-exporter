// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if NVE_HAS_NDMF
using nadena.dev.ndmf.localization;
#endif

namespace com.github.hkrn
{
    internal static class UnityExtensions
    {
        public static System.Numerics.Vector2 ToVector2(this Vector2 self)
        {
            return new System.Numerics.Vector2(self.x, self.y);
        }

        public static System.Numerics.Vector2 ToVector2WithCoordinateSpace(this Vector2 self)
        {
            return new System.Numerics.Vector2(self.x, 1.0f - self.y);
        }

        public static System.Numerics.Vector3 ToVector3(this Vector3 self)
        {
            return new System.Numerics.Vector3(self.x, self.y, self.z);
        }

        public static System.Numerics.Vector3 ToVector3WithCoordinateSpace(this Vector3 self)
        {
            return new System.Numerics.Vector3(-self.x, self.y, self.z);
        }

        public static System.Numerics.Vector4 ToVector4WithTangentSpace(this Vector4 self)
        {
            var v = (new Vector3(-self.x, self.y, self.z)).normalized;
            var w = Mathf.Sign(-self.w);
            return new System.Numerics.Vector4(v.x, v.y, v.z, w);
        }

        public static System.Numerics.Vector3 ToVector3(this Color self)
        {
            return new System.Numerics.Vector3(self.r, self.g, self.b);
        }

        public static System.Numerics.Vector3 ToVector3(this Color self, ColorSpace src, ColorSpace dst)
        {
            var color = ConvertColor(self, src, dst);
            return new System.Numerics.Vector3(color.r, color.g, color.b);
        }

        public static System.Numerics.Vector4 ToVector4(this Color self, ColorSpace src, ColorSpace dst)
        {
            var color = ConvertColor(self, src, dst);
            return new System.Numerics.Vector4(color.r, color.g, color.b, color.a);
        }

        public static System.Numerics.Quaternion ToQuaternionWithCoordinateSpace(this Quaternion self)
        {
            self.ToAngleAxis(out var angle, out var axis);
            var newAxis = axis.ToVector3WithCoordinateSpace();
            return System.Numerics.Quaternion.CreateFromAxisAngle(newAxis, -angle * Mathf.Deg2Rad);
        }

        private static System.Numerics.Matrix4x4 ToMatrix4(this Matrix4x4 self)
        {
            return new System.Numerics.Matrix4x4(
                self.m00, self.m01, self.m02, self.m03,
                self.m10, self.m11, self.m12, self.m13,
                self.m20, self.m21, self.m22, self.m23,
                self.m30, self.m31, self.m32, self.m33);
        }

        public static System.Numerics.Matrix4x4 ToNormalizedMatrix(this Matrix4x4 self)
        {
            var t = new Vector3(-self.m03, self.m13, self.m23);
            self.rotation.ToAngleAxis(out var angle, out var axis);
            var r = Quaternion.AngleAxis(-angle, new Vector3(-axis.x, axis.y, axis.z));
            var s = new Vector3
            {
                x = new Vector4(self.m00, self.m10, self.m20, self.m30).magnitude,
                y = new Vector4(self.m01, self.m11, self.m21, self.m31).magnitude,
                z = new Vector4(self.m02, self.m12, self.m22, self.m32).magnitude,
            };
            return Matrix4x4.TRS(t, r, s).ToMatrix4();
        }

        public static gltf.material.TextureFilterMode ToTextureFilterMode(this FilterMode self)
        {
            return self switch
            {
                FilterMode.Point => gltf.material.TextureFilterMode.Nearest,
                FilterMode.Bilinear => gltf.material.TextureFilterMode.Linear,
                FilterMode.Trilinear => gltf.material.TextureFilterMode.LinearMipmapLinear,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static gltf.material.TextureWrapMode ToTextureWrapMode(this TextureWrapMode self)
        {
            return self switch
            {
                TextureWrapMode.Clamp => gltf.material.TextureWrapMode.ClampToEdge,
                TextureWrapMode.Mirror => gltf.material.TextureWrapMode.MirroredRepeat,
                TextureWrapMode.Repeat => gltf.material.TextureWrapMode.Repeat,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        internal static Texture2D Blit(this Texture sourceTexture, TextureFormat textureFormat, ColorSpace cs,
            Material? material = null)
        {
            return Blit(sourceTexture, sourceTexture.width, sourceTexture.height, textureFormat, cs, material);
        }

        internal static Texture2D Blit(this Texture sourceTexture, int width, int height, TextureFormat textureFormat,
            ColorSpace cs, Material? material)
        {
            var rw = cs == ColorSpace.Gamma ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
            var activeRenderTexture = RenderTexture.active;
            var destRenderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, rw);
            if (material)
            {
                Graphics.Blit(sourceTexture, destRenderTexture, material);
            }
            else
            {
                Graphics.Blit(sourceTexture, destRenderTexture);
            }

            RenderTexture.active = destRenderTexture;
            var outputTexture = new Texture2D(width, height, textureFormat, false, cs == ColorSpace.Linear)
            {
                name = sourceTexture.name
            };
            outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = activeRenderTexture;
            RenderTexture.ReleaseTemporary(destRenderTexture);
            return outputTexture;
        }

        private static Color ConvertColor(Color self, ColorSpace src, ColorSpace dst)
        {
            var color = (src, dst) switch
            {
                (ColorSpace.Gamma, ColorSpace.Linear) => self.linear,
                (ColorSpace.Linear, ColorSpace.Gamma) => self.gamma,
                _ => self
            };
            return color;
        }
    }

    internal static class Translator
    {
        internal static readonly Localizer Instance = new("en-us", () =>
        {
            var path = AssetDatabase.GUIDToAssetPath("9d1da1be88b74198955d08e38cddf4b4");
            var english = AssetDatabase.LoadAssetAtPath<LocalizationAsset>($"{path}/en-us.po");
            return new List<LocalizationAsset> { english };
        });

        public static string _(string key) => Instance.GetLocalizedString(key);
    }

    internal static class AssetPathUtils
    {
        private const string CloneSuffix = "(Clone)";

        public static string BasePath => "Assets/NDMF VRM Exporter";

        public static string GetOutputPath(GameObject gameObject)
        {
            return GetBasePath(gameObject, BasePath);
        }

        public static string GetTempPath(GameObject gameObject)
        {
            return GetBasePath(gameObject, FileUtil.GetUniqueTempPathInProject());
        }

        public static string TrimCloneSuffix(string name)
        {
            while (name.EndsWith(CloneSuffix, StringComparison.Ordinal))
            {
                name = name[..^CloneSuffix.Length];
            }

            return name;
        }

        private static string GetBasePath(GameObject gameObject, string basePath)
        {
            var sceneName = !string.IsNullOrEmpty(gameObject.scene.name)
                ? StripInvalidFileNameCharacters(gameObject.scene.name)
                : "Untitled";
            var gameObjectName = TrimCloneSuffix(StripInvalidFileNameCharacters(gameObject.name));
            return $"{basePath}/{sceneName}/{gameObjectName}";
        }

        private static string StripInvalidFileNameCharacters(string name)
        {
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }
    }

    internal static class Utility
    {
        public static (string, SkinnedMeshRenderer?) FindBlendShape(GameObject root, string blendShapeName)
        {
            // skinned mesh listを取得
            var targetShapeName = blendShapeName
                .ToLower()
                .Replace("_", "")
                .Replace(" ", "")
                .Replace("-", "");

            var skinnedMeshs = root.GetComponentsInChildren<SkinnedMeshRenderer>();

            // 完全一致検索
            foreach (var skinnedMesh in skinnedMeshs)
            {
                if(skinnedMesh == null || skinnedMesh.sharedMesh == null)
                {
                    continue;
                }

                // スキンメッシュのブレンドシェイプインデックスを取得
                var blendShapeCount = skinnedMesh.sharedMesh.blendShapeCount;
                for (var i = 0; i < blendShapeCount; ++i)
                {
                    var blendShapeNameLower = skinnedMesh.sharedMesh.GetBlendShapeName(i);
                    if (blendShapeNameLower == blendShapeName)
                    {
                        return (skinnedMesh.sharedMesh.GetBlendShapeName(i), skinnedMesh);
                    }
                }
            }

            // あいまい検索
            foreach (var skinnedMesh in skinnedMeshs)
            {
                if(skinnedMesh == null || skinnedMesh.sharedMesh == null)
                {
                    continue;
                }

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
    }
}
#endif // NVE_HAS_NDMF
