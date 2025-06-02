using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SlidePlayer : MonoBehaviour
{
    [Header("幻灯片设置")]
    [Tooltip("幻灯片幕布对象")]
    public GameObject screenObject;
    
    [Tooltip("幻灯片图片数组")]
    public Texture2D[] slideTextures;
    
    [Tooltip("幻灯片切换时间(秒)")]
    public float slideDuration = 5f;
    
    [Tooltip("幻灯片切换过渡时间(秒)")]
    public float transitionDuration = 1f;
    
    [Header("控制")]
    [Tooltip("是否自动播放")]
    public bool autoPlay = true;
    
    [Tooltip("是否循环播放")]
    public bool loop = false;
    
    // 当前幻灯片索引
    private int currentSlideIndex = 0;
    
    // 是否正在播放
    private bool isPlaying = false;
    
    // 材质引用
    private Material screenMaterial;
    
    // 下一张幻灯片的材质
    private Material nextSlideMaterial;
    
    private void Start()
    {
        // 获取幕布对象的材质
        if (screenObject != null)
        {
            Renderer renderer = screenObject.GetComponent<Renderer>();
            if (renderer != null && renderer.materials.Length > 1)
            {
                // 创建新的材质实例而不是直接使用共享材质
                screenMaterial = new Material(renderer.materials[1]);
                Material[] materials = renderer.materials;
                materials[1] = screenMaterial;
                renderer.materials = materials;
                
                // 设置材质的渲染模式为透明
                screenMaterial.SetFloat("_Surface", 1); // 1 = Transparent
                screenMaterial.SetFloat("_Blend", 0); // 0 = Alpha
                screenMaterial.SetFloat("_AlphaClip", 0); // 0 = No Alpha Clip
                screenMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                screenMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                screenMaterial.SetInt("_ZWrite", 0);
                screenMaterial.renderQueue = 3000;
                
                // 设置纹理的旋转矩阵（顺时针旋转90度）
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                    new Vector3(0.5f, 0.5f, 0), // 旋转中心点
                    Quaternion.Euler(0, 0, -90), // 顺时针旋转90度
                    Vector3.one // 缩放保持不变
                );
                // screenMaterial.SetMatrix("_BaseMap_ST", rotationMatrix);
                
                // 创建下一张幻灯片的材质
                nextSlideMaterial = new Material(screenMaterial);
                Color nextColor = Color.white;
                nextColor.a = 0f;
                nextSlideMaterial.SetColor("_BaseColor", nextColor);
                
                Debug.Log("已成功设置幻灯片材质（第二个材质）并应用90度旋转");
            }
            else
            {
                Debug.LogError("幕布对象没有足够的材质！需要至少2个材质。");
            }
        }
        else
        {
            Debug.LogError("未设置幕布对象！");
        }
        
        // 如果设置了自动播放，开始播放
        if (autoPlay)
        {
            StartPlaying();
        }
        else
        {
            JumpToSlide(0);
        }
    }
    
    /// <summary>
    /// 开始播放幻灯片
    /// </summary>
    public void StartPlaying()
    {
        if (!isPlaying && slideTextures != null && slideTextures.Length > 0)
        {
            isPlaying = true;
            currentSlideIndex = 0;
            StartCoroutine(PlaySlides());
        }
    }
    
    /// <summary>
    /// 停止播放幻灯片
    /// </summary>
    public void StopPlaying()
    {
        isPlaying = false;
        StopAllCoroutines();
    }
    
    /// <summary>
    /// 播放下一张幻灯片
    /// </summary>
    public void PlayNextSlide()
    {
        if (slideTextures != null && slideTextures.Length > 0)
        {
            currentSlideIndex = (currentSlideIndex + 1) % slideTextures.Length;
            StartCoroutine(TransitionToSlide(currentSlideIndex));
        }
    }
    
    /// <summary>
    /// 播放上一张幻灯片
    /// </summary>
    public void PlayPreviousSlide()
    {
        if (slideTextures != null && slideTextures.Length > 0)
        {
            currentSlideIndex = (currentSlideIndex - 1 + slideTextures.Length) % slideTextures.Length;
            StartCoroutine(TransitionToSlide(currentSlideIndex));
        }
    }
    
    /// <summary>
    /// 跳转到指定幻灯片
    /// </summary>
    public void JumpToSlide(int index)
    {
        if (slideTextures != null && index >= 0 && index < slideTextures.Length)
        {
            currentSlideIndex = index;
            StartCoroutine(TransitionToSlide(currentSlideIndex));
        }
    }
    
    /// <summary>
    /// 播放幻灯片序列
    /// </summary>
    private IEnumerator PlaySlides()
    {
        while (isPlaying)
        {
            // 显示当前幻灯片
            yield return StartCoroutine(TransitionToSlide(currentSlideIndex));
            
            // 等待指定时间
            yield return new WaitForSeconds(slideDuration);
            
            // 移动到下一张
            currentSlideIndex++;
            
            // 检查是否需要循环
            if (currentSlideIndex >= slideTextures.Length)
            {
                if (loop)
                {
                    currentSlideIndex = 0;
                }
                else
                {
                    isPlaying = false;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 过渡到指定幻灯片
    /// </summary>
    private IEnumerator TransitionToSlide(int index)
    {
        if (screenMaterial == null || nextSlideMaterial == null) yield break;
        
        Renderer renderer = screenObject.GetComponent<Renderer>();
        if (renderer == null || renderer.materials.Length <= 1) yield break;
        
        // 设置下一张幻灯片的纹理
        nextSlideMaterial.SetTexture("_BaseMap", slideTextures[index]);
        nextSlideMaterial.SetColor("_BaseColor", Color.white);
        
        // 设置纹理的旋转矩阵（顺时针旋转90度）
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(
            new Vector3(0.5f, 0.5f, 0), // 旋转中心点
            Quaternion.Euler(0, 0, -90), // 顺时针旋转90度
            Vector3.one // 缩放保持不变
        );
        // nextSlideMaterial.SetMatrix("_BaseMap_ST", rotationMatrix);
        
        // 淡入淡出过渡
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            
            // 当前幻灯片淡出
            Color currentColor = screenMaterial.GetColor("_BaseColor");
            currentColor.a = 1f - t;
            screenMaterial.SetColor("_BaseColor", currentColor);
            
            // 更新第二个材质
            Material[] materials = renderer.materials;
            materials[1] = screenMaterial;
            renderer.materials = materials;
            
            // 下一张幻灯片淡入
            Color nextColor = nextSlideMaterial.GetColor("_BaseColor");
            nextColor.a = t;
            nextSlideMaterial.SetColor("_BaseColor", nextColor);
            
            yield return null;
        }
        
        // 确保最终状态正确
        screenMaterial.SetTexture("_BaseMap", slideTextures[index]);
        Color finalColor = Color.white;
        finalColor.a = 1f;
        screenMaterial.SetColor("_BaseColor", finalColor);
        
        // 设置纹理的旋转矩阵（顺时针旋转90度）
        // screenMaterial.SetMatrix("_BaseMap_ST", rotationMatrix);
        
        // 更新第二个材质
        Material[] finalMaterials = renderer.materials;
        finalMaterials[1] = screenMaterial;
        renderer.materials = finalMaterials;
        
        Debug.Log($"已切换到幻灯片 {index}");
    }
    
    /// <summary>
    /// 加载幻灯片图片
    /// </summary>
    public void LoadSlides(Texture2D[] textures)
    {
        if (textures != null && textures.Length > 0)
        {
            slideTextures = textures;
            currentSlideIndex = 0;
            
            // 如果正在播放，重新开始播放
            if (isPlaying)
            {
                StopPlaying();
                StartPlaying();
            }
        }
    }
    
    /// <summary>
    /// 从Resources文件夹加载幻灯片
    /// </summary>
    public void LoadSlidesFromResources(string folderPath)
    {
        Debug.Log($"尝试从Resources/{folderPath}加载幻灯片...");
        
        // 检查文件系统
        string fullPath = System.IO.Path.Combine(Application.dataPath, "Resources", folderPath);
        Debug.Log($"检查文件夹是否存在: {fullPath}");
        if (System.IO.Directory.Exists(fullPath))
        {
            string[] files = System.IO.Directory.GetFiles(fullPath, "*.*");
            Debug.Log($"在文件夹中找到 {files.Length} 个文件:");
            foreach (string file in files)
            {
                Debug.Log($"文件: {file}");
            }
        }
        else
        {
            Debug.LogError($"文件夹不存在: {fullPath}");
        }
        
        // 列出Resources文件夹下的所有资源
        Object[] allResources = Resources.LoadAll(folderPath);
        Debug.Log($"在Resources/{folderPath}中找到 {allResources.Length} 个资源");
        foreach (Object obj in allResources)
        {
            Debug.Log($"找到资源: {obj.name}, 类型: {obj.GetType()}");
        }
        
        Texture2D[] textures = Resources.LoadAll<Texture2D>(folderPath);
        if (textures != null && textures.Length > 0)
        {
            Debug.Log($"成功加载了 {textures.Length} 张幻灯片");
            LoadSlides(textures);
        }
        else
        {
            Debug.LogError($"在Resources/{folderPath}中未找到幻灯片图片。请确保：\n1. 文件夹位于Resources目录下\n2. 图片格式正确（PNG/JPG）\n3. 图片导入设置正确\n\n当前完整路径: Assets/Resources/{folderPath}");
        }
    }
} 