using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class AssetImportTests
{
    [TestCase("Assets/Kesho/Branding/app-icon.png",1024)]
    [TestCase("Assets/Kesho/Resources/Art/cast.png",512)]
    public void RequiredPngImportsAsTwoDimensionalTexture(string path,int minimumSize)
    {
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        Assert.IsNotNull(importer,"Texture importer: "+path);
        Assert.AreEqual(TextureImporterType.Default,importer.textureType);
        Assert.AreEqual(TextureImporterShape.Texture2D,importer.textureShape);
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Assert.IsNotNull(texture,"Imported Texture2D: "+path);
        Assert.GreaterOrEqual(texture.width,minimumSize); Assert.GreaterOrEqual(texture.height,minimumSize);
        Assert.IsFalse(importer.mipmapEnabled); Assert.IsFalse(importer.isReadable);
    }
    [TestCase("music")][TestCase("ambience")][TestCase("click")][TestCase("step")]
    [TestCase("paper")][TestCase("bedding")][TestCase("campus")]
    public void RequiredSoundHasAUsableAudioImporterAndClip(string id)
    {
        string path="Assets/Kesho/Resources/Sound/"+id+".ogg";
        Assert.IsInstanceOf<AudioImporter>(AssetImporter.GetAtPath(path),path);
        var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        Assert.IsNotNull(clip,path); Assert.Greater(clip.length,0); Assert.Greater(clip.samples,0);
    }
}
