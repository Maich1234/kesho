using System;
using System.Collections.Generic;
using System.Linq;

namespace Kesho.Core
{
    [Serializable] public sealed class PersonMemory
    {
        public string id;
        public int trust, obligation;
        public List<string> facts = new List<string>();
        public PersonMemory Copy() { var n=(PersonMemory)MemberwiseClone(); n.facts=new List<string>(facts); return n; }
    }
    [Serializable] public sealed class DirectorRecord
    {
        public string occurrence, eventId, choiceId, outcome;
        public int day;
        public DirectorRecord Copy() { return (DirectorRecord)MemberwiseClone(); }
    }
    [Serializable] public sealed class DirectorState
    {
        public int version=1, accountability;
        public string pendingEvent="", pendingOccurrence="";
        public List<PersonMemory> people=new List<PersonMemory>();
        public List<DirectorRecord> history=new List<DirectorRecord>();
        public DirectorState Copy()
        {
            var n=(DirectorState)MemberwiseClone();
            n.people=people.Select(p=>p.Copy()).ToList(); n.history=history.Select(h=>h.Copy()).ToList(); return n;
        }
        public PersonMemory Find(string id) { return people.FirstOrDefault(p=>p.id==id); }
        public void Remember(string id, string fact, int trust=0, int obligation=0)
        {
            var person=Find(id);
            if(person==null) { person=new PersonMemory { id=id }; people.Add(person); }
            person.trust=Math.Max(-100,Math.Min(100,person.trust+trust));
            person.obligation=Math.Max(0,Math.Min(100,person.obligation+obligation));
            if(!person.facts.Contains(fact)) person.facts.Add(fact);
        }
        public void Validate(LifeState life)
        {
            if(version!=1 || accountability<0 || accountability>100 || people==null || history==null || people.Count>32 || history.Count>256 || pendingEvent==null || pendingOccurrence==null)
                throw new ArgumentException("Invalid director state");
            var ids=new HashSet<string>();
            foreach(var p in people)
                if(p==null || !LifeDirector.People.Contains(p.id) || !ids.Add(p.id) || p.trust< -100 || p.trust>100 || p.obligation<0 || p.obligation>100 || p.facts==null || p.facts.Count>64 || p.facts.Distinct().Count()!=p.facts.Count || p.facts.Any(f=>string.IsNullOrEmpty(f)||f.Length>80))
                    throw new ArgumentException("Invalid person memory");
            ids.Clear();
            foreach(var h in history)
            {
                var e=h==null?null:LifeDirector.Find(h.eventId);
                if(e==null || string.IsNullOrEmpty(h.occurrence) || h.occurrence.Length>160 || !ids.Add(h.occurrence) || h.day<1 || h.day>life.day || !e.choices.Any(c=>c.id==h.choiceId) || (h.outcome!="steady" && h.outcome!="setback"))
                    throw new ArgumentException("Invalid director history");
            }
            if((pendingEvent=="")!=(pendingOccurrence=="")) throw new ArgumentException("Incomplete director moment");
            if(pendingEvent!="")
            {
                var e=LifeDirector.Find(pendingEvent);
                if(e==null || !LifeDirector.Eligible(e,life) || pendingOccurrence!=LifeDirector.Occurrence(life) || ids.Contains(pendingOccurrence)) throw new ArgumentException("Invalid pending director moment");
            }
        }
    }
    public sealed class DirectorChoice
    {
        public readonly string id, label, fact, result, setback;
        public readonly int cost, pressure, trust, obligation, setbackPercent;
        public DirectorChoice(string id,string label,string fact,string result,int cost=0,int pressure=0,int trust=0,int obligation=0,int setbackPercent=0,string setback="")
        { this.id=id; this.label=label; this.fact=fact; this.result=result; this.cost=cost; this.pressure=pressure; this.trust=trust; this.obligation=obligation; this.setbackPercent=setbackPercent; this.setback=setback; }
    }
    public sealed class DirectorEvent
    {
        public readonly string id, node, speaker, person, text, theme, required, blocked;
        public readonly int priority;
        public readonly DirectorChoice[] choices;
        public DirectorEvent(string id,string node,string speaker,string person,string text,string theme,int priority,string required,string blocked,params DirectorChoice[] choices)
        { this.id=id; this.node=node; this.speaker=speaker; this.person=person; this.text=text; this.theme=theme; this.priority=priority; this.required=required; this.blocked=blocked; this.choices=choices; }
    }
    /// <summary>Offline rules director v1. All text and effects are authored. No external model or scraped training data.</summary>
    public static class LifeDirector
    {
        public static readonly string[] People={"kito","rehema","admissions","support","household","landlord","copy_seller"};
        // References inform theme selection only; external publication text is not included in prompts or assets.
        public static readonly DirectorEvent[] Events={
            new DirectorEvent("bursary_check","after_registration","Rehema","rehema",
                "Bursary pending? Keep the reference. Some people charge to 'follow up'. Want me to show you the free tracking desk?","support",80,"bursary_pending","",
                new DirectorChoice("track","Check the reference together","aid_reference_checked","You keep the reference and the desk's opening hours. Rehema knows you prefer checking first.",pressure:-3,trust:3),
                new DirectorChoice("private","Keep my paperwork private","aid_privacy_requested","Rehema respects your boundary. You keep the reference to check yourself.",pressure:-1)),
            new DirectorEvent("shift_terms","after_registration","Rehema","rehema",
                "That work placement: ask about hours before saying yes. A job that swallows every class is an expensive job, bana.","work",80,"shift_applied","",
                new DirectorChoice("hours","Ask for written hours","shift_hours_requested","Rehema sends a request for the timetable. No shift or payment is guaranteed yet.",trust:2),
                new DirectorChoice("study","Protect class time first","classes_prioritised","You note your class commitments before following up. Rehema remembers that boundary.",pressure:-2,trust:1)),
            new DirectorEvent("shared_copies","after_registration","Rehema","rehema",
                "Three freshers, one photocopy machine. We can share the blank timetable sheet. Your documents stay yours.","cooperation",25,"","",
                new DirectorChoice("share","Contribute C10 for blank copies","shared_blank_copies","Everyone gets a blank timetable. Nobody handles your personal papers.",cost:10,pressure:-2,trust:2),
                new DirectorChoice("help","Help organise without paying","organised_copy_queue","You sort the queue while someone collects the blank sheets. Small help still counts.",pressure:-1,trust:2),
                new DirectorChoice("leave","Keep moving","declined_copy_group","Rehema waves you on. You can make friends without joining every plan.")),
            new DirectorEvent("mentor_checkin","after_registration","Rehema","rehema",
                "Sasa, registration survived. Have you eaten, or has the queue eaten your whole day?","rest",25,"","",
                new DirectorChoice("tea","Take a chai break · C20","took_chai_break","Ten quiet minutes and a hot cup. Rehema laughs about getting lost on her own first day.",cost:20,pressure:-5,trust:2),
                new DirectorChoice("water","Sit for water and a chat","took_water_break","You refill your bottle and rest in the shade. The city can wait a moment.",pressure:-4,trust:2)),
            new DirectorEvent("report_followup","morning_message","Student support · message","support",
                "We received your concern. Keep the original slip. Would you like a private appointment? This is a report, not a finding against anyone.","accountability",100,"concern_recorded","",
                new DirectorChoice("appointment","Request a private appointment","support_appointment","Support records your request. A staff member will need to verify the documents.",pressure:-3,trust:3,setbackPercent:25,setback:"The next appointment is full. Your request stays logged for a later slot; the complaint has not been dismissed."),
                new DirectorChoice("pause","Keep the reference; decide later","support_reference_kept","You save the reference without agreeing to an interview. Support knows you have not consented yet.",pressure:-1)),
            new DirectorEvent("fixer_boundary","morning_message","Kito · message","kito",
                "You went quiet after that shortcut. Meet alone and we sort the rest, sawa?","pressure",70,"accepted_deal","concern_recorded",
                new DirectorChoice("public","Only discuss this at the official desk","public_meeting_requested","You set a public meeting boundary. Kito knows your answer; no meeting or refund has happened.",pressure:-2,obligation:-10),
                new DirectorChoice("record","Save the message; do not reply","message_preserved","You keep a private copy. Kito learns nothing new from your silence.")),
            new DirectorEvent("family_budget","morning_message","{contact} · message","household",
                "First morning! No pressure to send money home. Just tell us what you need this week before you promise anybody anything.","family",20,"","accepted_deal",
                new DirectorChoice("budget","Share a rough weekly budget","weekly_budget_shared","Your contact knows you need time to settle. You agree to talk before making money promises.",pressure:-3,trust:3),
                new DirectorChoice("privacy","Say I'm safe; keep finances private","family_financial_boundary","You confirm you are safe and keep the details private. Your contact accepts that.",pressure:-1,trust:1))
        };
        public static DirectorEvent Find(string id) { return Events.FirstOrDefault(e=>e.id==id); }
        public static string Text(DirectorEvent e,LifeState s)
        {
            var memory=s.director.Find(e.person);
            if(e.id=="report_followup" && memory!=null && memory.facts.Contains("slip_check/report"))
                return "Your report about the shortcut payment is logged. Keep the slip. A private appointment can help us check it; nobody has been found guilty.";
            if(e.id=="mentor_checkin" && memory!=null && memory.trust>=3)
                return "You made it through! Come, breathe a little. My first day I queued at the library to pay fees. Even the librarian laughed.";
            return e.text;
        }
        public static string Occurrence(LifeState s) { return s.chapter+"/"+s.node+"/day-"+s.day; }
        public static bool Eligible(DirectorEvent e,LifeState s)
        { return !s.dead && e.node==s.node && s.Has(e.required) && (string.IsNullOrEmpty(e.blocked)||!s.Has(e.blocked)); }
        public static uint Draw(LifeState s)
        { uint x=(uint)s.rng; x^=x<<13; x^=x>>17; x^=x<<5; s.rng=x; return x; }
        public static bool Prepare(LifeState s)
        {
            if(s.director.pendingEvent!="") return true;
            string occurrence=Occurrence(s);
            if(s.director.history.Count>=256 || s.director.history.Any(h=>h.occurrence==occurrence)) return false;
            var candidates=Events.Where(e=>Eligible(e,s)).ToArray();
            if(candidates.Length==0) return false;
            // Context outranks randomness. Only equally relevant candidates draw a tie-break.
            int best=candidates.Max(e=>Score(e,s)); var top=candidates.Where(e=>Score(e,s)==best).ToArray();
            var chosen=top.Length==1?top[0]:top[(int)(Draw(s)%(uint)top.Length)];
            s.director.pendingEvent=chosen.id; s.director.pendingOccurrence=occurrence; return true;
        }
        static int Score(DirectorEvent e,LifeState s)
        {
            int score=e.priority;
            if(e.theme=="rest") score+=s.pressure/5;
            if(s.director.history.Any(h=>h.eventId==e.id)) score-=10;
            return score;
        }
        public static bool Resolve(LifeState s,string expectedOccurrence,string choiceId,out string result)
        {
            result=""; var e=Find(s.director.pendingEvent);
            if(e==null || expectedOccurrence!=s.director.pendingOccurrence || expectedOccurrence!=Occurrence(s) || !Eligible(e,s)) return false;
            var c=e.choices.FirstOrDefault(x=>x.id==choiceId);
            if(c==null || s.cash<c.cost) return false;
            bool setback=c.setbackPercent>0 && Draw(s)%100<c.setbackPercent;
            s.cash-=c.cost; s.pressure=Math.Max(0,Math.Min(100,s.pressure+c.pressure));
            s.director.Remember(e.person,c.fact,c.trust,c.obligation);
            if(e.id=="report_followup" && c.id=="appointment") s.director.accountability=Math.Min(100,s.director.accountability+1);
            result=setback?c.setback:c.result;
            s.director.history.Add(new DirectorRecord { occurrence=expectedOccurrence,eventId=e.id,choiceId=c.id,outcome=setback?"setback":"steady",day=s.day });
            s.director.pendingEvent=""; s.director.pendingOccurrence="";
            s.AddJournal("Day "+s.day+": "+c.label+" — "+result); return true;
        }
        public static void Observe(LifeState s,StoryNode node,Choice c)
        {
            // Knowledge is given to an actual participant, not broadcast to every NPC.
            string person=node.target=="rehema"?"rehema":node.target=="clerk"?"admissions":node.target=="ledger"?"support":
                node.target=="landlord"?"landlord":node.target=="parcel"?"copy_seller":node.target=="contact"&&node.scene=="Campus"?"kito":node.id=="family_call"?"household":null;
            if(person!=null) s.director.Remember(person,node.id+"/"+c.id,c.trust,person=="kito"&&c.id=="pay"?20:0);
        }
    }
}
