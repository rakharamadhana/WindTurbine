using UnityEngine;

#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
using UnityEngine.InputSystem;
#endif

// Based on Unity’s URP Template’s controller
namespace NotSlot.HandPainted2D
{
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("2D Hand Painted/Camera Controller")]
  public sealed partial class CameraController : MonoBehaviour
  {
    #region Inspector

    [Header("Movement Settings")]
    [SerializeField,
     Tooltip(
       "Exponential boost factor on translation, controllable by mouse wheel.")]
    private float boost = 3.5f;

    [SerializeField,
     Tooltip(
       "Time it takes to interpolate camera position 99% of the way to the target."),
     Range(0.001f, 1f)]
    private float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [SerializeField,
     Tooltip(
       "X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    private AnimationCurve mouseSensitivityCurve =
      new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f),
                         new Keyframe(1f, 2.5f, 0f, 0f));

    [SerializeField,
     Tooltip(
       "Time it takes to interpolate camera rotation 99% of the way to the target."),
     Range(0.001f, 1f)]
    private float rotationLerpTime = 0.01f;

    [SerializeField,
     Tooltip(
       "Whether or not to invert our Y axis for mouse input to rotation.")]
    private bool invertY = false;

    [Header("Bounds")]
    [SerializeField]
    private bool limitToBounds = false;

    [SerializeField]
    private float top = 0;

    [SerializeField]
    private float bottom = 0;

    [SerializeField]
    private float left = 0;

    [SerializeField]
    private float right = 0;

        [SerializeField] private bool lockCamera = true;        // keep camera fixed at assigned transform
        [SerializeField] private bool allowScrollZoom = false;  // optional: allow scroll to change orthographic size
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private Vector2 orthoSizeRange = new Vector2(3f, 20f);

        #endregion


        #region Fields

        private readonly State _targetState = new State();

    private readonly State _interpolatingState = new State();
        
        private Vector3 _smoothedVelocity = Vector3.zero;


#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
    private InputAction _movementAction;

    private InputAction _verticalMovementAction;

    private InputAction _lookAction;

    private InputAction _boostFactorAction;

    private bool _mouseRightButtonPressed;
#endif

        #endregion


        #region Properties

        private float BoostFactor =>
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      _boostFactorAction.ReadValue<Vector2>().y * 0.01f;
#else
      Input.mouseScrollDelta.y * 0.2f;
#endif

    private static bool IsBoostPressed
    {
      get {
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
        bool boost = Keyboard.current != null
          ? Keyboard.current.leftShiftKey.isPressed
          : false;
        boost |= Gamepad.current != null
          ? Gamepad.current.xButton.isPressed
          : false;
        return boost;
#else
        return Input.GetKey(KeyCode.LeftShift);
#endif
      }
    }

    private static bool IsEscapePressed =>
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      Keyboard.current != null ? Keyboard.current.escapeKey.isPressed : false;
#else
      Input.GetKey(KeyCode.Escape);
#endif

    private static bool IsCameraRotationAllowed
    {
      get {
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
        bool canRotate = Mouse.current != null
          ? Mouse.current.rightButton.isPressed
          : false;
        canRotate |= Gamepad.current != null
          ? Gamepad.current.rightStick.ReadValue().magnitude > 0
          : false;
        return canRotate;
#else
        return Input.GetMouseButton(1);
#endif
      }
    }

    private static bool IsRightMouseButtonDown =>
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      Mouse.current != null ? Mouse.current.rightButton.isPressed : false;
#else
      Input.GetMouseButtonDown(1);
#endif

    private static bool IsRightMouseButtonUp =>
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      Mouse.current != null ? !Mouse.current.rightButton.isPressed : false;
#else
      Input.GetMouseButtonUp(1);
#endif

    #endregion


    #region MonoBehaviour

#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
    private void Start ()
    {
      var map = new InputActionMap("Simple Camera Controller");

      _lookAction = map.AddAction("look", binding: "<Mouse>/delta");
      _movementAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
      _verticalMovementAction = map.AddAction("Vertical Movement");
      _boostFactorAction =
        map.AddAction("Boost Factor", binding: "<Mouse>/scroll");

      _lookAction.AddBinding("<Gamepad>/rightStick")
                 .WithProcessor("scaleVector2(x=15, y=15)");
      _movementAction.AddCompositeBinding("Dpad")
                     .With("Up", "<Keyboard>/w")
                     .With("Up", "<Keyboard>/upArrow")
                     .With("Down", "<Keyboard>/s")
                     .With("Down", "<Keyboard>/downArrow")
                     .With("Left", "<Keyboard>/a")
                     .With("Left", "<Keyboard>/leftArrow")
                     .With("Right", "<Keyboard>/d")
                     .With("Right", "<Keyboard>/rightArrow");
      _boostFactorAction.AddBinding("<Gamepad>/Dpad")
                        .WithProcessor("scaleVector2(x=1, y=4)");

      _movementAction.Enable();
      _lookAction.Enable();
      _verticalMovementAction.Enable();
      _boostFactorAction.Enable();
    }
#endif

    private void OnEnable ()
    {
      _targetState.SetFromTransform(transform);
      _interpolatingState.SetFromTransform(transform);
    }


        private void Update()
        {
            // Exit on Esc (keep if you still want it)
            if (IsEscapePressed)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }

            // ✅ Keep the camera exactly where it was placed in the scene
            if (lockCamera)
            {
                // Optional orthographic zoom with scroll
                if (allowScrollZoom)
                {
                    var cam = GetComponent<Camera>();
                    if (cam != null && cam.orthographic)
                    {
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
                float scroll = _boostFactorAction.ReadValue<Vector2>().y; // same source you used for BoostFactor
#else
                        float scroll = Input.mouseScrollDelta.y;
#endif
                        if (Mathf.Abs(scroll) > 0f)
                        {
                            cam.orthographicSize = Mathf.Clamp(
                                cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime,
                                orthoSizeRange.x, orthoSizeRange.y
                            );
                        }
                    }
                }

                // Ensure no drift from lingering states
                _targetState.SetFromTransform(transform);
                _interpolatingState.SetFromTransform(transform);
                _smoothedVelocity = Vector3.zero;
                return; // 🔒 Stop here: no movement/interpolation below runs
            }
        }

        #endregion


            #region Methods

        private Vector3 GetInputTranslationDirection ()
    {
      Vector3 direction = Vector3.zero;
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      var moveDelta = _movementAction.ReadValue<Vector2>();
      direction.x = moveDelta.x;
      direction.z = moveDelta.y;
#else
      if ( Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) )
        direction += Vector3.up;

      if ( Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) )
        direction += Vector3.down;

      if ( Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) )
        direction += Vector3.left;

      if ( Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) )
        direction += Vector3.right;
#endif
      return direction;
    }

    private Vector2 GetInputLookRotation ()
    {
#if ENABLE_INPUT_SYSTEM && WITH_NEW_INPUT
      return _lookAction.ReadValue<Vector2>();
#else
      return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) *
             10;
#endif
    }

    #endregion
  }
}