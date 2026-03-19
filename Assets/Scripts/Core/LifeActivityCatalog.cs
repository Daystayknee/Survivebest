using UnityEngine;

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

        public static string PickTvGenre() => Pick(TvGenres, "General show");
        public static string PickMovieGenre() => Pick(MovieGenres, "General movie");
        public static string PickBookGenre() => Pick(BookGenres, "General reading");
        public static string PickSingingStyle() => Pick(SingingStyles, "Open mic");
        public static string PickRandomOutfitStyle() => Pick(OutfitStyles, "Everyday");
        public static string PickHobbyActivity() => Pick(HobbyActivities, "General hobby");
        public static string PickFamilyMoment() => Pick(FamilyMoments, "Family time");
        public static string PickDatingActivity() => Pick(DatingActivities, "Relationship time");
        public static string PickNightlifeActivity() => Pick(NightlifeActivities, "Night out");
        public static string PickCreatorEconomyActivity() => Pick(CreatorEconomyActivities, "Content grind");
        public static string PickSelfCareActivity() => Pick(SelfCareActivities, "Self-care");
        public static string PickAdultErrand() => Pick(AdultErrands, "Adult errand");
        public static string PickGigWorkActivity() => Pick(GigWorkActivities, "Gig shift");

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
