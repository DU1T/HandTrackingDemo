using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

/// <summary>
/// Detecta un gesto estatico (XRHandShape o XRHandPose) y dispara UnityEvents.
/// Version limpia del sample StaticHandGesture, sin dependencias de UI.
/// </summary>
public class HandGestureDetector : MonoBehaviour
{
    [SerializeField] XRHandTrackingEvents m_HandTrackingEvents;
    [SerializeField] ScriptableObject m_HandShapeOrPose;
    [SerializeField] Transform m_TargetTransform;
    [SerializeField] float m_MinimumHoldTime = 0.2f;
    [SerializeField] float m_GestureDetectionInterval = 0.1f;

    public UnityEvent gesturePerformed;
    public UnityEvent gestureEnded;

    XRHandShape m_HandShape;
    XRHandPose m_HandPose;
    bool m_WasDetected;
    bool m_PerformedTriggered;
    float m_TimeOfLastConditionCheck;
    float m_HoldStartTime;

    void OnEnable()
    {
        m_HandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);

        m_HandShape = m_HandShapeOrPose as XRHandShape;
        m_HandPose = m_HandShapeOrPose as XRHandPose;
        if (m_HandPose != null && m_HandPose.relativeOrientation != null)
            m_HandPose.relativeOrientation.targetTransform = m_TargetTransform;
    }

    void OnDisable() => m_HandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        if (!isActiveAndEnabled ||
            Time.timeSinceLevelLoad < m_TimeOfLastConditionCheck + m_GestureDetectionInterval)
            return;

        var detected =
            m_HandTrackingEvents.handIsTracked &&
            m_HandShape != null && m_HandShape.CheckConditions(eventArgs) ||
            m_HandPose != null && m_HandPose.CheckConditions(eventArgs);

        if (!m_WasDetected && detected)
        {
            m_HoldStartTime = Time.timeSinceLevelLoad;
        }
        else if (m_WasDetected && !detected)
        {
            m_PerformedTriggered = false;
            gestureEnded?.Invoke();
        }

        m_WasDetected = detected;

        if (!m_PerformedTriggered && detected &&
            Time.timeSinceLevelLoad - m_HoldStartTime > m_MinimumHoldTime)
        {
            gesturePerformed?.Invoke();
            m_PerformedTriggered = true;
        }
    }
}