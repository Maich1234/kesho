using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Kesho.Core;
using Kesho.Runtime;

public sealed class CollegeSceneTests
{
    [UnityTest] public IEnumerator CampusMeshesAndAllHotspotsSurviveSceneLoad()
    {
        yield return SceneManager.LoadSceneAsync("Campus");
        var stage=Object.FindFirstObjectByType<Stage>(); Assert.IsNotNull(stage);
        var walker=stage.Player.GetComponent<StreetWalker>();
        foreach(var target in stage.Targets)
        {
            Assert.IsTrue(walker.WalkTo(target.Value.position),"Reachable hotspot: "+target.Key); walker.Stop();
        }
        Assert.AreEqual(0,stage.GetComponentsInChildren<Collider>().Length,"Explicit art meshes need no implicit stripped colliders");
        Assert.IsNotNull(stage.Player.Find("Head"));
        Assert.IsNotNull(stage.Player.Find("Left arm"));
        Assert.Greater(stage.GetComponentsInChildren<MeshFilter>().Length,30,"Campus includes authored geometry");
        var art=Resources.Load<Texture2D>("Art/cast"); Assert.IsNotNull(art); Assert.GreaterOrEqual(art.width,512);
        var shader=Resources.Load<Shader>("Shaders/Comic"); Assert.IsNotNull(shader);
        if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null) Assert.IsTrue(shader.isSupported);
        LogAssert.NoUnexpectedReceived();
    }
    [UnityTest] public IEnumerator EveryHairstyleCanBeChangedWithoutDuplicateVisibleHair()
    {
        yield return SceneManager.LoadSceneAsync("Campus"); var stage=Object.FindFirstObjectByType<Stage>();
        var life=new LifeState();
        for(int hair=0;hair<WorldCatalog.Hairstyles.Length;hair++)
        {
            life.hair=hair; life.skin=hair%4; stage.ApplyAppearance(life); yield return null;
            int count=0; foreach(Transform child in stage.Player.Find("Head")) if(child.name=="Hairstyle"&&child.gameObject.activeSelf) count++;
            Assert.AreEqual(1,count);
        }
        LogAssert.NoUnexpectedReceived();
    }
}
