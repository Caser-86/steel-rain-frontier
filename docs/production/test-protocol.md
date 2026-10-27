# 本地验证协议

文档核对：2026-10-27。以下脚本只在当前本地开发工作区存在，未随文档上传 GitHub。此页规定如何验证，不声明本轮已经运行测试。

## Unity 测试与构建

需要有效许可证和项目记录的 Unity 6000.3.18f1。脚本当前配置的 Editor 为 `D:\Program Files\Unity\6000.3.18f1\Editor\Unity.exe`。从仓库根目录执行，先关闭同工程交互式 Editor，串行运行：

```powershell
./scripts/Invoke-LocalVerification.ps1 -Mode EditMode -TaskId E06 -Filter LocalTutorialHintsTests
./scripts/Invoke-LocalVerification.ps1 -Mode PlayMode -TaskId E06 -Filter LocalSliceFlowTests

# 恢复并验证项目设置后，才构建玩家版本
./scripts/Invoke-LocalVerification.ps1 -Mode BuildLocalGraybox -TaskId E06
```

省略 Filter 可运行对应完整测试程序集。测试不附加 `-quit`；不依赖终端是否安装额外的 `unity` CLI。

验证脚本写入 `TestResults/local-v1/<TaskId>/<runId>/`，使用互斥锁、超时与原始设置快照。执行结束必须检查：

- 测试进程退出 0，生成有效 NUnit XML，`result=Passed`、`total>0`、`failed=0`、`skipped=0`。
- 构建日志/BuildReport 为 Succeeded，当前运行的程序集和数据产物存在；不能只看 EXE 启动桩时间或旧日志。
- `ProjectSettings.asset`、`EditorBuildSettings.asset` 按 SHA-256 原样恢复，不能把测试专用公司/产品带入玩家构建。
- 失败应区分许可证/启动、编译、fixture 生命周期和断言失败。不跳过测试或降低断言来制造通过。

## 玩家数据保护

- 使用隔离的 `CodexIsolatedTests` / `SteelRainFrontier_LocalV1Test_<runId>` 配置；不要在玩家配置下直接跑读写存档的旧场景。
- 不用 `PlayerPrefs.DeleteAll()`；fixture 只清理自己的测试键，未知键和玩家数据保留。
- 真实场景测试在进入 Play 前配置完整路径，结束后恢复 enabled/GUID；避免同名 legacy 场景被误加载。
- fixture 恢复 Time.timeScale、场景、对象和订阅。旧战役、无尽与 LocalV1 存档作用域分别验证。

旧夹具涉及的小队、检查点、分数、成就和设置键，以及防误运行证据保留在旧基线/存档修复报告中；不要将其旧键列表当成 LocalV1 新检查点设计。

## 不加载 Unity 引擎的逻辑测试

```powershell
./scripts/Invoke-LocalLogicTests.ps1
```

该入口用已安装编译器、Mono 和项目 NUnit 断言执行教程纯状态逻辑，输出至 `TestResults/local-v1/E06/logic-*/tests.log`。目前范围是七项同步逻辑测试；不运行场景、物理、渲染或完整 Unity Test Runner。

## 已有 Player 的程序集预览

```powershell
./scripts/Invoke-LocalManagedPreview.ps1 -CheckTests
```

依赖本机已有 LocalGraybox Player 和上次 Unity 编译缓存，不适用于干净克隆。脚本编译运行时代码，复制既有 Player 到新的 ManagedPreview 目录并核对程序集哈希；CheckTests 只编译 PlayMode 程序集。

这不重新导入纹理/场景/Prefab，不证明 Unity 测试通过，也不替代完整 BuildPipeline 构建。

## 证据边界

结果总表见 [切片记录](local-v1-slice-playtest.md)，当前事实见 [CONTEXT](../../CONTEXT.md)。

最近一次 Editor 尝试为 2026-10-21：无有效许可证，退出 198，没有 XML。2026-10-27 只核对文档及已有证据，没有重试许可证。2026-10-02 的 202/202、111/111 是历史结果；2026-10-21 的 7/7 仅是教程纯逻辑测试。窗口检查与完整通关也须分开记录。
