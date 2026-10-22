using System;
using System.Linq;
using System.Text.Json;
using Kesho.Core;

static class DirectorChecks
{
    static int checks;
    static void Check(bool value,string label) { checks++; if(!value) throw new Exception("Director: "+label); }
    static LifeState State(Chapter chapter,string node,long seed=12)
    { return new LifeState { chapter=chapter.id,node=node,rng=seed }; }
    static LifeState RoundTrip(LifeState state)
    {
        var options=new JsonSerializerOptions { IncludeFields=true };
        return JsonSerializer.Deserialize<LifeState>(JsonSerializer.Serialize(state,options),options);
    }
    public static void Run(Chapter chapter)
    {
        foreach(var e in LifeDirector.Events)
        {
            Check(chapter.Find(e.node)!=null && LifeDirector.People.Contains(e.person),"authored location and person "+e.id);
            Check(e.choices.Length>=2 && e.choices.Length<=3 && e.choices.Any(c=>c.cost==0),"affordable exit "+e.id);
            Check(e.choices.Select(c=>c.id).Distinct().Count()==e.choices.Length,"unique options "+e.id);
            Check(e.text.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length<=40,"brief event "+e.id);
            foreach(var option in e.choices)
            {
                var state=State(chapter,e.node); if(e.required!="") state.flags.Add(e.required);
                state.director.pendingEvent=e.id; state.director.pendingOccurrence=LifeDirector.Occurrence(state);
                state.Validate(chapter);
                var engine=new StoryEngine(chapter,state); string occurrence=state.director.pendingOccurrence;
                string result;
                Check(engine.ResolveMoment(occurrence,option.id,out result),"every option applies "+e.id+"/"+option.id);
                Check(!string.IsNullOrEmpty(result) && engine.State.cash==state.cash-option.cost,"only defined cash effect");
                Check(engine.State.node==state.node && engine.State.day==state.day,"side moment does not skip story or calendar");
                Check(option.disclose?engine.State.director.Find(e.person).facts.Contains(option.fact):engine.State.director.Find(e.person)==null,"only disclosed choices enter participant memory");
                Check(engine.State.director.people.Count==(option.disclose?1:0),"no knowledge broadcast");
                Check(!engine.ResolveMoment(occurrence,option.id,out result),"duplicate event choice rejected");
                var copy=RoundTrip(engine.State); copy.Validate(chapter);
                Check(copy.director.history.Count==1 && (option.disclose?copy.director.Find(e.person).facts.Contains(option.fact):copy.director.Find(e.person)==null),"private history and disclosed memory survive JSON");
                Check(!engine.PrepareMoment(),"one director event per beat and day");
            }
        }
        var s=State(chapter,"after_registration"); s.flags.Add("bursary_pending");
        var a=new StoryEngine(chapter,s); Check(a.PrepareMoment() && a.PendingMoment.id=="bursary_check","bursary relevance outranks generic scenes");
        var pending=RoundTrip(a.State); var b=new StoryEngine(chapter,pending);
        Check(b.PrepareMoment() && a.PendingMoment.id==b.PendingMoment.id && a.State.rng==b.State.rng,"reload does not reselect event");
        Check(!b.Apply(b.State.node,b.Current.choices[0].id),"pending director moment cannot be bypassed by story transition");
        string ignored;
        Check(!b.ResolveMoment("stale occurrence","track",out ignored) && b.State.director.pendingEvent!="","stale callback cannot mutate");
        b.RestoreCheckpoint(pending); Check(b.State.director.pendingEvent=="bursary_check","checkpoint retains unresolved moment");
        b.State.director.Find("rehema"); // No hidden participant is added by reading memory.
        Check(b.State.director.people.Count==0,"reading memory is side-effect free");

        s=State(chapter,"after_registration"); s.flags.Add("shift_applied");
        a=new StoryEngine(chapter,s); a.PrepareMoment(); Check(a.PendingMoment.id=="shift_terms","work history changes event selection");
        var seen=new System.Collections.Generic.HashSet<string>();
        for(int seed=1;seed<=64;seed++) { a=new StoryEngine(chapter,State(chapter,"after_registration",seed)); a.PrepareMoment(); seen.Add(a.PendingMoment.id); }
        Check(seen.SetEquals(new[]{"shared_copies","mentor_checkin"}),"equal relevance allows bounded variation");
        s=State(chapter,"after_registration"); s.pressure=60;
        a=new StoryEngine(chapter,s); a.PrepareMoment(); Check(a.PendingMoment.id=="mentor_checkin","high pressure favours rest");
        s.director.pendingEvent="shared_copies"; s.director.pendingOccurrence=LifeDirector.Occurrence(s); s.cash=0;
        a=new StoryEngine(chapter,s); long rng=a.State.rng;
        Check(!a.ResolveMoment(s.director.pendingOccurrence,"share",out ignored) && a.State.cash==0 && a.State.rng==rng && a.State.director.history.Count==0,"unaffordable choice is atomic");
        Check(a.ResolveMoment(s.director.pendingOccurrence,"help",out ignored),"cooperation does not require money");
        var isolation=a.State.Copy(); isolation.director.Find("rehema").facts.Add("external");
        Check(!a.State.director.Find("rehema").facts.Contains("external"),"deep copy isolates NPC memories");

        seen.Clear();
        for(int seed=1;seed<=200;seed++)
        {
            s=State(chapter,"morning_message",seed); s.flags.Add("concern_recorded");
            a=new StoryEngine(chapter,s); a.PrepareMoment(); var before=a.State.Copy();
            Check(a.PendingMoment.id=="report_followup","report context wins");
            a.ResolveMoment(a.State.director.pendingOccurrence,"appointment",out string first);
            string outcome=a.State.director.history.Last().outcome; seen.Add(outcome); long after=a.State.rng;
            a.RestoreCheckpoint(before); a.ResolveMoment(a.State.director.pendingOccurrence,"appointment",out string again);
            Check(first==again && after==a.State.rng,"same incident replays same result");
            Check(a.State.director.accountability==1 && !a.State.Has("convicted"),"a report logs action, not guilt");
        }
        Check(seen.SetEquals(new[]{"steady","setback"}),"authored appointment outcomes both reachable");
        s=State(chapter,"first_sleep"); s.flags.AddRange(new[]{"accepted_deal","copied_ledger","concern_recorded"});
        a=new StoryEngine(chapter,s); a.Apply(s.node,"rest");
        Check(a.State.Has("witness_visits") && a.State.Has("accepted_deal"),"report after payment retains both history and reporting follow-up");
        s=State(chapter,"fixer_offer"); a=new StoryEngine(chapter,s); a.Apply(s.node,"pay");
        Check(a.State.director.Find("kito").obligation==20 && a.State.director.Find("support")==null,"private fixer transaction not known to support");
        var report=LifeDirector.Find("report_followup");
        Check(LifeDirector.Text(report,a.State)==report.text,"world flags do not leak private payment into support dialogue");
        a.State.director.Remember("support","slip_check/report");
        Check(LifeDirector.Text(report,a.State)!=report.text,"support dialogue uses its own witnessed report");

        s=State(chapter,"college_gate"); s.director=null;
        a=new StoryEngine(chapter,s); Check(a.State.director.version==2 && a.State.lifeId==s.lifeId,"legacy save upgrades without replacing life");
        s=State(chapter,"morning_message"); s.flags.Add("accepted_deal"); s.director.version=1;
        s.director.Remember("kito","message_preserved"); s.director.Remember("kito","fixer_offer/pay",2,20);
        s.director.history.Add(new DirectorRecord { occurrence=LifeDirector.Occurrence(s),eventId="fixer_boundary",choiceId="record",outcome="steady",day=s.day });
        s.Validate(chapter); a=new StoryEngine(chapter,RoundTrip(s));
        Check(a.State.director.version==2 && !a.State.director.Find("kito").facts.Contains("message_preserved"),"old disclosure defect repaired on load");
        Check(a.State.director.Find("kito").facts.Contains("fixer_offer/pay") && a.State.director.Find("kito").obligation==20 && a.State.director.history.Count==1 && a.State.lifeId==s.lifeId,"migration keeps legitimate knowledge, obligations, player history and identity");
        Check(s.director.version==1 && s.director.Find("kito").facts.Contains("message_preserved"),"migration does not mutate checkpoint source");
        s=State(chapter,"college_gate"); for(int i=0;i<1000;i++) s.journal.Add("Past "+i);
        a=new StoryEngine(chapter,s); Check(a.Apply(s.node,"mentor") && a.State.journal.Count==1000 && a.State.journal[0]=="Past 1","full journal cannot block progression");
        s=a.State.Copy(); s.director.pendingEvent="invented"; s.director.pendingOccurrence=LifeDirector.Occurrence(s);
        bool rejected=false; try { s.Validate(chapter); } catch(ArgumentException) { rejected=true; }
        Check(rejected,"unknown event in save rejected");
        // Walk real opening routes with the director enabled, not only isolated event fixtures.
        for(int seed=1;seed<=64;seed++)
        {
            a=new StoryEngine(chapter,State(chapter,chapter.start,seed));
            var random=new Random(seed); int steps=0, moments=0;
            while(!a.Current.terminal)
            {
                Check(++steps<40,"director-enabled opening has no loop");
                if(a.PrepareMoment())
                {
                    var choices=a.PendingMoment.choices.Where(c=>c.cost<=a.State.cash).ToArray();
                    Check(choices.Length>0,"director always offers an affordable action");
                    Check(a.ResolveMoment(a.State.director.pendingOccurrence,choices[random.Next(choices.Length)].id,out ignored),"live-route director resolves");
                    moments++;
                    a=new StoryEngine(chapter,RoundTrip(a.State));
                }
                var storyChoices=a.Current.choices.Where(c=>a.Available(c)).ToArray();
                Check(storyChoices.Length>0,"director costs do not soft-lock the next story beat");
                Check(a.Apply(a.State.node,storyChoices[random.Next(storyChoices.Length)].id),"live-route story continues");
            }
            Check(moments==2 && a.State.Has("education_path"),"two integrated moments and college outcome per completed route");
        }
        Console.WriteLine("PASS: "+checks+" director checks (selection, memory, migration, atomic effects, persistence, checkpoints and consequences).");
    }
}
