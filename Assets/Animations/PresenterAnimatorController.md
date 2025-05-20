# 讲演者动画控制器设置指南

本文档将指导你如何在Unity中设置讲演者的动画控制器(Animator Controller)，以实现从座位站起、行走到讲台、上台阶以及演讲的完整动画流程。

## 创建Animator Controller

1. 在Unity的Project窗口中，找到Assets/Animations文件夹（如果不存在，请创建一个）
2. 右键点击 → Create → Animator Controller，将其命名为"PresenterAnimatorController"

## 设置动画状态

打开Animator窗口（Window → Animation → Animator），然后：

1. 删除默认的"Entry"和"Any State"之间的连接
2. 创建以下状态（右键点击空白处 → Create State → Empty）：
   - Seated (坐着)
   - StandUp (站起来)
   - Idle (站立/闲置)
   - Walk (行走)
   - Talking (演讲)

## 分配动画剪辑

对于每个状态，你需要分配相应的动画剪辑：

1. 选择"Seated"状态，在Inspector面板中：
   - Motion字段分配一个坐姿idle动画
   
2. 选择"StandUp"状态，在Inspector面板中：
   - Motion字段分配一个从坐到站的过渡动画
   
3. 选择"Idle"状态，在Inspector面板中：
   - Motion字段分配一个站立idle动画
   
4. 选择"Walk"状态，在Inspector面板中：
   - Motion字段分配一个行走动画
   
5. 选择"Talking"状态，在Inspector面板中：
   - Motion字段分配一个演讲动画

## 创建状态转换

接下来，需要创建状态之间的转换：

1. Entry → Seated:
   - 右键点击"Entry"，选择"Make Transition"，然后点击"Seated"
   
2. Seated → StandUp:
   - 右键点击"Seated"，选择"Make Transition"，然后点击"StandUp"
   - 在Inspector中设置Conditions：IsStanding = true
   
3. StandUp → Idle:
   - 右键点击"StandUp"，选择"Make Transition"，然后点击"Idle"
   - 在Inspector中勾选"Has Exit Time"
   - Exit Time设置为0.9（动画快结束时）
   - 在Conditions中设置：IsIdle = true
   
4. Idle → Walk:
   - 右键点击"Idle"，选择"Make Transition"，然后点击"Walk"
   - 在Inspector中设置Conditions：IsWalking = true, IsIdle = false
   
5. Walk → Idle:
   - 右键点击"Walk"，选择"Make Transition"，然后点击"Idle"
   - 在Inspector中设置Conditions：IsWalking = false, IsIdle = true
   
6. Idle → Talking:
   - 这个转换将在代码中处理，根据PresenterController中的startedPresentation状态

## 设置参数

在Animator窗口的Parameters选项卡中，添加以下参数：

1. IsStanding (类型: Bool)
2. IsWalking (类型: Bool)
3. IsIdle (类型: Bool)

## 优化状态转换

为每个转换设置适当的过渡时间和曲线：

1. StandUp转换：
   - 双击状态之间的箭头打开Transition设置
   - 设置Duration为1.0
   - 调整Transition曲线使其自然

2. Walk和Idle之间的转换：
   - 设置较短的Duration（约0.25）
   - 调整曲线使其响应迅速但不生硬

## 整合到角色中

1. 将创建好的Animator Controller拖到你的讲演者角色上
2. 确保角色上还有以下组件：
   - PresenterController
   - PresenterAnimatorController
   - CharacterController

## 调试提示

在Play模式中查看Animator窗口以确认：
- 状态转换是否按预期工作
- 参数是否正确更新
- 动画是否平滑过渡

## 添加台阶爬升支持

CharacterController组件已通过以下方式配置以支持台阶爬升：

```csharp
characterController.stepOffset = 0.3f; // 允许爬上约30厘米高的台阶
```

如果需要调整可爬台阶的高度，请修改PresenterController脚本中的stepOffset值。 