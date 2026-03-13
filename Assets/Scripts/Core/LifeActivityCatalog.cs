using UnityEngine;

namespace Survivebest.Core
{
    public static class LifeActivityCatalog
    {
        private static readonly string[] TvGenres = { "Sitcom", "Drama", "Documentary", "Sports", "Anime", "Reality" };
        private static readonly string[] MovieGenres = { "Comedy", "Action", "Romance", "Horror", "Family", "Thriller" };
        private static readonly string[] BookGenres = { "Fantasy", "Mystery", "Sci-fi", "Biography", "History", "Self-help" };
        private static readonly string[] SingingStyles = { "Pop", "R&B", "Rock", "Jazz", "Acoustic" };

        public static string PickTvGenre() => Pick(TvGenres, "General show");
        public static string PickMovieGenre() => Pick(MovieGenres, "General movie");
        public static string PickBookGenre() => Pick(BookGenres, "General reading");
        public static string PickSingingStyle() => Pick(SingingStyles, "Open mic");

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
