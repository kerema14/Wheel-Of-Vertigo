using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WheelOfVertigo.Core.Data;

namespace WheelOfVertigo.Tests
{
    /// <summary>
    /// Builds throwaway zone definitions for tests. Fields are set through SerializedObject, as the inspector
    /// does, so the definitions need no setters just for testing. Call <see cref="DestroyAll"/> in TearDown.
    /// </summary>
    public sealed class TestZones
    {
        private readonly List<Object> _created = new List<Object>();

        public ZoneDefinition Create(string name, int interval, params RewardDefinition[] unlocks)
        {
            var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
            zone.name = name;

            var so = new SerializedObject(zone);
            so.FindProperty("_interval").intValue = interval;
            SerializedProperty list = so.FindProperty("_unlocks");
            list.arraySize = unlocks.Length;
            for (int i = 0; i < unlocks.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = unlocks[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            _created.Add(zone);
            return zone;
        }

        public RewardDefinition CreateReward(string name)
        {
            var reward = ScriptableObject.CreateInstance<RewardDefinition>();
            reward.name = name;
            _created.Add(reward);
            return reward;
        }

        public void DestroyAll()
        {
            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }
    }
}
