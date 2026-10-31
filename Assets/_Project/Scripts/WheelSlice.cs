public class WheelSlice
{
    public SliceType Type { get; }
    public RewardDefinition Reward { get; }   // null for the bomb; holds icon + name
    public int Amount { get; }                // already scaled for this zone
    public bool IsBomb => Type == SliceType.Bomb;

    public WheelSlice(SliceType type, RewardDefinition reward, int amount)
    {
        Type = type;
        Reward = reward;
        Amount = amount;
    }
    // constructor sets all three

}