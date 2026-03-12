namespace Survivebest.Core.Procedural
{
    public sealed class RunSeed
    {
        public int MasterSeed { get; }

        public RunSeed(int masterSeed)
        {
            MasterSeed = masterSeed;
        }

        public int Derive(string channel)
        {
            unchecked
            {
                int hash = MasterSeed;
                if (string.IsNullOrEmpty(channel))
                {
                    return hash;
                }

                for (int i = 0; i < channel.Length; i++)
                {
                    hash = (hash * 31) + channel[i];
                }

                return hash;
            }
        }
    }
}
