using System;
using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class OfflineSave
    {
        readonly Chapter chapter;
        readonly VerifiedSaveStore store;
        public SaveSource LastReadSource { get { return store.LastReadSource; } }
        public OfflineSave(string directory, Chapter chapter)
        {
            this.chapter=chapter; store=new VerifiedSaveStore(directory,ValidJson);
        }
        bool ValidJson(string json)
        {
            try
            {
                var state=JsonUtility.FromJson<LifeState>(json);
                if(state==null) return false;
                state.Validate(chapter); return true;
            }
            catch(ArgumentException) { return false; }
            catch(NullReferenceException) { return false; }
        }
        public void Write(LifeState state,string slot="life")
        {
            state.Validate(chapter); store.Write(JsonUtility.ToJson(state,true),slot);
        }
        public LifeState Read(string slot="life")
        {
            string json=store.Read(slot); return json==null?null:JsonUtility.FromJson<LifeState>(json);
        }
    }
}
