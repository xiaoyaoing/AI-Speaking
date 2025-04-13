# 学术报告模拟器 - 开始页面创建指南

## 创建开始页面的步骤

1. 在Unity编辑器中，点击 File > New Scene，选择 "Basic (Built-in)" 模板，保存为 "StartScreen"

2. 在场景中添加一个 Canvas（右键点击 Hierarchy > UI > Canvas）

3. 在Canvas下添加以下UI元素：
   - 一个 Panel 作为背景
   - 一个 Text - TextMeshPro 作为标题 "学术报告模拟器"
   - 一个 Text - TextMeshPro 作为说明文本 "请输入您的学术报告内容"
   - 一个 InputField - TextMeshPro 用于输入报告文本
   - 一个 Button 作为"开始模拟"按钮

4. 调整 Panel 的大小，确保它能覆盖整个屏幕，并设置适当的背景颜色或图像

5. 将标题文本放在页面顶部，字体大小设为较大值（如42），居中对齐

6. 将说明文本放在标题下方，字体大小适中（如24），居中对齐

7. 将 InputField 放在说明文本下方，设置其大小足够大以容纳较长的文本输入，并启用垂直滚动：
   - 在 InputField 的检查器中，找到 "Content Type" 下拉菜单，选择 "Multi Line"
   - 设置 "Line Type" 为 "Multi Line Newline"
   - 增加 "Character Limit"（如10000）以允许输入较长的报告

8. 将"开始模拟"按钮放在 InputField 下方，设置适当的颜色和文本

9. 在 Hierarchy 中选择场景的 Canvas，添加 StartScreenManager 组件
   - 点击 Add Component 按钮
   - 输入 "StartScreenManager"
   - 在 Inspector 面板中的 StartScreenManager 组件下：
     - 将 "Report Text Input" 字段拖拽关联到你创建的 InputField
     - 将 "Start Simulation Button" 字段拖拽关联到你创建的按钮

10. 确保 Build Settings（File > Build Settings）中包含了 StartScreen 和 SampleScene 两个场景，并且 StartScreen 在列表的最前面

## 注意事项
- 确保 TextMeshPro 包已经导入到项目中
- InputField 应该设置为多行，以便用户输入较长的报告文本
- 确保按钮点击事件正确设置，关联到 StartScreenManager 的 StartSimulation 方法

## 场景流程
1. 应用启动时，首先显示 StartScreen 场景
2. 用户在输入框中输入学术报告文本
3. 用户点击"开始模拟"按钮后，应用加载主场景（SampleScene）
4. 在主场景中，ReportTextProcessor 组件将读取用户的输入并在适当的时机显示 