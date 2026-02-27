using Godot;

public partial class RunContext : Node
{
	private const string FallbackCharacterPath = "res://Data/Characters/RangedCharacter.tres";

	public static RunContext Instance { get; private set; }

	[Export] public CharacterDefinition DefaultCharacter;

	public CharacterDefinition SelectedCharacter { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		if (DefaultCharacter == null)
			DefaultCharacter = GD.Load<CharacterDefinition>(FallbackCharacterPath);
		CharacterStatsCsvService.ApplyTo(DefaultCharacter);
		if (SelectedCharacter == null)
			SelectedCharacter = DefaultCharacter;
		CharacterStatsCsvService.ApplyTo(SelectedCharacter);

		SelectedCharacter = ResolveSelectableCharacterOrDefault(SelectedCharacter);
	}

	public void SetSelectedCharacter(CharacterDefinition character)
	{
		CharacterStatsCsvService.ApplyTo(character);
		SelectedCharacter = ResolveSelectableCharacterOrDefault(character);
	}

	public CharacterDefinition GetSelectedOrDefault()
	{
		return SelectedCharacter ?? DefaultCharacter;
	}

	private CharacterDefinition ResolveSelectableCharacterOrDefault(CharacterDefinition candidate)
	{
		CharacterDefinition fallback = DefaultCharacter ?? candidate;
		CharacterStatsCsvService.ApplyTo(candidate);
		CharacterStatsCsvService.ApplyTo(fallback);
		if (candidate != null && MetaProgressionService.Instance.IsCharacterUnlocked(candidate.CharacterId))
			return candidate;
		if (fallback != null && MetaProgressionService.Instance.IsCharacterUnlocked(fallback.CharacterId))
			return fallback;
		return fallback;
	}
}
