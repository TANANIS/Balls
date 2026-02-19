# 待辦清單

## 已完成
- [x] 新增 `Esc` 暫停流程（暫停、繼續、重新開始、返回主選單）。
- [x] 新增設定介面（BGM、SFX、視窗模式、視窗尺寸）。
- [x] 設定存檔/讀檔（`user://settings.cfg`）。
- [x] 重寫開始畫面主版面與文案（繁體中文）。
- [x] 開始畫面新增 `Settings` 與 `離開遊戲`。
- [x] 暫停畫面新增 `Quit Game`。
- [x] 新增臨時宇宙背景（黑色遮罩 + 白色發光星點）。
- [x] 背景改為視差移動，提供角色移動參考。

## 進行中
- [ ] 進一步調校 Stability 驅動的 Pressure 手感（不同戰鬥密度與 phase）。
- [ ] 補齊 Universe Event 實際玩法效果（不只 UI 提示）。

## 下一步
- [ ] 加入高風險行為觸發的 Stability 回復（MVP：高壓區擊殺回復）。
- [ ] 新增 Stability / Event HUD（phase、modifier、警示狀態）。
- [ ] 擴充結算資料（死亡/崩潰/時間到的統計差異）。
- [ ] 大改玩家升級系統（build identity 與分支策略）。
- [ ] 加入局外養成（跨局資源與永久成長）。
- [ ] 依 phase 再調整障礙物與刷怪密度，並跑大範圍移動實測。

## 技術債
- [ ] 清理剩餘 time-ramp 參數，避免與 Stability phase 規則重疊。
- [ ] 補上 Stability phase 切換與 collapse 觸發的自動化測試。
- [ ] 稽核 Director / UI 系統註解一致性與命名統一。

## 玩家升級系統與分支策略（MVP）

### 升級區塊分類
- [ ] 建立 `UpgradeCategory`：`WeaponModifier`、`PressureModifier`、`AnomalySpecialist`、`SpatialControl`、`RiskAmplifier`
- [ ] 每個區塊先做 4-6 個基礎詞條（總量 20-30）
- [ ] 每個詞條需包含：效果敘述、數值、最大堆疊、權重、稀有度

### 詞條資料結構
- [ ] 建立 `UpgradeDef` 資料欄位：`id`、`name`、`category`、`rarity`、`weight`、`maxStack`
- [ ] 支援 `prerequisites`（前置）與 `exclusiveWith`（排他）
- [ ] 建立 `UpgradeRarity`：`Common`、`Rare`、`Epic`

### 抽選與分支策略
- [ ] 實作每次升級三選一
- [ ] 抽選需排除：已滿堆疊詞條、排他衝突詞條、未滿足前置詞條
- [ ] 實作「同類加權」：玩家每次選某分類，後續該分類出現權重小幅上升
- [ ] 保底規則：連續 N 次沒出特定稀有度時提高該稀有度權重

### 流派驗證（第一輪）
- [ ] 武器連射流：`WeaponModifier` + `RiskAmplifier`
- [ ] 壓力操控流：`PressureModifier` + `AnomalySpecialist`
- [ ] 控場位移流：`SpatialControl` + `PressureModifier`

### 系統串接
- [ ] 升級選擇後即時套用至玩家/戰鬥/壓力系統
- [ ] HUD 顯示目前 build 核心分類與關鍵詞條
- [ ] Run 結束統計：本局選擇分布、最終 build 類型、勝敗結果

### 驗收條件
- [ ] 30 分鐘遊玩內不出現無詞可抽或死循環
- [ ] 三個流派都能穩定成形，且體感差異明顯
- [ ] 同一流派重複遊玩仍有詞條變化，不會每局完全相同
