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
		if (SelectedCharacter == null)
			SelectedCharacter = DefaultCharacter;
	}

	public void SetSelectedCharacter(CharacterDefinition character)
	{
		SelectedCharacter = character ?? DefaultCharacter;
	}

	public CharacterDefinition GetSelectedOrDefault()
	{
		return SelectedCharacter ?? DefaultCharacter;
	}
}
