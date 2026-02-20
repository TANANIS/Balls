# TODO

## 當前版本規格（2026-02 sync）
- 單局總時長：`15:00`
- 節奏四階段（以環境狀態變化為主，不只是數值加難）
  - `00:00 - 03:00` 宇宙穩定：基礎敵人、壓力自然下降、無宇宙異常
  - `03:00 - 07:00` 能量異常：進入第一個壓力點
  - `07:00 - 11:00` 結構破碎：敵人種類升級、生成波段加快、壓力自然下降速度降低
  - `11:00 - 15:00` 坍縮臨界：宇宙異常疊加、高密度生怪、壓力下降近乎停滯、結束菁英怪出現
- 宇宙事件節點：每 `3:00` 一次（`03:00`、`06:00`、`09:00`、`12:00`）

## 已完成
- [x] `Esc` 暫停流程（含 Resume / Settings / Restart / ToTitle / Quit）
- [x] 設定面板（BGM / SFX / 視窗模式 / 視窗大小）
- [x] 設定存檔與讀檔（`user://settings.cfg`）
- [x] 主選單與重開流程整理
- [x] Universe Event 基礎 UI（倒數 / 通知）
- [x] 升級介面改為三選一
- [x] 升級選項顯示改為遊戲文案（移除程式欄位字樣）
- [x] ESC 暫停面板顯示當前 Build 與關鍵詞條
- [x] Run 結束顯示 Build 統計（分類占比 / 關鍵詞條）
- [x] 滑鼠點擊失效修正（UI 容器 `mouse_filter`）
- [x] 場景檔解析錯誤修正（`.tscn` UTF-8 no BOM）
- [x] 場景拆分第一階段：`MainScence` -> `WorldRoot` / `SystemsRoot` / `GameFlowUIRoot`
- [x] UI 拆分第二階段：`HudOverlay` / `StartPanel` / `PausePanel` / `RestartPanel`
- [x] `GameFlowUI` Inspector 綁定欄位精簡（NodePath 常量化）

## 玩家升級系統（MVP）

### 區塊分類與資料模型
- [x] 建立 `UpgradeCategory`：`WeaponModifier`、`PressureModifier`、`AnomalySpecialist`、`SpatialControl`、`RiskAmplifier`
- [x] 建立 `UpgradeRarity`：`Common`、`Rare`、`Epic`
- [x] 擴充 `UpgradeDefinition`：`id`、`name`、`category`、`rarity`、`weight`、`maxStack`
- [x] 支援 `prerequisites`（前置）與 `exclusiveWith`（排他）
- [x] 建立 20 條基礎詞條（`Data/Upgrades/DefaultUpgradeCatalog.tres`）

### 抽選與分支策略
- [x] 實作升級三選一抽選
- [x] 過濾規則：滿堆疊 / 排他衝突 / 前置未滿
- [x] 同類加權（玩家選某分類後，該分類權重上升）
- [x] Rare / Epic 保底（pity）

### 系統串接
- [x] 升級效果套用至玩家/壓力/穩定度系統
- [x] 新增 `UpgradeSystem` 統計輸出（分類占比 / 關鍵詞條）

## 待完成

### 核心玩法調校
- [ ] 三個流派首輪平衡與手感調整
- [ ] 升級池數值與權重微調（避免單一最優解）
- [ ] 前置/排他關係再設計（提高 build identity）

### Director / Universe Event
- [ ] Stability / Pressure / Event 三者更緊密耦合
- [ ] Universe Event 種類擴充與機制差異化
- [ ] Universe Event 對 Upgrade 池的動態影響
- [ ] 將 Universe Event 觸發對齊固定時間軸（`03:00`、`06:00`、`09:00`、`12:00`）
- [ ] 完成 `03:00 - 07:00` 第一輪宇宙事件玩法（目前尚未完成）
- [ ] 完成 `11:00 - 15:00` 坍縮臨界的異常疊加與終局菁英壓制體驗

### UI / Editor Workflow
- [ ] 提供 UI 狀態預覽模式（Editor 快速切換 Start / Pause / Restart / Upgrade）
- [ ] PausePanel 的 Settings 子場景獨立化（若仍覺得編輯繁瑣）
- [ ] 文件化場景拆分規範（命名/目錄/責任邊界）

### 驗收條件
- [ ] 15 分鐘遊玩內不出現無詞可抽或死循環
- [ ] 三個流派都能穩定成形，差異明顯
- [ ] 同流派重玩仍有變化，不會每局完全一致
- [ ] 四階段節奏邊界體感清晰（`03:00`、`07:00`、`11:00`）

20260220中兩回饋
升級有點突然，改成撿經驗值。
目前敵人的擊退感有點弱，造成打擊感不夠。
還是需要有血量UI

階段一隨機BOSS
階段二隨機獎勵
階段三隨機大事件
階段四隨機大BOSS
## 2026-02 Gameplay TODO (��z��)

### ���q�y�{�ƥ�]�|���q�^
- [ ] ���q�@�G�H�� BOSS�]�C���H��������A�קK���ơ^
- [ ] ���q�G�G�H�����y�]�ƥ󵲧��ᵹ Build �V���y�^
- [ ] ���q�T�G�H���j�ƥ�]���ܳ���/�W�h�������ƥ�^
- [ ] ���q�|�G�H���j BOSS�]�ק����O�P�M����O�˩w�^

### ��ԥ��ŭץ��]�ثe�L�j�^
- [ ] �W�[��ԧN�o�]�U�׳s���X�^
- [ ] �W�[��ԭ��I�]�K�������Ӷ˭��I�����^
- [ ] ���� Dash �N�o�]���C�L���i�X�^
- [ ] �U�ת�Ԩ���ͩR�ȡ]�W�[�e�������^
- [ ] �ɱj���{��P�P���ĩʡ]�ثe��P�L�O�^

### �ĤH�ͦ��P�ͦs�޿�
- [ ] �C�Ӷ��q�w�q���P�u�ͦs�޿�v�P���O�ӷ�
- [ ] �ͦ��W�h���ơ]�C���q�ͦ��ؼСB�Ǫ��զ��B�i���K�ס^
- [ ] �H���������ɸԲӸ`���]1/2/3/4 �����`�I�^

### Build �����X�R
- [ ] �s�W��Ե����y���G`DASH + MELEE COMBO`
