using System;

namespace Kesho.Core
{
    public enum DayPeriod { Morning, Afternoon, Evening, Night }
    public enum SpeakerKind { Player, InPerson, Phone, Offscreen, Narration }
    public sealed class SpeakerCue
    {
        public readonly string actor;
        public readonly SpeakerKind kind;
        public SpeakerCue(string actor,SpeakerKind kind) { this.actor=actor; this.kind=kind; }
    }
    public sealed class HotspotDescription
    {
        public readonly string title, text;
        public HotspotDescription(string title,string text) { this.title=title; this.text=text; }
    }
    /// <summary>Presentation identities use authored speaker tokens, never a player's chosen display name.</summary>
    public static class StoryPresentation
    {
        public static bool SameLayout(string scene,string savedScene)
        {
            return scene==savedScene || ((scene=="Room" || scene=="Night") && (savedScene=="Room" || savedScene=="Night"));
        }
        public static DayPeriod TimeFor(StoryNode node)
        {
            switch(node.timeOfDay)
            {
                case "morning": return DayPeriod.Morning;
                case "afternoon": return DayPeriod.Afternoon;
                case "evening": return DayPeriod.Evening;
                case "night": return DayPeriod.Night;
                case null: case "": return node.scene=="Night"?DayPeriod.Night:DayPeriod.Afternoon;
                default: throw new ArgumentException("Unknown time of day: "+node.id);
            }
        }
        public static string TargetActor(string scene,string target)
        {
            if(scene=="Campus" || scene=="Arrival")
            {
                switch(target)
                {
                    case "contact": return "kito";
                    case "rehema": return "rehema";
                    case "clerk": return "clerk";
                    case "ledger": return "support";
                    case "parcel": return "copy_seller";
                }
            }
            if(scene=="Courtyard" && target=="landlord") return "sifa";
            if((scene=="Office" || scene=="Dispatch") && target=="clerk") return "clerk";
            return "";
        }
        public static SpeakerCue Speaker(StoryNode node,string token)
        {
            if(token=="{name}") return new SpeakerCue("player",SpeakerKind.Player);
            if(token=="Noticeboard") return new SpeakerCue("",SpeakerKind.Narration);
            string actor;
            switch(token)
            {
                case "Kito": actor="kito"; break;
                case "Rehema": actor="rehema"; break;
                case "Sifa": actor="sifa"; break;
                case "Admissions clerk": actor="clerk"; break;
                case "Student support": case "Student support · message": actor="support"; break;
                case "Kiosk owner": actor="copy_seller"; break;
                case "Student in queue": actor="queued_student"; break;
                case "{contact}": actor="household"; break;
                case "Kito · message": actor="kito"; break;
                case "Phone": actor=""; break;
                default: return new SpeakerCue("",SpeakerKind.Offscreen);
            }
            if(node.target=="phone" || token.EndsWith(" · message",StringComparison.Ordinal)) return new SpeakerCue(actor,SpeakerKind.Phone);
            bool onCampus=node.scene=="Campus" || node.scene=="Arrival";
            bool present=(onCampus && actor!="sifa" && actor!="household" && actor!="") || (node.scene=="Courtyard" && actor=="sifa");
            return new SpeakerCue(actor,present?SpeakerKind.InPerson:SpeakerKind.Offscreen);
        }
        public static HotspotDescription Describe(string scene,string target)
        {
            if(scene=="Campus" || scene=="Arrival")
            {
                switch(target)
                {
                    case "notice": return new HotspotDescription("Official fee notice","Registration C600. Pay at admissions. Assistance is free. Keep your stamped receipt.");
                    case "rehema": return new HotspotDescription("Rehema · student mentor","A student mentor helps freshers find the right queue. Her lanyard matches the college notice.");
                    case "contact": return new HotspotDescription("Kito · knows his way around","He greets arrivals by the gate. No college lanyard. Plenty of confidence.");
                    case "ledger": return new HotspotDescription("Student support","Payment arrangements, housing advice and a private channel for concerns. No assistance fee.");
                    case "clerk": return new HotspotDescription("Admissions","Students hold letters and identification. A clerk checks each name and stamps receipts.");
                    case "parcel": return new HotspotDescription("Chai & copies","A thermos, a photocopier and a handwritten price list. The owner keeps the papers away from the tea.");
                }
            }
            if(scene=="Courtyard")
            {
                switch(target)
                {
                    case "contact": return new HotspotDescription("Karibu Court","Clothes dry above the shared yard. Someone has left a space on the bench. There is room to catch your breath.");
                    case "landlord": return new HotspotDescription("Sifa · Karibu Court","Sifa manages the rooms and knows who needs a spare mattress tonight. Ask about terms before making promises.");
                    case "supper": return new HotspotDescription("Shared supper table","A serving bowl sits on the courtyard table. Neighbours are settling in after their day.");
                    case "phone": return new HotspotDescription("A quiet corner","A place to make a private call away from the supper table.");
                }
            }
            if(scene=="Room" || scene=="Night")
            {
                switch(target)
                {
                    case "bed": return new HotspotDescription("A place to rest","A bed, a woven blanket and space to put your papers safely aside.");
                    case "phone": return new HotspotDescription("Your phone","Your messages and calls stay private. Nobody learns what you save unless you share it.");
                    case "door": return new HotspotDescription("The doorway","The shared courtyard is beyond this door. Your current story moment will tell you when it is time to leave.");
                }
            }
            return null;
        }
    }
}
