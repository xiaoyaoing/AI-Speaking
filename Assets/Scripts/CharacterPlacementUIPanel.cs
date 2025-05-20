using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 控制人物椅子放置的UI面板
/// </summary>
public class CharacterPlacementUIPanel : MonoBehaviour
{
    [Header("UI组件 - 人物放置")]
    [SerializeField] private Slider coverageSlider;
    [SerializeField] private TextMeshProUGUI coverageValueText;
    
    [SerializeField] private Toggle summerToggle;
    [SerializeField] private Toggle autumnToggle;
    
    [SerializeField] private Slider maleRatioSlider;
    [SerializeField] private TextMeshProUGUI maleRatioValueText;
    
    [SerializeField] private Button placeButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button closeButton;
    
    [Header("UI组件 - 动画播放")]
    [SerializeField] private Button playAnimationsButton;
    [SerializeField] private Button stopAnimationsButton;
    [SerializeField] private Dropdown animationSelectDropdown;
    [SerializeField] private Button playSelectedAnimationButton;
    [SerializeField] private Slider animationDurationSlider;
    [SerializeField] private TextMeshProUGUI animationDurationText;
    [SerializeField] private Toggle autoRotateToggle;
    
    [Header("UI组件 - 单角色动画")]
    [SerializeField] private Button playNearestCharacterAnimationsButton;
    [SerializeField] private TextMeshProUGUI currentCharacterNameText;
    
    [Header("配置")]
    [SerializeField] private CharacterPlacementManager placementManager;
    [SerializeField] private CharacterAnimationPlayer animationPlayer;
    [SerializeField] private Canvas uiCanvas;
    
    private void Start()
    {
        // 初始设置UI组件的值和事件
        SetupUI();
        
        // 设置动画名称下拉菜单
        if (animationPlayer != null && animationSelectDropdown != null)
        {
            SetupAnimationDropdown();
        }
    }
    
    private void SetupUI()
    {
        // 检查并连接覆盖率滑动条事件
        if (coverageSlider != null)
        {
            coverageSlider.onValueChanged.AddListener(OnCoverageSliderChanged);
            OnCoverageSliderChanged(coverageSlider.value); // 初始更新文本
        }
        
        // 夏装/秋装切换
        if (summerToggle != null)
        {
            summerToggle.onValueChanged.AddListener(OnSummerToggleChanged);
        }
        
        if (autumnToggle != null)
        {
            autumnToggle.onValueChanged.AddListener(OnAutumnToggleChanged);
        }
        
        // 性别比例滑动条
        if (maleRatioSlider != null)
        {
            maleRatioSlider.onValueChanged.AddListener(OnMaleRatioSliderChanged);
            OnMaleRatioSliderChanged(maleRatioSlider.value); // 初始更新文本
        }
        
        // 按钮
        if (placeButton != null)
        {
            placeButton.onClick.AddListener(OnPlaceButtonClicked);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // 动画控制
        if (playAnimationsButton != null)
        {
            playAnimationsButton.onClick.AddListener(OnPlayAnimationsButtonClicked);
        }
        
        if (stopAnimationsButton != null)
        {
            stopAnimationsButton.onClick.AddListener(OnStopAnimationsButtonClicked);
        }
        
        if (playSelectedAnimationButton != null)
        {
            playSelectedAnimationButton.onClick.AddListener(OnPlaySelectedAnimationButtonClicked);
        }
        
        if (animationDurationSlider != null)
        {
            animationDurationSlider.onValueChanged.AddListener(OnAnimationDurationSliderChanged);
            OnAnimationDurationSliderChanged(animationDurationSlider.value); // 初始更新文本
        }
        
        if (autoRotateToggle != null)
        {
            autoRotateToggle.onValueChanged.AddListener(OnAutoRotateToggleChanged);
        }
        
        // 单角色动画控制
        if (playNearestCharacterAnimationsButton != null)
        {
            playNearestCharacterAnimationsButton.onClick.AddListener(OnPlayNearestCharacterAnimationsButtonClicked);
        }
    }
    
    private void SetupAnimationDropdown()
    {
        animationSelectDropdown.ClearOptions();
        
        string[] animationNames = animationPlayer.GetAnimationNames();
        List<string> options = new List<string>(animationNames);
        
        animationSelectDropdown.AddOptions(options);
    }
    
    private void OnCoverageSliderChanged(float value)
    {
        if (coverageValueText != null)
        {
            coverageValueText.text = (value * 100).ToString("F0") + "%";
        }
        
        if (placementManager != null)
        {
            placementManager.SetCoverageRate(value);
        }
    }
    
    private void OnSummerToggleChanged(bool isOn)
    {
        if (isOn && autumnToggle != null)
        {
            autumnToggle.isOn = false;
        }
        
        if (placementManager != null && isOn)
        {
            placementManager.SetSummerClothing(true);
        }
    }
    
    private void OnAutumnToggleChanged(bool isOn)
    {
        if (isOn && summerToggle != null)
        {
            summerToggle.isOn = false;
        }
        
        if (placementManager != null && isOn)
        {
            placementManager.SetAutumnClothing(true);
        }
    }
    
    private void OnMaleRatioSliderChanged(float value)
    {
        if (maleRatioValueText != null)
        {
            maleRatioValueText.text = $"男:{(value * 100).ToString("F0")}% 女:{((1 - value) * 100).ToString("F0")}%";
        }
        
        if (placementManager != null)
        {
            placementManager.SetMaleRatio(value);
        }
    }
    
    private void OnPlaceButtonClicked()
    {
        if (placementManager != null)
        {
            placementManager.PlaceCharactersOnChairs();
        }
    }
    
    private void OnClearButtonClicked()
    {
        if (placementManager != null)
        {
            placementManager.ClearPlacedCharacters();
        }
    }
    
    private void OnCloseButtonClicked()
    {
        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnPlayAnimationsButtonClicked()
    {
        if (animationPlayer != null)
        {
            // 获取当前已放置的角色并设置给动画播放器
            if (placementManager != null)
            {
                List<GameObject> characters = placementManager.GetPlacedCharacters();
                if (characters.Count > 0)
                {
                    animationPlayer.SetCharacters(characters);
                    animationPlayer.PlayAllAnimations();
                }
                else
                {
                    Debug.LogWarning("没有放置的角色可以播放动画");
                }
            }
            else
            {
                animationPlayer.PlayAllAnimations();
            }
        }
    }
    
    private void OnStopAnimationsButtonClicked()
    {
        if (animationPlayer != null)
        {
            animationPlayer.StopAnimations();
        }
    }
    
    private void OnPlaySelectedAnimationButtonClicked()
    {
        if (animationPlayer != null && animationSelectDropdown != null)
        {
            string selectedAnimation = animationSelectDropdown.options[animationSelectDropdown.value].text;
            
            // 获取当前已放置的角色并设置给动画播放器
            if (placementManager != null)
            {
                List<GameObject> characters = placementManager.GetPlacedCharacters();
                if (characters.Count > 0)
                {
                    animationPlayer.SetCharacters(characters);
                    animationPlayer.PlaySpecificAnimation(selectedAnimation);
                }
                else
                {
                    Debug.LogWarning("没有放置的角色可以播放动画");
                }
            }
            else
            {
                animationPlayer.PlaySpecificAnimation(selectedAnimation);
            }
        }
    }
    
    private void OnPlayNearestCharacterAnimationsButtonClicked()
    {
        if (animationPlayer != null)
        {
            // 查找最近的角色并播放所有动画
            animationPlayer.PlayAllAnimationsForNearestCharacter();
            
            // 更新当前角色名称文本
            if (currentCharacterNameText != null && animationPlayer.GetCharacterCount() > 0)
            {
                GameObject character = animationPlayer.GetCurrentCharacter();
                if (character != null)
                {
                    currentCharacterNameText.text = $"当前角色: {character.name}";
                }
            }
        }
    }
    
    private void OnAnimationDurationSliderChanged(float value)
    {
        if (animationDurationText != null)
        {
            animationDurationText.text = value.ToString("F1") + "秒";
        }
        
        if (animationPlayer != null)
        {
            animationPlayer.SetAnimationDuration(value);
        }
    }
    
    private void OnAutoRotateToggleChanged(bool isOn)
    {
        if (animationPlayer != null)
        {
            animationPlayer.SetAutoRotateCharacters(isOn);
        }
    }
    
    public void Show()
    {
        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
} 