using System;
using UnityEngine;

public class GamePresenter : MonoBehaviour
{
    [SerializeField] private int _seed;
    [SerializeField] private WheelSpinView _wheelView;
    [SerializeField] private WheelConfig _bronzeWheelConfig;
    [SerializeField] private WheelConfig _silverWheelConfig;
    [SerializeField] private WheelConfig _goldWheelConfig;
    private SlicePicker _picker;
    private System.Random _rng;
    private ZoneRules _zoneRules;
    private GameModel _gameModel;
    private void Awake() {
        _rng = _seed == 0 ? new System.Random() : new System.Random(_seed);
        _picker = new SlicePicker(_rng);
        _zoneRules = new ZoneRules(5, 30, 10);
        _gameModel = new GameModel(NextZone());
    }

    private Zone NextZone()
    {
        if (_gameModel==null || _gameModel.CurrentZone == null) { return new Zone(1, ZoneType.Normal, new Wheel(_bronzeWheelConfig, 1, _zoneRules)); }
        int newZoneIndex = _gameModel.CurrentZone.Index + 1;
        ZoneType newZoneType = _zoneRules.GetZoneType(newZoneIndex);
        WheelConfig wheelConfig = _bronzeWheelConfig;
        switch(newZoneType){
            case ZoneType.Normal:
                wheelConfig = _bronzeWheelConfig;
                break;
            case ZoneType.Safe:
                wheelConfig = _silverWheelConfig;
                break;
            case ZoneType.Super:
                wheelConfig = _goldWheelConfig;
                break;
        }


        Zone newZone = new Zone(newZoneIndex, newZoneType, new Wheel(wheelConfig, newZoneIndex, _zoneRules));
        return newZone;
    }

    private void OnEnable() => _wheelView.SpinRequested += OnSpinRequested;   // connect
    private void OnDisable() => _wheelView.SpinRequested -= OnSpinRequested;   // disconnect

    private void OnSpinRequested()                                             // the handler
    {
        _wheelView.SetSpinInteractable(false);
        int index = _picker.Pick(_gameModel.CurrentWheel.Slices.Count);
        _gameModel.CurrentSlice = _gameModel.CurrentWheel.GetSlice(index);
        _wheelView.PlaySpin(index, () => OnSpinFinished(index));
    }

    private void OnSpinFinished(int index) {
        //for now just reenable the button right away, in the future we will wait for the reward/kill popup to be finished.
        _wheelView.SetSpinInteractable(true);
        /* update the model, re-enable the button */ }
}