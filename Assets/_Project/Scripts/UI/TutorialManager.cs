using System;
using System.Collections;
using System.Collections.Generic;
using ArquipelagoPerdidoRPG.Core;
using ArquipelagoPerdidoRPG.Inventory;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ArquipelagoPerdidoRPG.UI
{
    public enum TutorialStepDetector
    {
        MovementKeys,
        SprintKey,
        JumpKey,
        CollectBlockWithInteract,
        InventoryOpened,
        InventoryConsumablesCategory,
        InventoryItemSelected,
        InventoryClosed
    }

    [Serializable]
    public class TutorialStep
    {
        public string id;
        [TextArea(2, 4)] public string instruction;
        public string highlightedKey;
        public TutorialStepDetector detector;
        public KeyCode keyCode;
    }

    public class TutorialManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text instructionText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueButtonText;

        [Header("Camera")]
        [SerializeField] private TutorialCinematicCamera cinematicCamera;

        [Header("Step Settings")]
        [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();
        [SerializeField] private float collectDistance = 3f;
        [SerializeField] private float collectRayDistance = 6f;

        private int _currentStepIndex;
        private int _inventoryOpenStepIndex = -1;
        private bool _collectibleInteracted;
        private Vector3 _startPlayerPosition;
        private Quaternion _startLookRotation;
        private Transform _playerRoot;
        private Transform _lookRoot;

        public bool IsCompleted { get; private set; }
        public int CurrentStepIndex => _currentStepIndex;
        public int TotalSteps => steps.Count;

        public event Action<int, TutorialStep> OnStepChanged;
        public event Action OnTutorialCompleted;

        private void Awake()
        {
            AutoResolveReferences();
            EnsureDefaultSteps();
            CacheStepIndices();
            EnsureTutorialEnvironmentFallback();
            EnsureTutorialPlayerMovementFallback();
            CacheTargets();
            SetContinueAvailability(false);
            RefreshCurrentStepUi();
        }

        private void Start()
        {
            // Failsafe: limpa estado herdado de UI/pausa antes de iniciar o tutorial.
            GameManager.Instance?.CloseAllUIAndResume();
            EnsureTutorialPlayerMovementFallback();

            // Garante inicio limpo no tutorial, mesmo com objetos persistentes de execucoes anteriores.
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.CloseInventory();
                InventoryManager.Instance.SelectCategory(ItemCategory.Consumables);
                InventoryManager.Instance.SetAllowOpen(false);
                
                // Dupla garantia: fecha UI mesmo se estava aberta
                var inventoryUI = FindAnyObjectByType<InventoryUI>();
                if (inventoryUI != null)
                {
                    inventoryUI.ForcePanelClosed();
                }
            }

            CacheTargets();
            RefreshCurrentStepUi();
            
            // Setup cinemática de abertura
            SetupAndPlayOpeningCinematic();
            
            // Triple check: garante que inventário fica fechado após 1 frame (depois de todos os eventos)
            StartCoroutine(EnsureInventoryClosedNextFrame());
        }

        private IEnumerator EnsureInventoryClosedNextFrame()
        {
            yield return null; // Aguarda 1 frame
            yield return null; // Aguarda mais 1 frame pra garantir

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ForceFullReset();
                
                var inventoryUI = FindAnyObjectByType<InventoryUI>();
                if (inventoryUI != null)
                {
                    inventoryUI.ForcePanelClosed();
                }
            }
        }

        private void SetupAndPlayOpeningCinematic()
        {
            // Procura ou cria a camera cinematográfica
            if (cinematicCamera == null)
            {
                cinematicCamera = FindAnyObjectByType<TutorialCinematicCamera>();
                if (cinematicCamera == null)
                {
                    GameObject cinematicCameraGO = new GameObject("CinematicCamera");
                    cinematicCamera = cinematicCameraGO.AddComponent<TutorialCinematicCamera>();
                }
            }

            // Posiciona no céu perto da luz (acima do spawn do player)
            if (_playerRoot != null)
            {
                Vector3 skyPos = _playerRoot.position + new Vector3(0, 50f, -30f);
                cinematicCamera.transform.position = skyPos;
                cinematicCamera.transform.LookAt(_playerRoot.position + Vector3.up * 1.5f);
            }

            // Executa a cinemática
            cinematicCamera.PlayCinematicAndSwitchToPlayer(_playerRoot);
        }

        private static void EnsureTutorialPlayerMovementFallback()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                return;
            }

            StarterAssets.FirstPersonController firstPerson = player.GetComponentInChildren<StarterAssets.FirstPersonController>(true);
            if (firstPerson != null)
            {
                firstPerson.enabled = true;
            }

            StarterAssets.StarterAssetsInputs starterInputs = player.GetComponentInChildren<StarterAssets.StarterAssetsInputs>(true);
            if (starterInputs != null)
            {
                starterInputs.enabled = true;
                starterInputs.cursorLocked = true;
                starterInputs.cursorInputForLook = true;
            }

#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.PlayerInput playerInput = player.GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>(true);
            if (playerInput != null)
            {
                playerInput.enabled = true;
                playerInput.ActivateInput();
                if (playerInput.actions != null && playerInput.actions.FindActionMap("Player", false) != null)
                {
                    playerInput.SwitchCurrentActionMap("Player");
                }
            }
#endif

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (IsCompleted || _currentStepIndex >= steps.Count)
            {
                return;
            }

            EnforceStepGuards();

            if (IsCurrentStepCompleted())
            {
                AdvanceStep();
            }
        }

        private void EnforceStepGuards()
        {
            if (InventoryManager.Instance == null)
            {
                return;
            }

            if (_currentStepIndex >= 0
                && _currentStepIndex < steps.Count
                && steps[_currentStepIndex] != null
                && steps[_currentStepIndex].detector == TutorialStepDetector.InventoryOpened)
            {
                InventoryManager.Instance.SetAllowOpen(true);
            }

            // Evita inventario aberto antes da etapa correta, mesmo com estado legado/persistente.
            if (_inventoryOpenStepIndex >= 0 && _currentStepIndex < _inventoryOpenStepIndex && InventoryManager.Instance.IsOpen)
            {
                InventoryManager.Instance.CloseInventory();
            }

            // Se a etapa atual exige categoria Consumiveis, garante categoria correta quando o inventario estiver aberto.
            if (_currentStepIndex == _inventoryOpenStepIndex + 1 && InventoryManager.Instance.IsOpen)
            {
                if (InventoryManager.Instance.SelectedCategory != ItemCategory.Consumables)
                {
                    InventoryManager.Instance.SelectCategory(ItemCategory.Consumables);
                }
            }
        }

        private bool IsCurrentStepCompleted()
        {
            TutorialStep step = steps[_currentStepIndex];
            switch (step.detector)
            {
                case TutorialStepDetector.MovementKeys:
                    return IsAnyMovementKeyPressed();

                case TutorialStepDetector.SprintKey:
                    return IsSprintPressed();

                case TutorialStepDetector.JumpKey:
                    return IsJumpPressed();

                case TutorialStepDetector.CollectBlockWithInteract:
                    return TryInteractWithCollectible();

                case TutorialStepDetector.InventoryOpened:
                    return InventoryManager.Instance != null && InventoryManager.Instance.IsOpen;

                case TutorialStepDetector.InventoryConsumablesCategory:
                    return InventoryManager.Instance != null
                        && InventoryManager.Instance.IsOpen
                        && InventoryManager.Instance.SelectedCategory == ItemCategory.Consumables;

                case TutorialStepDetector.InventoryItemSelected:
                    return InventoryManager.Instance != null
                        && InventoryManager.Instance.IsOpen
                        && InventoryManager.Instance.SelectedItem != null;

                case TutorialStepDetector.InventoryClosed:
                    return InventoryManager.Instance != null && !InventoryManager.Instance.IsOpen;

                default:
                    return false;
            }
        }

        private void AdvanceStep()
        {
            _currentStepIndex++;

            // Desbloqueia o inventario exatamente quando o jogador chega na etapa de abertura.
            if (_inventoryOpenStepIndex >= 0 && _currentStepIndex == _inventoryOpenStepIndex)
            {
                InventoryManager.Instance?.SetAllowOpen(true);
            }

            if (_currentStepIndex >= steps.Count)
            {
                FinishTutorial();
                return;
            }

            CacheTargets();
            RefreshCurrentStepUi();
            OnStepChanged?.Invoke(_currentStepIndex, steps[_currentStepIndex]);
        }

        private void FinishTutorial()
        {
            IsCompleted = true;
            SetContinueAvailability(true);
            // Restaura permissao ao sair do tutorial (entrada no OpenWorld).
            InventoryManager.Instance?.SetAllowOpen(true);

            if (instructionText != null)
            {
                instructionText.text = "Voce esta pronto.";
            }

            if (progressText != null)
            {
                progressText.text = $"Etapas concluidas: {steps.Count}/{steps.Count}";
            }

            if (statusText != null)
            {
                statusText.text = "Tutorial concluido. Clique em Continuar para entrar no OpenWorld.";
            }

            if (continueButtonText != null)
            {
                continueButtonText.text = "Entrar no OpenWorld";
            }

            OnTutorialCompleted?.Invoke();
        }

        private void RefreshCurrentStepUi()
        {
            if (_currentStepIndex < 0 || _currentStepIndex >= steps.Count)
            {
                return;
            }

            TutorialStep step = steps[_currentStepIndex];
            if (instructionText != null)
            {
                string keyToken = string.IsNullOrWhiteSpace(step.highlightedKey) ? string.Empty : $" {step.highlightedKey}";
                instructionText.text = $"Objetivo {_currentStepIndex + 1}/{steps.Count}:{keyToken}\n{step.instruction}";
            }

            if (progressText != null)
            {
                progressText.text = $"Progresso: {_currentStepIndex}/{steps.Count}";
            }

            if (statusText != null)
            {
                statusText.text = "Complete a etapa atual para avancar.";
            }

            if (continueButtonText != null)
            {
                continueButtonText.text = "Continuar (bloqueado ate concluir)";
            }
        }

        private void SetContinueAvailability(bool canContinue)
        {
            if (continueButton != null)
            {
                continueButton.interactable = canContinue;
            }
        }

        private void EnsureDefaultSteps()
        {
            // Forca o fluxo padrao em runtime para evitar listas antigas salvas na cena.
            steps = BuildDefaultSteps();
        }

        private static List<TutorialStep> BuildDefaultSteps()
        {
            return new List<TutorialStep>
            {
                new TutorialStep
                {
                    id = "move_keys",
                    detector = TutorialStepDetector.MovementKeys,
                    highlightedKey = "[WASD]",
                    instruction = "Pressione qualquer tecla de movimento (W, A, S ou D)."
                },
                new TutorialStep
                {
                    id = "sprint",
                    detector = TutorialStepDetector.SprintKey,
                    highlightedKey = "[SHIFT]",
                    instruction = "Pressione Shift para aprender a correr."
                },
                new TutorialStep
                {
                    id = "jump",
                    detector = TutorialStepDetector.JumpKey,
                    highlightedKey = "[SPACE]",
                    instruction = "Pressione Space para pular."
                },
                new TutorialStep
                {
                    id = "collect_block",
                    detector = TutorialStepDetector.CollectBlockWithInteract,
                    highlightedKey = "[E]",
                    instruction = "Aproxime-se do bloco e pressione E para coletar."
                },
                new TutorialStep
                {
                    id = "inventory_open",
                    detector = TutorialStepDetector.InventoryOpened,
                    highlightedKey = "[I]",
                    instruction = "Abra o inventario pressionando I."
                }
            };
        }

        private void CacheStepIndices()
        {
            _inventoryOpenStepIndex = -1;

            for (int i = 0; i < steps.Count; i++)
            {
                TutorialStep step = steps[i];
                if (step == null)
                {
                    continue;
                }

                if (step.detector == TutorialStepDetector.InventoryOpened)
                {
                    _inventoryOpenStepIndex = i;
                }

            }
        }

        private void CacheTargets()
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _playerRoot = player.transform;
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                _lookRoot = cam.transform;
            }
            else if (_playerRoot != null)
            {
                _lookRoot = _playerRoot;
            }

            if (_playerRoot != null)
            {
                _startPlayerPosition = _playerRoot.position;
            }

            if (_lookRoot != null)
            {
                _startLookRotation = _lookRoot.rotation;
            }
        }

        private static void EnsureTutorialEnvironmentFallback()
        {
            GameObject legacyGround = GameObject.Find("TutorialGround");
            if (legacyGround != null)
            {
                UnityEngine.Object.Destroy(legacyGround);
            }

            if (GameObject.Find("TutorialTerrain") == null)
            {
                TerrainData terrainData = new TerrainData
                {
                    heightmapResolution = 129,
                    size = new Vector3(180f, 20f, 180f)
                };

                GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                terrainObject.name = "TutorialTerrain";
                terrainObject.transform.position = new Vector3(-90f, 0f, -90f);
            }

            if (GameObject.Find("TutorialCollectibleBlock") == null)
            {
                GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Cube);
                collectible.name = "TutorialCollectibleBlock";
                collectible.transform.position = new Vector3(3f, 1f, 3f);
                collectible.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }

            Light directional = UnityEngine.Object.FindAnyObjectByType<Light>();
            if (directional == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                directional = lightObj.AddComponent<Light>();
            }

            directional.type = LightType.Directional;
            directional.intensity = 1.15f;
            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void AutoResolveReferences()
        {
            Transform root = transform;
            instructionText ??= FindTextByPath(root, "Panel_TutorialInfo/Text_TutorialInfo");
            progressText ??= FindTextByPath(root, "Panel_TutorialInfo/Text_TutorialProgress");
            statusText ??= FindTextByPath(root, "Panel_TutorialInfo/Text_TutorialStatus");
            continueButton ??= FindByPath<Button>(root, "Button_Continue");
            continueButtonText ??= FindTextByPath(root, "Button_Continue/Text");
        }

        private void OnDestroy()
        {
            // Garante que a flag nao fique bloqueada caso o tutorial seja destruido de forma inesperada.
            InventoryManager.Instance?.SetAllowOpen(true);
        }

        private static T FindByPath<T>(Transform root, string path) where T : Component
        {
            if (root == null)
            {
                return null;
            }

            Transform node = root.Find(path);
            if (node == null)
            {
                return null;
            }

            return node.GetComponent<T>();
        }

        private static Text FindTextByPath(Transform root, string path)
        {
            return FindByPath<Text>(root, path);
        }

        private static bool IsKeyDown(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (keyCode)
            {
                case KeyCode.P:
                    return keyboard.pKey.wasPressedThisFrame;
                case KeyCode.I:
                    return keyboard.iKey.wasPressedThisFrame;
                case KeyCode.Escape:
                    return keyboard.escapeKey.wasPressedThisFrame;
                case KeyCode.E:
                    return keyboard.eKey.wasPressedThisFrame;
                case KeyCode.Space:
                    return keyboard.spaceKey.wasPressedThisFrame;
                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    return keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                default:
                    return false;
            }
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

        private static bool IsAnyMovementKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyboard.wKey.wasPressedThisFrame
                || keyboard.aKey.wasPressedThisFrame
                || keyboard.sKey.wasPressedThisFrame
                || keyboard.dKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.S)
                || Input.GetKeyDown(KeyCode.D);
#endif
        }

        private static bool IsSprintPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
#endif
        }

        private static bool IsJumpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        private bool TryInteractWithCollectible()
        {
            if (_collectibleInteracted)
            {
                return true;
            }

            if (!IsKeyDown(KeyCode.E))
            {
                return false;
            }

            GameObject collectible = GameObject.Find("TutorialCollectibleBlock");
            if (collectible == null)
            {
                _collectibleInteracted = true;
                return true;
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, collectRayDistance))
                {
                    if (hit.collider != null && hit.collider.gameObject == collectible)
                    {
                        UnityEngine.Object.Destroy(collectible);
                        _collectibleInteracted = true;
                        return true;
                    }
                }
            }

            if (_playerRoot == null)
            {
                CacheTargets();
            }

            Transform playerTransform = _playerRoot != null ? _playerRoot : transform;
            float distance = Vector3.Distance(playerTransform.position, collectible.transform.position);
            if (distance > Mathf.Max(collectDistance, collectRayDistance))
            {
                return false;
            }

            UnityEngine.Object.Destroy(collectible);
            _collectibleInteracted = true;
            return true;
        }
    }
}
