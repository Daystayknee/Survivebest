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
        public List<string> StartHooks = new();
        public List<string> TensionHooks = new();
        public List<string> TurningPointHooks = new();
        public List<string> AlternateLoopHooks = new();
        public List<string> RecommendedArcIds = new();
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

    [Serializable]
    public sealed class ScenarioBeatRecord
    {
        public int Day;
        public string Category;
        public string Summary;
    }

    public static class ScenarioAuthorshipCatalog
    {
        private static readonly List<ScenarioTemplateDefinition> Templates = new()
        {
            CreateTemplate(
                "human_broke_unsafe_rental", "Broke young adult in unsafe rental", ScenarioTemplateKind.Human,
                "Starts in a cheap rental with cash flow already failing.",
                "Housing precarity, deferred maintenance, and adult paperwork keep stacking.",
                "A landlord warning or outage makes staying feel dangerous.",
                "Keeps stabilizing just enough to avoid collapse, then slips again.",
                new[] { "move-in with a cracked window AC and a borrowed folding table", "start with half a paycheck already spoken for", "begin in a unit where every appliance sounds temporary" },
                new[] { "the landlord stops answering maintenance texts", "the rent ledger and grocery math start fighting each other", "neighbors normalize danger because everybody is tired" },
                new[] { "a landlord warning gets taped to the door", "a power outage makes the apartment feel medically unsafe", "a friend offers a couch that solves one problem and creates three" },
                new[] { "stay and bargain with worse conditions", "leave fast and become socially unmoored", "treat instability like a normal season and keep going" },
                new[] { "burnout", "reinvention" },
                "human", "housing", "finance"),
            CreateTemplate(
                "human_teen_family_school", "Teen with family pressure and school stress", ScenarioTemplateKind.Human,
                "Starts balancing grades, expectations, and no private room to breathe.",
                "School hierarchy and family obligation pull in opposite directions.",
                "A plagiarism accusation or skipped class forces a public explanation.",
                "Keeps choosing small avoidances that become larger consequences.",
                new[] { "start with two adults projecting futures onto one exhausted teenager", "begin the semester already behind and pretending otherwise", "carry family hope like it is a second backpack" },
                new[] { "teachers read silence as laziness", "family pressure turns every grade into a referendum on character", "friends make escape look easy until the fallout lands" },
                new[] { "a plagiarism accusation makes panic look like guilt", "skipping class becomes visible to the wrong adult", "a guidance meeting exposes how thin the coping routine really is" },
                new[] { "double down and become the version everyone asked for", "quietly burn out while still technically complying", "blow up the schedule and rebuild it around survival" },
                new[] { "burnout" },
                "human", "school", "family"),
            CreateTemplate(
                "human_recently_divorced_parent", "Recently divorced parent", ScenarioTemplateKind.Human,
                "Starts with split routines, legal residue, and kids watching everything.",
                "Caregiving load outpaces emotional recovery.",
                "A custody or money conflict turns logistics back into grief.",
                "Keeps calling it co-parenting while silently living in crisis management.",
                new[] { "start with the family calendar rewritten in another person's handwriting", "begin in two half-homes that still smell like one life", "wake up determined to be calm for the children and fail by lunch" },
                new[] { "every text with the ex reopens the courtroom inside the body", "childcare costs turn healing into a luxury item", "the children notice tone long before adults admit strain" },
                new[] { "a custody disagreement drags private hurt back into official language", "a missed pickup detonates a week of fragile peace", "a school event forces both parents into the same room too soon" },
                new[] { "become efficient and lonelier", "date too early out of hunger for relief", "redefine family as something steadier than the marriage was" },
                new[] { "caretaker_overload", "burnout" },
                "human", "family", "legal"),
            CreateTemplate(
                "human_excon_stay_clean", "Ex-con trying to stay clean", ScenarioTemplateKind.Human,
                "Starts with strict routines, fragile trust, and limited opportunity.",
                "Every social tie carries old risk or needed support.",
                "A relapse-adjacent temptation or police questioning threatens progress.",
                "Keeps surviving by white-knuckling the same dangerous neighborhood patterns.",
                new[] { "start with a parole calendar and one person willing to vouch", "begin each day trying to look employable and invisible at once", "carry old reputation into every supposedly fresh start" },
                new[] { "former connections still know exactly how broke you are", "clean living is expensive in ways nobody romantic describes", "authority reads nervousness as proof of guilt" },
                new[] { "a police stop makes hard-won progress feel fictional", "easy money reappears disguised as an emergency solution", "a clean friend or sponsor finally asks for the truth" },
                new[] { "stay rigid and lonely", "slip publicly and rebuild from the wreckage", "redefine success as boring stability instead of dramatic redemption" },
                new[] { "reinvention", "burnout" },
                "human", "crime", "recovery"),
            CreateTemplate(
                "human_small_town_beauty_queen", "Small-town beauty queen stuck at home", ScenarioTemplateKind.Human,
                "Starts with a polished image and nowhere meaningful to spend it.",
                "Reputation maintenance fights boredom, resentment, and stalled ambition.",
                "A viral embarrassment or family conflict punctures the local myth.",
                "Keeps performing success to an audience that remembers high school too clearly.",
                new[] { "start with old crowns boxed beside current bills", "begin in a town where everybody recognizes the face but not the cost", "wake up overdressed for a life that stopped scaling" },
                new[] { "admiration curdles into surveillance in a place this small", "beauty remains useful long after it stops feeling like selfhood", "family proximity makes reinvention look rude" },
                new[] { "a viral embarrassment makes the curated self unsustainable", "an ex or relative weaponizes old expectations in public", "a local event forces a choice between performance and honesty" },
                new[] { "double down on the brand", "leave and let rumor explain the departure", "stay and become something less decorative and more real" },
                new[] { "reinvention" },
                "human", "reputation", "small-town"),
            CreateTemplate(
                "human_musician_with_debt", "Aspiring musician with debt", ScenarioTemplateKind.Human,
                "Starts with talent, late-night gigs, and a bank app nobody wants to open.",
                "Creative hunger collides with financial drag.",
                "A breakthrough opportunity arrives at the exact moment rent destabilizes.",
                "Keeps trading sleep, dignity, and stability for one more chance.",
                new[] { "start with unpaid demos and a phone full of almost-promises", "begin in a room where instruments take up more space than savings", "wake up hearing possibility and overdraft in the same minute" },
                new[] { "art keeps asking for risk after money has run out", "nightlife attention never pays on time", "friends cannot tell whether to encourage the dream or stage an intervention" },
                new[] { "a gig that could change everything conflicts with a bill that cannot move", "a label or collaborator shows interest when the landlord loses patience", "public praise arrives too late to soften private panic" },
                new[] { "sell out a little and survive", "stay pure and go under", "reshape the dream into something less cinematic and more durable" },
                new[] { "burnout", "reinvention" },
                "human", "creative", "finance"),
            CreateTemplate(
                "human_overworked_nurse_family", "Overworked nurse caring for family", ScenarioTemplateKind.Human,
                "Starts on a punishing night-shift schedule with relatives still needing everything.",
                "Medical labor never fully ends at the workplace door.",
                "A family emergency lands in the middle of burnout.",
                "Keeps functioning through competence while body and grief invoice the cost later.",
                new[] { "start after a twelve-hour shift with more caregiving waiting at home", "begin in fluorescent exhaustion with no clean boundary between job and love", "wake up already behind on sleep and emotional processing" },
                new[] { "everybody trusts you because you keep coming through", "healthcare competence becomes the family language for self-erasure", "night work distorts intimacy and recovery at the same time" },
                new[] { "a family emergency collides with an impossible shift", "a charting error or blood bag audit arrives on the worst possible week", "the first public crack in composure changes how everyone reads you" },
                new[] { "accept help and grieve the invincible image", "collapse and let the system call it personal weakness", "rebuild life around limits before the body imposes harsher ones" },
                new[] { "caretaker_overload", "burnout" },
                "human", "medical", "caregiving"),
            CreateTemplate(
                "vampire_newly_turned_starving", "Newly turned and starving", ScenarioTemplateKind.Vampire,
                "Starts with hunger louder than ethics and no stable ritual yet.",
                "Masquerade discipline arrives slower than appetite.",
                "A near-feeding disaster creates the first real witness risk.",
                "Keeps promising control tomorrow after every ugly night.",
                new[] { "start the first week undead and already lying badly", "begin with body changes no human routine can explain", "wake after dusk with appetite outranking identity" },
                new[] { "hunger keeps reinterpreting every moral boundary as negotiable", "night schedule anomalies become impossible to hide gracefully", "each saved mistake teaches fear faster than discipline" },
                new[] { "a feeding incident leaves marks and a witness rumor", "someone photographs the wrong moment under streetlight", "a trusted person notices the body is changing wrong" },
                new[] { "become more disciplined or more monstrous", "confess to one person and narrow the lie", "run before the first neighborhood myth hardens into evidence" },
                new[] { "secret_exposure" },
                "vampire", "hunger", "secrecy"),
            CreateTemplate(
                "vampire_ancient_reinventing", "Ancient vampire reinventing identity again", ScenarioTemplateKind.Vampire,
                "Starts another century wearing a fresh name and curated past.",
                "Old patterns keep surfacing inside each new persona.",
                "Someone recognizes a detail that should have died generations ago.",
                "Keeps abandoning entire identities before intimacy can expose repetition.",
                new[] { "start with a new wardrobe, new district, and old sorrow", "begin an elegant lie assembled from decades of practice", "wake into another reinvention already carrying historic residue" },
                new[] { "time makes style easier than authenticity", "every intimacy risk contains archival exposure", "the oldest self keeps leaking through the newest performance" },
                new[] { "someone identifies a signature habit or face fragment from the wrong century", "an archived photograph resurfaces with impossible continuity", "a lover or enemy names the repetition out loud" },
                new[] { "shed another name", "stay and risk being known deeply", "let history become part of the persona instead of the thing hidden behind it" },
                new[] { "reinvention", "secret_exposure" },
                "vampire", "identity", "history"),
            CreateTemplate(
                "vampire_hiding_in_family", "Vampire hiding in a human family household", ScenarioTemplateKind.Vampire,
                "Starts inside domestic routine where love makes every secret harder.",
                "Household intimacy keeps catching schedule, appetite, and body anomalies.",
                "A child, partner, or elder notices a pattern that should not exist.",
                "Keeps converting tenderness into cover stories and exhaustion.",
                new[] { "start with blackout curtains explained as migraines", "begin in a house where somebody always notices who ate and who did not", "wake into family love that makes disappearance morally expensive" },
                new[] { "domestic closeness catches anomalies better than surveillance does", "feeding ethics become harder inside a home built on trust", "small lies multiply because children and elders are observant in different ways" },
                new[] { "someone notices the night schedule is too perfect to be ordinary", "a neck bruise rumor enters the household orbit", "a family emergency forces exposure risk in daylight-adjacent conditions" },
                new[] { "tell one person and make the household complicit", "leave to protect them and wound them", "stay hidden and become emotionally unrecognizable" },
                new[] { "secret_exposure", "caretaker_overload" },
                "vampire", "family", "secrecy"),
            CreateTemplate(
                "vampire_donor_manipulator", "Donor-dependent manipulator losing control", ScenarioTemplateKind.Vampire,
                "Starts with a carefully managed donor arrangement and too much confidence.",
                "Dependence turns charm into coercion faster than admitted.",
                "A donor resists, disappears, or tells someone else.",
                "Keeps calling it mutual right up until leverage becomes obvious.",
                new[] { "start with a private arrangement that sounds civilized on paper", "begin by calling need a relationship because that hurts less", "wake expecting loyalty from someone who was never truly free" },
                new[] { "dependency keeps rewriting consent in self-serving language", "donor instability quickly becomes both emotional and logistical", "control feels safer each time intimacy threatens honesty" },
                new[] { "a donor pushes back in public or vanishes", "a friend uncovers the pattern and names it predation", "evidence of manipulation enters text threads and rumor systems" },
                new[] { "tighten control and become the villain", "release the donor and lose stability", "learn how much of the bond was hunger wearing romance" },
                new[] { "secret_exposure" },
                "vampire", "feeding", "control"),
            CreateTemplate(
                "vampire_clan_heir", "Politically trapped clan heir", ScenarioTemplateKind.Vampire,
                "Starts with status, surveillance, and zero private mistakes allowed.",
                "Faction duty keeps choking any authentic life choice.",
                "A succession conflict makes loyalty visibly expensive.",
                "Keeps surviving by becoming more useful than free.",
                new[] { "start with inherited privilege that feels like elegant confinement", "begin beneath faction etiquette polished over generations of threat", "wake knowing any misstep will be read politically first and personally never" },
                new[] { "legacy keeps deciding who desire is allowed to touch", "status grants insulation but abolishes privacy", "political obedience slowly devours self-authorship" },
                new[] { "a succession struggle forces a side to be chosen", "a secret alliance becomes expensive to hide", "failure to perform loyalty attracts predatory attention from kin" },
                new[] { "submit and rule later", "defect and survive smaller", "redesign power around chosen loyalty instead of inherited duty" },
                new[] { "reinvention", "secret_exposure" },
                "vampire", "politics", "status"),
            CreateTemplate(
                "vampire_hunter_suspicion", "Vampire under hunter suspicion", ScenarioTemplateKind.Vampire,
                "Starts with odd sightings, thin alibis, and somebody asking sharper questions.",
                "Evidence accumulates socially before it accumulates legally.",
                "A digital photo or witness contradiction makes the pattern hard to dismiss.",
                "Keeps burning relationships to preserve secrecy for one more week.",
                new[] { "start after one too many visible anomalies", "begin with neighborhood rumor moving faster than proof", "wake into a week where ordinary mistakes now look supernatural" },
                new[] { "witnesses compare notes more effectively than expected", "digital traces make old cover stories obsolete", "hunter logic turns coincidence into pattern with frightening speed" },
                new[] { "a digital photo survives deletion and keeps circulating", "someone compares timelines and finds a daylight impossibility", "confrontation arrives before a replacement identity is ready" },
                new[] { "successfully bury the evidence", "become known by one dangerous person", "flee before the town finishes assembling the truth" },
                new[] { "secret_exposure" },
                "vampire", "hunter", "risk"),
            CreateTemplate(
                "vampire_ethical_night_shift", "Ethical vampire in medical night shift world", ScenarioTemplateKind.Vampire,
                "Starts in a hospital ecosystem where blood access and conscience coexist badly.",
                "Helping people all night keeps colliding with private predation logistics.",
                "A blood bag audit or injury rumor lands too close to truth.",
                "Keeps trying to be good inside a system full of temptation and exhaustion.",
                new[] { "start on night shift where the smell of blood is both work and temptation", "begin with healthcare ethics stronger than the body agrees with", "wake into another rotation of competent care and private appetite" },
                new[] { "fatigue lowers the wall between service and feeding urge", "coworkers notice patterns faster than predators expect", "hospital systems generate records that are harder to seduce than people" },
                new[] { "a blood bag audit narrows the lie", "an injury rumor links too neatly to your schedule", "a coworker offers concern that feels dangerously close to discovery" },
                new[] { "confess to one trusted colleague", "leave the job before the paper trail finishes forming", "hold the ethical line and accept a smaller life to keep it" },
                new[] { "secret_exposure", "caretaker_overload" },
                "vampire", "medical", "ethical")
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

        public static SoftStoryArcDefinition PickArc(IRandomService random, ScenarioTemplateDefinition template = null, params string[] preferredTags)
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

                if (template != null && template.RecommendedArcIds != null && template.RecommendedArcIds.Contains(arc.ArcId))
                {
                    weight += 2.5f;
                }

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

        public static string PickAuthoredStart(ScenarioTemplateDefinition template, IRandomService random)
            => PickHook(template?.StartHooks, template?.Start, random, "A life starts already in motion.");

        public static string PickAuthoredTension(ScenarioTemplateDefinition template, IRandomService random)
            => PickHook(template?.TensionHooks, template?.CoreTension, random, "Pressures keep accumulating without a clean category.");

        public static string PickAuthoredTurningPoint(ScenarioTemplateDefinition template, IRandomService random)
            => PickHook(template?.TurningPointHooks, template?.TurningPoint, random, "A turning point forces hidden pressure into public consequence.");

        public static string PickAuthoredAlternateLoop(ScenarioTemplateDefinition template, IRandomService random)
            => PickHook(template?.AlternateLoopHooks, template?.AlternateLoop, random, "The scenario loops into a compromised normal that still carries risk.");

        public static List<ScenarioBeatRecord> BuildBeatTimeline(ScenarioTemplateDefinition template, SoftStoryArcDefinition arc, int daysSimulated, IRandomService random)
        {
            List<ScenarioBeatRecord> beats = new();
            int safeDays = Math.Max(1, daysSimulated);
            beats.Add(new ScenarioBeatRecord { Day = 1, Category = "start", Summary = PickAuthoredStart(template, random) });
            beats.Add(new ScenarioBeatRecord { Day = Math.Max(1, safeDays / 3), Category = "tension", Summary = PickAuthoredTension(template, random) });

            if (arc != null && arc.PressureSteps != null)
            {
                for (int i = 0; i < arc.PressureSteps.Count; i++)
                {
                    int day = Math.Min(safeDays, Math.Max(1, 1 + (i * safeDays / Math.Max(1, arc.PressureSteps.Count))));
                    beats.Add(new ScenarioBeatRecord { Day = day, Category = "arc_pressure", Summary = arc.PressureSteps[i] });
                }
            }

            beats.Add(new ScenarioBeatRecord { Day = Math.Max(2, safeDays / 2), Category = "turning_point", Summary = PickAuthoredTurningPoint(template, random) });
            beats.Add(new ScenarioBeatRecord { Day = safeDays, Category = "alternate_loop", Summary = PickAuthoredAlternateLoop(template, random) });
            beats.Sort((a, b) => a.Day.CompareTo(b.Day));
            return beats;
        }

        public static string BuildResolutionSummary(ScenarioTemplateDefinition template, SoftStoryArcDefinition arc, ScenarioResolutionState state)
        {
            string label = template != null ? template.Label : "This life";
            string arcLabel = arc != null ? arc.Label : "Unscripted arc";
            return state switch
            {
                ScenarioResolutionState.Stabilized => $"{label} ends in a tense stabilization after the {arcLabel.ToLowerInvariant()}.",
                ScenarioResolutionState.PartiallyExposedButDismissed => $"{label} leaks just enough truth to feel dangerous, but not enough to stick publicly.",
                ScenarioResolutionState.KnownByTrustedPerson => $"{label} narrows the lie to one trusted witness who now changes every future choice.",
                ScenarioResolutionState.Blackmailed => $"{label} survives, but only by entering the next loop already compromised.",
                ScenarioResolutionState.UnderHunterAttention => $"{label} remains mobile, but hunter attention makes every routine look temporary.",
                ScenarioResolutionState.ForcedRelocation => $"{label} cannot continue in the same place, so survival now means relocation.",
                ScenarioResolutionState.TotalPublicRupture => $"{label} ends in full rupture, with secrecy or composure no longer recoverable.",
                ScenarioResolutionState.RecoveryFork => $"{label} reaches a fork where help, rest, and honesty become structurally necessary.",
                ScenarioResolutionState.EmotionalCollapse => $"{label} pushes past sustainable pressure and breaks in a way that can no longer stay private.",
                ScenarioResolutionState.RedefinedLife => $"{label} does not win cleanly, but it does become something newly survivable.",
                _ => $"{label} remains unresolved inside the {arcLabel.ToLowerInvariant()}."
            };
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

        private static string PickHook(List<string> hooks, string fallback, IRandomService random, string defaultFallback)
        {
            if (hooks != null && hooks.Count > 0 && random != null)
            {
                return hooks[random.NextInt(0, hooks.Count)];
            }

            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            return defaultFallback;
        }

        private static ScenarioTemplateDefinition CreateTemplate(
            string id,
            string label,
            ScenarioTemplateKind kind,
            string start,
            string tension,
            string turningPoint,
            string alternateLoop,
            string[] startHooks,
            string[] tensionHooks,
            string[] turningPointHooks,
            string[] alternateLoopHooks,
            string[] recommendedArcIds,
            params string[] tags)
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
                StartHooks = startHooks != null ? new List<string>(startHooks) : new List<string>(),
                TensionHooks = tensionHooks != null ? new List<string>(tensionHooks) : new List<string>(),
                TurningPointHooks = turningPointHooks != null ? new List<string>(turningPointHooks) : new List<string>(),
                AlternateLoopHooks = alternateLoopHooks != null ? new List<string>(alternateLoopHooks) : new List<string>(),
                RecommendedArcIds = recommendedArcIds != null ? new List<string>(recommendedArcIds) : new List<string>(),
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
