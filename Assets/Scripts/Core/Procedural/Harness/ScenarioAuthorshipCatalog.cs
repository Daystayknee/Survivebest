using System;
using System.Collections.Generic;
using Survivebest.Core.Procedural;

namespace Survivebest.Core.Procedural.Harness
{
    public enum ScenarioTemplateKind
    {
        Human,
        Vampire
    }

    public enum ScenarioResolutionState
    {
        Undefined,
        Stabilized,
        PartiallyExposedButDismissed,
        KnownByTrustedPerson,
        Blackmailed,
        UnderHunterAttention,
        ForcedRelocation,
        TotalPublicRupture,
        RecoveryFork,
        EmotionalCollapse,
        RedefinedLife
    }

    [Serializable]
    public sealed class ScenarioTemplateDefinition
    {
        public string TemplateId;
        public string Label;
        public ScenarioTemplateKind Kind;
        public string Start;
        public string CoreTension;
        public string TurningPoint;
        public string AlternateLoop;
        public List<string> Tags = new();
    }

    [Serializable]
    public sealed class SoftStoryArcDefinition
    {
        public string ArcId;
        public string Label;
        public string Summary;
        public List<string> PressureSteps = new();
        public List<string> OutcomeStates = new();
        public List<string> Tags = new();
    }

    public static class ScenarioAuthorshipCatalog
    {
        private static readonly List<ScenarioTemplateDefinition> Templates = new()
        {
            CreateTemplate("human_broke_unsafe_rental", "Broke young adult in unsafe rental", ScenarioTemplateKind.Human, "Starts in a cheap rental with cash flow already failing.", "Housing precarity, deferred maintenance, and adult paperwork keep stacking.", "A landlord warning or outage makes staying feel dangerous.", "Keeps stabilizing just enough to avoid collapse, then slips again.", "human", "housing", "finance"),
            CreateTemplate("human_teen_family_school", "Teen with family pressure and school stress", ScenarioTemplateKind.Human, "Starts balancing grades, expectations, and no private room to breathe.", "School hierarchy and family obligation pull in opposite directions.", "A plagiarism accusation or skipped class forces a public explanation.", "Keeps choosing small avoidances that become larger consequences.", "human", "school", "family"),
            CreateTemplate("human_recently_divorced_parent", "Recently divorced parent", ScenarioTemplateKind.Human, "Starts with split routines, legal residue, and kids watching everything.", "Caregiving load outpaces emotional recovery.", "A custody or money conflict turns logistics back into grief.", "Keeps calling it co-parenting while silently living in crisis management.", "human", "family", "legal"),
            CreateTemplate("human_excon_stay_clean", "Ex-con trying to stay clean", ScenarioTemplateKind.Human, "Starts with strict routines, fragile trust, and limited opportunity.", "Every social tie carries old risk or needed support.", "A relapse-adjacent temptation or police questioning threatens progress.", "Keeps surviving by white-knuckling the same dangerous neighborhood patterns.", "human", "crime", "recovery"),
            CreateTemplate("human_small_town_beauty_queen", "Small-town beauty queen stuck at home", ScenarioTemplateKind.Human, "Starts with a polished image and nowhere meaningful to spend it.", "Reputation maintenance fights boredom, resentment, and stalled ambition.", "A viral embarrassment or family conflict punctures the local myth.", "Keeps performing success to an audience that remembers high school too clearly.", "human", "reputation", "small-town"),
            CreateTemplate("human_musician_with_debt", "Aspiring musician with debt", ScenarioTemplateKind.Human, "Starts with talent, late-night gigs, and a bank app nobody wants to open.", "Creative hunger collides with financial drag.", "A breakthrough opportunity arrives at the exact moment rent destabilizes.", "Keeps trading sleep, dignity, and stability for one more chance.", "human", "creative", "finance"),
            CreateTemplate("human_overworked_nurse_family", "Overworked nurse caring for family", ScenarioTemplateKind.Human, "Starts on a punishing night-shift schedule with relatives still needing everything.", "Medical labor never fully ends at the workplace door.", "A family emergency lands in the middle of burnout.", "Keeps functioning through competence while body and grief invoice the cost later.", "human", "medical", "caregiving"),
            CreateTemplate("vampire_newly_turned_starving", "Newly turned and starving", ScenarioTemplateKind.Vampire, "Starts with hunger louder than ethics and no stable ritual yet.", "Masquerade discipline arrives slower than appetite.", "A near-feeding disaster creates the first real witness risk.", "Keeps promising control tomorrow after every ugly night.", "vampire", "hunger", "secrecy"),
            CreateTemplate("vampire_ancient_reinventing", "Ancient vampire reinventing identity again", ScenarioTemplateKind.Vampire, "Starts another century wearing a fresh name and curated past.", "Old patterns keep surfacing inside each new persona.", "Someone recognizes a detail that should have died generations ago.", "Keeps abandoning entire identities before intimacy can expose repetition.", "vampire", "identity", "history"),
            CreateTemplate("vampire_hiding_in_family", "Vampire hiding in a human family household", ScenarioTemplateKind.Vampire, "Starts inside domestic routine where love makes every secret harder.", "Household intimacy keeps catching schedule, appetite, and body anomalies.", "A child, partner, or elder notices a pattern that should not exist.", "Keeps converting tenderness into cover stories and exhaustion.", "vampire", "family", "secrecy"),
            CreateTemplate("vampire_donor_manipulator", "Donor-dependent manipulator losing control", ScenarioTemplateKind.Vampire, "Starts with a carefully managed donor arrangement and too much confidence.", "Dependence turns charm into coercion faster than admitted.", "A donor resists, disappears, or tells someone else.", "Keeps calling it mutual right up until leverage becomes obvious.", "vampire", "feeding", "control"),
            CreateTemplate("vampire_clan_heir", "Politically trapped clan heir", ScenarioTemplateKind.Vampire, "Starts with status, surveillance, and zero private mistakes allowed.", "Faction duty keeps choking any authentic life choice.", "A succession conflict makes loyalty visibly expensive.", "Keeps surviving by becoming more useful than free.", "vampire", "politics", "status"),
            CreateTemplate("vampire_hunter_suspicion", "Vampire under hunter suspicion", ScenarioTemplateKind.Vampire, "Starts with odd sightings, thin alibis, and somebody asking sharper questions.", "Evidence accumulates socially before it accumulates legally.", "A digital photo or witness contradiction makes the pattern hard to dismiss.", "Keeps burning relationships to preserve secrecy for one more week.", "vampire", "hunter", "risk"),
            CreateTemplate("vampire_ethical_night_shift", "Ethical vampire in medical night shift world", ScenarioTemplateKind.Vampire, "Starts in a hospital ecosystem where blood access and conscience coexist badly.", "Helping people all night keeps colliding with private predation logistics.", "A blood bag audit or injury rumor lands too close to truth.", "Keeps trying to be good inside a system full of temptation and exhaustion.", "vampire", "medical", "ethical")
        };

        private static readonly List<SoftStoryArcDefinition> Arcs = new()
        {
            CreateArc("secret_exposure", "Secret exposure arc", "A secrecy-driven arc where evidence accumulates socially before it becomes undeniable.",
                new[] { "suspicious social memory", "odd injury pattern", "unusual night schedule", "witness rumor", "digital photo evidence", "confrontation opportunity" },
                new[] { "successfully hidden", "partially exposed but dismissed", "known by one trusted person", "blackmailed", "under hunter attention", "forced relocation", "total public rupture" },
                "vampire", "secrecy", "pressure"),
            CreateArc("burnout", "Burnout arc", "A pressure spiral built from ordinary neglect and collapsing recovery capacity.",
                new[] { "missed sleep", "missed meals", "missed bills", "workplace pressure", "social withdrawal", "health decline" },
                new[] { "recovery fork", "emotional collapse", "redefined life" },
                "human", "stress", "recovery"),
            CreateArc("reinvention", "Identity reinvention arc", "A looping attempt to outrun an old self through aesthetic and social redesign.",
                new[] { "new presentation", "new district", "new social orbit", "old pattern leak", "recognition scare", "self-confrontation" },
                new[] { "stabilized", "redefined life", "forced relocation" },
                "identity", "social", "loop"),
            CreateArc("caretaker_overload", "Caretaker overload arc", "An arc where useful love curdles into depletion and resentment.",
                new[] { "extra shift", "family emergency", "medical paperwork pile-up", "missed self-care", "public composure crack", "help or collapse fork" },
                new[] { "recovery fork", "emotional collapse", "known by one trusted person" },
                "caregiving", "medical", "family")
        };

        public static IReadOnlyList<ScenarioTemplateDefinition> GetTemplates() => Templates;
        public static IReadOnlyList<SoftStoryArcDefinition> GetArcs() => Arcs;

        public static ScenarioTemplateDefinition FindTemplate(string templateId)
            => Templates.Find(x => x != null && string.Equals(x.TemplateId, templateId, StringComparison.OrdinalIgnoreCase));

        public static SoftStoryArcDefinition FindArc(string arcId)
            => Arcs.Find(x => x != null && string.Equals(x.ArcId, arcId, StringComparison.OrdinalIgnoreCase));

        public static ScenarioTemplateDefinition PickTemplate(IRandomService random, ScenarioTemplateKind? kind = null)
        {
            if (random == null)
            {
                return Templates.Count > 0 ? Templates[0] : null;
            }

            List<ScenarioTemplateDefinition> candidates = Templates.FindAll(x => x != null && (!kind.HasValue || x.Kind == kind.Value));
            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[random.NextInt(0, candidates.Count)];
        }

        public static SoftStoryArcDefinition PickArc(IRandomService random, params string[] preferredTags)
        {
            if (Arcs.Count == 0)
            {
                return null;
            }

            WeightedTable<SoftStoryArcDefinition> table = new();
            for (int i = 0; i < Arcs.Count; i++)
            {
                SoftStoryArcDefinition arc = Arcs[i];
                float weight = 1f;
                if (preferredTags != null)
                {
                    for (int j = 0; j < preferredTags.Length; j++)
                    {
                        if (arc.Tags.Contains(preferredTags[j]))
                        {
                            weight += 1.2f;
                        }
                    }
                }

                table.AddOption(arc, weight);
            }

            return table.Pick(random) ?? Arcs[0];
        }

        public static ScenarioResolutionState ResolveOutcome(SoftStoryArcDefinition arc, float pressure, int incidentCount, bool vampireRisk)
        {
            if (arc == null)
            {
                return ScenarioResolutionState.Undefined;
            }

            if (string.Equals(arc.ArcId, "secret_exposure", StringComparison.OrdinalIgnoreCase))
            {
                if (pressure >= 95f || incidentCount >= 5) return ScenarioResolutionState.TotalPublicRupture;
                if (pressure >= 82f) return ScenarioResolutionState.ForcedRelocation;
                if (pressure >= 70f) return vampireRisk ? ScenarioResolutionState.UnderHunterAttention : ScenarioResolutionState.Blackmailed;
                if (pressure >= 55f) return ScenarioResolutionState.KnownByTrustedPerson;
                if (pressure >= 40f) return ScenarioResolutionState.PartiallyExposedButDismissed;
                return ScenarioResolutionState.Stabilized;
            }

            if (string.Equals(arc.ArcId, "burnout", StringComparison.OrdinalIgnoreCase) || string.Equals(arc.ArcId, "caretaker_overload", StringComparison.OrdinalIgnoreCase))
            {
                if (pressure >= 85f || incidentCount >= 4) return ScenarioResolutionState.EmotionalCollapse;
                if (pressure >= 55f) return ScenarioResolutionState.RecoveryFork;
                return ScenarioResolutionState.RedefinedLife;
            }

            if (pressure >= 78f)
            {
                return ScenarioResolutionState.ForcedRelocation;
            }

            if (pressure >= 45f)
            {
                return ScenarioResolutionState.RedefinedLife;
            }

            return ScenarioResolutionState.Stabilized;
        }

        private static ScenarioTemplateDefinition CreateTemplate(string id, string label, ScenarioTemplateKind kind, string start, string tension, string turningPoint, string alternateLoop, params string[] tags)
        {
            return new ScenarioTemplateDefinition
            {
                TemplateId = id,
                Label = label,
                Kind = kind,
                Start = start,
                CoreTension = tension,
                TurningPoint = turningPoint,
                AlternateLoop = alternateLoop,
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            };
        }

        private static SoftStoryArcDefinition CreateArc(string id, string label, string summary, string[] steps, string[] outcomes, params string[] tags)
        {
            return new SoftStoryArcDefinition
            {
                ArcId = id,
                Label = label,
                Summary = summary,
                PressureSteps = steps != null ? new List<string>(steps) : new List<string>(),
                OutcomeStates = outcomes != null ? new List<string>(outcomes) : new List<string>(),
                Tags = tags != null ? new List<string>(tags) : new List<string>()
            };
        }
    }
}
