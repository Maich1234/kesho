using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Kesho.Core;

static class Program
{
    static int checks;
    static Chapter chapter;
    static void Check(bool value,string name) { checks++; if(!value) throw new Exception("FAIL: "+name); }
    static StoryEngine Fresh(long seed=12) { return new StoryEngine(chapter,new LifeState { node=chapter.start,rng=seed }); }
    static void Pick(StoryEngine engine,string id) { Check(engine.Apply(engine.State.node,id),"choose "+id+" at "+engine.State.node); }
    static void ExpectThrow(Action action,string name) { bool threw=false; try { action(); } catch(ArgumentException) { threw=true; } catch(InvalidOperationException) { threw=true; } Check(threw,name); }
    static void Main(string[] args)
    {
        string path=args.Length>0?args[0]:"unity/Assets/Kesho/Resources/Story/opening.json";
        chapter=JsonSerializer.Deserialize<Chapter>(File.ReadAllText(path),new JsonSerializerOptions { IncludeFields=true }); chapter.Validate();
        Check(chapter.nodes.Length==31,"31 connected beats");
        foreach(var node in chapter.nodes)
        {
            foreach(var line in node.lines) Check(line.text.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length<=35,"short spoken line "+line.voice);
            Check(node.choices.Length<=3,"no more than three choices "+node.id);
        }
        var engine=Fresh(); var original=engine.State.Copy();
        Pick(engine,"warm"); Check(!engine.Apply("arrival","warm"),"duplicate input cannot recommit");
        Check(engine.State.trust==1,"one relationship change");
        engine.RestoreCheckpoint(original); Check(engine.State.node=="arrival","checkpoint restores beat");
        original.flags.Add("external_mutation"); Check(!engine.State.Has("external_mutation"),"restore copies collections");
        var another=Fresh(); ExpectThrow(()=>engine.RestoreCheckpoint(another.State),"cross-life rewind forbidden");
        engine.State.permanent=true; ExpectThrow(()=>engine.RestoreCheckpoint(original),"permanent mode cannot rewind");
        var malformed=Fresh().State; malformed.cash=-1; ExpectThrow(()=>new StoryEngine(chapter,malformed),"negative money rejected");
        malformed=Fresh().State; malformed.activeSeconds=double.NaN; ExpectThrow(()=>new StoryEngine(chapter,malformed),"invalid timer rejected");
        malformed=Fresh().State; malformed.poseScene="Other"; ExpectThrow(()=>new StoryEngine(chapter,malformed),"unknown location rejected");
        engine=Fresh(); engine.State.node="interview"; engine.State.cash=100;
        Check(!engine.Apply("interview","pay")&&engine.State.cash==100,"unaffordable fee cannot mutate state");
        engine=Fresh(); engine.State.node="second_sleep"; engine.State.flags.Add("accepted_deal"); original=engine.State.Copy();
        Pick(engine,"rest"); string outcome=engine.State.flags.Last(); long after=engine.State.rng;
        engine.RestoreCheckpoint(original); Pick(engine,"rest"); Check(engine.State.flags.Last()==outcome&&engine.State.rng==after,"reload preserves stochastic outcome");
        var outcomes=new HashSet<string>();
        for(int seed=1;seed<=200;seed++)
        {
            engine=Fresh(seed); engine.State.node="second_sleep"; engine.State.flags.Add("accepted_deal"); engine.Apply("second_sleep","rest"); outcomes.Add(engine.State.flags.Last());
            Check(engine.VisibleLines().Length==1,"night does not leak other branches");
        }
        Check(outcomes.SetEquals(new[]{"collector_visits","collector_calls"}),"different seeds produce both plausible deal outcomes");
        engine=Fresh(); engine.State.node="second_sleep"; engine.State.flags.Add("copied_ledger"); Pick(engine,"rest"); Check(engine.State.Has("witness_visits"),"investigation yields corroborating witness");
        engine=Fresh(); engine.State.node="second_sleep"; Pick(engine,"rest"); Check(engine.State.Has("contact_warns"),"ordinary path has its own consequence");
        Check(!DialogueGuard.Accept("I know the secret",new[]{"unearned_secret"},engine.State),"unearned facts rejected");
        Check(!DialogueGuard.Accept(new string('x',221),new string[0],engine.State),"side dialogue length bounded");
        Check(DialogueGuard.Accept("We should talk.",new[]{"contact_warns"},engine.State),"known facts permitted");
        Check(PaceDirector.NextMood(engine.State,true,false)=="quiet","dialogue cannot trigger competing pressure cue");
        int terminalPaths=0; var reached=new HashSet<string>();
        foreach(string background in WorldCatalog.Backgrounds)
        {
            engine=Fresh(123); engine.State.background=background;
            var queue=new Queue<LifeState>(); queue.Enqueue(engine.State.Copy());
            while(queue.Count>0)
            {
                var state=queue.Dequeue(); var branch=new StoryEngine(chapter,state); reached.Add(state.node);
                Check(state.cash>=0 && state.day<=2,"valid economy/calendar on every authored path");
                if(branch.Current.terminal) { terminalPaths++; Check(state.Has("education_path")||state.Has("work_path"),"ending retains a viable next path"); continue; }
                int available=0;
                foreach(var choice in branch.Current.choices)
                {
                    if(!branch.Available(choice)) continue; available++;
                    var child=new StoryEngine(chapter,state); Check(child.Apply(state.node,choice.id),"reachable transition applies");
                    Check(child.State.decisions.Count<32,"no padded loop"); queue.Enqueue(child.State.Copy());
                }
                Check(available>0,"no blocked life");
            }
        }
        Check(reached.Count==chapter.nodes.Length,"all beats exercised through real transitions");
        Console.WriteLine("PASS: "+checks+" assertions; "+terminalPaths+" complete branch paths across all backgrounds. Core only; Unity rendering/device QA pending.");
    }
}
