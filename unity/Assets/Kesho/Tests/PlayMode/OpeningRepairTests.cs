using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Kesho.Core;
using Kesho.Runtime;
using Object=UnityEngine.Object;

public sealed class OpeningRepairTests
{
    GameSession session;
    OfflineSave previousSaves, testSaves;
    string directory;
    Chapter chapter;
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    void Call(string method,params object[] args) { typeof(GameSession).GetMethod(method,Private).Invoke(session,args); }
    void Set(string name,object value) { typeof(GameSession).GetField(name,Private).SetValue(session,value); }
    Stage Stage { get { return Object.FindFirstObjectByType<Stage>(); } }
    Button Button(string text)
    {
        var button=session.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.GetComponentInChildren<Text>() && b.GetComponentInChildren<Text>().text==text);
        Assert.IsNotNull(button,"Visible button: "+text); return button;
    }
    bool HasText(string value) { return session.GetComponentsInChildren<Text>().Any(t=>t.text==value); }
    IEnumerator WaitScene(string id)
    {
        float deadline=Time.realtimeSinceStartup+15;
        while(Stage==null || Stage.sceneId!=id) { Assert.Less(Time.realtimeSinceStartup,deadline,"Scene load: "+id); yield return new WaitForSecondsRealtime(.25f); }
        yield return new WaitForSecondsRealtime(.25f);
    }
    LifeState State(string node)
    { return new LifeState { chapter=chapter.id,node=node,rng=12 }; }

    [UnitySetUp] public IEnumerator Setup()
    {
        session=GameSession.Instance; if(session) Call("Menu");
        yield return SceneManager.LoadSceneAsync("Campus"); yield return new WaitForSecondsRealtime(.25f);
        session=GameSession.Instance; Assert.IsNotNull(session);
        chapter=JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text);
        previousSaves=(OfflineSave)typeof(GameSession).GetField("saves",Private).GetValue(session);
        directory=Path.Combine(Path.GetTempPath(),"kesho-repair-"+Guid.NewGuid());
        testSaves=new OfflineSave(directory,chapter); Set("saves",testSaves); Call("Menu");
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        if(session && previousSaves!=null) { Call("Menu"); Set("saves",previousSaves); }
        if(testSaves!=null) testSaves.Flush();
        if(directory!=null && Directory.Exists(directory)) Directory.Delete(directory,true);
        yield return new WaitForSecondsRealtime(.25f);
    }

    [UnityTest] public IEnumerator PaidDirectorResultSurvivesNestedSettingsAndJournalWithoutReapplying()
    {
        var state=State("after_registration"); state.director.pendingEvent="shared_copies"; state.director.pendingOccurrence=LifeDirector.Occurrence(state);
        Call("Begin",state,false); Stage.Player.position=Stage.Targets["rehema"].position+Vector3.forward; session.Interact(); yield return new WaitForSecondsRealtime(.25f);
        var pay=Button("Contribute C10 for blank copies"); pay.onClick.Invoke(); pay.onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        string result=LifeDirector.Find("shared_copies").choices[0].result;
        Assert.AreEqual(1190,session.Engine.State.cash); Assert.AreEqual(1,session.Engine.State.director.history.Count);
        Assert.AreEqual(Stage.Player.position.x,testSaves.Read("checkpoint").posX,.01f,"Checkpoint captures the decision location, not an old autosave pose");
        Assert.IsTrue(HasText(result)); Assert.IsTrue(HasText("Day 1  ·  C1190"),"HUD reflects committed cost");
        long rng=session.Engine.State.rng;
        session.Settings(); yield return new WaitForSecondsRealtime(.25f);
        Button("Your decisions").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        session.Settings(); yield return new WaitForSecondsRealtime(.25f);
        session.Back(); yield return new WaitForSecondsRealtime(.25f); Assert.IsTrue(HasText("What you knew then"));
        session.Back(); yield return new WaitForSecondsRealtime(.25f); Assert.IsTrue(HasText("Sound & comfort"));
        session.Back(); yield return new WaitForSecondsRealtime(.25f); Assert.IsTrue(HasText(result),"Return to the exact result, not the main line");
        Assert.AreEqual(1190,session.Engine.State.cash); Assert.AreEqual(rng,session.Engine.State.rng); Assert.AreEqual(1,testSaves.Read().director.history.Count);
        Button("Continue").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        for(int i=0;i<10 && !HasText(session.Engine.Current.choices[0].label);i++)
        {
            string title=session.GetComponentsInChildren<Button>().Any(b=>b.GetComponentInChildren<Text>() && b.GetComponentInChildren<Text>().text=="Respond")?"Respond":"Continue";
            Button(title).onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        }
        Button(session.Engine.Current.choices[0].label).onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        Assert.AreEqual("family_call",session.Engine.State.node); Assert.AreEqual(1190,session.Engine.State.cash);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest] public IEnumerator CourtyardInspectionRestoresItsOwnDescription()
    {
        Call("Begin",State("courtyard_arrival"),false); yield return WaitScene("Courtyard");
        Stage.Player.position=Stage.Targets["contact"].position;
        Assert.IsTrue(session.TryInspect(Stage.Targets["contact"].position)); yield return new WaitForSecondsRealtime(.25f); yield return new WaitForSecondsRealtime(.25f);
        Assert.IsTrue(HasText("Karibu Court")); Assert.IsFalse(HasText("Kito · knows his way around"));
        session.Settings(); yield return new WaitForSecondsRealtime(.25f); session.Back(); yield return new WaitForSecondsRealtime(.25f);
        Assert.IsTrue(HasText("Karibu Court"));
        Button("Back to exploring").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f); Assert.IsTrue(session.CanExplore);
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest] public IEnumerator SpeakerBindingNeverAnimatesTheInteractionTargetByAccident()
    {
        Stage.PresentSpeaker(StoryPresentation.Speaker(chapter.Find("college_gate"),"Rehema"),"contact");
        Assert.AreEqual("rehema",Stage.SpeakingActorId);
        Assert.IsFalse(Stage.GetComponentsInChildren<ComicActor>().Single(a=>a.name=="Kito").Talking);
        Stage.PresentSpeaker(StoryPresentation.Speaker(chapter.Find("queue"),"Admissions clerk"),"clerk");
        Assert.AreEqual("clerk",Stage.SpeakingActorId);
        Stage.PresentSpeaker(StoryPresentation.Speaker(chapter.Find("queue"),"Student in queue"),"clerk");
        Assert.AreEqual("queued_student",Stage.SpeakingActorId);
        Stage.PresentSpeaker(StoryPresentation.Speaker(chapter.Find("family_call"),"{contact}"),"phone");
        Assert.AreEqual("",Stage.SpeakingActorId); Assert.IsFalse(Stage.GetComponentsInChildren<ComicActor>().Any(a=>a.Talking));
        Stage.PresentSpeaker(StoryPresentation.Speaker(chapter.Find("proof"),"{name}"),"contact");
        Assert.AreEqual("player",Stage.SpeakingActorId); Assert.IsTrue(Stage.Player.GetComponent<ComicActor>().Talking);
        yield return new WaitForSecondsRealtime(.25f); LogAssert.NoUnexpectedReceived();
    }

    [UnityTest] public IEnumerator MorningBeatOverridesLegacyNightSceneLighting()
    {
        Call("Begin",State("morning_message"),false); yield return WaitScene("Night");
        Assert.AreEqual(DayPeriod.Morning,Stage.CurrentPeriod);
        Assert.AreEqual(1f,Shader.GetGlobalFloat("_KeshoExposure"),.001f);
        Assert.Greater(Stage.transform.Find("Window glass").GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor").b,.8f);
        Assert.Greater(Stage.GetComponentInChildren<Light>().intensity,1);
        Stage.Player.position=Stage.Targets["phone"].position+Vector3.forward; session.Interact(); yield return new WaitForSecondsRealtime(.25f);
        Assert.IsFalse(Stage.GetComponentsInChildren<ComicActor>().Any(a=>a.Talking),"A family phone message has no local speaking actor");
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest] public IEnumerator SameSceneCheckpointKeepsTheRestoredPosition()
    {
        Call("Begin",State("college_gate"),false);
        var checkpoint=session.Engine.State.Copy(); checkpoint.poseScene="Campus"; checkpoint.posX=-2; checkpoint.posZ=2;
        testSaves.Write(checkpoint,"checkpoint"); Stage.Player.position=new Vector3(0,0,4);
        session.Settings(); yield return new WaitForSecondsRealtime(.25f); Button("Branch from last checkpoint").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        Assert.Less(Vector3.Distance(Stage.Player.position,new Vector3(-2,0,2)),.01f);
        Assert.AreEqual(-2,testSaves.Read().posX); Assert.AreEqual(2,testSaves.Read().posZ);
        LogAssert.NoUnexpectedReceived();
    }
    [UnityTest] public IEnumerator CrossSceneCheckpointKeepsPositionAndAuthoredLighting()
    {
        Call("Begin",State("college_gate"),false);
        var checkpoint=session.Engine.State.Copy(); checkpoint.node="view_room"; checkpoint.poseScene="Room"; checkpoint.posX=1; checkpoint.posZ=2;
        testSaves.Write(checkpoint,"checkpoint"); session.Settings(); yield return new WaitForSecondsRealtime(.25f);
        Button("Branch from last checkpoint").onClick.Invoke(); yield return WaitScene("Room");
        Assert.Less(Vector3.Distance(Stage.Player.position,new Vector3(1,0,2)),.01f);
        Assert.AreEqual(DayPeriod.Evening,Stage.CurrentPeriod);
        Assert.AreEqual("Room",testSaves.Read().poseScene); Assert.AreEqual(checkpoint.lifeId,session.Engine.State.lifeId);
        LogAssert.NoUnexpectedReceived();
    }
    [UnityTest] public IEnumerator SleepingKeepsThePhysicalRoomPositionAcrossTimeVariants()
    {
        Call("Begin",State("first_sleep"),false); yield return WaitScene("Room");
        var position=Stage.Targets["bed"].position+Vector3.forward*.5f; Stage.Player.position=position;
        Assert.AreEqual(DayPeriod.Night,Stage.CurrentPeriod);
        session.Interact(); yield return new WaitForSecondsRealtime(.25f); Button("Respond").onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        Button(session.Engine.Current.choices[0].label).onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
        Button("Skip transition").onClick.Invoke(); yield return WaitScene("Night");
        Assert.AreEqual(DayPeriod.Morning,Stage.CurrentPeriod); Assert.Less(Vector3.Distance(position,Stage.Player.position),.01f);
        var checkpoint=testSaves.Read("checkpoint"); Assert.AreEqual("Room",checkpoint.poseScene); Assert.AreEqual(position.z,checkpoint.posZ,.01f);
        LogAssert.NoUnexpectedReceived();
    }
}
