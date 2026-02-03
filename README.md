# Project Genesis: 2D 幾何極簡生存遊戲

這是一份開發紀錄與架構指南。本專案核心目標是建立一個**高飽和、高壓、且具備清晰權責分離**的生存射擊遊戲原型。

## 🛠 架構哲學 (Core Philosophy)

本專案不採用傳統的「萬能類別（God Object）」設計，而是嚴格遵循以下原則：

1. **裁決權中心化**：只有 `CombatSystem` 能決定傷害是否成立，其他模組僅能發出「請求」。
2. **狀態與行為分離**：
* **狀態 (State)**：你是誰？（HP、是否無敵、是否死亡）→ 由 `Health` 模組維護。
* **行為 (Behavior)**：你在做什麼？（移動、Dash、射擊）→ 由獨立行為節點處理。


3. **資料流單向化**：`Request -> Resolve -> Apply`。

---

## 🏗 專案結構 (Project Structure)

### Scene Tree 節點規範

```text
Game (Node2D)
├─ Player (CharacterBody2D)      # 門面：負責調度與輸入轉發
│  ├─ PlayerHealth (Node)        # 狀態：HP、無敵 Timer
│  ├─ PlayerMovement (Node)      # 行為：加速/摩擦運動
│  ├─ PlayerDash (Node)          # 行為：位移與 I-Frame 注入
│  └─ PlayerWeapon (Node)        # 行為：射擊與子彈生成
├─ Systems (Node)                # 權威中心
│  ├─ CombatSystem (Node)        # 核心：唯一傷害裁決入口
│  └─ SpawnSystem (Node)         # 節奏：敵人生成管理
├─ Projectiles (Node2D)          # 容器：子彈
└─ Enemies (Node2D)              # 容器：敵人

```

### 📁 資料夾組織

* `/Scripts/Player/`: 玩家模組化腳本。
* `/Scripts/Systems/`: 全域權威系統（Combat, Spawn）。
* `/Scripts/Projectiles/`: 子彈邏輯（僅限傳感與飛行）。
* `/_Legacy/`: 棄用的事件驅動型碰撞腳本（僅供參考，禁止引用）。

---

## ⚔️ 戰鬥管線 (Combat Pipeline)

當攻擊發生時，必須經過以下流程：

1. **偵測 (Detection)**：`Bullet` 或 `EnemyHitbox` 偵測到目標。
2. **請求 (Request)**：建立 `DamageRequest` 物件，包含來源、傷害量、擊退方向。
3. **裁決 (Authority)**：呼叫 `CombatSystem.RequestDamage(req)`。
* 檢查目標是否已死。
* 檢查目標是否處於無敵幀。


4. **落地 (Execution)**：若通過裁決，`CombatSystem` 呼叫目標的 `TakeDamage()`。

---

## 🚦 碰撞層級定義 (Collision Layers)

| Layer | 名稱 | 職責 |
| --- | --- | --- |
| 1 | World | 地形、牆壁、障礙物 |
| 2 | PlayerBody | 玩家物理本體（處理 MoveAndSlide 碰撞） |
| 3 | EnemyBody | 敵人物理本體 |
| 4 | PlayerBullet | 玩家子彈偵測層 |
| 5 | PlayerHurtbox | 玩家受傷判定區 |
| 6 | EnemyHurtbox | 敵人受傷判定區 |
| 7 | EnemyHitbox | 敵人攻擊判定區 |

---

## 📝 開發者備忘錄 (Dev Notes)

### 關於 Timer 的放置

* **不要把所有 Timer 都塞在 Player.cs！**
* 如果是「受傷無敵時間」，請去 `PlayerHealth.cs`。
* 如果是「Dash 冷卻時間」，請去 `PlayerDash.cs`。
* 如果是「射擊間隔」，請去 `PlayerWeapon.cs`。

### 關於物理座標

* 視覺 Sprite 的偏移（Offset）不代表物理位置。Debug 時永遠以節點的 `GlobalPosition` 為準。

---

## 🚀 待辦清單 (Roadmap)

* [ ] **DebugSystem**: 實作碰撞區可視化與傷害 Log。
* [ ] **Enemy AI**: 實作基礎追蹤與避障。
* [ ] **Spawn 2.0**: 實作避障生成與難度曲線調整。
* [ ] **UI MVP**: 實作幾何風格的 3 格血條。

---