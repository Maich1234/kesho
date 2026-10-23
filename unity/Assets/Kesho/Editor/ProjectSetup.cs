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
            // SessionState survives domain reloads, but not a fresh cloud editor process.
            // An existing cached pipeline does not prove the rest of setup previously succeeded.
            if(!SessionState.GetBool("kesho.configured.0.6.1",false))
            {
                Configure(); SessionState.SetBool("kesho.configured.0.6.1",true);
            }
        }
        [MenuItem("Kesho/Configure mobile project")]
        public static void Configure()
        {
            // Normalize and synchronously reimport before loading textures or mutating the pipeline.
            // A file existing on disk is not proof that its cached importer produced a Texture2D.
            var icon=ImportTexture("Assets/Kesho/Branding/app-icon.png",1024);
            ImportTexture("Assets/Kesho/Resources/Art/cast.png",2048);
            foreach(string id in new[]{"music","ambience","click","step","paper","bedding","campus"})
            {
                string path="Assets/Kesho/Resources/Sound/"+id+".ogg";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
                if(!AssetDatabase.LoadAssetAtPath<AudioClip>(path)) throw new BuildFailedException("Failed to import required audio: "+path);
            }
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
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);
            PlayerSettings.WebGL.template="PROJECT:Kesho";
            PlayerSettings.companyName="Wabunifu Labs"; PlayerSettings.productName="Kesho"; PlayerSettings.bundleVersion="0.6.2";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.wabunifu.kesho.unity");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode=8;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            // Legacy input is used deliberately by the first touch controller and uGUI module.
            var playerSettings=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if(playerSettings.Length==0) throw new BuildFailedException("Player settings could not be loaded.");
            var settings=new SerializedObject(playerSettings[0]);
            var input=settings.FindProperty("activeInputHandler"); if(input!=null) { input.intValue=0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            EditorBuildSettings.scenes=new[]{"Campus","Arrival","Courtyard","Room","Office","Dispatch","Night"}.Select(id=>new EditorBuildSettingsScene("Assets/Kesho/Scenes/"+id+".unity",true)).ToArray();
            AssetDatabase.SaveAssets();
            Validate();
        }
        static Texture2D ImportTexture(string path,int size)
        {
            if(!File.Exists(Path.Combine(Application.dataPath,"..",path)))
                throw new BuildFailedException("Required texture file is missing from checkout: "+path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null) throw new BuildFailedException("Required texture has no TextureImporter: "+path);
            importer.textureType=TextureImporterType.Default; importer.textureShape=TextureImporterShape.Texture2D;
            importer.mipmapEnabled=false; importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=size;
            importer.isReadable=false; importer.alphaIsTransparency=true; importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(!texture) throw new BuildFailedException("Required texture exists but failed Texture2D import: "+path);
            return texture;
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
            if(EditorBuildSettings.scenes.Length==0 || EditorBuildSettings.scenes[0].path!="Assets/Kesho/Scenes/Campus.unity" || !EditorBuildSettings.scenes[0].enabled)
                throw new BuildFailedException("Campus must be the enabled startup scene. Run Kesho/Configure mobile project.");
            foreach(var scene in EditorBuildSettings.scenes)
                if(scene.enabled && !File.Exists(Path.Combine(Application.dataPath,"..",scene.path))) throw new BuildFailedException("Missing build scene: "+scene.path);
            Debug.Log("Kesho: "+chapter.nodes.Length+" story beats validated. Voice and device QA are separate gates.");
        }
        [MenuItem("Kesho/Build Android development APK")]
        public static void Android()
        {
            Configure();
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Builds","Android","Kesho-Unity.apk")); Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(), locationPathName=output, target=BuildTarget.Android, options=BuildOptions.Development });
            if(report.summary.result!=BuildResult.Succeeded) throw new BuildFailedException("Kesho Android export failed");
        }
    }
}
