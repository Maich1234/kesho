using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Kesho.Runtime;

public sealed class PolishTests
{
    int previousMotion;
    [SetUp] public void Setup() { previousMotion=PlayerPrefs.GetInt("kesho.reduce_motion",0); PlayerPrefs.SetInt("kesho.reduce_motion",0); Comfort.Refresh(); }
    [TearDown] public void Cleanup() { PlayerPrefs.SetInt("kesho.reduce_motion",previousMotion); Comfort.Refresh(); }
    [UnityTest] public IEnumerator DialoguePoolReusesShellAndReplacesCallbacks()
    {
        var root=new GameObject("Pool regression",typeof(RectTransform)); var hud=root.AddComponent<StoryHud>();
        try
        {
            int oldCalls=0,newCalls=0; hud.Open("First"); var shell=hud.Panel;
            var first=hud.Button("Old",()=>oldCalls++); yield return new WaitForSecondsRealtime(.25f);
            first.onClick.Invoke(); Assert.AreEqual(1,oldCalls);
            hud.Open("Second"); var second=hud.Button("New",()=>newCalls++);
            Assert.AreSame(shell,hud.Panel); Assert.AreSame(first,second);
            second.onClick.Invoke(); Assert.AreEqual(0,newCalls,"An invisible replacement cannot accept taps");
            yield return new WaitForSecondsRealtime(.25f); second.onClick.Invoke(); second.onClick.Invoke();
            Assert.AreEqual(1,newCalls); Assert.AreEqual(1,oldCalls,"Pooled controls remove old callbacks");
            hud.Close(); Assert.IsFalse(hud.PanelOpen);
        }
        finally { Object.Destroy(root); }
    }
    [UnityTest] public IEnumerator CustomisationCyclePreservesPanelAndField()
    {
        var root=new GameObject("Form regression",typeof(RectTransform)); var hud=root.AddComponent<StoryHud>();
        try
        {
            hud.Open("Create"); var shell=hud.Panel; hud.Field("Nia",_=>{}); int hair=0;
            hud.CycleButton(()=>"Hair "+hair,()=>hair++); var field=root.GetComponentInChildren<InputField>();
            yield return new WaitForSecondsRealtime(.25f);
            root.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>() && b.GetComponentInChildren<Text>().text=="Hair 0").onClick.Invoke();
            Assert.AreSame(shell,hud.Panel); Assert.AreSame(field,root.GetComponentInChildren<InputField>()); Assert.AreEqual(1,hair);
        }
        finally { Object.Destroy(root); }
    }
    [UnityTest] public IEnumerator MuteImmediatelySilencesEveryActiveSource()
    {
        var root=new GameObject("Audio regression"); var audio=root.AddComponent<Soundscape>(); var previous=audio.Volume;
        try
        {
            audio.Cue("click",Vector3.zero); audio.SetVolume(0);
            Assert.IsTrue(root.GetComponentsInChildren<AudioSource>().All(s=>s.mute && s.volume==0));
            yield return null; Assert.IsTrue(root.GetComponentsInChildren<AudioSource>().All(s=>s.mute));
            audio.SetVolume(.5f); Assert.IsTrue(root.GetComponentsInChildren<AudioSource>().All(s=>!s.mute));
        }
        finally { audio.SetVolume(previous); Object.Destroy(root); }
    }
    [UnityTest] public IEnumerator BedPoseFacesUpAndStaysWithinTheMattress()
    {
        yield return SceneManager.LoadSceneAsync("Room"); yield return null;
        var stage=Object.FindFirstObjectByType<Stage>(); stage.RestPose(true);
        var actor=stage.Player.GetComponent<ComicActor>();
        Assert.IsTrue(actor.Resting); Assert.Greater(Vector3.Dot(stage.Player.forward,Vector3.up),.99f);
        var head=stage.Player.Find("Head").position;
        Assert.That(head.z,Is.InRange(-4f,0f)); Assert.AreEqual(-4,head.x,.01f);
        Assert.IsFalse(stage.Player.Find("Grounding shadow").gameObject.activeSelf);
        stage.RestPose(false); Assert.IsFalse(actor.Resting); Assert.AreEqual(0,stage.Player.position.y,.01f);
    }
    [UnityTest] public IEnumerator OccupiedPeopleRemainReachableWithoutWalkingThroughThem()
    {
        yield return SceneManager.LoadSceneAsync("Campus"); yield return null;
        var stage=Object.FindFirstObjectByType<Stage>(); var walker=stage.Player.GetComponent<StreetWalker>();
        Assert.IsFalse(stage.Walkable(new Vector2Int(4,3)),"Kito occupies this cell");
        Assert.IsFalse(stage.Walkable(new Vector2Int(-4,1)),"Rehema occupies this cell");
        foreach(var target in stage.Targets) { Assert.IsTrue(walker.WalkTo(target.Value.position),target.Key); walker.Stop(); }
        foreach(var actor in stage.GetComponentsInChildren<ComicActor>())
            Assert.LessOrEqual(actor.GetComponentsInChildren<MeshRenderer>().Length,17,"Rigid joint batches reduce renderer fragmentation");
        float before=stage.View.orthographicSize; stage.Conversation("contact",true);
        Assert.AreEqual(before,stage.View.orthographicSize,"Zoom target changes without instant size jump");
        yield return new WaitForSecondsRealtime(.25f);
        Assert.Less(stage.View.orthographicSize,before); Assert.Greater(stage.View.orthographicSize,5.69f);
        LogAssert.NoUnexpectedReceived();
    }
}
