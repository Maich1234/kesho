using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Kesho.Core;

static class SaveQueueChecks
{
    public static void Run()
    {
        int checks=0;
        Action<bool,string> check=(ok,message)=> { checks++; if(!ok) throw new Exception(message); };
        using(var entered=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            var written=new List<string>();
            var queue=new OrderedSaveWriter((payload,slot)=> { entered.Set(); if(!release.Wait(5000)) throw new TimeoutException(); written.Add(payload+":"+slot); });
            check(queue.Enqueue("snapshot-1"),"accept autosave");
            try
            {
                check(entered.Wait(5000),"background writer starts");
                check(!queue.Enqueue("snapshot-2"),"bound pending work instead of unbounded disk backlog");
            }
            finally { release.Set(); }
            queue.Flush(); written.Add("critical-newer:life");
            check(written.Count==2 && written[0]=="snapshot-1:life" && written[1]=="critical-newer:life","drain prevents stale autosave overwriting a critical save");
            check(!queue.TryFailure(out _),"successful queue has no failure");
        }
        var failing=new OrderedSaveWriter((payload,slot)=>throw new IOException("disk full"));
        failing.Enqueue("state"); failing.Flush();
        check(failing.TryFailure(out var error) && error is IOException,"background disk failure is observable on owner thread");
        check(!failing.TryFailure(out _),"failure consumed once");
        int retries=0; var recover=new OrderedSaveWriter((payload,slot)=> { if(retries++==0) throw new IOException(); });
        recover.Enqueue("first"); recover.Flush(); recover.Enqueue("second"); recover.Flush();
        check(retries==2 && recover.TryFailure(out _),"failure does not poison future writes or vanish silently");
        Console.WriteLine("PASS: "+checks+" ordered autosave concurrency checks.");
    }
}
