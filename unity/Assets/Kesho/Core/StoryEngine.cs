using System;
using System.Linq;

namespace Kesho.Core
{
    /// <summary>Only authored transitions may mutate the private life. Generated text has no command channel.</summary>
    public sealed class StoryEngine
    {
        public readonly Chapter Chapter;
        public LifeState State { get; private set; }
        public StoryNode Current { get { return Chapter.Find(State.node); } }
        public StoryEngine(Chapter chapter, LifeState saved = null)
        {
            chapter.Validate(); Chapter = chapter;
            State = saved == null ? new LifeState { node = chapter.start, chapter = chapter.id } : saved.Copy();
            State.Validate(chapter);
        }
        public bool Available(Choice choice)
        {
            return !State.dead && Current.choices.Contains(choice) && State.cash >= choice.cost && State.Has(choice.requireFlag) && (string.IsNullOrEmpty(choice.blockFlag) || !State.Has(choice.blockFlag));
        }
        public string UnavailableReason(Choice c)
        {
            if (State.cash < c.cost) return "Need C" + c.cost;
            return "You have not established this yet.";
        }
        public bool Apply(string expectedNode, string choiceId)
        {
            if (State.node != expectedNode || State.dead) return false;
            Choice c = Current.choices.FirstOrDefault(x => x.id == choiceId);
            string command = expectedNode + "/" + choiceId;
            if (c == null || !Available(c) || State.decisions.Contains(command)) return false;
            LifeState next = State.Copy();
            checked { next.cash = next.cash - c.cost + c.reward; }
            next.trust = Math.Max(-100, Math.Min(100, next.trust + c.trust));
            foreach (string flag in c.flags ?? new string[0]) if (!next.flags.Contains(flag)) next.flags.Add(flag);
            next.decisions.Add(command);
            next.journal.Add("Day " + next.day + ": " + c.label);
            if (c.effect == "sleep") next.day++;
            if (c.effect == "night_resolution") { ResolveNight(next); next.day++; }
            next.node = c.next; next.Validate(Chapter); State = next;
            return true;
        }
        public void RestoreCheckpoint(LifeState checkpoint)
        {
            if (State.lifeId != checkpoint.lifeId) throw new InvalidOperationException("Checkpoint belongs to another life");
            if (State.permanent || checkpoint.permanent) throw new InvalidOperationException("Permanent lives cannot rewind");
            checkpoint.Validate(Chapter); State = checkpoint.Copy();
        }
        public string Format(string text)
        {
            string contact = State.background == "Family household" ? "Sifa" : State.background == "Independent parent" ? "Leila" : "Musa";
            return (text ?? "").Replace("{name}", State.name).Replace("{contact}", contact);
        }
        public Line[] VisibleLines()
        {
            return Current.lines.Where(l => State.Has(l.whenFlag) && (string.IsNullOrEmpty(l.unlessFlag) || !State.Has(l.unlessFlag))).ToArray();
        }
        static void ResolveNight(LifeState s)
        {
            // One committed draw per decision; save the generator state. No reload-to-reroll.
            uint x = (uint)s.rng; x ^= x << 13; x ^= x >> 17; x ^= x << 5; s.rng = x;
            if (s.Has("accepted_deal"))
            {
                s.pressure = Math.Min(100, s.pressure + 25);
                s.flags.Add((x % 100) < 60 ? "collector_visits" : "collector_calls");
            }
            else if (s.Has("copied_ledger")) { s.flags.Add("witness_visits"); s.pressure = Math.Min(100, s.pressure + 15); }
            else { s.flags.Add("contact_warns"); s.pressure = Math.Min(100, s.pressure + 8); }
        }
    }
    public static class DialogueGuard
    {
        // Optional future generated side dialogue: known facts only; no state-writing API.
        public static bool Accept(string text, string[] claimedFacts, LifeState state)
        {
            return text != null && text.Length > 0 && text.Length <= 220 && text.IndexOf('<') < 0 && claimedFacts != null && claimedFacts.All(f => !string.IsNullOrEmpty(f) && state.flags.Contains(f));
        }
    }
    public static class PaceDirector
    {
        public static string NextMood(LifeState state, bool inConversation, bool sensitiveScene)
        {
            if (inConversation || sensitiveScene) return "quiet";
            if (state.Has("collector_visits") || state.Has("collector_calls")) return "unease";
            return state.pressure > 10 ? "watchful" : "room_to_breathe";
        }
    }
}
