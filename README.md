# Project Genesis
Godot 4 (Mono/C#) 的 2D Top-Down 生存動作原型，目標是固定 15:00 一局的可重複遊玩流程。

## 目前核心架構
- 戰鬥結算唯一入口: `Scripts/Systems/Core/CombatSystem.cs`
- 升級進程: `Scripts/Systems/Progression/ProgressionSystem.cs` + `Scripts/Systems/UpgradeSystem.cs`
- 生怪與節奏: `Scripts/Systems/Director/SpawnSystem.cs` + `Data/Director/*.csv`
- UI 只讀取狀態、不反向依賴遊戲系統: `Scripts/UI/*`

## Runtime 流程（現行）
1. 敵人死亡後由 `ExperienceDropSystem` 產生 `ExperiencePickup`。
2. 玩家拾取後，`ProgressionSystem` 增加 EXP。
3. EXP 滿足需求後排入升級佇列。
4. `UpgradeMenu` 開啟，`UpgradeSystem` 套用升級。
5. `SpawnSystem` 依階段與資料表調整生怪壓力。

## 專案資料夾用途（位置）
- `Scenes/`: 場景組裝（玩家、UI、世界、系統根節點）。
- `Scripts/`: C# 遊戲邏輯（Player / Enemy / Systems / UI / Audio）。
- `Data/`: 遊戲資料與平衡表（角色、導演資料、卡片本地化等）。
- `Assets/`: 美術與音效資源（含 `.import`）。
- `Prefabs/`: 可重用場景預置（子彈、拾取物、升級選單等）。
- `Enemies/`: 敵人場景。
- `docs/`: 設計、流程、重構、待辦與規格文件。
- `log.md`: 內部開發記錄（主要給 Codex/維護用途）。

## 文件用途與位置
- `docs/ARCHITECTURE.md`: 系統邊界、資料流、核心守則與編碼規範。
- `docs/SYSTEM_FLOW.md`: 系統流程圖（Mermaid）。
- `docs/GAME_CONCEPT.md`: 遊戲概念與核心循環定義。
- `docs/GameDirector_Design.md`: Director 與 15:00 節奏設計。
- `docs/CARDS.md`: 卡片系統規格與分層。
- `docs/CARDS_CHANGELOG.md`: 卡片平衡與規格變更紀錄。
- `docs/SCRIPT_REFACTOR_PLAN.md`: 腳本拆分/重構規劃。
- `docs/CODE_STRUCTURE_AUDIT_2026-02-21.md`: 結構稽核與風險。
- `docs/SCENE_SPLIT_NOTES.md`: 場景拆分說明。
- `docs/TODO.md`: 下一步工作清單。
- `Assets/Sprites/Skills/README.md`: 技能 VFX 素材路徑與命名規範。
- `log.md`: 迭代紀錄與交接備忘。

## 文件維護規則（給協作與 Codex）
- `docs/` 內文件為規格來源，功能改動後要同步更新。
- `log.md` 允許快速紀錄與上下文備忘。
- 文字檔請使用 UTF-8；Godot 的 `.tres/.tscn` 使用 UTF-8 無 BOM。

## Git 與輸出檔規則
- 不提交 Godot 匯出產物與安裝包相關資源（例如 `.exe`, `.pck`, 匯出資料夾）。
- `.godot/` 應維持忽略狀態（快取/中介資料）。

## 技術資訊
- Engine: Godot 4.x (Mono)
- Language: C#
- Platform: PC prototype
