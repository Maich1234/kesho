using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Kesho.Runtime;

public sealed class InteractionTests
{
    int previousMotion;
    [SetUp] public void Setup() { previousMotion=PlayerPrefs.GetInt("kesho.reduce_motion",0); PlayerPrefs.SetInt("kesho.reduce_motion",0); Comfort.Refresh(); }
    [TearDown] public void Cleanup() { PlayerPrefs.SetInt("kesho.reduce_motion",previousMotion); Comfort.Refresh(); }
    [UnityTest] public IEnumerator DuplicateButtonEventsCommitOnlyOnce()
    {
        var root=new GameObject("HUD test",typeof(RectTransform)); var hud=root.AddComponent<StoryHud>();
        try
        {
            hud.Open("Touch guard"); int calls=0;
            var button=hud.Button("Commit",()=>calls++);
            button.onClick.Invoke(); Assert.AreEqual(0,calls,"Invisible panels cannot commit");
            yield return new WaitForSecondsRealtime(.25f);
            button.onClick.Invoke(); button.onClick.Invoke();
            Assert.AreEqual(1,calls,"Duplicate pointer events must not commit twice");
            yield return new WaitForSecondsRealtime(.25f);
            button.onClick.Invoke(); Assert.AreEqual(2,calls,"A later deliberate retry is accepted");
        }
        finally { Object.Destroy(root); }
    }
    [UnityTest] public IEnumerator SaveWarningSurvivesChangingPanels()
    {
        var root=new GameObject("HUD test",typeof(RectTransform)); var hud=root.AddComponent<StoryHud>();
        try
        {
            hud.Notice("Could not save"); hud.Open("Settings"); yield return null;
            var text=root.GetComponentsInChildren<Text>();
            Assert.IsTrue(System.Array.Exists(text,t=>t.text=="Could not save"));
            hud.Close(); Assert.IsTrue(System.Array.Exists(root.GetComponentsInChildren<Text>(),t=>t.text=="Could not save"));
        }
        finally { Object.Destroy(root); }
    }
}
