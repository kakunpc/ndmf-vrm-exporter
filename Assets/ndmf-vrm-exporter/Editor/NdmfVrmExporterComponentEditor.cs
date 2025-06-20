// SPDX-FileCopyrightText: 2024-present hkrn
// SPDX-License-Identifier: MPL

#nullable enable
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if NVE_HAS_VRCHAT_AVATAR_SDK
using System.Net.Http;
using System.Threading;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
#endif // NVE_HAS_VRCHAT_AVATAR_SDK

namespace com.github.hkrn
{
        [CustomEditor(typeof(NdmfVrmExporterComponent))]
    public sealed class NdmfVrmExporterComponentEditor : Editor
    {
        private SerializedProperty _metadataFoldoutProp = null!;
        private SerializedProperty _authorsProp = null!;
        private SerializedProperty _versionProp = null!;
        private SerializedProperty _copyrightInformationProp = null!;
        private SerializedProperty _contactInformationProp = null!;
        private SerializedProperty _referencesProp = null!;
        private SerializedProperty _enableContactInformationOnVRChatAutofillProp = null!;
        private SerializedProperty _licenseUrlProp = null!;
        private SerializedProperty _thirdPartyLicensesProp = null!;
        private SerializedProperty _otherLicenseUrlProp = null!;
        private SerializedProperty _avatarPermissionProp = null!;
        private SerializedProperty _commercialUsageProp = null!;
        private SerializedProperty _creditNotationProp = null!;
        private SerializedProperty _modificationProp = null!;
        private SerializedProperty _metadataAllowFoldoutProp = null!;
        private SerializedProperty _allowExcessivelyViolentUsageProp = null!;
        private SerializedProperty _allowExcessivelySexualUsageProp = null!;
        private SerializedProperty _allowPoliticalOrReligiousUsageProp = null!;
        private SerializedProperty _allowAntisocialOrHateUsageProp = null!;
        private SerializedProperty _allowRedistributionProp = null!;
        private SerializedProperty _thumbnailProp = null!;
        private SerializedProperty _expressionFoldoutProp = null!;
        private SerializedProperty _expressionPresetHappyBlendShape = null!;
        private SerializedProperty _expressionPresetAngryBlendShape = null!;
        private SerializedProperty _expressionPresetSadBlendShape = null!;
        private SerializedProperty _expressionPresetRelaxedBlendShape = null!;
        private SerializedProperty _expressionPresetSurprisedBlendShape = null!;
        private SerializedProperty _expressionCustomBlendShapes = null!;
        private SerializedProperty _springBoneFoldoutProp = null!;
        private SerializedProperty _excludedSpringBoneColliderTransformsProp = null!;
        private SerializedProperty _excludedSpringBoneTransformsProp = null!;
        private SerializedProperty _constraintFoldoutProp = null!;
        private SerializedProperty _excludedConstraintTransformsProp = null!;
        private SerializedProperty _mtoonFoldoutProp = null!;
        private SerializedProperty _enableMToonRimLightProp = null!;
        private SerializedProperty _enableMToonMatCapProp = null!;
        private SerializedProperty _enableMToonOutlineProp = null!;
        private SerializedProperty _enableBakingAlphaMaskProp = null!;
        private SerializedProperty _debugFoldoutProp = null!;
        private SerializedProperty _makeAllNodeNamesUniqueProp = null!;
        private SerializedProperty _enableVertexColorOutputProp = null!;
        private SerializedProperty _disableVertexColorOnLiltoonProp = null!;
        private SerializedProperty _enableGenerateJsonFileProp = null!;
        private SerializedProperty _deleteTemporaryObjectsProp = null!;
        private SerializedProperty _ktxToolPathProp = null!;
        private SerializedProperty _metadataModeSelection = null!;
        private SerializedProperty _expressionModeSelection = null!;
        private SerializedProperty _blinkProp = null!;
        private SerializedProperty _blinkLProp = null!;
        private SerializedProperty _blinkRProp = null!;
#if NVE_HAS_VRCHAT_AVATAR_SDK
        private struct VRChatAvatarToMetadata
        {
            public string Author { get; init; }
            public string CopyrightInformation { get; init; }
            public string ContactInformation { get; init; }
            public string Version { get; init; }
            public string OriginThumbnailPath { get; init; }
        };

        private Task<VRChatAvatarToMetadata>? _retrieveAvatarTask;
        private CancellationTokenSource _cancellationTokenSource = new();
        private string? _retrieveAvatarTaskErrorMessage;
#endif // NVE_HAS_VRCHAT_AVATAR_SDK

        private void OnEnable()
        {
            _metadataFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.metadataFoldout));
            _authorsProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.authors));
            _versionProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.version));
            _copyrightInformationProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.copyrightInformation));
            _contactInformationProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.contactInformation));
            _referencesProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.references));
            _enableContactInformationOnVRChatAutofillProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent
                    .enableContactInformationOnVRChatAutofill));
            _licenseUrlProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.licenseUrl));
            _thirdPartyLicensesProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.thirdPartyLicenses));
            _otherLicenseUrlProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.otherLicenseUrl));
            _avatarPermissionProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.avatarPermission));
            _commercialUsageProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.commercialUsage));
            _creditNotationProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.creditNotation));
            _modificationProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.modification));
            _metadataAllowFoldoutProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.metadataAllowFoldout));
            _allowExcessivelyViolentUsageProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.allowExcessivelyViolentUsage));
            _allowExcessivelySexualUsageProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.allowExcessivelySexualUsage));
            _allowPoliticalOrReligiousUsageProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.allowPoliticalOrReligiousUsage));
            _allowAntisocialOrHateUsageProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.allowAntisocialOrHateUsage));
            _allowRedistributionProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.allowRedistribution));
            _thumbnailProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.thumbnail));
            _expressionFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionFoldout));
            _expressionPresetHappyBlendShape =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionPresetHappyBlendShape));
            _expressionPresetAngryBlendShape =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionPresetAngryBlendShape));
            _expressionPresetSadBlendShape =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionPresetSadBlendShape));
            _expressionPresetRelaxedBlendShape =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionPresetRelaxedBlendShape));
            _expressionPresetSurprisedBlendShape =
                serializedObject.FindProperty(
                    nameof(NdmfVrmExporterComponent.expressionPresetSurprisedBlendShape));
            _expressionCustomBlendShapes =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionCustomBlendShapes));
            _springBoneFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.springBoneFoldout));
            _excludedSpringBoneColliderTransformsProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.excludedSpringBoneColliderTransforms));
            _excludedSpringBoneTransformsProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.excludedSpringBoneTransforms));
            _constraintFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.constraintFoldout));
            _excludedConstraintTransformsProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.excludedConstraintTransforms));
            _mtoonFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.mtoonFoldout));
            _enableMToonRimLightProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableMToonRimLight));
            _enableMToonMatCapProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableMToonMatCap));
            _enableMToonOutlineProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableMToonOutline));
            _enableBakingAlphaMaskProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableBakingAlphaMaskTexture));
            _debugFoldoutProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.debugFoldout));
            _makeAllNodeNamesUniqueProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.makeAllNodeNamesUnique));
            _enableVertexColorOutputProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableVertexColorOutput));
            _disableVertexColorOnLiltoonProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.disableVertexColorOnLiltoon));
            _enableGenerateJsonFileProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.enableGenerateJsonFile));
            _deleteTemporaryObjectsProp =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.deleteTemporaryObjects));
            _ktxToolPathProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.ktxToolPath));
            _metadataModeSelection =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.metadataModeSelection));
            _expressionModeSelection =
                serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.expressionModeSelection));
            _blinkProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.blink));
            _blinkLProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.blinkL));
            _blinkRProp = serializedObject.FindProperty(nameof(NdmfVrmExporterComponent.blinkR));
            var component = (NdmfVrmExporterComponent)target;
            component.expressionPresetHappyBlendShape.gameObject = component.gameObject;
            component.expressionPresetAngryBlendShape.gameObject = component.gameObject;
            component.expressionPresetSadBlendShape.gameObject = component.gameObject;
            component.expressionPresetRelaxedBlendShape.gameObject = component.gameObject;
            component.expressionPresetSurprisedBlendShape.gameObject = component.gameObject;
            foreach (var property in component.expressionCustomBlendShapes)
            {
                property.gameObject = component.gameObject;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var component = (NdmfVrmExporterComponent)target;
            if (component.enabled)
            {
                if (!component.HasAvatarRoot)
                {
                    EditorGUILayout.HelpBox(Translator._("component.validation.avatar-root"),
                        MessageType.Error);
                }
                else if (!component.HasAuthor)
                {
                    EditorGUILayout.HelpBox(Translator._("component.validation.author"),
                        MessageType.Error);
                }
                else if (!component.HasLicenseUrl)
                {
                    EditorGUILayout.HelpBox(Translator._("component.validation.license-url"),
                        MessageType.Error);
                }
                else
                {
                    var outputPath = $"{AssetPathUtils.GetOutputPath(component.gameObject)}.vrm";
                    EditorGUILayout.HelpBox(string.Format(Translator._("component.output.path"), outputPath),
                        MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(Translator._("component.validation.not-enabled"),
                    MessageType.Warning);
            }

#if NVE_HAS_VRCHAT_AVATAR_SDK
            if (_retrieveAvatarTask != null && _retrieveAvatarTask!.IsCompleted)
            {
                if (_retrieveAvatarTask.Exception != null)
                {
                    foreach (var exception in _retrieveAvatarTask.Exception.InnerExceptions)
                    {
                        if (exception is not ApiErrorException ae)
                        {
                            continue;
                        }

                        if (ae.StatusCode != HttpStatusCode.Unauthorized)
                        {
                            continue;
                        }

                        _retrieveAvatarTaskErrorMessage =
                            Translator._("component.metadata.information.vrchat.error.authorization");
                        break;
                    }

                    Debug.LogError($"Failed to retrieve metadata via VRChat: {_retrieveAvatarTask.Exception}");
                }
                else if (_retrieveAvatarTask.IsCanceled)
                {
                    Debug.Log("Cancelled to retrieve metadata via VRChat");
                }
                else
                {
                    var result = _retrieveAvatarTask.Result;
                    if (component.HasAuthor)
                    {
                        component.authors[0] = result.Author;
                    }
                    else
                    {
                        component.authors = new List<string> { result.Author };
                    }

                    component.copyrightInformation = result.CopyrightInformation;
                    if (_enableContactInformationOnVRChatAutofillProp.boolValue)
                    {
                        component.contactInformation = result.ContactInformation;
                    }

                    component.version = result.Version;
                    if (!string.IsNullOrEmpty(result.OriginThumbnailPath))
                    {
                        var baseTexture = new Texture2D(2, 2);
                        baseTexture.LoadImage(File.ReadAllBytes(result.OriginThumbnailPath));
                        var lowerSize = Mathf.Min(baseTexture.width, baseTexture.height);
                        var intermediateTexture = new Texture2D(lowerSize, lowerSize, baseTexture.format, false);
                        var srcX = Math.Max(baseTexture.width - lowerSize, 0) / 2;
                        var srcY = Math.Max(baseTexture.height - lowerSize, 0) / 2;
                        Graphics.CopyTexture(baseTexture, 0, 0, srcX, srcY, lowerSize, lowerSize, intermediateTexture,
                            0, 0, 0, 0);
                        var filePath = result.OriginThumbnailPath.Replace(".origin.png", ".png");
                        var destTexture =
                            intermediateTexture.Blit(1024, 1024, TextureFormat.ARGB32, ColorSpace.Gamma, null);
                        var bytes = destTexture.EncodeToPNG();
                        DestroyImmediate(destTexture);
                        File.WriteAllBytes(filePath, bytes);
                        AssetDatabase.ImportAsset(filePath);
                        component.thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                    }

                    Debug.Log("Succeeded to retrieve metadata via VRChat");
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _retrieveAvatarTask = null;
            }
#endif // NVE_HAS_VRCHAT_AVATAR_SDK

            var thumbnail = _thumbnailProp.objectReferenceValue as Texture2D;
            if (thumbnail && thumbnail is not null && thumbnail.width != thumbnail.height)
            {
                EditorGUILayout.HelpBox(Translator._("component.validation.avatar-thumbnail"), MessageType.Warning);
            }

            var metadataFoldout = EditorGUILayout.Foldout(_metadataFoldoutProp.boolValue,
                Translator._("component.category.metadata"));
            if (metadataFoldout)
            {
                DrawMetadata();
            }

            EditorGUILayout.Separator();
            var expressionsFoldout = EditorGUILayout.Foldout(_expressionFoldoutProp.boolValue,
                Translator._("component.category.expressions"));
            if (expressionsFoldout)
            {
                DrawExpressions();
            }

            EditorGUILayout.Separator();
            var mtoonFoldout =
                EditorGUILayout.Foldout(_mtoonFoldoutProp.boolValue, Translator._("component.category.mtoon"));
            if (mtoonFoldout)
            {
                DrawMToonOptions();
            }

            EditorGUILayout.Separator();
            var springBoneFoldout = EditorGUILayout.Foldout(_springBoneFoldoutProp.boolValue,
                Translator._("component.category.spring-bone"));
            if (springBoneFoldout)
            {
                DrawSpringBoneOptions();
            }

            EditorGUILayout.Separator();
            var constraintFoldout = EditorGUILayout.Foldout(_constraintFoldoutProp.boolValue,
                Translator._("component.category.constraint"));
            if (constraintFoldout)
            {
                DrawConstraintOptions();
            }

            EditorGUILayout.Separator();
            var debugFoldout =
                EditorGUILayout.Foldout(_debugFoldoutProp.boolValue, Translator._("component.category.debug"));
            if (debugFoldout)
            {
                DrawDebugOptions();
            }

            _metadataFoldoutProp.boolValue = metadataFoldout;
            _expressionFoldoutProp.boolValue = expressionsFoldout;
            _mtoonFoldoutProp.boolValue = mtoonFoldout;
            _springBoneFoldoutProp.boolValue = springBoneFoldout;
            _constraintFoldoutProp.boolValue = constraintFoldout;
            _debugFoldoutProp.boolValue = debugFoldout;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMetadata()
        {
            var value = GUILayout.Toolbar(_metadataModeSelection.intValue, new[]
            {
                Translator._("component.metadata.information"),
                Translator._("component.metadata.licenses"),
                Translator._("component.metadata.permissions"),
            });
            switch (value)
            {
                case 0:
                {
                    DrawInformation();
                    break;
                }
                case 1:
                {
                    DrawLicenses();
                    break;
                }
                case 2:
                {
                    DrawPermissions();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _metadataModeSelection.intValue = value;
        }

        private void DrawInformation()
        {
            DrawPropertyField(Translator._("component.metadata.information.avatar-thumbnail"), _thumbnailProp);
            EditorGUILayout.PropertyField(_authorsProp,
                new GUIContent(Translator._("component.metadata.information.authors")));
            DrawPropertyField(Translator._("component.metadata.information.version"), _versionProp);
            DrawPropertyField(Translator._("component.metadata.information.copyright-information"),
                _copyrightInformationProp);
            DrawPropertyField(Translator._("component.metadata.information.contact-information"),
                _contactInformationProp);
            EditorGUILayout.PropertyField(_referencesProp,
                new GUIContent(Translator._("component.metadata.information.references")));
#if NVE_HAS_VRCHAT_AVATAR_SDK
            var component = (NdmfVrmExporterComponent)target;
            if (!component.transform.TryGetComponent<PipelineManager>(out var pipelineManager) ||
                string.IsNullOrEmpty(pipelineManager.blueprintId))
                return;
            EditorGUILayout.Separator();
            if (_retrieveAvatarTask != null)
            {
                if (GUILayout.Button(Translator._("component.metadata.information.vrchat.cancel")))
                {
                    _cancellationTokenSource.Cancel();
                    Debug.Log("Requested to cancel retrieving metadata task");
                }
            }
            else if (GUILayout.Button(Translator._("component.metadata.information.vrchat.retrieve")))
            {
                async Task<VRChatAvatarToMetadata> RetrieveAvatarTask(string blueprintId, CancellationToken token)
                {
                    var packageJsonFile =
                        await File.ReadAllTextAsync($"Packages/{NdmfVrmExporter.PackageJson.Name}/package.json", token);
                    var packageJson = NdmfVrmExporter.PackageJson.LoadFromString(packageJsonFile);
                    var avatar = await VRCApi.GetAvatar(blueprintId, cancellationToken: token);
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd($"{packageJson.DisplayName}/{packageJson.Version}");
                    var result = await client.GetAsync(avatar.ImageUrl, token);
                    var filePath = string.Empty;
                    if (result.IsSuccessStatusCode)
                    {
                        var bytes = await result.Content.ReadAsByteArrayAsync();
                        var basePath = $"{AssetPathUtils.BasePath}/VRChatSDKAvatarThumbnails";
                        Directory.CreateDirectory(basePath);
                        filePath = $"{basePath}/{avatar.ID}.origin.png";
                        await File.WriteAllBytesAsync(filePath, bytes, token);
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot retrieve thumbnail: {result.ReasonPhrase}");
                    }

                    return new VRChatAvatarToMetadata
                    {
                        Author = avatar.AuthorName,
                        CopyrightInformation = $"Copyright \u00a9 {avatar.CreatedAt.Year} {avatar.AuthorName}",
                        ContactInformation = $"https://vrchat.com/home/user/{avatar.AuthorId}",
                        Version =
                            $"{avatar.UpdatedAt.Year}.{avatar.UpdatedAt.Month}.{avatar.UpdatedAt.Day}+{avatar.Version}",
                        OriginThumbnailPath = filePath,
                    };
                }

                _retrieveAvatarTaskErrorMessage = null;
                _retrieveAvatarTask = RetrieveAvatarTask(pipelineManager.blueprintId, _cancellationTokenSource.Token);
                Debug.Log("Start to retrieve metadata via VRChat API");
            }

            EditorGUILayout.Space();
            DrawToggleLeft(Translator._("component.metadata.information.vrchat.enable-contact-information"),
                _enableContactInformationOnVRChatAutofillProp);

            if (!string.IsNullOrEmpty(_retrieveAvatarTaskErrorMessage))
            {
                EditorGUILayout.HelpBox(_retrieveAvatarTaskErrorMessage, MessageType.Error);
            }

#endif // NVE_HAS_VRCHAT_AVATAR_SDK
        }

        private void DrawLicenses()
        {
            DrawPropertyField(Translator._("component.metadata.licenses.license-url"), _licenseUrlProp);
            DrawPropertyField(Translator._("component.metadata.licenses.third-party-license"), _thirdPartyLicensesProp);
            DrawPropertyField(Translator._("component.metadata.licenses.other-license-url"), _otherLicenseUrlProp);
        }

        private void DrawPermissions()
        {
            DrawPropertyField(Translator._("component.metadata.permissions.avatar-permission"), _avatarPermissionProp);
            DrawPropertyField(Translator._("component.metadata.permissions.commercial-usage"), _commercialUsageProp);
            DrawPropertyField(Translator._("component.metadata.permissions.credit-notation"), _creditNotationProp);
            DrawPropertyField(Translator._("component.metadata.permissions.modification"), _modificationProp);
            var foldout = EditorGUILayout.Foldout(_metadataAllowFoldoutProp.boolValue,
                Translator._("component.metadata.permissions.allow.header"));
            if (foldout)
            {
                DrawPropertyField(Translator._("component.metadata.permissions.allow.redistribution"),
                    _allowRedistributionProp);
                DrawPropertyField(Translator._("component.metadata.permissions.allow.excessively-violent-usage"),
                    _allowExcessivelyViolentUsageProp);
                DrawPropertyField(Translator._("component.metadata.permissions.allow.excessively-sexual-usage"),
                    _allowExcessivelySexualUsageProp);
                DrawPropertyField(Translator._("component.metadata.permissions.allow.political-or-religious-usage"),
                    _allowPoliticalOrReligiousUsageProp);
                DrawPropertyField(Translator._("component.metadata.permissions.allow.antisocial-or-hate-usage"),
                    _allowAntisocialOrHateUsageProp);
            }

            _metadataAllowFoldoutProp.boolValue = foldout;
        }

        private void DrawExpressions()
        {
            var value = GUILayout.Toolbar(_expressionModeSelection.intValue, new[]
            {
                Translator._("component.expression.preset"),
                Translator._("component.expression.custom"),
            });
            switch (value)
            {
                case 0:
                {
                    DrawPresetExpressions();
                    break;
                }
                case 1:
                {
                    DrawCustomExpression();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _expressionModeSelection.intValue = value;
        }

        private void DrawPresetExpressions()
        {
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField("Blink", _blinkProp);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField("Blink Left", _blinkLProp);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField("Blink Right", _blinkRProp);
            }

            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField(Translator._("component.expression.preset.happy"), _expressionPresetHappyBlendShape);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField(Translator._("component.expression.preset.angry"), _expressionPresetAngryBlendShape);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField(Translator._("component.expression.preset.sad"), _expressionPresetSadBlendShape);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField(Translator._("component.expression.preset.relaxed"),
                    _expressionPresetRelaxedBlendShape);
            }
            {
                using var _ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
                DrawPropertyField(Translator._("component.expression.preset.surprised"),
                    _expressionPresetSurprisedBlendShape);
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button(Translator._("component.expression.preset.from-mmd")))
            {
                void SetFromMmdExpression(string targetBlendShapeName, SerializedProperty prop)
                {
                    var option = (VrmExpressionProperty)prop.boxedValue;
                    option.SetFromMmdExpression(targetBlendShapeName);
                    prop.boxedValue = option;
                }

                SetFromMmdExpression("笑い", _expressionPresetHappyBlendShape);
                SetFromMmdExpression("怒り", _expressionPresetAngryBlendShape);
                SetFromMmdExpression("困る", _expressionPresetSadBlendShape);
                SetFromMmdExpression("なごみ", _expressionPresetRelaxedBlendShape);
                SetFromMmdExpression("びっくり", _expressionPresetSurprisedBlendShape);
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button(Translator._("component.expression.preset.reset-all")))
            {
                var gameObject = ((NdmfVrmExporterComponent)target).gameObject;

                VrmExpressionProperty WrapVrmExpression(VrmExpressionProperty property)
                {
                    property.gameObject = gameObject;
                    return property;
                }

                _expressionPresetHappyBlendShape.boxedValue = WrapVrmExpression(VrmExpressionProperty.Happy);
                _expressionPresetAngryBlendShape.boxedValue = WrapVrmExpression(VrmExpressionProperty.Angry);
                _expressionPresetSadBlendShape.boxedValue = WrapVrmExpression(VrmExpressionProperty.Sad);
                _expressionPresetRelaxedBlendShape.boxedValue = WrapVrmExpression(VrmExpressionProperty.Relaxed);
                _expressionPresetSurprisedBlendShape.boxedValue = WrapVrmExpression(VrmExpressionProperty.Surprised);
            }
        }

        private void DrawCustomExpression()
        {
            var go = (NdmfVrmExporterComponent)target;
            foreach (var property in go.expressionCustomBlendShapes)
            {
                property.gameObject = go.gameObject;
            }

            EditorGUILayout.PropertyField(_expressionCustomBlendShapes, GUIContent.none);

            EditorGUILayout.Separator();
            if (GUILayout.Button(Translator._("component.expression.custom.mmd")))
            {
                bool SetFromMmdExpression(string blendShape, bool isBinary)
                {
                    // すでに存在していたら無視
                    if (go.expressionCustomBlendShapes.Exists(x => x.expressionName == blendShape))
                    {
                        return true;
                    }

                    var (targetBlendShapeName, skinnedMeshRenderer) = Utility.FindBlendShape(go.gameObject, blendShape);

                    if (string.IsNullOrEmpty(targetBlendShapeName) ||
                        targetBlendShapeName != blendShape) // 完全一致
                    {
                        return false;
                    }


                    var value = new VrmExpressionProperty
                    {
                        baseType = VrmExpressionProperty.BaseType.BlendShape,
                        gameObject = skinnedMeshRenderer!.gameObject,
                        blendShapeName = targetBlendShapeName,
                        expressionName = blendShape,
                        overrideBlink = vrm.core.ExpressionOverrideType.Block,
                        overrideLookAt = vrm.core.ExpressionOverrideType.Block,
                        overrideMouth = vrm.core.ExpressionOverrideType.Block,
                        isBinary = isBinary,
                        isPreset = false
                    };

                    go.expressionCustomBlendShapes.Add(value);
                    return true;
                }

                var mmdMorphs = new[]
                {
                    "通常",
                    "まばたき",
                    "笑い",
                    "ウィンク",
                    "ウィンク右",
                    "ウィンク２",
                    "ウィンク２右",
                    "なごみ",
                    "はぅ",
                    "びっくり",
                    "じと目",
                    "ｷﾘｯ",
                    "はちゅ目",
                    "はちゅ目縦潰れ",
                    "はちゅ目横潰れ",
                    "星目",
                    "はぁと",
                    "瞳小",
                    "瞳縦潰れ",
                    "光下",
                    "恐ろしい子！",
                    "ハイライト消し",
                    "映り込み消し",
                    "あ",
                    "い",
                    "う",
                    "え",
                    "お",
                    "あ２",
                    "ワ",
                    "ω",
                    "ω□",
                    "にやり",
                    "にやり2",
                    "にっこり",
                    "ぺろっ",
                    "てへぺろ",
                    "てへぺろ２",
                    "口角上げ",
                    "口角下げ",
                    "口横広げ",
                    "歯無し上",
                    "歯無し下",
                    "ハンサム",
                    "真面目",
                    "困る",
                    "にこり",
                    "怒り",
                    "上",
                    "下",
                    "前",
                    "眉頭左",
                    "眉頭右",
                    "照れ",
                    "涙",
                    "がーん",
                    "青ざめる",
                    "髪影消",
                    "輪郭",
                    "メガネ",
                    "みっぱい"
                };

                var oldLength = go.expressionCustomBlendShapes.Count;
                List<string> failedMorphs = new List<string>();

                foreach (var morph in mmdMorphs)
                {
                    if (!SetFromMmdExpression(morph, false))
                    {
                        failedMorphs.Add(morph);
                    }
                }

                var newLength = go.expressionCustomBlendShapes.Count;
                if (newLength - oldLength != 0)
                {
                    Debug.LogWarning($"Failed to set {mmdMorphs.Length} expressions from MMD. " +
                                     $"Only {newLength - oldLength} expressions were added." +
                                     (failedMorphs.Count > 0
                                         ? $" Failed morphs: {string.Join(", ", failedMorphs)}"
                                         : ""));
                }
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button(Translator._("component.expression.custom.perfect-sync")))
            {
                bool SetFromPerfectSyncExpression(string blendShape, bool isBinary)
                {
                    // すでに存在していたら無視
                    if (go.expressionCustomBlendShapes.Exists(x => x.expressionName == blendShape))
                    {
                        return true;
                    }

                    var (targetBlendShapeName, skinnedMeshRenderer) = Utility.FindBlendShape(go.gameObject, blendShape);

                    if (string.IsNullOrEmpty(targetBlendShapeName))
                    {
                        return false;
                    }

                    var value = new VrmExpressionProperty
                    {
                        baseType = VrmExpressionProperty.BaseType.BlendShape,
                        gameObject = skinnedMeshRenderer!.gameObject,
                        blendShapeName = targetBlendShapeName,
                        expressionName = blendShape,
                        overrideBlink = vrm.core.ExpressionOverrideType.Block,
                        overrideLookAt = vrm.core.ExpressionOverrideType.Block,
                        overrideMouth = vrm.core.ExpressionOverrideType.Block,
                        isBinary = isBinary,
                        isPreset = false
                    };

                    go.expressionCustomBlendShapes.Add(value);
                    return true;
                }

                var perfectSyncMorphs = new[]
                {
                    "browDownLeft",
                    "browDownRight",
                    "browInnerUp",
                    "browOuterUpLeft",
                    "browOuterUpRight",
                    "cheekPuff",
                    "cheekSquintLeft",
                    "cheekSquintRight",
                    "eyeBlinkLeft",
                    "eyeBlinkRight",
                    "eyeLookDownLeft",
                    "eyeLookDownRight",
                    "eyeLookInLeft",
                    "eyeLookInRight",
                    "eyeLookOutLeft",
                    "eyeLookOutRight",
                    "eyeLookUpLeft",
                    "eyeLookUpRight",
                    "eyeSquintLeft",
                    "eyeSquintRight",
                    "eyeWideLeft",
                    "eyeWideRight",
                    "jawForward",
                    "jawLeft",
                    "jawOpen",
                    "jawRight",
                    "mouthClose",
                    "mouthDimpleLeft",
                    "mouthDimpleRight",
                    "mouthFrownLeft",
                    "mouthFrownRight",
                    "mouthFunnel",
                    "mouthLeft",
                    "mouthLowerDownLeft",
                    "mouthLowerDownRight",
                    "mouthPressLeft",
                    "mouthPressRight",
                    "mouthPucker",
                    "mouthRight",
                    "mouthRollLower",
                    "mouthRollUpper",
                    "mouthShrugLower",
                    "mouthShrugUpper",
                    "mouthSmileLeft",
                    "mouthSmileRight",
                    "mouthStretchLeft",
                    "mouthStretchRight",
                    "mouthUpperUpLeft",
                    "mouthUpperUpRight",
                    "noseSneerLeft",
                    "noseSneerRight",
                    "tongueOut"
                };

                    var oldLength = go.expressionCustomBlendShapes.Count;
                    List<string> failedMorphs = new List<string>();

                foreach (var morph in perfectSyncMorphs)
                {
                    if (!SetFromPerfectSyncExpression(morph, false))
                    {
                        failedMorphs.Add(morph);
                    }
                }

                var newLength = go.expressionCustomBlendShapes.Count;
                if (newLength - oldLength != 0)
                {
                    Debug.LogWarning($"Failed to set {perfectSyncMorphs.Length} expressions from Perfect Sync. " +
                                     $"Only {newLength - oldLength} expressions were added." +
                                     (failedMorphs.Count > 0
                                         ? $" Failed morphs: {string.Join(", ", failedMorphs)}"
                                         : ""));
                }
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button(Translator._("component.expression.custom.reset-all")))
            {
                go.expressionCustomBlendShapes.Clear();
            }
        }

        private void DrawMToonOptions()
        {
            DrawToggleLeft(Translator._("component.mtoon.enable.rim-light"), _enableMToonRimLightProp);
            DrawToggleLeft(Translator._("component.mtoon.enable.mat-cap"), _enableMToonMatCapProp);
            DrawToggleLeft(Translator._("component.mtoon.enable.outline"), _enableMToonOutlineProp);
            DrawToggleLeft(Translator._("component.mtoon.enable.bake-alpha-mask"), _enableBakingAlphaMaskProp);
        }

        private void DrawSpringBoneOptions()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                _excludedSpringBoneColliderTransformsProp,
                new GUIContent(Translator._("component.spring-bone.exclude.colliders")));
            EditorGUILayout.PropertyField(_excludedSpringBoneTransformsProp,
                new GUIContent(Translator._("component.spring-bone.exclude.bones")));
            EditorGUI.indentLevel--;
        }

        private void DrawConstraintOptions()
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                _excludedConstraintTransformsProp,
                new GUIContent(Translator._("component.constraint.exclude.constraints")));
            EditorGUI.indentLevel--;
        }

        private void DrawDebugOptions()
        {
            DrawToggleLeft(Translator._("component.debug.make-all-node-names-unique"), _makeAllNodeNamesUniqueProp);
            DrawToggleLeft(Translator._("component.debug.enable-vertex-color-output"), _enableVertexColorOutputProp);
            DrawToggleLeft(Translator._("component.debug.disable-vertex-color-on-liltoon"),
                _disableVertexColorOnLiltoonProp);
            DrawToggleLeft(Translator._("component.debug.enable-generate-json"), _enableGenerateJsonFileProp);
            DrawToggleLeft(Translator._("component.debug.delete-temporary-files"), _deleteTemporaryObjectsProp);
            DrawPropertyField(Translator._("component.debug.ktx-tool-path"), _ktxToolPathProp);
        }

        private static void DrawPropertyField(string label, SerializedProperty prop)
        {
            GUILayout.Label(label);
            EditorGUILayout.PropertyField(prop, GUIContent.none);
        }

        private static void DrawToggleLeft(string label, SerializedProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.ToggleLeft(label, prop.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.boolValue = value;
            }
        }
    }

}
#endif // NVE_HAS_NDMF
