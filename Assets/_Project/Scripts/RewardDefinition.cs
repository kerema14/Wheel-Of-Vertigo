using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Reward Definition", fileName = "reward_")]
public class RewardDefinition : ScriptableObject
{
    [SerializeField] private string _displayName;
    [SerializeField] private Sprite _icon;

    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
}