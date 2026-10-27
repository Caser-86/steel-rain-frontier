# Steel Rain: Frontier · 钢铁雨：边境

Unity 6 / C# 的 2D 横版射击项目，当前目标是一个可在本机 Windows 游玩的单人任务。项目仍在开发，不是已验收成品。

> 文档核对：2026-10-27。GitHub 的游戏代码与本地开发工作区尚未同步；本次只整理说明文档。下面分别说明两者，避免把本地进展写成下载仓库后就有的功能。

## 当前有哪两个版本

| 范围 | 实际内容 | 验证边界 |
|---|---|---|
| GitHub `main` | 较早的四人原型；已提交场景为 Boot、MainMenu、Level01、Level02 | 构建代码还引用未提交的 Level03–05；不能凭旧功能清单宣称五关完整可打包 |
| 本地 LocalV1 工作区 | 两名队员、三枪固定库存、四场遭遇、灰盒路线、中文菜单/HUD、暂停/起点重试、分阶段提示 | 仅做了有限窗口检查；全程战斗、失败闭环、检查点、首领及全 AI 视听尚未完成 |

详细事实与证据见 [CONTEXT.md](CONTEXT.md)；下一步见 [TODO.md](TODO.md)。本地新代码、资源、验证日志和 EXE 不随这次文档同步上传。

## 在本机打开

### 已有本地开发工作区

当前推荐的教程预览：

`Builds/ManagedPreview/managed-20261021-181113-26632/SteelRainFrontierLocalGraybox.exe`

这是以此前 Windows Player 为基础更新 C# 程序集的独立预览，不是新的完整 Unity 打包，也不代表资源重新导入成功。路径只在本地工作区存在，GitHub 没有提供这个 EXE。

Unity 中的本地入口为 `Assets/Production/Scenes/LocalV1/LocalBoot.unity`，流转到 LocalMainMenu 和 LocalFirstMission。详细记录见 [本地切片验证](docs/production/local-v1-slice-playtest.md)。

### 从 GitHub 下载的代码

使用项目记录的 **Unity 6000.3.18f1**，先检查 Console 与资源引用，再打开已提交的 `Assets/Scenes/Boot.unity` 或 `Level01_VerticalSlice.unity`。本次文档整理没有重新运行该远端代码的 Unity 工程。

远端 `BuildTools.BuildWindows` 使用固定场景表，其中 Level03–05 当前未提交，因此不能承诺直接构建成功。不要把 `Build All` 当作普通启动步骤：它是内容生成命令，执行前应保全已有内容。

## LocalV1 键鼠操作

以下是本地 `LocalInput.inputactions` 的绑定，不适用于远端旧输入实现。

| 操作 | 按键 |
|---|---|
| 移动 / 方向 | WASD / 方向键 |
| 跳跃 | 空格 |
| 射击 | J / 鼠标左键 |
| 闪避 | 左 Shift |
| 换存活队员 | Tab |
| 切换 / 直选武器 | E / 1–3 |
| 切换射击形态 | Q |
| 暂停 / 继续 | Esc |
| 交互预留 | G；补给为接触拾取 |
| 援护预留 | F；完整援护行为尚未实现 |

完整键位可在暂停菜单“操作说明”查看。短提示按移动、遇敌、受伤和取得特殊武器的条件出现，每条最多两秒，暂停不推进计时。

步枪为无限基础弹药；霰弹枪和火箭筒使用独立有限弹药。当前 LocalV1 是两名队员，不应套用旧版“四名角色、死亡回手枪”的说明。

## 本地构建与检查

以下脚本目前仅在本地开发工作区存在：

```powershell
# 实际运行不依赖 Unity 引擎的教程逻辑测试
./scripts/Invoke-LocalLogicTests.ps1

# 需要有效 Unity Editor 许可证；串行执行
./scripts/Invoke-LocalVerification.ps1 -Mode EditMode -TaskId E06
./scripts/Invoke-LocalVerification.ps1 -Mode PlayMode -TaskId E06
./scripts/Invoke-LocalVerification.ps1 -Mode BuildLocalGraybox -TaskId E06

# 基于已有 Player / 编译缓存的程序集预览，不替代完整构建
./scripts/Invoke-LocalManagedPreview.ps1 -CheckTests
```

最近一次 Editor 尝试是 **2026-10-21**：因许可证无效退出 198，未生成测试 XML。2026-10-27 本轮只整理文档，没有重新测试许可证、编译或游戏。

已记录的结果：

- 2026-10-02：EditMode 202/202、PlayMode 111/111，LocalGraybox 构建成功；这是历史版本的结果。
- 2026-10-21：教程纯逻辑测试 7/7 实际执行通过；运行时代码与 PlayMode 测试程序集编译成功。
- 2026-10-21：窗口中观察并操作了菜单、显示设置、灰盒渲染、暂停/起点重试、首次提示与操作说明；未完成全关战斗试玩。
- 新增真实步枪子弹遭遇测试已写并编译，尚未在 Unity 中执行；受控射击测试也不等于路线或生存通关。

## AI 资源要求与现状

原创美术、动画、音效和音乐以真实 AI 生成为目标，运行时不调用生成服务。当前尚未全部满足：资源清单中 11 项为 `bound`，不等于 `accepted`；旧角色精灵、程序化灰盒和音频仍存在。

旧 `Assets/Art/AI_Generated/_PROMPTS*.md` 是历史提示词/导入约定，不是已经生成资源的证明，也不是当前 LocalV1 的完成清单。

## 目录与文档

- `Assets/Game`：运行时、Editor 构建工具和测试。
- `Assets/Scenes`、`Assets/Art`、`Assets/Prefabs`：旧版内容；不因缺少直接引用自动删除。
- 本地 `Assets/Prototypes`：可生成的对照原型；`Assets/Production/.../LocalV1`：当前独立切片。
- [CONTEXT.md](CONTEXT.md)：状态摘要与本地/远端边界。
- [TODO.md](TODO.md)：当前优先级。
- [AGENTS.md](AGENTS.md)：协作、保护和验证规则。
- [本地切片验证](docs/production/local-v1-slice-playtest.md)：带日期的验证记录。
- [RELEASE_NOTES.md](RELEASE_NOTES.md)：历史版本记录，不作为当前验收声明。

本地详细设计/任务分册保留在 `docs/superpowers`，旧方案保留在 `docs/archive`。历史计划、程序化素材、旧五关和无尽内容都不能用来推断当前 LocalV1 已经完成。
