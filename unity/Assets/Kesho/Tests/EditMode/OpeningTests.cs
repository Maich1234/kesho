using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Kesho.Core;
using Kesho.Runtime;

public sealed class OpeningTests
{
    Chapter Chapter() { return JsonUtility.FromJson<Chapter>(Resources.Load<TextAsset>("Story/opening").text); }
    [Test] public void UnityDeserializerLoadsTheAuthoredGraph() { var c=Chapter(); c.Validate(); Assert.AreEqual(31,c.nodes.Length); }
    [Test] public void SaveRecoversBackupAndKeepsRandomState()
    {
        string path=Path.Combine(Path.GetTempPath(),"kesho-test-"+Guid.NewGuid());
        try
        {
            var chapter=Chapter(); var engine=new StoryEngine(chapter); var save=new OfflineSave(path,chapter);
            save.Write(engine.State); engine.Apply("arrival","warm"); save.Write(engine.State);
            Assert.AreEqual("family",save.Read().node);
            File.WriteAllText(Path.Combine(path,"life.json"),"broken");
            Assert.AreEqual("arrival",save.Read().node);
        }
        finally { if(Directory.Exists(path)) Directory.Delete(path,true); }
    }
    [Test] public void PermanentLivesCannotUseCheckpoints()
    {
        var engine=new StoryEngine(Chapter()); var checkpoint=engine.State.Copy(); engine.State.permanent=true;
        Assert.Throws<InvalidOperationException>(()=>engine.RestoreCheckpoint(checkpoint));
    }
}
