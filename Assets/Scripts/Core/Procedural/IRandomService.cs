namespace Survivebest.Core.Procedural
{
    public interface IRandomService
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
        bool Roll(float chance);
    }
}
