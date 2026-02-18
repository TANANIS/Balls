# 待辦清單

## 進行中
- [ ] 完成以 Stability 驅動的 Pressure 調校，並在各種戰鬥情境下進行手感驗證。
- [ ] 完成宇宙事件實際玩法效果（Energy Surge / Gravity Inversion），不只停留在 UI 提示。

## 下一步
- [ ] 加入高風險行為觸發的 Stability 回復（第一版 MVP：高壓區擊殺回復）。
- [ ] 新增 Stability / Event HUD 區塊，明確顯示 phase 與當前 modifier。
- [ ] 在結算中加入終局原因拆分（`Player Down` vs `Universe Collapsed`）。
- [ ] 依 phase 重新調整障礙物與刷怪密度，並以大範圍移動實測。
- [ ] 大改玩家升級系統（重做節奏、可選分支與 build identity）。
- [ ] 加入局外養成系統（跨局資源與永久成長）。
- [ ] 新增 `Esc` 暫停功能（暫停、繼續、返回主選單）。
- [ ] 新增設定介面（音樂、音效、視窗大小與基礎畫面選項）。

## 技術債
- [ ] 清理或遷移剩餘的時間爬升參數，避免與 Stability phase 規則重疊。
- [ ] 補上 Stability phase 切換與 collapse 觸發的自動化測試。
- [ ] 在 phase-2 / phase-3 調整後，統一稽核 Director 系統註解一致性。
