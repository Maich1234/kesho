using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Kesho.Core;
using Kesho.Runtime;

public sealed class OpeningTests
{
    Chapter Chapter() { return JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text); }
    [Test] public void UnityDeserializerLoadsTheAuthoredGraph() { var c=Chapter(); c.Validate(); Assert.AreEqual("college_gate",c.start); }
    [Test] public void SaveRecoversBackupAndKeepsRandomState()
    {
        string path=Path.Combine(Path.GetTempPath(),"kesho-test-"+Guid.NewGuid());
        try
        {
            var chapter=Chapter(); var engine=new StoryEngine(chapter); var save=new OfflineSave(path,chapter);
            save.Write(engine.State); engine.Apply("college_gate","mentor"); save.Write(engine.State);
            Assert.AreEqual("mentor",save.Read().node);
            File.WriteAllText(Path.Combine(path,"life.json"),"broken");
            Assert.AreEqual("college_gate",save.Read().node);
        }
        finally { if(Directory.Exists(path)) Directory.Delete(path,true); }
    }
    [Test] public void SaveRejectsTruncationAndKeepsLifeIdentity()
    {
        string path=Path.Combine(Path.GetTempPath(),"kesho-test-"+Guid.NewGuid());
        try
        {
            var chapter=Chapter(); var engine=new StoryEngine(chapter); var save=new OfflineSave(path,chapter);
            save.Write(engine.State); Assert.AreEqual(engine.State.lifeId,save.Read().lifeId);
            File.WriteAllText(Path.Combine(path,"life.json"),"{\"node\":\"arrival\"}");
            Assert.IsNull(save.Read(),"Partial JSON must not silently fill fresh-life defaults");
        }
        finally { if(Directory.Exists(path)) Directory.Delete(path,true); }
    }
    [Test] public void PermanentLivesCannotUseCheckpoints()
    {
        var engine=new StoryEngine(Chapter()); var checkpoint=engine.State.Copy(); engine.State.permanent=true;
        Assert.Throws<InvalidOperationException>(()=>engine.RestoreCheckpoint(checkpoint));
    }
    [Test] public void UnitySaveKeepsPendingDirectorSelectionAndPrivateMemory()
    {
        string path=Path.Combine(Path.GetTempPath(),"kesho-director-"+Guid.NewGuid());
        try
        {
            var chapter=Chapter(); var state=new LifeState { chapter=chapter.id,node="morning_message",rng=12 };
            state.flags.Add("concern_recorded");
            state.director.Remember("support","slip_check/report");
            var engine=new StoryEngine(chapter,state); Assert.IsTrue(engine.PrepareMoment());
            var save=new OfflineSave(path,chapter); save.Write(engine.State);
            var restored=new StoryEngine(chapter,save.Read());
            Assert.AreEqual(engine.State.director.pendingEvent,restored.State.director.pendingEvent);
            Assert.AreEqual(engine.State.rng,restored.State.rng);
            Assert.IsNull(restored.State.director.Find("kito"));
            string occurrence=restored.State.director.pendingOccurrence;
            Assert.IsTrue(restored.ResolveMoment(occurrence,"appointment",out string result));
            save.Write(restored.State); var after=save.Read();
            Assert.AreEqual(1,after.director.history.Count);
            Assert.IsTrue(after.director.Find("support").facts.Contains("support_appointment"));
            Assert.IsFalse(restored.ResolveMoment(occurrence,"appointment",out result));
        }
        finally { if(Directory.Exists(path)) Directory.Delete(path,true); }
    }
    [Test] public void LegacyUnityJsonWithoutDirectorKeepsLifeIdentity()
    {
        var chapter=Chapter(); var original=new LifeState { chapter=chapter.id,node=chapter.start };
        original.director=null;
        string json=JsonUtility.ToJson(original);
        // Old saves omit the entire field; construct the legacy shape from this JSON, not a fresh life.
        int start=json.IndexOf(",\"director\":",StringComparison.Ordinal);
        Assert.Greater(start,0);
        json=json.Substring(0,start)+"}";
        var engine=new StoryEngine(chapter,JsonUtility.FromJson<LifeState>(json));
        Assert.AreEqual(original.lifeId,engine.State.lifeId);
        Assert.AreEqual(2,engine.State.director.version);
        Assert.AreEqual(0,engine.State.director.history.Count);
    }
    [Test] public void VersionOneDirectorCorrectsPrivateKnowledgeWithoutErasingTheLife()
    {
        var chapter=Chapter(); var state=new LifeState { chapter=chapter.id,node=chapter.start };
        state.director.version=1; state.director.Remember("kito","message_preserved"); state.director.Remember("kito","fixer_offer/pay",2,20);
        var restored=new StoryEngine(chapter,JsonUtility.FromJson<LifeState>(JsonUtility.ToJson(state)));
        Assert.AreEqual(state.lifeId,restored.State.lifeId); Assert.AreEqual(2,restored.State.director.version);
        Assert.IsFalse(restored.State.director.Find("kito").facts.Contains("message_preserved"));
        Assert.IsTrue(restored.State.director.Find("kito").facts.Contains("fixer_offer/pay"));
        Assert.AreEqual(20,restored.State.director.Find("kito").obligation);
    }
}
