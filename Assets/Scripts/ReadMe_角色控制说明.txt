角色第一人称控制系统使用说明
===========================

一、功能概述
-----------
该系统提供了完整的角色第一人称控制功能，包括：

1. 自动查找并选择场景中的角色
2. 第一人称/第三人称视角切换
3. 多角色之间自由切换
4. 使用WASD键移动角色
5. 跳跃、奔跑等人类行为
6. 自动播放角色动画

二、使用方法
-----------

### 1. 快速设置

在场景中添加一个空物体，并附加以下两个脚本中的一个：

- `EnhancedFirstPersonController` - 推荐，整合了所有功能的简化版本
- `CameraMovable` - 基础版本，只有相机跟随和角色控制功能

如果使用 `EnhancedFirstPersonController`，只需设置主相机引用（不设置会自动查找Main Camera）。

### 2. 控制说明

- **基本移动**：
  - W/S：前进/后退
  - A/D：左移/右移
  - 空格：跳跃
  - 左Shift：奔跑

- **视角控制**：
  - 鼠标移动：控制视角方向
  - F键：切换第一人称/第三人称视角
  - Tab键：切换到下一个角色
  - ESC键：解锁/锁定鼠标

### 3. 动画系统

系统会根据移动状态自动播放角色动画：
- 站立状态：播放"idle1"动画
- 行走状态：播放"walk"动画
- 奔跑状态：播放"run"动画

三、配置参数
-----------

### 1. CameraMovable 参数

- **相机设置**：
  - `mouseSensitivity`：鼠标灵敏度
  - `clampAngle`：垂直视角限制角度
  
- **角色设置**：
  - `targetCharacter`：要跟随的目标角色
  - `cameraOffset`：相机与角色头部的偏移量
  
- **移动设置**：
  - `moveSpeed`：基本移动速度
  - `sprintMultiplier`：奔跑速度倍率
  - `jumpForce`：跳跃力度
  - `gravity`：重力大小

### 2. EnhancedFirstPersonController 参数

- **相机设置**：
  - `mainCamera`：主相机引用
  - `firstPersonCameraPrefab`：自定义相机预制体（可选）
  
- **角色设置**：
  - `targetCharacter`：初始目标角色
  - `autoSelectOnStart`：是否自动选择最近的角色
  - `switchCharacterKey`：切换角色的快捷键
  
- **UI设置**：
  - `statusText`：状态文本显示（可选）

四、注意事项
-----------

1. 系统会自动为目标角色添加 `CharacterController` 组件（如果没有）
2. 角色需要有 `Animator` 组件才能播放动画
3. 默认的第一人称视角可能会导致看不到角色本身，这是正常的
4. 如果角色穿透地面，可能需要调整 `CharacterController` 的参数
5. 如果鼠标锁定解除后无法再次锁定，可能是因为游戏窗口失去了焦点

五、常见问题解决
--------------

1. **问题**: 角色不动
   **解决**: 检查是否正确设置了 targetCharacter，以及角色是否有 CharacterController 组件

2. **问题**: 相机位置不正确
   **解决**: 调整 cameraOffset 参数，第一人称推荐 (0, 0.2f, 0.1f)，第三人称推荐 (0, 2f, -5f)

3. **问题**: 动画不播放
   **解决**: 确保角色有 Animator 组件，且动画控制器包含 "idle1"、"walk" 和 "run" 状态

4. **问题**: 角色可以穿墙
   **解决**: 检查场景中的碰撞体是否正确设置，可能需要为墙体添加碰撞器

六、自定义扩展
-----------

1. **添加更多动作**：
   修改 `PlayMovementAnimation` 方法，添加更多动画状态

2. **改变移动方式**：
   修改 `HandleMovement` 方法，可以添加更复杂的移动逻辑

3. **添加交互功能**：
   在 `Update` 方法中添加检测按键并执行交互的代码 