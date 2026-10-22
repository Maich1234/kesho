using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Kesho.Core;

namespace Kesho.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        static ProjectSetup() { EditorApplication.delayCall += FirstImport; }
        static void FirstImport()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=FirstImport; return; }
            if(GraphicsSettings.defaultRenderPipeline==null) Configure();
        }
        [MenuItem("Kesho/Configure mobile project")]
        public static void Configure()
        {
            EnsureAssetFolder("Assets/Kesho/Settings");
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Kesho/Settings/MobileRenderer.asset");
            if(!renderer) { renderer=ScriptableObject.CreateInstance<UniversalRendererData>(); AssetDatabase.CreateAsset(renderer,"Assets/Kesho/Settings/MobileRenderer.asset"); }
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Kesho/Settings/MobilePipeline.asset");
            if(!pipeline)
            {
                pipeline=ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                var serialized=new SerializedObject(pipeline); var list=serialized.FindProperty("m_RendererDataList");
                if(list==null) throw new BuildFailedException("Pinned URP renderer configuration was not found.");
                list.arraySize=1; list.GetArrayElementAtIndex(0).objectReferenceValue=renderer; serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(pipeline,"Assets/Kesho/Settings/MobilePipeline.asset");
            }
            pipeline.renderScale=.85f; pipeline.msaaSampleCount=2; pipeline.shadowDistance=25;
            GraphicsSettings.defaultRenderPipeline=pipeline;
            QualitySettings.renderPipeline=pipeline; QualitySettings.vSyncCount=0;
            // Resources reference keeps URP/Lit available for the runtime scene blockouts.
            EnsureAssetFolder("Assets/Kesho/Resources/Materials");
            if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Kesho/Resources/Materials/Blockout.mat"))
                AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/Kesho/Resources/Materials/Blockout.mat");
            var portraitImporter=AssetImporter.GetAtPath("Assets/Kesho/Resources/Art/cast.png") as TextureImporter;
            if(portraitImporter!=null && (portraitImporter.mipmapEnabled || portraitImporter.npotScale!=TextureImporterNPOTScale.None))
            {
                portraitImporter.mipmapEnabled=false; portraitImporter.npotScale=TextureImporterNPOTScale.None;
                portraitImporter.maxTextureSize=2048; portraitImporter.isReadable=false; portraitImporter.SaveAndReimport();
            }
            PlayerSettings.companyName="Wabunifu Labs"; PlayerSettings.productName="Kesho"; PlayerSettings.bundleVersion="0.5.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.wabunifu.kesho.unity");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode=4;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            // Legacy input is used deliberately by the first touch controller and uGUI module.
            var playerSettings=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if(playerSettings.Length==0) throw new BuildFailedException("Player settings could not be loaded.");
            var settings=new SerializedObject(playerSettings[0]);
            var input=settings.FindProperty("activeInputHandler"); if(input!=null) { input.intValue=0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            EditorBuildSettings.scenes=new[]{"Campus","Arrival","Courtyard","Room","Office","Dispatch","Night"}.Select(id=>new EditorBuildSettingsScene("Assets/Kesho/Scenes/"+id+".unity",true)).ToArray();
            AssetDatabase.SaveAssets();
        }
        static void EnsureAssetFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path)) return;
            string parent=Path.GetDirectoryName(path).Replace('\\','/'); EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
        [MenuItem("Kesho/Validate opening content")]
        public static void Validate()
        {
            var chapter=JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text); chapter.Validate();
            Debug.Log("Kesho: "+chapter.nodes.Length+" story beats validated. Voice and device QA are separate gates.");
        }
        [MenuItem("Kesho/Build Android development APK")]
        public static void Android()
        {
            Configure(); Validate(); Directory.CreateDirectory("Builds/Android");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Select(s=>s.path).ToArray(), locationPathName="Builds/Android/Kesho-Unity.apk", target=BuildTarget.Android, options=BuildOptions.Development });
            if(report.summary.result!=BuildResult.Succeeded) throw new BuildFailedException("Kesho Android export failed");
        }
    }
}
