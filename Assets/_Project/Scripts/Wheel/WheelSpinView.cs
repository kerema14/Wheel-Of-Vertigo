using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class WheelSpinView : MonoBehaviour
{
    private const int SliceCount = 8;          // later this comes from WheelConfig
    private const int FullTurns = 3;
    private const float SpinDuration = 4.5f;

    [SerializeField] private Button _spinButton;
    [SerializeField] private RectTransform _spinRoot;

    public event Action SpinRequested;

    private Tween _spinTween;

    public void SetSpinInteractable(bool value) => _spinButton.interactable = value;

    public void PlaySpin(int sliceIndex, Action onComplete)
    {
        float sliceAngle = sliceIndex * 360f / SliceCount;   // same angle the slot layout uses
        float target = -(360f * FullTurns + sliceAngle);      // clockwise, ending on the slice

        _spinTween = _spinRoot
            .DOLocalRotate(new Vector3(0f, 0f, target), SpinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => onComplete?.Invoke());
    }


    private void OnValidate()
    {
        if (_spinButton == null)
        {
            Transform t = transform.Find("ui_button_spin");
            if (t != null) _spinButton = t.GetComponent<Button>();
        }
        
    }
    private void OnEnable() => _spinButton.onClick.AddListener(HandleSpinClicked);

    private void OnDisable()
    {
        _spinButton.onClick.RemoveListener(HandleSpinClicked);
        _spinTween?.Kill();   // don't let a running spin call back into a disabled view
    }

    private void HandleSpinClicked() => SpinRequested?.Invoke();
}