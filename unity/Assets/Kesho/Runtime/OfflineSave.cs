using System;
using System.IO;
using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class OfflineSave
    {
        readonly string directory;
        readonly Chapter chapter;
        public OfflineSave(string directory,Chapter chapter) { this.directory=directory; this.chapter=chapter; }
        public void Write(LifeState state,string slot="life")
        {
            if(slot!="life"&&slot!="checkpoint") throw new ArgumentException("Unknown slot");
            state.Validate(chapter); Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,slot+".json"), temp=path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(state,true));
            JsonUtility.FromJson<LifeState>(File.ReadAllText(temp)).Validate(chapter);
            if(File.Exists(path))
            {
                try { File.Replace(temp,path,path+".bak"); }
                catch(PlatformNotSupportedException) { FallbackReplace(temp,path); }
            }
            else File.Move(temp,path);
        }
        static void FallbackReplace(string temp,string path)
        {
            File.Copy(path,path+".bak",true); File.Delete(path); File.Move(temp,path);
        }
        public LifeState Read(string slot="life")
        {
            if(slot!="life"&&slot!="checkpoint") throw new ArgumentException("Unknown slot");
            string path=Path.Combine(directory,slot+".json");
            foreach(string candidate in new[]{path,path+".tmp",path+".bak"})
            {
                if(!File.Exists(candidate)) continue;
                try { var state=JsonUtility.FromJson<LifeState>(File.ReadAllText(candidate)); if(state==null) continue; state.Validate(chapter); return state; }
                catch(Exception e) when(e is IOException || e is ArgumentException || e is NullReferenceException) { Debug.LogWarning("Invalid save candidate skipped: "+Path.GetFileName(candidate)); }
            }
            return null;
        }
    }
}
