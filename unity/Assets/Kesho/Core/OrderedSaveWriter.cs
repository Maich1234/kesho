using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace Kesho.Core
{
    /// <summary>One immutable autosave at a time. Critical saves drain it before committing newer state.</summary>
    public sealed class OrderedSaveWriter
    {
        readonly Action<string,string> write;
        readonly ConcurrentQueue<Exception> failures=new ConcurrentQueue<Exception>();
        Task pending=Task.CompletedTask;
        public OrderedSaveWriter(Action<string,string> write) { this.write=write; }
        public bool Enqueue(string payload,string slot="life")
        {
            // Owner/main thread only. Never capture mutable game state or Unity objects in the worker.
            if(!pending.IsCompleted) return false;
            pending=Task.Run(()=> { try { write(payload,slot); } catch(Exception e) { failures.Enqueue(e); } }); return true;
        }
        public void Flush() { pending.GetAwaiter().GetResult(); }
        public bool TryFailure(out Exception error) { return failures.TryDequeue(out error); }
    }
}
