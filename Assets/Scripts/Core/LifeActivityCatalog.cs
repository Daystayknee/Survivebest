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

        public static string PickTvGenre() => Pick(TvGenres, "General show");
        public static string PickMovieGenre() => Pick(MovieGenres, "General movie");
        public static string PickBookGenre() => Pick(BookGenres, "General reading");
        public static string PickSingingStyle() => Pick(SingingStyles, "Open mic");
        public static string PickRandomOutfitStyle() => Pick(OutfitStyles, "Everyday");
        public static string PickHobbyActivity() => Pick(HobbyActivities, "General hobby");
        public static string PickFamilyMoment() => Pick(FamilyMoments, "Family time");

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
