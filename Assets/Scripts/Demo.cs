using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Demo : MonoBehaviour
{
    [Header("角色控制系统设置")]
    public GameObject controllerPrefab; // 可选：预制好的控制器
    public bool autoCreateController = true; // 是否自动创建控制器
    public bool useFirstPersonController = true; // 是否使用第一人称控制器

    [Header("UI设置")]
    public TextMeshProUGUI instructionsText; // 指令文本UI
    public TextMeshProUGUI statusText; // 状态文本UI
    public Button toggleControlsButton; // 切换控制指令显示的按钮
    public GameObject controlsPanel; // 控制面板

    private GameObject controllerInstance;
    private bool showingControls = true;

    void Start()
    {
        // 初始化UI
        InitializeUI();

        // 如果需要自动创建控制器
        if (autoCreateController)
        {
            CreateController();
        }
    }

    // 初始化UI
    private void InitializeUI()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(showingControls);
        }

        if (toggleControlsButton != null)
        {
            toggleControlsButton.onClick.AddListener(ToggleControlsDisplay);
        }

        UpdateInstructionsText();
    }

    // 创建角色控制器
    private void CreateController()
    {
        // 如果有预制体，直接实例化
        if (controllerPrefab != null)
        {
            controllerInstance = Instantiate(controllerPrefab);
            controllerInstance.name = "CharacterController";
            SetupController(controllerInstance);
            return;
        }

        // 否则创建新对象并添加相应组件
        controllerInstance = new GameObject("CharacterController");
        
        if (useFirstPersonController)
        {
            FirstPersonCameraController controller = controllerInstance.AddComponent<FirstPersonCameraController>();
            // 尝试查找场景中的第一个角色作为目标
            PresenterController[] presenters = FindObjectsOfType<PresenterController>();
            if (presenters.Length > 0)
            {
                controller.target = presenters[0].transform;
            }
        }

        SetupController(controllerInstance);
    }

    // 设置控制器的额外配置
    private void SetupController(GameObject controller)
    {
        if (useFirstPersonController)
        {
            FirstPersonCameraController fpController = controller.GetComponent<FirstPersonCameraController>();
            if (fpController != null && statusText != null)
            {
                // 可以在这里设置FirstPersonCameraController的其他属性
            }
        }
    }

    // 切换控制指令显示
    public void ToggleControlsDisplay()
    {
        showingControls = !showingControls;
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(showingControls);
        }
    }

    // 更新指令文本
    private void UpdateInstructionsText()
    {
        if (instructionsText == null) return;

        string instructions = "控制说明：\n" +
            "WASD - 移动角色\n" +
            "空格 - 跳跃\n" +
            "左Shift - 奔跑\n" +
            "鼠标 - 控制视角\n" +
            "F键 - 切换第一/第三人称\n" +
            "Tab键 - 切换角色\n" +
            "ESC键 - 解锁/锁定鼠标";

        instructionsText.text = instructions;
    }
} 