using UnityEngine;
using System.Collections.Generic;
using System;

namespace Survivebest.Core
{
    public static class LifeActivityCatalog
    {
        public const int MaxLifeAffirmingChoiceSetCount = 64;
        private static readonly string[] TvGenres = { "Sitcom", "Drama", "Documentary", "Sports", "Anime", "Reality", "Mystery", "Historical", "Travel", "Cooking", "Crime Procedural", "Fantasy Epic", "Financial Thriller", "Medical", "Teen Slice-of-Life" };
        private static readonly string[] MovieGenres = { "Comedy", "Action", "Romance", "Horror", "Family", "Thriller", "Adventure", "Musical", "Sci-Fi", "Animation", "Noir", "Coming-of-Age", "Biopic", "Psychological Drama", "Post-Apocalyptic" };
        private static readonly string[] BookGenres = { "Fantasy", "Mystery", "Sci-fi", "Biography", "History", "Self-help", "Poetry", "Philosophy", "Business", "True Crime", "Graphic Novel", "Memoir", "Magical Realism", "Folklore", "Ecology" };
        private static readonly string[] SingingStyles = { "Pop", "R&B", "Rock", "Jazz", "Acoustic", "Classical", "Folk", "Soul" };
        private static readonly string[] OutfitStyles = { "Casual", "Workwear", "Formal", "Sport", "Cozy", "Streetwear", "Traditional", "Outdoor", "Evening", "Festival" };
        private static readonly string[] HobbyActivities = { "Photography Walk", "Board Games", "Sketching", "Woodworking", "Community Volunteering", "Journaling", "Pottery", "Dance Practice", "Language Study", "Bird Watching", "Urban foraging class", "Miniature painting night", "Home espresso experimentation", "Patchwork sewing session", "Calligraphy drills", "Community garden volunteer hour", "DIY bicycle tune-up", "Weekend ceramics market browse", "Embroidery sampler night", "Neighborhood cleanup walk", "Community choir rehearsal", "Beginner boxing class", "Street sketch meetup", "Vintage camera restoration", "Local history archive volunteering", "Neighborhood chess ladder", "DIY terrarium build", "Weekend coding jam", "Public speaking workshop", "Plant propagation session" };
        private static readonly string[] FamilyMoments = { "Family Dinner", "Story Time", "Weekend Picnic", "Movie Night", "Home Workout", "Garden Time", "Celebration Prep", "Memory Album" };
        private static readonly string[] DatingActivities = { "Dating-app swipe session", "Late-night rooftop date", "Coffee first date", "Flirty voice-note exchange", "Friends-with-benefits talk", "Couples therapy check-in", "Anniversary dinner", "Situationship boundary talk", "flirt badly at bar", "secretly stalk ex profile", "call grandma for romantic advice you will ignore", "Sunrise breakfast date", "Therapy-informed boundary reset talk", "Museum date with voice notes after", "Long walk reconciliation attempt", "Double-date game night", "Bookstore date with matching picks", "Cookoff date challenge", "Picnic with awkward playlist", "Rainy-day board game date", "Volunteer shift together", "Retro arcade date night", "Late-night stargazing drive", "Career-support planning date", "Meet-the-friends dinner", "Relationship budget planning check-in" };
        private static readonly string[] NightlifeActivities = { "Dance floor crawl", "Warehouse rave", "Cocktail bar lounge", "Karaoke night", "After-hours diner run", "House party", "Comedy club date", "Underground art opening", "Live jazz basement set", "Late-train skyline loop", "Open-deck DJ warmup", "All-night LAN meetup" };
        private static readonly string[] CreatorEconomyActivities = { "Livestream setup", "Edit short-form content", "Brand pitch email", "Podcast recording", "Photo dump curation", "Subscriber Q&A", "Merch mockup session", "Late-night doomscroll analytics review", "Script hook brainstorming sprint", "Brand-safe comment moderation block", "Livestream clipping backlog cleanup", "Creator collab proposal drafting", "Newsletter audience recap", "Sponsor deck revision", "Weekly posting calendar build", "Thumbnail A/B test sprint", "Community poll planning", "Creator tax receipt cleanup", "Clip subtitles localization pass", "Audio cleanup and mastering session" };
        private static readonly string[] SelfCareActivities = { "Therapy journaling", "Guided meditation", "Skincare reset", "Gym recovery session", "Deep-clean apartment sprint", "Meal prep block", "Digital detox hour", "Boundaries check-in", "prep blackout curtains", "sterilize feeding tools", "doomscroll until you admit you need sleep", "Breathwork plus stretching reset", "Midweek friend check-in walk", "Sleep-hygiene routine reboot", "Declutter one stress hotspot", "Hydration + protein recovery plan", "Low-light evening wind-down", "No-phone morning block", "Sensory reset shower routine", "Rebuild weekly habits checklist", "Mindful neighborhood walk", "Unsent letters emotional release", "30-minute stretch and mobility", "Calendar overwhelm cleanup", "Personal values review", "Therapy homework reflection" };
        private static readonly string[] AdultErrands = { "Pay rent", "Split utility bill", "Book a doctor visit", "Call your parent back", "Refill prescriptions", "Handle taxes", "Negotiate a raise", "Clean out the fridge", "deep clean fridge", "check bank app", "wait in ER", "iron clothes", "Renew car registration", "Schedule dental cleaning", "Contest a billing error", "Update emergency contacts", "Plan monthly grocery budget", "Submit insurance claim", "Set up autopay reminders", "Replace lost ID paperwork", "Schedule HVAC maintenance", "Pick up school forms", "DMV address change", "File contractor invoice", "Review subscription charges", "Set household chore rotation" };
        private static readonly string[] GigWorkActivities = { "Rideshare shift", "Food delivery run", "Freelance edit sprint", "Tattoo flash booking", "Weekend market booth", "Remote client call", "Side-hustle reselling", "Night security shift", "Dog walking route", "Package sorting shift", "Event setup crew call", "Audio transcription sprint", "Virtual assistant admin block", "Furniture assembly contract", "Freelance thumbnail design batch", "Short-notice moving help", "Social media moderation shift", "Pop-up catering support", "Last-minute pet sitting", "Freelance poster design rush", "House cleaning callout", "Tech troubleshooting visit", "Event photography booking", "Podcast editing contract", "Courier bike route", "Online tutoring session" };
        private static readonly string[] SocialFeedActivities = { "Post a thirst-trap selfie", "Reply to a situationship story", "Plan brunch in the group chat", "Soft-launch a relationship", "Curate a private close-friends post", "Voice-note your best friend", "Update your dating profile", "Send a late-night meme check-in", "secretly stalk ex profile", "doomscroll in bed", "delete and rewrite a risky comment three times", "schedule tomorrow's content queue", "archive old drama screenshots", "pin your best budget tip thread", "post a sobriety progress check-in", "Post a neighborhood mutual-aid request", "Host a Q&A about your hustle setup", "Share a monthly reset checklist", "Update your boundaries status post", "Drop a gratitude carousel", "Share a no-buy month progress thread", "Post a pet adoption update", "Upload study accountability story", "Start a local event RSVP poll", "Share meal prep before/after", "Post career pivot wins", "Launch a community giveaway", "Drop a relationship boundaries PSA", "Post weekly household reset reel", "Start a neighborhood watch update thread" };
        private static readonly string[] ComputerActivities = { "Inbox zero sprint", "Install security updates", "Upgrade streaming setup", "Digital declutter session", "Learn keyboard shortcuts", "Assemble a tiny home server", "Tune PC performance profile", "Back up saves to cloud storage", "Calibrate microphone and webcam", "Organize photos by year" };
        private static readonly string[] WebChatActivities = { "Late-night web chat with friends", "Ask for tech help in a community room", "Moderate a heated hobby chat", "Plan a game night in server chat", "Join a career networking thread", "Share a progress update in support chat", "Help a new member with setup issues", "Host a Q&A in your creator room", "Patch relationship tension in direct chat", "Quietly lurk and read the room vibe" };
        private static readonly string[] MiniGameActivities = { "Puzzle speedrun challenge", "Rhythm streak warmup", "Co-op survival mini run", "Typing duel match", "Retro arcade score chase", "Tower-defense quick round", "Card strategy showdown", "Platform challenge gauntlet", "Trivia blitz session", "Boss-rush practice run" };
        private static readonly string[] HomeUpgradeProjects = { "Apartment glow-up corner", "Gallery wall refresh", "LED mood-light setup", "Closet reset", "Bathroom shelf styling", "Cozy balcony makeover", "Desk cable-management overhaul", "Kitchen organization sprint", "entryway shoe storage rebuild", "under-bed rotation bins", "noise-dampening curtain install", "DIY pantry labeling pass" };
        private static readonly string[] AmbitionFocuses = { "Emergency fund grind", "Soft life reset", "Fitness comeback arc", "Creative breakthrough", "Promotion chase", "Dating confidence era", "Healing season", "Move-out plan", "Debt-free checkpoint", "Build neighborhood trust", "Career pivot runway", "Stability before spectacle" };
        private static readonly string[] CollectibleHobbies = { "Vinyl collecting", "Sneaker collecting", "Action figure hunting", "Comic back-issue digging", "Trading card collecting", "Thrifted mug collecting", "Keychain collecting", "Retro game collecting", "Perfume sampling", "Stamp collecting" };
        private static readonly string[] SentimentalObjects = { "concert ticket stub", "childhood blanket", "favorite hoodie", "signed baseball", "polaroid strip", "holiday ornament", "lucky coin", "handwritten recipe card", "old game cartridge", "friendship bracelet" };
        private static readonly string[] EverydayCarryItems = { "phone charger", "water bottle", "lip balm", "transit card", "headphones", "pocket notebook", "keys", "wallet", "gum pack", "hand sanitizer", "mini first-aid pouch", "portable battery bank", "folding tote bag", "protein bar", "earplugs" };
        private static readonly string[] HumanExperienceMoments = { "late-night grocery run", "mall wandering with no plan", "thrift-store luck streak", "yard-sale find", "group-chat debate over dinner", "waiting-room small talk", "school pickup scramble", "weekend Costco run", "laundry-room negotiation", "parking-lot car snack reset", "make ramen at 1am", "forgotten birthday spiral", "power outage dinner", "loose pet scare on the block" };
        private static readonly string[] BodyDetails = { "waking up with a dry mouth", "needing to stretch after sleeping badly", "one eye opening slower than the other from exhaustion", "cold feet on tile floor", "feeling gross after sleeping in yesterday's clothes", "mood lifting a little after finally washing your hair", "greasy-hair doubt versus fresh-hair confidence", "discomfort from wearing tight clothes too long", "soreness after standing all shift", "bladder urgency spiking when anxiety hits", "appetite dropping under stress", "craving salty food after sweating", "eating too fast making your stomach and mood worse", "bad sleep causing clumsy moments all day", "swallowing back nausea until you can sit down", "needing a quiet minute to wake up before socializing", "running hotter and getting irritable", "relief after taking your shoes off at home", "wanting a comfort blanket even when temperature is fine" };
        private static readonly string[] HygieneMaintenanceDetails = { "brushing your teeth feels like a tiny reset", "forgetting deodorant quietly dents confidence", "a late shower makes bedtime feel human again", "washing your face after crying steadies your mood", "a dirty mirror makes grooming feel worse", "using the last clean towel triggers laundry panic", "clean sheets boost sleep quality more than just sleeping", "hair tied up versus down changes chore comfort", "sleeping in pajamas rests better than sleeping in jeans", "lipstick, scent, or grooming shifts self-image without magic stat spikes", "shaving and trim upkeep feel like maintenance, not vanity", "hands feeling sticky after cooking", "wet socks ruining a whole mood", "needing lotion when weather turns dry", "skin irritation from cheap detergent or rough weather" };
        private static readonly string[] TinyHomeLifeDetails = { "one room always gets messy first", "a chair that becomes the clothes chair", "favorite mug ritual", "junk drawer archaeology", "a kitchen light that feels too harsh at night", "stale room smell when trash gets ignored", "opening the fridge and feeling like there's nothing to eat anyway", "comfort from hearing a fan at night", "panic when toilet paper runs low", "one blanket that always gets chosen", "the sink getting overwhelming before you notice", "walking past a mess and saying you'll do it later", "a room feeling emotionally different after an argument", "home feeling safer after dishes are done", "clutter subtly raising stress", "a broken appliance making the whole day feel cursed", "relief from coming home and changing clothes immediately", "sitting on the bed for one second and losing an hour" };
        private static readonly string[] FoodEmotionDetails = { "meals reading as survival, comfort, self-care, or celebration", "toast, noodles, or cereal as low-energy depression food", "soup as the sick-day answer", "a sweet drink when emotionally fried", "coffee on an empty stomach backfiring", "leftovers becoming a whole life category", "ingredient household versus ready-food household", "eating standing in the kitchen because the day is chaos", "guilt when food spoils", "comfort food tied to memory", "not cooking because cleanup feels worse than hunger", "cooking for someone feeling different than cooking for yourself", "late-night snack when loneliness spikes", "special meal right after payday", "skipping meals to save money feeling bleak and real", "bad mood triggering weird craving combos" };
        private static readonly string[] MoneyStressDetails = { "checking your bank balance before anything fun", "postponing small pleasures because a bill is coming", "deciding whether gas, groceries, meds, or comfort wins today", "guilt after impulse buying", "cheap products causing future inconvenience", "eating differently right before payday", "technically having money but not feeling safe", "relief after a bill is finally paid", "resenting detergent, toilet paper, and trash bags as recurring costs", "replacing an item only when it becomes unbearable", "wealthier households buying convenience while poorer households buy time-consuming alternatives", "small treats feeling huge when broke", "the mood of the whole house changing on paycheck day" };
        private static readonly string[] EmotionalMicroStates = { "emotionally numb", "irritated but trying to stay polite", "fragile in a hard-to-name way", "embarrassed but pretending not to be", "restless", "overstimulated", "lonely in a room full of people", "weirdly proud after doing something basic", "touched by a small kindness", "low-grade dread", "comforted but still sad", "wanting attention but not wanting to talk", "craving control through cleaning or organizing", "feeling unattractive for no clear reason", "feeling off all day", "feeling accomplished from tiny tasks while life is heavy" };
        private static readonly string[] SocialDetails = { "greeting someone differently based on your last interaction", "awkward silence after a weird joke", "conversation sounds fine but body language shows tension", "texting courage feeling different from face-to-face courage", "waiting before replying because you're upset", "apologizing badly", "saying fake I'm fine", "overexplaining when nervous", "getting quieter around certain personalities", "being extra nice when asking for a favor", "feeling different in groups than one-on-one", "someone hovering in the kitchen because they want connection", "lingering after a conversation because neither person wants to end it", "staying polite while angry because others are nearby", "being more honest late at night", "feeling rejected by tone more than words", "housemates noticing each other's routines without explicit dialogue", "social battery draining from the wrong kind of company" };
        private static readonly string[] RelationshipRealismDetails = { "affection expressed by remembering tiny preferences", "resentment growing when obvious chores go ignored", "attraction shifting with confidence, scent, effort, tenderness, and cruelty", "repeated interruptions causing low-level hostility", "someone always saying be safe", "someone noticing when you skip eating", "someone not asking how you are because they're mad", "comfort through shared routine instead of speeches", "closeness from boring errands together", "emotional distance after practical conflicts", "a relationship feeling worse in a messy environment", "tension after one person outgrows old routines", "tiny loyalty tests like covering for someone, defending them, or saving them the last portion" };
        private static readonly string[] PresentTenseWorldDetails = { "hearing rain before opening the door", "weather changing what people wear indoors", "stepping outside and regretting an outfit instantly", "traffic noise shifting mood", "fluorescent-lit stores feeling emotionally different from warm homes", "finding one grocery item missing instead of full-apocalypse shelves", "a neighborhood dog barking at the wrong moment", "a distant siren making the world feel bigger", "a bus running late wrecking timing", "store line length changing your decision", "wet umbrella and muddy shoes creating home consequences", "the same room feeling emotionally different by day versus night" };
        private static readonly string[] TimePressureDetails = { "not enough time to do everything before work", "losing time to tiny transitions", "being too tired to do the thing you finally have time for", "choosing which basic need to neglect today", "spending the evening recovering instead of living", "one interruption cascading into a bad day", "procrastination followed by panic cleaning", "staying up late for alone time then paying for it tomorrow", "I'll do it tomorrow becoming a life pattern", "weekends vanishing into maintenance" };
        private static readonly string[] IdentityDetails = { "certain clothes becoming safe clothes", "some outfits acting as confidence armor", "mirror mood differing from actual attractiveness", "living as different versions of self at home, work, and socially", "favorite color quietly steering choices", "a hairstyle becoming part of identity", "shame around certain habits", "wanting to be seen a certain way and missing the mark", "feeling more like yourself in one room than another", "sentimental objects stabilizing identity", "style shifts after heartbreak, promotion, recovery, grief, or new confidence" };
        private static readonly string[] MemoryThroughObjectsDetails = { "a hoodie from someone important", "a cracked mug kept anyway", "a grocery receipt left on the counter", "an old perfume bottle", "a chair where hard conversations always happen", "a medicine bottle on the nightstand changing the whole vibe", "a child's drawing on the fridge", "a dead phone at the worst time", "an overstuffed purse or backpack signaling lifestyle pressure", "a half-used notebook", "a packed lunch container", "a spare blanket set aside for guests", "one object making a room feel lived in instead of decorated" };
        private static readonly string[] SurvivalButHumanDetails = { "hunger making people short-tempered, not only weaker", "cold causing hesitation to shower", "no clean clothes shrinking life choices", "low money changing food and social plans at once", "illness making tiny tasks feel huge", "poor sleep degrading work, romance, and patience together", "a dirty home slowing recovery", "embarrassment from needing help", "pushing through survival tasks while emotionally depleted", "comfort rituals functioning as survival tools" };
        private static readonly string[] TravelDetails = { "sitting in the car for a minute before going inside", "forgetting something and needing to go back", "gas anxiety in the background", "a messy passenger seat tracking life chaos", "car smell changing mood", "windshield glare, wet seats, or a hot steering wheel wearing you down", "getting home mentally tired from the trip, not just the task" };
        private static readonly string[] ShoppingDetails = { "comparing brands because of money", "buying the good version only on better weeks", "impulse aisle temptations", "checkout embarrassment when the total is too high", "forgetting one crucial item", "carrying too many bags at once instead of making two trips", "buyer's remorse versus tiny joy from a treat" };
        private static readonly string[] ClothingDetails = { "inside clothes versus outside clothes", "repeating favorite outfits too often", "outfit regret right after stepping outside", "clothes that look good but feel bad all day", "comfort outfits during stress", "changing because of smell, temperature, shame, or mood instead of fashion", "laundry delays shrinking available identity choices" };
        private static readonly string[] BedAndSleepDetails = { "one side of the bed feeling emotionally different", "doomscrolling instead of sleeping", "waking in the middle of the night and overthinking", "naps that help versus naps that ruin the day", "sleeping better after clean sheets", "bad dreams leaking into morning mood", "going to bed too late because night feels like your only free time" };
        private static readonly string[] WeatherLifeDetails = { "rain making errands feel heavier", "sunny weather causing guilt about staying inside", "humidity ruining hair and patience", "cold making showering harder", "gray skies making everything feel slower", "wind making outfits and plans annoying", "weather changing social energy, appetite, and cleaning motivation" };
        private static readonly string[] SoundDetails = { "fridge hum at night", "neighbor noise changing comfort", "hearing someone moving around the house", "TV in the background making home feel occupied", "silence after an argument feeling louder than music", "rain on windows shifting emotional tone", "certain sounds becoming comforting rituals" };
        private static readonly string[] SmellDetails = { "clean laundry smell feeling like safety", "stale room smell adding stress", "food smell lingering in clothes", "rain smell through an open window", "perfume or scent becoming part of identity", "trash smell making chores feel urgent", "someone else's scent on fabric stirring emotion" };
        private static readonly string[] BathroomDetails = { "running low on toilet paper causing quiet panic", "mirror mood changing by time of day", "night bathroom trips reducing sleep quality", "feeling more human after brushing teeth", "messy sink making self-care harder", "a hot shower used as emotional coping", "skipping full grooming because energy is low" };
        private static readonly string[] KitchenDetails = { "opening cabinets repeatedly and still not knowing what to eat", "dish pile deciding whether cooking feels possible", "cooking smells creating comfort or overwhelm", "snack scavenging versus meal prep mindset", "using the last clean fork", "standing at the fridge tired and indecisive", "eating over the sink when life is chaotic" };
        private static readonly string[] CleaningDetails = { "rage cleaning", "stress cleaning", "cleaning as avoidance", "one small clean area creating relief", "clutter blindness until it suddenly feels unbearable", "I'll just do one thing turning into a reset spiral", "cleaning for guests differently than for yourself" };
        private static readonly string[] IllnessDetails = { "feeling off before fully getting sick", "sickness changing light and sound tolerance", "medicine taste becoming part of the experience", "soup, blanket, or shower becoming survival ritual", "weird guilt from resting", "getting behind on life after being sick for one day", "recovery happening gradually instead of instantly" };
        private static readonly string[] PainDetails = { "headache shortening patience", "back pain changing posture and kindness", "sore feet after long days", "cramps changing mood and plans", "old injuries flaring with weather", "pain making people quieter, sharper, or needier", "relief moments feeling emotional, not only physical" };
        private static readonly string[] WorkDetails = { "pre-work dread versus post-work depletion", "uniform changing self-perception", "coming home too tired to enjoy freedom", "fake customer-service friendliness", "mental carryover from a bad shift", "needing transition time before talking to people", "paycheck day feeling like emotional weather" };
        private static readonly string[] SchoolLearningDetails = { "avoiding assignments until they become scary", "one hard task poisoning the whole day", "pride from finishing something small", "social tension with classmates or teachers", "study environment changing success", "school clutter like papers, bags, and chargers", "a tired brain changing confidence" };
        private static readonly string[] PhoneTextingDetails = { "rereading a message before replying", "typing and deleting", "delayed replies hurting feelings", "sending dry texts when tired", "checking phone for comfort not purpose", "not wanting to answer but not wanting to be alone", "tone misunderstandings through text" };
        private static readonly string[] FriendshipDetails = { "some friends being for fun and others for comfort", "not texting first because of pride", "feeling closer through errands than speeches", "friends noticing routines", "drifting apart through timing instead of drama", "easy friendships versus high-maintenance friendships", "someone becoming safe to be ugly around" };
        private static readonly string[] RomanceDetails = { "affection shown in tiny practical acts", "being extra sensitive to tone from someone you like", "getting dressed differently for them", "resentment from unequal labor", "feeling close through shared boring routines", "rejection shifting body confidence", "late-night honesty opening emotional doors" };
        private static readonly string[] FamilyDetails = { "family roles staying sticky as people change", "one person always fixing things", "one person always forgetting things", "guilt around not helping enough", "family homes holding emotional gravity", "feeling like a child again around certain relatives", "affection shown through food, reminders, nagging, or worry" };
        private static readonly string[] CommuteTransitDetails = { "missing a light and feeling the whole schedule slip", "scanning traffic apps before deciding when to leave", "train platform mood shifting by crowd density", "commute podcasts becoming emotional anchors", "arriving early but still feeling rushed", "a delayed connection derailing evening plans", "the ride home becoming decompression time" };
        private static readonly string[] UtilityBillDetails = { "checking thermostat against next month's bill anxiety", "avoiding long showers to protect utilities budget", "peak-hour electricity habits shaping routines", "a surprise bill resetting the week's mood", "negotiating lights and laundry timing at home", "running appliances at odd hours to save money", "bill-paid relief making the house feel lighter" };
        private static readonly string[] PetCareDetails = { "pet routines forcing structure on chaotic days", "guilt when a walk gets delayed", "pet mess changing morning priorities", "comfort from small pet rituals", "vet reminders creating background stress", "buying pet essentials before personal treats", "a pet's mood subtly affecting household tone" };
        private static readonly string[] DigitalOverloadDetails = { "tab overload mirroring mental overload", "notification fatigue flattening focus", "doomscrolling past the point of feeling better", "muting chats to protect attention", "screen-time guilt after midnight", "switching apps instead of resting", "clearing notifications as a fake productivity win" };
        private static readonly string[] HolidayPressureDetails = { "holiday planning fatigue before celebrations even start", "gift budgeting stress versus generosity", "family expectation friction around traditions", "social media holiday comparisons hurting mood", "cleaning rush before guests arrive", "post-holiday emotional crash", "small traditions providing real stability" };
        private static readonly string[] ChildcareLoadDetails = { "daycare pickup timing stress", "unexpected school closure reshaping the whole day", "co-parent handoff tension", "bedtime negotiation marathon", "childcare costs colliding with bills", "packing snacks, clothes, and backup plans", "guilt about screen-time tradeoffs during survival weeks" };
        private static readonly string[] DisabilityAccessDetails = { "route planning around accessible entrances", "fatigue budgeting before errands", "medication and mobility aids as daily logistics", "paperwork loops for accommodations", "sensory-friendly scheduling for public spaces", "advocating for accessibility at work or school", "small environment changes restoring independence" };
        private static readonly string[] DisasterPreparednessDetails = { "storm prep checklist before supply runs", "backup water and food rotation", "charging power banks before outage risk", "family emergency contact drill", "home hazard check before severe weather", "balancing panic buying versus calm planning", "post-event cleanup and claim paperwork fatigue" };
        private static readonly string[] CivicLifeDetails = { "local election reminders changing household conversations", "attending a school board or council meeting", "neighborhood mutual-aid coordination", "tracking law changes that affect your routine", "public transit budget votes impacting commute plans", "community trust rising after visible service fixes", "burnout from caring about every civic issue at once" };
        private static readonly string[] FaithCommunityDetails = { "weekly ritual as emotional reset", "community meal prep and volunteer duty", "belief friction inside mixed-faith households", "quiet spiritual practice before a stressful shift", "moral support from trusted elders", "holiday observance logistics and budget pressure", "rebuilding trust in community after conflict" };
        private static readonly string[] SurvivalPracticalActivities =
        {
            "Gather wood and tinder",
            "Build and maintain a fire",
            "Identify safe wild plants and berries",
            "Construct a debris shelter",
            "Build a lean-to with weatherproofing",
            "Track animal signs near water routes",
            "Set simple snare and fish traps",
            "Butcher and preserve harvested food",
            "Craft survival tools and improvised weapons",
            "Filter and boil drinking water",
            "Scout nearby terrain and mark safe routes",
            "Cook a survival meal over open fire"
        };
        private static readonly string[] LifeAffirmingIntentions =
        {
            "build trust",
            "protect their peace",
            "heal old wounds",
            "grow their craft",
            "care for their loved ones",
            "find belonging",
            "create beauty",
            "stabilize finances",
            "strengthen their body",
            "make the neighborhood safer"
        };
        private static readonly string[] LifeAffirmingActionVerbs =
        {
            "mentor",
            "repair",
            "explore",
            "practice",
            "volunteer",
            "research",
            "negotiate",
            "perform",
            "train",
            "design"
        };
        private static readonly string[] LifeAffirmingActionTargets =
        {
            "a community program",
            "a home sanctuary",
            "a local business",
            "a difficult relationship",
            "a survival route",
            "a creative project",
            "an animal rescue effort",
            "a mentorship circle",
            "a bloodline archive",
            "a civic improvement plan"
        };
        private static readonly string[] LifeAffirmingActionContexts =
        {
            "during sunrise prep",
            "during a storm warning",
            "after a tense argument",
            "on a neighborhood event day",
            "while recovering from burnout",
            "before a major trial",
            "during weekend free time",
            "after work exhaustion",
            "during a moonlit shift",
            "before a family milestone"
        };
        private static readonly string[] LifeAffirmingDomains =
        {
            "relationships",
            "health",
            "craft",
            "home",
            "community",
            "survival",
            "legacy",
            "learning",
            "leadership",
            "recovery"
        };
        private static readonly string[] LifeAffirmingCadences =
        {
            "as a daily ritual",
            "as a weekly anchor",
            "as a monthly milestone",
            "as a dawn routine",
            "as an after-work reset",
            "as a long-term plan",
            "as a quiet night practice",
            "as a season-long commitment"
        };
        private static readonly string[] LifeAffirmingUrgencies =
        {
            "starting now",
            "before tonight ends",
            "before the next shift",
            "before next week",
            "before the next full moon",
            "before the next council vote"
        };
        private static readonly Dictionary<LifeStage, string[]> OutfitStylesByLifeStage = new()
        {
            { LifeStage.Baby, new[] { "Swaddle", "Sleep Sack", "Soft Onesie", "Play Mat Set", "Weather Coverall" } },
            { LifeStage.Infant, new[] { "Printed Onesie", "Cozy Knit Set", "Daycare Outfit", "Soft Romper", "Weather Coverall" } },
            { LifeStage.Toddler, new[] { "Playdate Outfit", "Story Time Pajamas", "Park Explorer Set", "Tiny Streetwear", "Mini Formalwear" } },
            { LifeStage.Child, new[] { "School Casual", "Sport Uniform", "Birthday Party Outfit", "Rainy Day Layers", "Youth Streetwear", "Family Photo Look" } },
            { LifeStage.Preteen, new[] { "Club Activity Fit", "Weekend Casual", "Youth Streetwear", "Rec Center Sport Set", "Preteen Formalwear", "Sleepover Cozy Set" } },
            { LifeStage.Teen, new[] { "Teen Streetwear", "Study Session Layers", "Date Night Fit", "School Formalwear", "Gig Outfit", "Athleisure" } },
            { LifeStage.YoungAdult, new[] { "Brunch Casual", "Office Workwear", "Gym Athleisure", "Nightlife Look", "Festival Fit", "Apartment Cozywear", "Creator Streetwear" } },
            { LifeStage.Adult, new[] { "Workwear", "Business Formal", "Weekend Casual", "Parent-on-the-go Utility", "Dinner Date Look", "Outdoor Layers", "Loungewear" } },
            { LifeStage.OlderAdult, new[] { "Refined Casual", "Garden Utility", "Warm Layered Knitwear", "Celebration Formal", "Walking Outfit", "Relaxed Eveningwear" } },
            { LifeStage.Elder, new[] { "Soft Knit Set", "Sunday Best", "Comfort Layers", "Weatherproof Outerwear", "Family Gathering Formal", "Home Lounge Set" } }
        };

        public static string PickTvGenre() => Pick(TvGenres, "General show");
        public static string PickMovieGenre() => Pick(MovieGenres, "General movie");
        public static string PickBookGenre() => Pick(BookGenres, "General reading");
        public static string PickSingingStyle() => Pick(SingingStyles, "Open mic");
        public static string PickRandomOutfitStyle() => Pick(OutfitStyles, "Everyday");
        public static string PickRandomOutfitStyle(LifeStage lifeStage) => Pick(GetOutfitStylesForLifeStage(lifeStage), "Everyday");
        public static string PickHobbyActivity() => Pick(HobbyActivities, "General hobby");
        public static string PickFamilyMoment() => Pick(FamilyMoments, "Family time");
        public static string PickDatingActivity() => Pick(DatingActivities, "Relationship time");
        public static string PickNightlifeActivity() => Pick(NightlifeActivities, "Night out");
        public static string PickCreatorEconomyActivity() => Pick(CreatorEconomyActivities, "Content grind");
        public static string PickSelfCareActivity() => Pick(SelfCareActivities, "Self-care");
        public static string PickAdultErrand() => Pick(AdultErrands, "Adult errand");
        public static string PickGigWorkActivity() => Pick(GigWorkActivities, "Gig shift");
        public static string PickSocialFeedActivity() => Pick(SocialFeedActivities, "Social check-in");
        public static string PickComputerActivity() => Pick(ComputerActivities, "Computer time");
        public static string PickWebChatActivity() => Pick(WebChatActivities, "Web chat");
        public static string PickMiniGameActivity() => Pick(MiniGameActivities, "Mini game");
        public static string PickHomeUpgradeProject() => Pick(HomeUpgradeProjects, "Home refresh");
        public static string PickAmbitionFocus() => Pick(AmbitionFocuses, "Personal growth");
        public static string PickCollectibleHobby() => Pick(CollectibleHobbies, "Small collection");
        public static string PickSentimentalObject() => Pick(SentimentalObjects, "keepsake");
        public static string PickEverydayCarryItem() => Pick(EverydayCarryItems, "daily item");
        public static string PickHumanExperienceMoment() => Pick(HumanExperienceMoments, "everyday moment");
        public static string PickBodyDetail() => Pick(BodyDetails, "small body signal");
        public static string PickHygieneMaintenanceDetail() => Pick(HygieneMaintenanceDetails, "hygiene maintenance beat");
        public static string PickTinyHomeLifeDetail() => Pick(TinyHomeLifeDetails, "tiny home-life beat");
        public static string PickFoodEmotionDetail() => Pick(FoodEmotionDetails, "food-emotion beat");
        public static string PickMoneyStressDetail() => Pick(MoneyStressDetails, "money-stress beat");
        public static string PickEmotionalMicroState() => Pick(EmotionalMicroStates, "in-between emotional state");
        public static string PickSocialDetail() => Pick(SocialDetails, "social detail beat");
        public static string PickRelationshipRealismDetail() => Pick(RelationshipRealismDetails, "relationship realism beat");
        public static string PickPresentTenseWorldDetail() => Pick(PresentTenseWorldDetails, "present-tense world detail");
        public static string PickTimePressureDetail() => Pick(TimePressureDetails, "time-pressure detail");
        public static string PickIdentityDetail() => Pick(IdentityDetails, "identity detail");
        public static string PickMemoryThroughObjectsDetail() => Pick(MemoryThroughObjectsDetails, "memory-through-objects detail");
        public static string PickSurvivalButHumanDetail() => Pick(SurvivalButHumanDetails, "survival-but-human detail");
        public static string PickTravelDetail() => Pick(TravelDetails, "travel detail");
        public static string PickShoppingDetail() => Pick(ShoppingDetails, "shopping detail");
        public static string PickClothingDetail() => Pick(ClothingDetails, "clothing detail");
        public static string PickBedAndSleepDetail() => Pick(BedAndSleepDetails, "bed and sleep detail");
        public static string PickWeatherLifeDetail() => Pick(WeatherLifeDetails, "weather life detail");
        public static string PickSoundDetail() => Pick(SoundDetails, "sound detail");
        public static string PickSmellDetail() => Pick(SmellDetails, "smell detail");
        public static string PickBathroomDetail() => Pick(BathroomDetails, "bathroom detail");
        public static string PickKitchenDetail() => Pick(KitchenDetails, "kitchen detail");
        public static string PickCleaningDetail() => Pick(CleaningDetails, "cleaning detail");
        public static string PickIllnessDetail() => Pick(IllnessDetails, "illness detail");
        public static string PickPainDetail() => Pick(PainDetails, "pain detail");
        public static string PickWorkDetail() => Pick(WorkDetails, "work detail");
        public static string PickSchoolLearningDetail() => Pick(SchoolLearningDetails, "school/learning detail");
        public static string PickPhoneTextingDetail() => Pick(PhoneTextingDetails, "phone/texting detail");
        public static string PickFriendshipDetail() => Pick(FriendshipDetails, "friendship detail");
        public static string PickRomanceDetail() => Pick(RomanceDetails, "romance detail");
        public static string PickFamilyDetail() => Pick(FamilyDetails, "family detail");
        public static string PickCommuteTransitDetail() => Pick(CommuteTransitDetails, "commute/transit detail");
        public static string PickUtilityBillDetail() => Pick(UtilityBillDetails, "utility/bill detail");
        public static string PickPetCareDetail() => Pick(PetCareDetails, "pet care detail");
        public static string PickDigitalOverloadDetail() => Pick(DigitalOverloadDetails, "digital overload detail");
        public static string PickHolidayPressureDetail() => Pick(HolidayPressureDetails, "holiday pressure detail");
        public static string PickChildcareLoadDetail() => Pick(ChildcareLoadDetails, "childcare-load detail");
        public static string PickDisabilityAccessDetail() => Pick(DisabilityAccessDetails, "disability/access detail");
        public static string PickDisasterPreparednessDetail() => Pick(DisasterPreparednessDetails, "disaster preparedness detail");
        public static string PickCivicLifeDetail() => Pick(CivicLifeDetails, "civic-life detail");
        public static string PickFaithCommunityDetail() => Pick(FaithCommunityDetails, "faith/community detail");
        public static string PickSurvivalPracticalActivity() => Pick(SurvivalPracticalActivities, "basic survival task");
        public static IReadOnlyList<string> GetSurvivalPracticalActivities() => SurvivalPracticalActivities;
        public static int GetGeneratedLifeAffirmingChoiceCount()
            => LifeAffirmingIntentions.Length * LifeAffirmingActionVerbs.Length * LifeAffirmingActionTargets.Length * LifeAffirmingActionContexts.Length
               * LifeAffirmingDomains.Length * LifeAffirmingCadences.Length * LifeAffirmingUrgencies.Length;

        public static string PickLifeAffirmingChoice(string actorDescriptor = "character")
        {
            string intention = Pick(LifeAffirmingIntentions, "find stability");
            string verb = Pick(LifeAffirmingActionVerbs, "improve");
            string target = Pick(LifeAffirmingActionTargets, "their routine");
            string context = Pick(LifeAffirmingActionContexts, "today");
            string domain = Pick(LifeAffirmingDomains, "life");
            string cadence = Pick(LifeAffirmingCadences, "as an ongoing practice");
            string urgency = Pick(LifeAffirmingUrgencies, "soon");
            return $"{actorDescriptor} chooses to {verb} {target} to {intention} in {domain} {context}, {cadence}, {urgency}";
        }

        public static string PickNpcLifeAffirmingChoice(string npcId, string socialTone, string memoryTone)
            => PickLifeAffirmingChoice($"npc {npcId} trying to {socialTone} and {memoryTone}");

        public static string PickAnimalLifeAffirmingChoice(string animalId, string moodTag, string instinctTag, string caregiverId = null)
        {
            string actor = string.IsNullOrWhiteSpace(caregiverId)
                ? $"animal {animalId}"
                : $"animal {animalId} with caregiver {caregiverId}";
            return PickLifeAffirmingChoice($"{actor} as a {moodTag} trying to {instinctTag}");
        }

        public static string PickVampireLifeAffirmingChoice(string characterId, string focus)
            => PickLifeAffirmingChoice($"vampire {characterId} choosing to {focus}");

        public static IReadOnlyList<string> BuildLifeAffirmingChoiceSet(string actorDescriptor = "character", int count = 3)
            => BuildLifeAffirmingChoiceSet(actorDescriptor, count, Environment.TickCount);

        public static IReadOnlyList<string> BuildLifeAffirmingChoiceSet(string actorDescriptor, int count, int seed)
        {
            int safeCount = Mathf.Clamp(count, 1, MaxLifeAffirmingChoiceSetCount);
            List<string> results = new(safeCount);
            System.Random random = new(seed);
            HashSet<int> usedSignatures = new();

            while (results.Count < safeCount)
            {
                int signature = random.Next();
                if (!usedSignatures.Add(signature))
                {
                    continue;
                }

                int intentionIndex = (int)(Math.Abs((long)signature) % LifeAffirmingIntentions.Length);
                int verbIndex = (int)(Math.Abs((long)signature / 3) % LifeAffirmingActionVerbs.Length);
                int targetIndex = (int)(Math.Abs((long)signature / 5) % LifeAffirmingActionTargets.Length);
                int contextIndex = (int)(Math.Abs((long)signature / 7) % LifeAffirmingActionContexts.Length);
                int domainIndex = (int)(Math.Abs((long)signature / 11) % LifeAffirmingDomains.Length);
                int cadenceIndex = (int)(Math.Abs((long)signature / 13) % LifeAffirmingCadences.Length);
                int urgencyIndex = (int)(Math.Abs((long)signature / 17) % LifeAffirmingUrgencies.Length);
                string intention = LifeAffirmingIntentions[intentionIndex];
                string verb = LifeAffirmingActionVerbs[verbIndex];
                string target = LifeAffirmingActionTargets[targetIndex];
                string context = LifeAffirmingActionContexts[contextIndex];
                string domain = LifeAffirmingDomains[domainIndex];
                string cadence = LifeAffirmingCadences[cadenceIndex];
                string urgency = LifeAffirmingUrgencies[urgencyIndex];
                results.Add($"{actorDescriptor} chooses to {verb} {target} to {intention} in {domain} {context}, {cadence}, {urgency}");
            }

            return results;
        }

        public static int GetTotalChoiceCount()
        {
            return TvGenres.Length + MovieGenres.Length + BookGenres.Length + SingingStyles.Length + OutfitStyles.Length
                + HobbyActivities.Length + FamilyMoments.Length + DatingActivities.Length + NightlifeActivities.Length
                + CreatorEconomyActivities.Length + SelfCareActivities.Length + AdultErrands.Length + GigWorkActivities.Length
                + SocialFeedActivities.Length + ComputerActivities.Length + WebChatActivities.Length + MiniGameActivities.Length
                + HomeUpgradeProjects.Length + AmbitionFocuses.Length + CollectibleHobbies.Length
                + SentimentalObjects.Length + EverydayCarryItems.Length + HumanExperienceMoments.Length
                + BodyDetails.Length + HygieneMaintenanceDetails.Length + TinyHomeLifeDetails.Length
                + FoodEmotionDetails.Length + MoneyStressDetails.Length + EmotionalMicroStates.Length
                + SocialDetails.Length + RelationshipRealismDetails.Length + PresentTenseWorldDetails.Length
                + TimePressureDetails.Length + IdentityDetails.Length + MemoryThroughObjectsDetails.Length
                + SurvivalButHumanDetails.Length + TravelDetails.Length + ShoppingDetails.Length
                + ClothingDetails.Length + BedAndSleepDetails.Length + WeatherLifeDetails.Length
                + SoundDetails.Length + SmellDetails.Length + BathroomDetails.Length
                + KitchenDetails.Length + CleaningDetails.Length + IllnessDetails.Length
                + PainDetails.Length + WorkDetails.Length + SchoolLearningDetails.Length
                + PhoneTextingDetails.Length + FriendshipDetails.Length + RomanceDetails.Length
                + FamilyDetails.Length + CommuteTransitDetails.Length + UtilityBillDetails.Length
                + PetCareDetails.Length + DigitalOverloadDetails.Length + HolidayPressureDetails.Length
                + ChildcareLoadDetails.Length + DisabilityAccessDetails.Length + DisasterPreparednessDetails.Length
                + CivicLifeDetails.Length + FaithCommunityDetails.Length
                + SurvivalPracticalActivities.Length
                + GetGeneratedLifeAffirmingChoiceCount();
        }

        public static string BuildChoiceDepthSummary()
        {
            return $"LifeActivityCatalog depth: {GetTotalChoiceCount()} options across 65 authored pools plus {GetGeneratedLifeAffirmingChoiceCount()} life-affirming combinational choices.";
        }

        public static IReadOnlyList<string> GetOutfitStylesForLifeStage(LifeStage lifeStage)
            => OutfitStylesByLifeStage.TryGetValue(lifeStage, out string[] styles) && styles != null && styles.Length > 0
                ? styles
                : OutfitStyles;

        private static string Pick(string[] values, string fallback)
        {
            if (values == null || values.Length == 0)
            {
                return fallback;
            }

            return values[Random.Range(0, values.Length)];
        }
    }
}
