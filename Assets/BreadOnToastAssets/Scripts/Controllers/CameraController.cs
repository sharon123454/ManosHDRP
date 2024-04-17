using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    [SerializeField] private float _camMoveSpeed = 10f, _camRotationSpeed = 100f;
    [SerializeField] private float _screenMinX = -5f, _screenMaxX = 30f;
    [SerializeField] private float _screenMinZ = -5f, _screenMaxZ = 50f;
    [Range(1, 10)]
    [SerializeField] private float _camOrbitSpeed = 1f;
    [SerializeField] private float _minZoom = -4f, _maxZoom = 5f;
    [SerializeField] private float _zoomAmount = 1f, _zoomSpeed = 5f, _zoomDampening = 1.5f;
    [SerializeField] private float _lerpDistanceFromUnit = 1.5f, _lerpSpeed = 5f;
    [SerializeField] private bool _blockScreenEdge = true;
    [Range(0f, 1f)]
    [SerializeField] private float _edgePercentageToMove = 0.009f;

    private static Coroutine _lerpCameraCoroutine;
    private float _zoomHeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"There's more than one CameraController! {transform} - {Instance}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void OnEnable()
    {
        //UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        //ManosInputController.Instance.Zoom.performed += ManosInputController_OnZoomChanged;
        _zoomHeight = transform.localPosition.y;
    }
    void Update()
    {
        UpdateMovement();
        UpdateRotation();
        UpdateZoom();

        if (_blockScreenEdge)
            CheckMouseAtScreenEdge();
    }
    private void OnDisable()
    {
        //ManosInputController.Instance.Zoom.performed -= ManosInputController_OnZoomChanged;
        //UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }

    private void CheckMouseAtScreenEdge()
    {
        Vector2 mousePos = Vector2.zero;//ManosInputController.Instance.GetPointerPosition();
        Vector3 moveVector = Vector3.zero;

        if (mousePos.x < _edgePercentageToMove * Screen.width)
            moveVector -= transform.right;
        else if (mousePos.x > (1 - _edgePercentageToMove) * Screen.width)
            moveVector += transform.right;

        if (mousePos.y < _edgePercentageToMove * Screen.height)
            moveVector -= transform.forward;
        else if (mousePos.y > (1 - _edgePercentageToMove) * Screen.height)
            moveVector += transform.forward;

        if (moveVector == Vector3.zero) { return; }

        if (_lerpCameraCoroutine != null)
            StopCoroutine(_lerpCameraCoroutine);

        Vector3 newPosition = transform.position + (_camMoveSpeed * Time.deltaTime * moveVector);
        if (newPosition.x > _screenMinX && newPosition.x < _screenMaxX && newPosition.z > _screenMinZ && newPosition.z < _screenMaxZ)
            transform.position = newPosition;
    }
    private void UpdateMovement()
    {
        Vector3 moveVector = Vector3.zero;//ManosInputController.Instance.GetMoveDirection(transform);

        if (moveVector == Vector3.zero) { return; }

        if (_lerpCameraCoroutine != null)
            StopCoroutine(_lerpCameraCoroutine);

        Vector3 newPosition = transform.position + (_camMoveSpeed * Time.deltaTime * moveVector);
        if (newPosition.x > _screenMinX && newPosition.x < _screenMaxX && newPosition.z > _screenMinZ && newPosition.z < _screenMaxZ)
            transform.position = newPosition;
    }
    private void UpdateRotation()
    {
        Vector3 rotationVector = Vector3.zero;//ManosInputController.Instance.GetRotateCamBy();
        Vector3 pointerDelta = Vector3.zero;//ManosInputController.Instance.PointerDelta.ReadValue<Vector2>();

        //if (ManosInputController.Instance.RotateRight.inProgress && pointerDelta.x < 0)
        //    rotationVector.y -= _camOrbitSpeed;
        //if (ManosInputController.Instance.RotateLeft.inProgress && pointerDelta.x > 0)
        //    rotationVector.y += _camOrbitSpeed;

        if (rotationVector == Vector3.zero)
            return;

        transform.eulerAngles += _camRotationSpeed * Time.deltaTime * rotationVector;
    }
    private void UpdateZoom()
    {
        if (transform.localPosition.y == _zoomHeight)
            return;

        Vector3 zoomTarget = new Vector3(transform.localPosition.x, _zoomHeight, transform.localPosition.z);
        zoomTarget -= _zoomSpeed * (_zoomHeight - transform.localPosition.y) * _cinemachineVirtualCamera.transform.forward.normalized;

        transform.localPosition = Vector3.Lerp(transform.localPosition, zoomTarget, Time.deltaTime * _zoomDampening);
    }

    //private void ManosInputController_OnZoomChanged(InputAction.CallbackContext inputValue)
    //{
    //    float value = -inputValue.ReadValue<Vector2>().y / 120;//input is 120 so divided to get 1

    //    if (Mathf.Abs(value) > 0.1f)
    //    {
    //        _zoomHeight += value * _zoomAmount;
    //        _zoomHeight = Mathf.Clamp(_zoomHeight, _minZoom, _maxZoom);
    //    }
    //}
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, Unit newlySelectedUnit)
    {
        _lerpCameraCoroutine = StartCoroutine(LerpToUnit(newlySelectedUnit.transform.position));
    }

    private IEnumerator LerpToUnit(Vector3 unitPos)
    {
        Vector3 destination = new Vector3(unitPos.x, transform.position.y, unitPos.z);
        float distance = Vector3.Distance(transform.position, destination);
        float remainingDistance = distance;

        while (remainingDistance > _lerpDistanceFromUnit)
        {
            transform.position = Vector3.Lerp(transform.position, destination, 1 - (remainingDistance / distance));
            remainingDistance -= _lerpSpeed * Time.deltaTime;
            yield return null;
        }
    }

}