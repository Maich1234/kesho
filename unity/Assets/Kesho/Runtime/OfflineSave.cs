using Unity.Profiling;
using System;
using UnityEngine;
using Kesho.Core;

namespace Kesho.Runtime
{
    public sealed class OfflineSave
    {
        static readonly ProfilerMarker profileAutosave=new ProfilerMarker("Kesho.OfflineSave.Autosave");
        static readonly ProfilerMarker profileWrite=new ProfilerMarker("Kesho.OfflineSave.Write");
        readonly Chapter chapter;
        readonly VerifiedSaveStore store;
        readonly OrderedSaveWriter writer;
        public SaveSource LastReadSource { get { return store.LastReadSource; } }
        public OfflineSave(string directory, Chapter chapter)
        {
            this.chapter=chapter; store=new VerifiedSaveStore(directory,ValidJson); writer=new OrderedSaveWriter(store.Write);
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
            using(profileWrite.Auto())
            {
                writer.Flush(); state.Validate(chapter); store.Write(JsonUtility.ToJson(state),slot);
                if(slot=="life") while(writer.TryFailure(out _)) { }
            }
        }
        public bool Autosave(LifeState state)
        {
            using(profileAutosave.Auto())
            {
            #if UNITY_WEBGL && !UNITY_EDITOR
                // Browser builds have no worker threads; retain durable synchronous fallback.
                Write(state); return true;
#else
                state.Validate(chapter); return writer.Enqueue(JsonUtility.ToJson(state));
#endif
            }
        }
        public void Flush() { writer.Flush(); }
        public bool TryFailure(out Exception error) { return writer.TryFailure(out error); }
        public LifeState Read(string slot="life")
        {
            writer.Flush(); string json=store.Read(slot); return json==null?null:JsonUtility.FromJson<LifeState>(json);
        }
    }
}
