using System.Collections.Generic;
using System.Linq;
using ArquipelagoPerdidoRPG.Core;
using ArquipelagoPerdidoRPG.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ArquipelagoPerdidoRPG.Player
{
    public class PlayerControllerBridge : MonoBehaviour
    {
        [Header("Auto Discover")]
        [SerializeField] private bool autoDiscover = true;

        [Header("Gameplay Components")]
        [SerializeField] private List<Behaviour> componentsToDisable = new List<Behaviour>();

        [Header("Input System")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string gameplayActionMap = "Player";
        [SerializeField] private string uiActionMap = "UI";

        [Header("Fallback")]
        [SerializeField] private bool enableKeyboardFallback = true;
        [SerializeField] private float fallbackLookSensitivity = 0.1f;
        [SerializeField] private bool invertFallbackLookY = true;
        [SerializeField] private bool enableDirectTutorialMovementFallback = true;
        [SerializeField] private float directFallbackMoveSpeed = 5f;
        [SerializeField] private bool diagnosticsLogs = true;

        private StarterAssets.StarterAssetsInputs _starterInputs;
        private CharacterController _characterController;
        private Transform _cameraTransform;
        private float _nextDiagnosticsTime;

        private void Awake()
        {
            if (autoDiscover)
            {
                AutoDiscoverComponents();
            }

            if (playerInput == null)
            {
                playerInput = GetComponentInChildren<PlayerInput>(true);
            }

            _starterInputs = GetComponentInChildren<StarterAssets.StarterAssetsInputs>(true);
            ResolveGameplayReferences();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (!enableKeyboardFallback)
            {
                return;
            }

            if (_starterInputs == null)
            {
                _starterInputs = GetComponentInChildren<StarterAssets.StarterAssetsInputs>(true);
                if (_starterInputs == null)
                {
                    return;
                }
            }

            bool gameplayEnabled = GameManager.Instance == null || GameManager.Instance.IsGameplayInputEnabled;
            if (!gameplayEnabled)
            {
                _starterInputs.MoveInput(Vector2.zero);
                _starterInputs.LookInput(Vector2.zero);
                _starterInputs.JumpInput(false);
                _starterInputs.SprintInput(false);
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float x = 0f;
            float y = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                x -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                x += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                y -= 1f;
            }
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                y += 1f;
            }

            Vector2 move = new Vector2(x, y);
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            Vector2 look = Vector2.zero;
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue() * fallbackLookSensitivity;
                if (invertFallbackLookY)
                {
                    delta.y = -delta.y;
                }

                look = delta;
            }

            _starterInputs.MoveInput(move);
            _starterInputs.LookInput(look);
            _starterInputs.JumpInput(keyboard.spaceKey.isPressed);
            _starterInputs.SprintInput(keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

            if (enableDirectTutorialMovementFallback)
            {
                ApplyDirectTutorialMovement(move, keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
            }
#endif
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameplayInputStateChanged += OnGameplayInputStateChanged;
                OnGameplayInputStateChanged(GameManager.Instance.IsGameplayInputEnabled);
            }
            else
            {
                OnGameplayInputStateChanged(true);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameplayInputStateChanged -= OnGameplayInputStateChanged;
            }
        }

        private void Start()
        {
            EnsureGameplayControlsIfNeeded();

            if (GameManager.Instance != null)
            {
                OnGameplayInputStateChanged(GameManager.Instance.IsGameplayInputEnabled);
            }
            else
            {
                OnGameplayInputStateChanged(true);
            }

            LogDiagnostics("Start");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureGameplayControlsIfNeeded();

            if (GameManager.Instance != null)
            {
                OnGameplayInputStateChanged(GameManager.Instance.IsGameplayInputEnabled);
            }
            else
            {
                OnGameplayInputStateChanged(true);
            }

            LogDiagnostics($"SceneLoaded:{scene.name}");
        }

        private void OnGameplayInputStateChanged(bool isEnabled)
        {
            ResolveGameplayReferences();

            for (int i = 0; i < componentsToDisable.Count; i++)
            {
                if (componentsToDisable[i] != null)
                {
                    componentsToDisable[i].enabled = isEnabled;
                }
            }

            if (playerInput != null)
            {
                if (!playerInput.enabled)
                {
                    playerInput.enabled = true;
                }

                playerInput.ActivateInput();

                string gameplayMap = string.IsNullOrWhiteSpace(gameplayActionMap) ? "Player" : gameplayActionMap;
                string targetMap = isEnabled
                    ? gameplayMap
                    : (string.IsNullOrWhiteSpace(uiActionMap) ? "UI" : uiActionMap);

                if (!string.IsNullOrWhiteSpace(targetMap))
                {
                    if (playerInput.actions != null && playerInput.actions.FindActionMap(targetMap, false) != null)
                    {
                        playerInput.SwitchCurrentActionMap(targetMap);
                    }
                    else if (playerInput.actions != null && playerInput.actions.FindActionMap(gameplayMap, false) != null)
                    {
                        playerInput.SwitchCurrentActionMap(gameplayMap);
                    }
                }
            }

            Cursor.lockState = isEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isEnabled;

            if (_starterInputs != null)
            {
                _starterInputs.cursorLocked = isEnabled;
                _starterInputs.cursorInputForLook = isEnabled;
            }

            LogDiagnostics($"OnGameplayInputStateChanged:{isEnabled}");
        }

        private void EnsureGameplayControlsIfNeeded()
        {
            ResolveGameplayReferences();

            Scene activeScene = SceneManager.GetActiveScene();
            bool isGameplayScene = activeScene.name == SceneNames.Tutorial
                || activeScene.name == SceneNames.OpenWorld
                || activeScene.name == SceneNames.LegacyGame
                || activeScene.name == SceneNames.Dungeon01
                || activeScene.name == SceneNames.Dungeon02
                || activeScene.name == SceneNames.BossRoom01
                || activeScene.name == SceneNames.Interior01
                || activeScene.name == SceneNames.Interior02;

            if (!isGameplayScene)
            {
                return;
            }

            if (InventoryManager.Instance != null && InventoryManager.Instance.IsOpen)
            {
                InventoryManager.Instance.CloseInventory();
            }

            OnGameplayInputStateChanged(true);

            string actionMap = playerInput != null && playerInput.currentActionMap != null
                ? playerInput.currentActionMap.name
                : "<null>";
            Debug.Log($"PlayerControllerBridge: gameplay controls garantidos em '{activeScene.name}'. ActionMap atual: {actionMap}");
        }

        private void ResolveGameplayReferences()
        {
            StarterAssets.FirstPersonController firstPerson = GetComponentInChildren<StarterAssets.FirstPersonController>(true);
            if (firstPerson != null)
            {
                _characterController = firstPerson.GetComponent<CharacterController>();

                StarterAssets.StarterAssetsInputs inputs = firstPerson.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (inputs != null)
                {
                    _starterInputs = inputs;
                }

                PlayerInput input = firstPerson.GetComponent<PlayerInput>();
                if (input != null)
                {
                    playerInput = input;
                }
            }

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void ApplyDirectTutorialMovement(Vector2 move, bool sprintPressed)
        {
            if (_characterController == null)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != SceneNames.Tutorial)
            {
                return;
            }

            bool gameplayEnabled = GameManager.Instance == null || GameManager.Instance.IsGameplayInputEnabled;
            if (!gameplayEnabled || move.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            Vector3 forward = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            Vector3 right = _cameraTransform != null ? _cameraTransform.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveWorld = (forward * move.y + right * move.x).normalized;
            float speed = sprintPressed ? directFallbackMoveSpeed * 1.6f : directFallbackMoveSpeed;
            _characterController.Move(moveWorld * speed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (!diagnosticsLogs)
            {
                return;
            }

            if (Time.unscaledTime < _nextDiagnosticsTime)
            {
                return;
            }

            _nextDiagnosticsTime = Time.unscaledTime + 2.5f;
            LogDiagnostics("Tick");
        }

        private void LogDiagnostics(string source)
        {
            if (!diagnosticsLogs)
            {
                return;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            string actionMap = playerInput != null && playerInput.currentActionMap != null
                ? playerInput.currentActionMap.name
                : "<null>";

            StarterAssets.FirstPersonController firstPerson = GetComponentInChildren<StarterAssets.FirstPersonController>(true);
            StarterAssets.StarterAssetsInputs starterInputs = GetComponentInChildren<StarterAssets.StarterAssetsInputs>(true);

            int playersByTag = GameObject.FindGameObjectsWithTag("Player").Length;
            int playersByName = GameObject.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Count(t => t != null && t.name == "Player");

            bool gmExists = GameManager.Instance != null;
            bool gmGameplayEnabled = gmExists && GameManager.Instance.IsGameplayInputEnabled;
            bool gmPaused = gmExists && GameManager.Instance.IsPaused;

            Debug.Log(
                $"[DIAG][PlayerControllerBridge][{source}] " +
                $"Scene={sceneName} TimeScale={Time.timeScale:0.00} " +
                $"GM.Exists={gmExists} GM.Paused={gmPaused} GM.GameplayEnabled={gmGameplayEnabled} " +
                $"PlayerInput.Exists={(playerInput != null)} PlayerInput.Enabled={(playerInput != null && playerInput.enabled)} ActionMap={actionMap} " +
                $"FPC.Exists={(firstPerson != null)} FPC.Enabled={(firstPerson != null && firstPerson.enabled)} " +
                $"Inputs.Exists={(starterInputs != null)} Inputs.Enabled={(starterInputs != null && starterInputs.enabled)} " +
                $"Cursor={Cursor.lockState}/{Cursor.visible} Players(Tag)={playersByTag} Players(Name)={playersByName}");
        }

        private void AutoDiscoverComponents()
        {
            componentsToDisable.Clear();

            Behaviour[] behaviours = GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (typeName == "FirstPersonController" || typeName == "StarterAssetsInputs" || typeName == "ThirdPersonController")
                {
                    componentsToDisable.Add(behaviour);
                }
            }
        }
    }
}
