using UnityEngine;
using System.Collections.Generic;

namespace Survivebest.Core
{
    public static class LifeActivityCatalog
    {
        private static readonly string[] TvGenres = { "Sitcom", "Drama", "Documentary", "Sports", "Anime", "Reality", "Mystery", "Historical", "Travel", "Cooking" };
        private static readonly string[] MovieGenres = { "Comedy", "Action", "Romance", "Horror", "Family", "Thriller", "Adventure", "Musical", "Sci-Fi", "Animation" };
        private static readonly string[] BookGenres = { "Fantasy", "Mystery", "Sci-fi", "Biography", "History", "Self-help", "Poetry", "Philosophy", "Business", "True Crime" };
        private static readonly string[] SingingStyles = { "Pop", "R&B", "Rock", "Jazz", "Acoustic", "Classical", "Folk", "Soul" };
        private static readonly string[] OutfitStyles = { "Casual", "Workwear", "Formal", "Sport", "Cozy", "Streetwear", "Traditional", "Outdoor", "Evening", "Festival" };
        private static readonly string[] HobbyActivities = { "Photography Walk", "Board Games", "Sketching", "Woodworking", "Community Volunteering", "Journaling", "Pottery", "Dance Practice", "Language Study", "Bird Watching" };
        private static readonly string[] FamilyMoments = { "Family Dinner", "Story Time", "Weekend Picnic", "Movie Night", "Home Workout", "Garden Time", "Celebration Prep", "Memory Album" };
        private static readonly string[] DatingActivities = { "Dating-app swipe session", "Late-night rooftop date", "Coffee first date", "Flirty voice-note exchange", "Friends-with-benefits talk", "Couples therapy check-in", "Anniversary dinner", "Situationship boundary talk" };
        private static readonly string[] NightlifeActivities = { "Dance floor crawl", "Warehouse rave", "Cocktail bar lounge", "Karaoke night", "After-hours diner run", "House party", "Comedy club date", "Underground art opening" };
        private static readonly string[] CreatorEconomyActivities = { "Livestream setup", "Edit short-form content", "Brand pitch email", "Podcast recording", "Photo dump curation", "Subscriber Q&A", "Merch mockup session", "Late-night doomscroll analytics review" };
        private static readonly string[] SelfCareActivities = { "Therapy journaling", "Guided meditation", "Skincare reset", "Gym recovery session", "Deep-clean apartment sprint", "Meal prep block", "Digital detox hour", "Boundaries check-in" };
        private static readonly string[] AdultErrands = { "Pay rent", "Split utility bill", "Book a doctor visit", "Call your parent back", "Refill prescriptions", "Handle taxes", "Negotiate a raise", "Clean out the fridge" };
        private static readonly string[] GigWorkActivities = { "Rideshare shift", "Food delivery run", "Freelance edit sprint", "Tattoo flash booking", "Weekend market booth", "Remote client call", "Side-hustle reselling", "Night security shift" };
        private static readonly string[] SocialFeedActivities = { "Post a thirst-trap selfie", "Reply to a situationship story", "Plan brunch in the group chat", "Soft-launch a relationship", "Curate a private close-friends post", "Voice-note your best friend", "Update your dating profile", "Send a late-night meme check-in" };
        private static readonly string[] HomeUpgradeProjects = { "Apartment glow-up corner", "Gallery wall refresh", "LED mood-light setup", "Closet reset", "Bathroom shelf styling", "Cozy balcony makeover", "Desk cable-management overhaul", "Kitchen organization sprint" };
        private static readonly string[] AmbitionFocuses = { "Emergency fund grind", "Soft life reset", "Fitness comeback arc", "Creative breakthrough", "Promotion chase", "Dating confidence era", "Healing season", "Move-out plan" };
        private static readonly string[] CollectibleHobbies = { "Vinyl collecting", "Sneaker collecting", "Action figure hunting", "Comic back-issue digging", "Trading card collecting", "Thrifted mug collecting", "Keychain collecting", "Retro game collecting", "Perfume sampling", "Stamp collecting" };
        private static readonly string[] SentimentalObjects = { "concert ticket stub", "childhood blanket", "favorite hoodie", "signed baseball", "polaroid strip", "holiday ornament", "lucky coin", "handwritten recipe card", "old game cartridge", "friendship bracelet" };
        private static readonly string[] EverydayCarryItems = { "phone charger", "water bottle", "lip balm", "transit card", "headphones", "pocket notebook", "keys", "wallet", "gum pack", "hand sanitizer" };
        private static readonly string[] HumanExperienceMoments = { "late-night grocery run", "mall wandering with no plan", "thrift-store luck streak", "yard-sale find", "group-chat debate over dinner", "waiting-room small talk", "school pickup scramble", "weekend Costco run", "laundry-room negotiation", "parking-lot car snack reset" };
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
        public static string PickHomeUpgradeProject() => Pick(HomeUpgradeProjects, "Home refresh");
        public static string PickAmbitionFocus() => Pick(AmbitionFocuses, "Personal growth");
        public static string PickCollectibleHobby() => Pick(CollectibleHobbies, "Small collection");
        public static string PickSentimentalObject() => Pick(SentimentalObjects, "keepsake");
        public static string PickEverydayCarryItem() => Pick(EverydayCarryItems, "daily item");
        public static string PickHumanExperienceMoment() => Pick(HumanExperienceMoments, "everyday moment");
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
