# Citizens PRO 资源清理工具

这个Python脚本可以帮助你删除Citizens PRO资源包中的多余人物资源，只保留每种类型人物的1/4，从而大幅减少项目大小。

## 功能特点

- 🎯 **智能分组**: 自动识别并按类型分组人物资源
- 🔄 **同步清理**: 同时清理预制体文件和对应的模型文件
- 🛡️ **安全预览**: 支持预览模式，可以先查看将要删除的文件
- ⚙️ **可配置**: 支持自定义保留比例
- 📊 **详细报告**: 显示详细的删除和保留统计信息

## 支持的人物类型

脚本能够识别以下人物类型：

- **business** - 商务人员 (business01_f, business02_m 等)
- **casual** - 休闲人员 (casual01_f, casual02_m 等)
- **sportive** - 运动人员 (sportive01_f, sportive02_m 等)
- **nude** - 裸体模型 (nude01_f, nude02_m 等)
- **granny** - 老奶奶 (granny01, granny02 等)
- **child** - 儿童 (child01_f, child02_m 等)
- **Man_XX** - 编号男性 (Man_11, Man_22 等)
- **Girl_XX** - 编号女性 (Girl_11, Girl_22 等)
- **soccerplayer** - 足球运动员 (soccerplayer01_m 等)
- **player** - 玩家角色 (player_m, player_f 等)

## 使用方法

### 1. 预览模式 (推荐先运行)

```bash
python delete_citizens_pro.py --dry-run
```

这会显示将要删除的文件，但不会实际删除任何内容。

### 2. 实际删除

```bash
python delete_citizens_pro.py
```

### 3. 自定义保留比例

```bash
# 保留50%的资源
python delete_citizens_pro.py --keep-ratio 0.5

# 保留10%的资源  
python delete_citizens_pro.py --keep-ratio 0.1
```

### 4. 指定项目路径

```bash
python delete_citizens_pro.py --base-path "D:/MyUnityProject"
```

## 命令行参数

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `--dry-run` | 预览模式，不实际删除文件 | False |
| `--keep-ratio` | 保留比例 (0-1之间) | 0.25 (25%) |
| `--base-path` | 项目根目录路径 | 当前目录 |

## 清理范围

脚本会清理以下位置的文件：

### 预制体文件
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Male/Summer/`
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Male/Winter/`
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Male/Beach/`
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Female/Summer/`
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Female/Winter/`
- `Assets/Citizens PRO/Citizens PRO/People Prefabs/Female/Beach/`

### 模型文件
- `Assets/Citizens PRO/Citizens PRO/Models/People 2.0/Male/`
- `Assets/Citizens PRO/Citizens PRO/Models/People 2.0/Female/`

## 示例输出

```
Citizens PRO 资源清理工具
基础路径: .
保留比例: 0.25
模式: 预览模式
==================================================

处理预制体: Male/Summer
找到 40 个预制体文件

  处理组: business_m (7 个文件)
    保留: 1 个
    删除: 6 个

  处理组: casual_m (18 个文件)
    保留: 4 个
    删除: 14 个

...

==================================================
清理完成!
删除项目: 342
保留项目: 50
```

## 注意事项

⚠️ **重要提醒**:

1. **备份项目**: 在运行脚本之前，请务必备份你的Unity项目
2. **关闭Unity**: 运行脚本时请关闭Unity编辑器
3. **预览优先**: 建议先使用 `--dry-run` 参数预览要删除的文件
4. **路径检查**: 确保脚本在正确的项目目录中运行

## 工作原理

1. **扫描文件**: 脚本扫描Citizens PRO的预制体和模型目录
2. **智能分组**: 根据文件名模式将人物按类型和性别分组
3. **随机选择**: 在每个组内随机选择要保留的文件
4. **同步删除**: 删除预制体文件时，同时删除对应的模型文件夹和.meta文件

## 恢复方法

如果需要恢复删除的文件：

1. 从备份中恢复
2. 或者重新导入Citizens PRO资源包
3. 或者使用Unity的版本控制系统恢复

## 技术要求

- Python 3.6+
- 标准库模块: `os`, `shutil`, `re`, `argparse`, `pathlib`, `collections`, `random`

## 故障排除

### 问题: "找不到 Citizens PRO 资源"
**解决**: 确保脚本在包含 `Assets/Citizens PRO` 目录的项目根目录中运行

### 问题: "权限被拒绝"
**解决**: 
1. 关闭Unity编辑器
2. 确保文件没有被其他程序占用
3. 以管理员权限运行脚本

### 问题: "无法解析文件名"
**解决**: 这是正常的警告，表示某些文件名不符合预期模式，这些文件会被跳过

## 许可证

此脚本仅用于管理Citizens PRO资源包，请确保你拥有该资源包的合法使用权。 