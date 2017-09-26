using UnityEngine;
using UnityEngine.Events;

// Copied from LeapMotion SDK
public class Detector : MonoBehaviour
{
    public bool IsActive { get { return _isActive; } }
    private bool _isActive = false;
    /** Dispatched when the detector activates (becomes true). 
        * @since 4.1.2
        */
    [Tooltip("Dispatched when condition is detected.")]
    public UnityEvent OnActivate;
    /** Dispatched when the detector deactivates (becomes false). 
        * @since 4.1.2
        */
    [Tooltip("Dispatched when condition is no longer detected.")]
    public UnityEvent OnDeactivate;

    /**
    * Invoked when this detector activates.
    * Subclasses must call this function when the detector's conditions become true.
    * @since 4.1.2
    */
    public virtual void Activate()
    {
        if (!IsActive)
        {
            _isActive = true;
            OnActivate.Invoke();
        }
    }

    /**
    * Invoked when this detector deactivates.
    * Subclasses must call this function when the detector's conditions change from true to false.
    * @since 4.1.2
    */
    public virtual void Deactivate()
    {
        if (IsActive)
        {
            _isActive = false;
            OnDeactivate.Invoke();
        }
    }

    //Gizmo colors
    protected Color OnColor = Color.green;
    protected Color OffColor = Color.red;
    protected Color LimitColor = Color.blue;
    protected Color DirectionColor = Color.white;
    protected Color NormalColor = Color.gray;
}
