# 主场景（SampleScene）更新指南

## 更新主场景以显示用户输入的报告文本

1. 在Unity编辑器中，打开 SampleScene 场景

2. 在场景的 Canvas 下添加一个新的 UI 元素：
   - 右键点击 Canvas > UI > Text - TextMeshPro
   - 命名为 "ReportText"
   - 设置其位置为报告者面前或者合适的位置（如演讲台上）
   - 调整大小使其足够显示报告内容
   - 设置字体大小适中（如20-24）
   - 设置颜色为易于阅读的颜色（如黑色或深蓝色）
   - 可以添加适当的背景（如白色半透明背景）以提高可读性

3. 在场景的主摄像机或任何合适的游戏对象上添加 ReportTextProcessor 组件：
   - 选择主摄像机（或创建一个空游戏对象）
   - 点击 Add Component 按钮
   - 输入 "ReportTextProcessor"
   - 在 Inspector 面板中的 ReportTextProcessor 组件下：
     - 将 "Report Display Text" 字段拖拽关联到你创建的 ReportText

4. 可选：调整 ReportTextProcessor 的参数：
   - Scroll Speed：控制文本显示速度（默认0.05秒/字符）
   - Start Delay：控制开始显示前的延迟时间（默认3秒）

## 修改Build Settings

1. 打开 Build Settings（File > Build Settings）
2. 确保 StartScreen 场景在列表中排在 SampleScene 之前
3. 如果是新创建的 StartScreen 场景，点击 "Add Open Scenes" 添加到构建列表中

## 场景流程测试

1. 运行 StartScreen 场景（确保它是第一个加载的场景）
2. 在输入框中输入测试报告文本
3. 点击"开始模拟"按钮
4. 验证 SampleScene 加载后是否正确显示输入的报告文本
5. 文本应该以打字机效果逐字显示

## 可能的增强功能

1. 添加报告段落格式化支持（保留换行符等）
2. 添加文本放大/缩小控制
3. 添加暂停/继续显示控制
4. 添加文本速度调整控制
5. 为报告文本添加动画效果（如淡入/淡出）

## 注意事项

- 确保 StartScreenManager 脚本和 ReportTextProcessor 脚本正确编译和附加
- 如果在主场景中已有类似的文本显示功能，可以考虑整合而不是添加新的
- 报告文本显示位置应该符合场景的整体布局和报告情境 