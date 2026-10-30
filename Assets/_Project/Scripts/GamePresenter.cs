using UnityEngine;

public class GamePresenter : MonoBehaviour
{
    [SerializeField] private WheelSpinView _wheelView;
    private SlicePicker _picker;
    private System.Random _rng;
    private void Awake() { 
        _rng = new System.Random();
        _picker = new SlicePicker(_rng); }
    private void OnEnable() => _wheelView.SpinRequested += OnSpinRequested;   // connect
    private void OnDisable() => _wheelView.SpinRequested -= OnSpinRequested;   // disconnect

    private void OnSpinRequested()                                             // the handler
    {
        _wheelView.SetSpinInteractable(false);
        int index = _picker.Pick(8);
        _wheelView.PlaySpin(index, () => OnSpinFinished(index));
    }

    private void OnSpinFinished(int index) {
        //for now just reenable the button right away, in the future we will wait for the reward/kill popup to be finished.
        _wheelView.SetSpinInteractable(true);
        /* update the model, re-enable the button */ }
}