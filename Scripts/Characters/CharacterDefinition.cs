using Godot;

[GlobalClass]
public partial class CharacterDefinition : Resource
{
	[Export] public string CharacterId = "ranged";
	[Export] public string DisplayName = "Ranger";
	[Export(PropertyHint.MultilineText)] public string Description = "";

	[ExportGroup("Ability Slots")]
	[Export] public AttackAbilityKind PrimaryAbility = AttackAbilityKind.Ranged;
	[Export] public AttackAbilityKind SecondaryAbility = AttackAbilityKind.Melee;
	[Export] public MobilityAbilityKind MobilityAbility = MobilityAbilityKind.Dash;

	[Export] public string PrimaryAction = InputActions.AttackPrimary;
	[Export] public string SecondaryAction = InputActions.AttackSecondary;
	[Export] public string MobilityAction = InputActions.Dash;

	[ExportGroup("Movement Stats")]
	[Export] public float MoveMaxSpeed = 320f;
	[Export] public float MoveAccel = 2200f;
	[Export] public float MoveFriction = 2600f;
	[Export] public float MoveStopThreshold = 5f;

	[ExportGroup("Health Stats")]
	[Export] public int MaxHp = 3;
	[Export] public float HurtIFrame = 0.5f;
	[Export] public int RegenAmount = 0;
	[Export] public float RegenIntervalSeconds = 60f;

	[ExportGroup("Ranged Ability Stats")]
	[Export] public int RangedDamage = 1;
	[Export] public float RangedCooldown = 0.32f;
	[Export] public float RangedProjectileSpeed = 760f;
	[Export] public PrimaryFirePattern RangedFirePattern = PrimaryFirePattern.Single;
	[Export] public float RangedBurstShotInterval = 0.08f;

	[ExportGroup("Melee Ability Stats")]
	[Export] public int MeleeDamage = 3;
	[Export] public float MeleeCooldown = 0.35f;
	[Export] public float MeleeRange = 160f;
	[Export] public float MeleeArcDegrees = 180f;

	[ExportGroup("Dash Ability Stats")]
	[Export] public float DashSpeed = 900f;
	[Export] public float DashDuration = 0.12f;
	[Export] public float DashCooldown = 0.6f;
	[Export] public float DashIFrame = 0.08f;
}
