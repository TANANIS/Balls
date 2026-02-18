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
