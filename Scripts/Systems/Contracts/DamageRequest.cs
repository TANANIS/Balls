using Godot;

/*
 * DamageRequest.cs
 *
 * 職責：
 * - 只是一個「傷害請求資料包」
 * - 不含任何裁決邏輯
 * - Sensor（命中偵測者）負責填好資料後送進 CombatSystem
 */

public readonly struct DamageRequest
{
    // 來源：誰發動了這次傷害（子彈、敵人、陷阱…）
    public readonly Node Source;

    // 目標：誰要承受這次傷害（通常是 Player 或 Enemy）
    public readonly Node Target;

    // 基礎傷害：裁決前的原始傷害值
    public readonly int BaseDamage;

    // 事件時間：可用於除錯與防重入（例如同 frame 多次）
    public readonly ulong Frame;

    // 事件位置：可選，用於命中特效/擊退方向計算（目前先保留）
    public readonly Vector2 WorldPos;

    // 自訂標籤：可用於區分來源類型（"melee" / "bullet" / "contact" / "trap"）
    public readonly string Tag;

    public DamageRequest(
        Node source,
        Node target,
        int baseDamage,
        Vector2 worldPos,
        string tag = ""
    )
    {
        Source = source;
        Target = target;
        BaseDamage = baseDamage;
        WorldPos = worldPos;
        Tag = tag;

        // 以引擎 frame 作為事件戳記，CombatSystem 可用它做防重入
        Frame = Engine.GetPhysicsFrames();
    }

    public bool IsValid()
    {
        // 最基本的資料完整性檢查
        if (Source == null || Target == null) return false;
        if (BaseDamage <= 0) return false;
        return true;
    }
}
