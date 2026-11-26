# 工业时代 2 局域网版

这个项目在当前 Survivalcraft API 1.9.5.1 联机分支下编译成独立的 `IndustrialEra2-LAN.scmod`。工业时代本体来自作者发布的 1.0.0.2 程序集，经反编译恢复成可编辑的 C#。运行需要原版资源和 RecipaediaEX 2.0.0.0。

## 代码入口

- `Source/SCIENEW/IndustrialModLoader.cs` 负责模组加载和游戏钩子。
- `Source/SCIENEW/IndustrialEraLanAdapter.cs` 负责设备和电网快照、机器库存，以及客户机请求。
- `Source/SCIENEW/IndustrialMachineControl.cs` 负责发电参数、机器启停和插入器槽位操作。
- `Source/SCIENEW/IndustrialMachineState.cs` 定义机器界面需要同步的状态。
- `Source/SCIENEW/Component*.cs` 和 `Source/SCIENEW.VoltNet/` 分别是机器组件和电网逻辑。

`SCIENEW.GEN.dll`、原始资源及第三方依赖取自发布包。打包时会使用本项目编译的 `SCIENEW.dll`。

## 在 Windows 上构建

`Packages/` 需要这两个作者发布包。首次配置时可从[作者发布页](https://gitee.com/LX__LX/scie-publish-page/releases/tag/1.0.0.2)下载。

| 作者文件 | 本地文件 |
| --- | --- |
| 工业时代2-1.0.0.2.scmod | `Packages/IndustrialEra2-1.0.0.2.scmod` |
| RecipaediaEX-2.0.0.0.scmod | `Packages/RecipaediaEX-2.0.0.0.scmod` |

用 .NET 10 SDK 在仓库根目录运行。已有 Windows 构建产物时直接执行第二条命令。

```powershell
dotnet build .\Survivalcraft.Windows\Survivalcraft.Windows.csproj -c Release -m:1
dotnet build .\Survivalcraft.IndustrialEra2\Survivalcraft.IndustrialEra2.csproj -c Release -m:1 -p:SkipNetworkModPackage=true -p:SkipCompatibilityModPackage=true -p:SkipContentZip=true
```

构建后的两个 `.scmod` 位于本项目的 `bin/Release/net10.0-windows/win-x64/scmod/`。房主和客机使用同一套编译产物。可用 `-p:InstallModPath=...` 在构建完成后复制到指定游戏目录的 `Mods`。工业时代原版和此构建使用同一个包 ID，游戏目录中只需放一份工业时代。

## 局域网覆盖范围

房主负责机器模拟、生产、科技树和工业世界物体。客机操作发电参数、机器设置、台面库存和配方时，由房主执行并回传状态。工业载具的输入、机载库存和设备状态也接入了房主处理。摄像头与显示器的连接从房主同步。

同机 Windows 房主与客机在独立存档验证了进房、设备放置与拆除、储罐取水、发电机供电、装罐生产、采矿机选区和移动钻头、配方批量放入流体、科技树实时解锁、充电器模式、摄像头角度、显示器连接，以及掉落物和投射物生成与移除。配方流体测试使用可重复的测试配方，验证只消耗所需的容器数量。

逐项联机核对覆盖 1,963 条注册记录。1,774 个库存物品值通过客机搬运和掉落物生成、移除测试。246 个可放置方块通过放置和拆除测试。65 种流体通过储罐状态测试，空储罐占位值另外核对。113 个具体实体模板在两端加载，另有 5 个继承模板完成注册核对。

专项交互覆盖客机放出飞艇、汽车、汽艇、蒸汽船、潜艇、拖拉机、机枪实体、火车和矿车，以及三种流体收集、手持电钻充电和两种无线电设备调频。拖拉机沿用原版库存物品已使用的汽车模型和贴图。测试使用 Windows 同机的两个独立进程和临时存档。

设备完整生产链、服装效果及载具驾驶仍需逐项验证。扩展真值表的表达式同步、红白机画面与手柄共享及其他平台不在上述已验证范围内。
