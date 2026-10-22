using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Kesho.Core;

static class Program
{
    static int checks; static Chapter chapter;
    static void Check(bool value,string name) { checks++; if(!value) throw new Exception("FAIL: "+name); }
    static StoryEngine Fresh(long seed=12) { return new StoryEngine(chapter,new LifeState { chapter=chapter.id,node=chapter.start,rng=seed }); }
    static void Pick(StoryEngine e,string id) { Check(e.Apply(e.State.node,id),"choose "+id+" at "+e.State.node); }
    static void ExpectThrow(Action action,string name) { bool threw=false; try { action(); } catch(ArgumentException) { threw=true; } catch(InvalidOperationException) { threw=true; } Check(threw,name); }
    static void Main(string[] args)
    {
        StorageChecks.Run();
        string path=args.Length>0?args[0]:"unity/Assets/Kesho/Resources/Story/opening.json";
        chapter=JsonSerializer.Deserialize<Chapter>(File.ReadAllText(path),new JsonSerializerOptions { IncludeFields=true }); chapter.Validate();
        DirectorChecks.Run(chapter);
        Check(chapter.start=="college_gate" && chapter.Find(chapter.start).scene=="Campus","opens at college");
        foreach(var n in chapter.nodes)
        {
            foreach(var line in n.lines) Check(line.text.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length<=35,"short spoken line "+line.voice);
            Check(n.choices.Length<=3,"no more than three responses "+n.id);
        }
        var engine=Fresh(); var original=engine.State.Copy(); Pick(engine,"mentor");
        Check(!engine.Apply(chapter.start,"mentor") && engine.State.trust==1,"duplicate tap cannot mutate twice");
        engine.RestoreCheckpoint(original); Check(engine.State.node==chapter.start,"checkpoint restores opening");
        original.flags.Add("external_mutation"); Check(!engine.State.Has("external_mutation"),"restored collections isolated");
        ExpectThrow(()=>engine.RestoreCheckpoint(Fresh().State),"cross-life rewind forbidden");
        engine.State.permanent=true; ExpectThrow(()=>engine.RestoreCheckpoint(original),"permanent mode cannot rewind");
        var malformed=Fresh().State; malformed.cash=-1; ExpectThrow(()=>new StoryEngine(chapter,malformed),"negative money rejected");
        malformed=Fresh().State; malformed.activeSeconds=double.NaN; ExpectThrow(()=>new StoryEngine(chapter,malformed),"invalid timer rejected");
        malformed=Fresh().State; malformed.poseScene="Other"; ExpectThrow(()=>new StoryEngine(chapter,malformed),"unknown location rejected");
        malformed=Fresh().State; malformed.hair=5; ExpectThrow(()=>new StoryEngine(chapter,malformed),"unknown hairstyle rejected");
        malformed=Fresh().State; malformed.character="Unknown"; ExpectThrow(()=>new StoryEngine(chapter,malformed),"unknown character rejected");
        foreach(string character in WorldCatalog.Characters) for(int hair=0;hair<5;hair++)
        {
            var state=Fresh().State; state.character=character; state.hair=hair; state.Validate(chapter);
            var copy=JsonSerializer.Deserialize<LifeState>(JsonSerializer.Serialize(state,new JsonSerializerOptions { IncludeFields=true }),new JsonSerializerOptions { IncludeFields=true });
            copy.Validate(chapter); Check(copy.character==character&&copy.hair==hair,"appearance survives serialization");
        }
        engine=Fresh(); engine.State.node="fixer_offer"; engine.State.cash=100;
        Check(!engine.Apply("fixer_offer","pay")&&engine.State.cash==100,"unaffordable shortcut cannot charge");
        engine=Fresh(); engine.State.node="first_sleep"; engine.State.flags.Add("accepted_deal"); original=engine.State.Copy();
        Pick(engine,"rest"); string outcome=engine.State.flags.Last(); long after=engine.State.rng;
        engine.RestoreCheckpoint(original); Pick(engine,"rest"); Check(engine.State.flags.Last()==outcome&&engine.State.rng==after,"checkpoint does not reroll consequences");
        Check(engine.State.day==2,"sleep advances the calendar once");
        var outcomes=new HashSet<string>();
        for(int seed=1;seed<=200;seed++)
        {
            engine=Fresh(seed); engine.State.node="first_sleep"; engine.State.flags.Add("accepted_deal"); Pick(engine,"rest"); outcomes.Add(engine.State.flags.Last());
            Check(engine.VisibleLines().Length==1,"morning only reveals this branch");
        }
        Check(outcomes.SetEquals(new[]{"collector_visits","collector_calls"}),"both plausible fixer outcomes reached");
        engine=Fresh(); engine.State.node="first_sleep"; engine.State.flags.Add("copied_ledger"); Pick(engine,"rest"); Check(engine.State.Has("witness_visits"),"investigation creates witness route");
        engine=Fresh(); engine.State.node="first_sleep"; Pick(engine,"rest"); Check(engine.State.Has("contact_warns"),"official route has distinct follow-up");
        Check(!DialogueGuard.Accept("Secret",new[]{"unearned_secret"},engine.State),"unknown facts rejected");
        Check(!DialogueGuard.Accept(new string('x',221),Array.Empty<string>(),engine.State),"AI side line length bounded");
        Check(DialogueGuard.Accept("We should talk.",new[]{"contact_warns"},engine.State),"known facts accepted");
        Check(PaceDirector.NextMood(engine.State,true,false)=="quiet","conversation ducks pressure cues");
        int paths=0; var reached=new HashSet<string>();
        foreach(string background in WorldCatalog.Backgrounds)
        {
            engine=Fresh(123); engine.State.background=background; var queue=new Queue<LifeState>(); queue.Enqueue(engine.State.Copy());
            while(queue.Count>0)
            {
                var state=queue.Dequeue(); var branch=new StoryEngine(chapter,state); reached.Add(state.node);
                Check(state.cash>=0 && state.day<=2,"valid money and calendar on every branch");
                Check(branch.VisibleLines().Length>0,"no empty conversation on reachable state");
                if(branch.Current.terminal)
                {
                    paths++; Check(state.Has("education_path"),"every completion has a college place");
                    Check(state.Has("shared_room")||state.Has("rented_room"),"every completion secures a bed");
                    Check(state.Has("fees_paid")||state.Has("fees_deferred"),"fees settled or explicitly deferred"); continue;
                }
                int available=0;
                foreach(var c in branch.Current.choices)
                {
                    if(!branch.Available(c)) continue; available++;
                    var child=new StoryEngine(chapter,state); Check(child.Apply(state.node,c.id),"reachable transition applies");
                    Check(child.State.decisions.Count<chapter.nodes.Length,"no padded loop"); queue.Enqueue(child.State.Copy());
                }
                Check(available>0,"no soft lock from funds or flags");
            }
        }
        Check(reached.Count==chapter.nodes.Length,"all authored beats covered");
        Console.WriteLine("PASS: "+checks+" assertions; "+paths+" complete paths across all backgrounds; "+reached.Count+" beats. Unity render/device QA remains separate.");
    }
}
