using System;
using System.Collections.Generic;
using System.Linq;

namespace Kesho.Core
{
    [Serializable] public sealed class Chapter
    {
        public string id;
        public string start;
        public StoryNode[] nodes;
        public StoryNode Find(string id) { return nodes.FirstOrDefault(n => n.id == id); }
        public void Validate()
        {
            if (nodes == null || nodes.Length == 0 || Find(start) == null) throw new ArgumentException("Missing opening node");
            var ids = new HashSet<string>();
            foreach (var n in nodes)
            {
                if (string.IsNullOrEmpty(n.id) || !ids.Add(n.id)) throw new ArgumentException("Duplicate/empty node");
                if (!WorldCatalog.Scenes.Contains(n.scene) || !WorldCatalog.Targets.Contains(n.target)) throw new ArgumentException("Unknown scene/target: " + n.id);
                StoryPresentation.TimeFor(n);
                if (n.lines == null || n.lines.Length == 0 || n.choices == null) throw new ArgumentException("Incomplete beat: " + n.id);
                var choices = new HashSet<string>();
                foreach (var c in n.choices)
                {
                    if (string.IsNullOrEmpty(c.id) || !choices.Add(c.id) || Find(c.next) == null || c.cost < 0 || c.reward < 0)
                        throw new ArgumentException("Invalid choice in " + n.id);
                    if (c.effect != "" && c.effect != "sleep" && c.effect != "night_resolution") throw new ArgumentException("Unknown effect");
                }
                if (n.choices.Length == 0 && !n.terminal) throw new ArgumentException("Non-terminal dead end: " + n.id);
            }
            var seen = new HashSet<string>(); var queue = new Queue<string>(); queue.Enqueue(start);
            while (queue.Count > 0) { var id = queue.Dequeue(); if (!seen.Add(id)) continue; foreach (var c in Find(id).choices) queue.Enqueue(c.next); }
            if (seen.Count != nodes.Length) throw new ArgumentException("Unreachable authored scene");
        }
    }
    [Serializable] public sealed class StoryNode
    {
        public string id, scene, target, objective, sound, timeOfDay;
        public Line[] lines;
        public Choice[] choices;
        public bool terminal, comic;
    }
    [Serializable] public sealed class Line { public string speaker, text, voice, whenFlag, unlessFlag; }
    [Serializable] public sealed class Choice
    {
        public string id, label, next, requireFlag, blockFlag, effect;
        public int cost, reward, trust;
        public string[] flags;
    }
    [Serializable] public sealed class LifeState
    {
        public int version = 1;
        public string chapter = "college_first_day", node, name = "Amani", gender = "Man", belief = "Personal / private", background = "Family household";
        public string lifeId = Guid.NewGuid().ToString("N"), poseScene = "";
        public float posX, posZ;
        public string character = "Amani";
        public int hair;
        public int age = 19, cash = 1200, day = 1, trust, pressure, skin = 1, outfit;
        public bool permanent, dead;
        public long rng = 18273;
        public double activeSeconds;
        public List<string> flags = new List<string>();
        public List<string> decisions = new List<string>();
        public List<string> journal = new List<string>();
        public DirectorState director = new DirectorState();
        public void AddJournal(string entry)
        {
            if(journal.Count>=1000) journal.RemoveAt(0);
            journal.Add(entry);
        }
        public bool Has(string flag) { return string.IsNullOrEmpty(flag) || flags.Contains(flag); }
        public LifeState Copy()
        {
            var copy = (LifeState)MemberwiseClone();
            copy.flags = new List<string>(flags); copy.decisions = new List<string>(decisions); copy.journal = new List<string>(journal);
            copy.director = director == null ? new DirectorState() : director.Copy();
            return copy;
        }
        public void Validate(Chapter chapterData)
        {
            if (string.IsNullOrEmpty(lifeId) || lifeId.Length != 32 || poseScene == null || (poseScene != "" && !WorldCatalog.Scenes.Contains(poseScene)) || float.IsNaN(posX) || float.IsNaN(posZ) || float.IsInfinity(posX) || float.IsInfinity(posZ) || Math.Abs(posX)>12 || Math.Abs(posZ)>10) throw new ArgumentException("Invalid save location");
            if (version != 1 || chapter != chapterData.id || chapterData.Find(node) == null || age < 18 || cash < 0 || day < 1 || day > 100000 || hair < 0 || hair > 4 || !WorldCatalog.Characters.Contains(character) || skin < 0 || skin > 3 || outfit < 0 || outfit > 3 || trust < -100 || trust > 100 || pressure < 0 || pressure > 100 || rng < 1 || rng > uint.MaxValue || activeSeconds < 0 || double.IsNaN(activeSeconds) || double.IsInfinity(activeSeconds)) throw new ArgumentException("Invalid save state");
            if (flags == null || decisions == null || journal == null || flags.Count > 1000 || decisions.Count > 10000 || journal.Count > 1000 || flags.Any(f => f == null) || decisions.Any(d => d == null) || journal.Any(j => j == null)) throw new ArgumentException("Invalid save history");
            if (name == null || name.Length > 32 || !WorldCatalog.Backgrounds.Contains(background) || !WorldCatalog.Genders.Contains(gender) || !WorldCatalog.Beliefs.Contains(belief)) throw new ArgumentException("Invalid character");
            // Missing director is a legacy 0.4.0 save; Copy upgrades it without changing the existing life.
            if(director!=null) director.Validate(this);
        }
    }
    public static class WorldCatalog
    {
        public static readonly string[] Scenes = { "Arrival", "Courtyard", "Room", "Office", "Dispatch", "Campus", "Night" };
        public static readonly string[] Targets = { "contact", "landlord", "supper", "bed", "phone", "clerk", "notice", "foreman", "parcel", "rehema", "door", "ledger", "exit" };
        public static readonly string[] Characters = { "Amani", "Nia", "Custom" };
        public static readonly string[] Hairstyles = { "Fade", "Natural puff", "Braids", "Twists", "Loc bun" };
        public static readonly string[] Backgrounds = { "Family household", "Independent parent", "Community upbringing" };
        public static readonly string[] Genders = { "Woman", "Man", "Non-binary" };
        public static readonly string[] Beliefs = { "Personal / private", "Christian", "Muslim", "Traditional belief", "No religion" };
    }
}
