using Godot;
using System.Text;

public partial class GameFlowUI
{
	private void EnterCharacterSelect()
	{
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startCharacterSelectOpen = true;
		if (_startMainVBox != null)
			_startMainVBox.Visible = false;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = true;

		if (_selectedCharacterDefinition == null)
			_selectedCharacterDefinition = RunContext.Instance?.GetSelectedOrDefault() ?? _rangedCharacter ?? _meleeCharacter ?? _tankCharacter;

		RefreshCharacterSelectUi();
		_startCharacterConfirmButton?.GrabFocus();
	}

	private void RefreshCharacterSelectUi()
	{
		if (_startCharacterDescriptionLabel != null)
		{
			if (_selectedCharacterDefinition != null)
				_startCharacterDescriptionLabel.Text = BuildCharacterPresentation(_selectedCharacterDefinition);
			else
				_startCharacterDescriptionLabel.Text = Tr("UI.START.NO_CHARACTER_DEF");
		}

		if (_startCharacterRangedButton != null && _rangedCharacter != null)
			_startCharacterRangedButton.Text = _rangedCharacter.GetLocalizedDisplayName();
		if (_startCharacterMeleeButton != null && _meleeCharacter != null)
			_startCharacterMeleeButton.Text = _meleeCharacter.GetLocalizedDisplayName();
		if (_startCharacterTankButton != null && _tankCharacter != null)
			_startCharacterTankButton.Text = _tankCharacter.GetLocalizedDisplayName();
	}

	private string BuildCharacterPresentation(CharacterDefinition def)
	{
		bool zh = TranslationServer.GetLocale().StartsWith("zh");
		var sb = new StringBuilder();
		sb.Append(def.GetLocalizedDisplayName()).Append('\n');
		sb.Append(def.GetLocalizedDescription()).Append("\n\n");
		sb.Append(zh ? "攻擊：" : "Attack: ").Append(GetPrimaryRoleLabel(def, zh)).Append('\n');
		sb.Append(zh ? "機動：" : "Mobility: ").Append(GetMobilityRoleLabel(def, zh)).Append('\n');
		sb.Append(zh ? "生存：" : "Survival: ").Append(GetSurvivalRoleLabel(def, zh));
		return sb.ToString();
	}

	private static string GetPrimaryRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.PrimaryAbility == AttackAbilityKind.Melee)
			return zh ? "近戰" : "Melee";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged && def.RangedFirePattern == PrimaryFirePattern.Burst2)
			return zh ? "雙發連射" : "2-round burst";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged && def.RangedFirePattern == PrimaryFirePattern.Burst3)
			return zh ? "三發連射" : "3-round burst";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged)
			return zh ? "單發射擊" : "Single shot";

		return zh ? "基礎" : "Basic";
	}

	private static string GetMobilityRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.MobilityAbility == MobilityAbilityKind.Dash)
			return zh ? "空白鍵 Dash" : "Spacebar Dash";
		return zh ? "基礎移動" : "Base movement";
	}

	private static string GetSurvivalRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.RegenAmount > 0)
		{
			if (zh)
				return $"較高基礎生命 ({def.MaxHp})，每 {def.RegenIntervalSeconds:0} 秒回復 {def.RegenAmount}。";
			return $"Higher base HP ({def.MaxHp}), recovers {def.RegenAmount} every {def.RegenIntervalSeconds:0}s.";
		}

		return zh
			? $"基礎生命 {def.MaxHp}。"
			: $"Base HP {def.MaxHp}.";
	}

	private void OnCharacterRangedPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _rangedCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterMeleePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _meleeCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterTankPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _tankCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterSelectBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startCharacterSelectOpen = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = true;
		_startButton?.GrabFocus();
	}

	private void OnCharacterSelectConfirmPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		RunContext.Instance?.SetSelectedCharacter(_selectedCharacterDefinition);
		StartRun();
	}
}
