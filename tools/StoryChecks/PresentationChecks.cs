using System;
using System.Linq;
using Kesho.Core;

static class PresentationChecks
{
    static int checks;
    static void Check(bool ok,string label) { checks++; if(!ok) throw new Exception("Presentation: "+label); }
    public static void Run(Chapter chapter)
    {
        foreach(var node in chapter.nodes)
        {
            Check(!string.IsNullOrEmpty(node.timeOfDay),"explicit time for "+node.id);
            var period=StoryPresentation.TimeFor(node);
            Check(node.id!="morning_message" || period==DayPeriod.Morning,"morning is morning despite legacy scene name");
            foreach(var line in node.lines)
            {
                var cue=StoryPresentation.Speaker(node,line.speaker);
                Check(cue.kind!=SpeakerKind.InPerson || new[]{"kito","rehema","clerk","support","copy_seller","queued_student","sifa"}.Contains(cue.actor),"all in-person speakers bind to an actor");
                if(line.speaker=="{name}") Check(cue.actor=="player" && cue.kind==SpeakerKind.Player,"player identity independent of chosen name");
                else if(node.target=="phone") Check(cue.kind==SpeakerKind.Phone,"remote lines cannot animate a nearby actor");
            }
        }
        var gate=chapter.Find("college_gate");
        Check(StoryPresentation.Speaker(gate,"Rehema").actor=="rehema","Rehema can speak while Kito remains the interaction target");
        var queue=chapter.Find("queue");
        Check(StoryPresentation.Speaker(queue,"Student in queue").actor=="queued_student","queue student has a distinct identity");
        Check(StoryPresentation.Speaker(queue,"Admissions clerk").actor=="clerk","clerk identity differs from queue student");
        Check(StoryPresentation.Speaker(chapter.Find("view_room"),"Sifa").kind==SpeakerKind.Offscreen,"absent Sifa is offscreen");
        Check(StoryPresentation.Speaker(chapter.Find("morning_message"),"Kito · message").kind==SpeakerKind.Phone,"director message stays remote");
        Check(StoryPresentation.Speaker(chapter.Find("fee_notice"),"Noticeboard").kind==SpeakerKind.Narration,"written notices do not talk");
        Check(StoryPresentation.Describe("Campus","contact").title.Contains("Kito"),"campus contact is Kito");
        Check(!StoryPresentation.Describe("Courtyard","contact").title.Contains("Kito") && !StoryPresentation.Describe("Courtyard","contact").text.Contains("Kito"),"courtyard contact never inherits Kito description");
        Check(StoryPresentation.Describe("Room","contact")==null,"unknown scene/target does not inherit another scene");
        Check(StoryPresentation.TargetActor("Courtyard","contact")=="","courtyard empty spot has no invented speaker");
        Check(StoryPresentation.TargetActor("Courtyard","landlord")=="sifa","landlord binds to Sifa");
        Check(StoryPresentation.TimeFor(chapter.Find("opening_end"))==DayPeriod.Morning,"morning persists after phone scene");
        Check(StoryPresentation.TimeFor(chapter.Find("first_sleep"))==DayPeriod.Night,"sleep uses night lighting");
        Check(StoryPresentation.SameLayout("Room","Night") && StoryPresentation.SameLayout("Night","Room"),"room time variants preserve physical location");
        Check(!StoryPresentation.SameLayout("Courtyard","Campus") && !StoryPresentation.SameLayout("Room",""),"different or absent locations do not transfer a pose");
        var invalid=new StoryNode {id="bad",scene="Room",timeOfDay="nonsense"}; bool rejected=false;
        try { StoryPresentation.TimeFor(invalid); } catch(ArgumentException) { rejected=true; }
        Check(rejected,"unknown authored time rejected");
        Console.WriteLine("PASS: "+checks+" presentation checks (speaker identity, phone/offscreen rules, location descriptions and authored time).");
    }
}
