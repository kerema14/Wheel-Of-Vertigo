using UnityEngine;

public class SlicePicker
{
    private readonly System.Random _random;
    public SlicePicker(System.Random random) => _random = random;
    public int Pick(int sliceCount) => _random.Next(sliceCount);
}