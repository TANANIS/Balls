/*
 * IDamageable.cs
 *
 * 職責：
 * - 定義「可承受傷害」的最小契約
 * - CombatSystem 只依賴這個介面，不依賴 Player/Enemy 的具體類別
 *
 * 注意：
 * - 這裡只放 CombatSystem 必須知道的東西
 * - 其他花俏資訊（護甲、屬性）未來要加也可以，但先別膨脹
 */

public interface IDamageable
{
    // 是否已死亡：避免重複扣血
    bool IsDead { get; }

    // 是否免疫：例如 Player 無敵時間、Enemy 盾牌狀態等
    bool IsInvincible { get; }

    // 承受傷害：由 CombatSystem 最終呼叫
    void TakeDamage(int amount, object source);

}
