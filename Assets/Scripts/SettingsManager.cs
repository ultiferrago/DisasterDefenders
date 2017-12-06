using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handlest the settings menu.
/// </summary>
public class SettingsManager : MonoBehaviour {

    // Settings file
    private static PersistentData.Settings settings = PersistentData.GetSettings();

    // Disaster Start Time GUI
    public Slider DisasterStartTimeSlider;
    public InputField DisasterStartTimeField;
    private float _startTimePercent { get { return ( _currentStartTimeValue / settings.MaxInitialDisasterMoveTime ); } }
    private int _currentStartTimeValue {
        get { return settings.InitialDisasterMoveTime; }
        set {
            settings.InitialDisasterMoveTime = value;
            DisasterStartTimeSlider.value = _startTimePercent;
            DisasterStartTimeField.text = $"{_currentStartTimeValue:##}";
        }
    }

    // Disaster Delta Time GUI
    public Slider DisasterTimeDeltaSlider;
    public InputField DisasterTimeDeltaField;
    private float _currentTimeDeltaPercent { get { return ( _currentTimeDelta / settings.MaxDisasterTimeDelta ); } }
    private int _currentTimeDelta {
        get { return settings.DisasterTimeDelta; }
        set {
            settings.DisasterTimeDelta = value;
            DisasterTimeDeltaField.text = $"{_currentTimeDelta:##}";
            DisasterTimeDeltaSlider.value = _currentTimeDeltaPercent;
        }
    }

    // Max Disasters GUI
    public InputField MaxDisasterField;
    public Slider MaxDisasterSlider;
    private float _maxActiveDisastersPercent { get { return _currentMaxDisasters / settings.MaxActiveDisastersPossible; } }
    private int _currentMaxDisasters {
        get { return settings.MaxActiveDisasters; }
        set {
            settings.MaxActiveDisasters = value;
            MaxDisasterField.text = $"{_currentTimeDelta:##}";
            MaxDisasterSlider.value = _maxActiveDisastersPercent;
        }
    }

    /// <summary>
    /// Ensure that all GUI elements are up to date with setting data.
    /// </summary>
    public void Start() {
        //DisasterStartTimeSlider.onValueChanged.AddListener( ( float valueChange ) => {
        //    _currentStartTimeValue = valueChange;
        //} );

        //// Start Time UI
        //DisasterStartTimeField.text = $"{_currentStartTimeValue:##}";
        //DisasterStartTimeSlider.value = _startTimePercent;

        //// Move Time UI
        //DisasterTimeDeltaField.text = $"{_currentTimeDelta:##}";
        //DisasterTimeDeltaSlider.value = _currentTimeDeltaPercentage;

        //// Active Disasters UI
        //MaxDisasterField.text = $"{_currentMaxDisasters:##}";
        //MaxDisasterSlider.value = _maxDisasterPercent;
    }
}