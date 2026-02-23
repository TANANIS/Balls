using Godot;
using System.Linq;
using System.Text;

public partial class GameFlowUI
{
	private void EnterCharacterSelect()
	{
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startCharacterSelectOpen = true;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: true);

		if (_selectedCharacterDefinition == null)
			_selectedCharacterDefinition = RunContext.Instance?.GetSelectedOrDefault() ?? _rangedCharacter ?? _meleeCharacter ?? _tankCharacter;
		_selectedCharacterDefinition = ResolveFirstUnlockedCharacterDefinition(_selectedCharacterDefinition);

		RefreshCharacterSelectUi();
		_startCharacterConfirmButton?.GrabFocus();
	}

	private void RefreshCharacterSelectUi()
	{
		if (_startCharacterFluxValueLabel != null)
			_startCharacterFluxValueLabel.Text = MetaProgressionService.Instance.CurrencyWallet.ToString();

		if (_startCharacterDescriptionLabel != null)
		{
			if (_selectedCharacterDefinition != null)
				_startCharacterDescriptionLabel.Text = BuildMetaProgressionPresentation(_selectedCharacterDefinition);
			else
				_startCharacterDescriptionLabel.Text = Tr("UI.START.NO_CHARACTER_DEF");
		}

		if (_startCharacterRangedButton != null && _rangedCharacter != null)
		{
			bool unlocked = IsCharacterUnlocked(_rangedCharacter);
			_startCharacterRangedButton.Text = unlocked
				? _rangedCharacter.GetLocalizedDisplayName()
				: $"{_rangedCharacter.GetLocalizedDisplayName()} [{TrOrDefault("UI.META.LOCKED_SHORT", "Locked", "\u672a\u89e3\u9396")}]";
			_startCharacterRangedButton.Disabled = false;
		}
		if (_startCharacterMeleeButton != null && _meleeCharacter != null)
		{
			bool unlocked = IsCharacterUnlocked(_meleeCharacter);
			_startCharacterMeleeButton.Text = unlocked
				? _meleeCharacter.GetLocalizedDisplayName()
				: $"{_meleeCharacter.GetLocalizedDisplayName()} [{TrOrDefault("UI.META.LOCKED_SHORT", "Locked", "\u672a\u89e3\u9396")}]";
			_startCharacterMeleeButton.Disabled = false;
		}
		if (_startCharacterTankButton != null && _tankCharacter != null)
		{
			bool unlocked = IsCharacterUnlocked(_tankCharacter);
			_startCharacterTankButton.Text = unlocked
				? _tankCharacter.GetLocalizedDisplayName()
				: $"{_tankCharacter.GetLocalizedDisplayName()} [{TrOrDefault("UI.META.LOCKED_SHORT", "Locked", "\u672a\u89e3\u9396")}]";
			_startCharacterTankButton.Disabled = false;
		}

		if (_startCharacterConfirmButton != null)
		{
			if (_selectedCharacterDefinition == null)
			{
				_startCharacterConfirmButton.Disabled = true;
			}
			else if (IsCharacterUnlocked(_selectedCharacterDefinition))
			{
				_startCharacterConfirmButton.Disabled = false;
				_startCharacterConfirmButton.Text = Tr("UI.START.CONFIRM_START_RUN");
			}
			else
			{
				int unlockCost = GetCharacterUnlockCost(_selectedCharacterDefinition);
				bool canUnlock = MetaProgressionService.Instance.CanUnlockCharacter(_selectedCharacterDefinition.CharacterId, out _);
				_startCharacterConfirmButton.Disabled = !canUnlock;
				_startCharacterConfirmButton.Text = $"{TrOrDefault("UI.META.UNLOCK", "Unlock", "\u89e3\u9396")} ({unlockCost} {TrOrDefault("UI.META.FLUX", "Flux", "Flux")})";
			}
		}
	}

	private string BuildMetaProgressionPresentation(CharacterDefinition def)
	{
		bool unlocked = IsCharacterUnlocked(def);
		var meta = MetaProgressionService.Instance;
		var sb = new StringBuilder();
		sb.Append(def.GetLocalizedDisplayName()).Append('\n');
		sb.Append($"{TrOrDefault("UI.META.FLUX", "Flux", "Flux")}: {meta.CurrencyWallet}").Append('\n');

		if (!unlocked)
		{
			int cost = 0;
			if (ProgressionDefs.TryGetCharacter(def.CharacterId, out CharacterDef defMeta))
				cost = defMeta.UnlockCost;
			sb.Append($"{TrOrDefault("UI.META.STATUS", "Status", "\u72c0\u614b")}: {TrOrDefault("UI.META.LOCKED", "Locked", "\u672a\u89e3\u9396")}").Append('\n');
			sb.Append($"{TrOrDefault("UI.META.UNLOCK_COST", "Unlock Cost", "\u89e3\u9396\u9700\u6c42")}: {cost} {TrOrDefault("UI.META.FLUX", "Flux", "Flux")}").Append('\n');
			sb.Append('\n').Append(TrOrDefault("UI.META.CHAR_LOCKED_DESC", "This character is not unlocked yet.", "\u6b64\u89d2\u8272\u5c1a\u672a\u89e3\u9396\u3002"));
			return sb.ToString();
		}

		int level = meta.GetCharacterLevel(def.CharacterId);
		bool zh = TranslationServer.GetLocale().StartsWith("zh");
		sb.Append($"{TrOrDefault("UI.META.CHAR_LEVEL", "Character Level", "\u89d2\u8272\u7b49\u7d1a")}: Lv.{level}").Append("\n\n");
		sb.Append(def.GetLocalizedDescription()).Append("\n\n");
		sb.Append(TrOrDefault("UI.META.ATTACK", "Attack", "\u653b\u64ca")).Append(": ").Append(GetPrimaryRoleLabel(def, zh)).Append('\n');
		sb.Append(TrOrDefault("UI.META.MOBILITY", "Mobility", "\u6a5f\u52d5")).Append(": ").Append(GetMobilityRoleLabel(def, zh)).Append('\n');
		sb.Append(TrOrDefault("UI.META.SURVIVAL", "Survival", "\u751f\u5b58")).Append(": ").Append(GetSurvivalRoleLabel(def, zh)).Append("\n\n");
		sb.Append(TrOrDefault("UI.META.ABILITY_TREE", "Character Ability Tree", "\u89d2\u8272\u80fd\u529b\u6a39")).Append(':').Append('\n');
		sb.Append(BuildAbilityTreeFrameworkText(def));
		return sb.ToString();
	}

	private string BuildAbilityTreeFrameworkText(CharacterDefinition def)
	{
		if (!ProgressionDefs.TryGetCharacter(def.CharacterId, out CharacterDef defMeta))
			return TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e");

		if (defMeta.AbilityNodes == null || defMeta.AbilityNodes.Count == 0)
			return TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e");

		var unlockedNodes = MetaProgressionService.Instance.GetUnlockedAbilityNodes(def.CharacterId);
		var sb = new StringBuilder();
		for (int i = 0; i < defMeta.AbilityNodes.Count; i++)
		{
			AbilityNodeDef node = defMeta.AbilityNodes[i];
			if (node == null)
				continue;

			bool unlocked = unlockedNodes.Contains(node.NodeId);
			string status = unlocked
				? TrOrDefault("UI.META.UNLOCKED", "Unlocked", "\u5df2\u89e3\u9396")
				: TrOrDefault("UI.META.LOCKED", "Locked", "\u672a\u89e3\u9396");
			sb.Append("- ").Append(node.NodeId).Append(" [").Append(status).Append("] ");
			sb.Append($"(Lv.{node.MinCharacterLevel} / {node.UnlockCost} {TrOrDefault("UI.META.FLUX", "Flux", "Flux")})");
			if (i < defMeta.AbilityNodes.Count - 1)
				sb.Append('\n');
		}

		return sb.Length == 0
			? TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e")
			: sb.ToString();
	}

	private static string GetPrimaryRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.PrimaryAbility == AttackAbilityKind.Melee)
			return zh ? "\u8fd1\u6230" : "Melee";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged && def.RangedFirePattern == PrimaryFirePattern.Burst2)
			return zh ? "\u4e8c\u9023\u767c" : "2-round burst";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged && def.RangedFirePattern == PrimaryFirePattern.Burst3)
			return zh ? "\u4e09\u9023\u767c" : "3-round burst";

		if (def.PrimaryAbility == AttackAbilityKind.Ranged)
			return zh ? "\u55ae\u767c\u5c04\u64ca" : "Single shot";

		return zh ? "\u57fa\u790e" : "Basic";
	}

	private static string GetMobilityRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.MobilityAbility == MobilityAbilityKind.Dash)
			return zh ? "\u7a7a\u767d\u9375\u885d\u523a" : "Spacebar Dash";
		return zh ? "\u57fa\u790e\u79fb\u52d5" : "Base movement";
	}

	private static string GetSurvivalRoleLabel(CharacterDefinition def, bool zh)
	{
		if (def.RegenAmount > 0)
		{
			if (zh)
				return $"\u8f03\u9ad8\u57fa\u790e\u751f\u547d ({def.MaxHp})\uff0c\u6bcf {def.RegenIntervalSeconds:0} \u79d2\u56de\u5fa9 {def.RegenAmount}\u3002";
			return $"Higher base HP ({def.MaxHp}), recovers {def.RegenAmount} every {def.RegenIntervalSeconds:0}s.";
		}

		return zh
			? $"\u57fa\u790e\u751f\u547d {def.MaxHp}\u3002"
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
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		_startButton?.GrabFocus();
	}

	private void OnCharacterSelectConfirmPressed()
	{
		if (_selectedCharacterDefinition == null)
		{
			AudioManager.Instance?.PlaySfxUiExit();
			return;
		}

		if (!IsCharacterUnlocked(_selectedCharacterDefinition))
		{
			bool unlocked = MetaProgressionService.Instance.TryUnlockCharacter(_selectedCharacterDefinition.CharacterId);
			if (!unlocked)
			{
				AudioManager.Instance?.PlaySfxUiExit();
				RefreshCharacterSelectUi();
				return;
			}

			AudioManager.Instance?.PlaySfxUiButton();
			RefreshCharacterSelectUi();
			return;
		}

		AudioManager.Instance?.PlaySfxUiButton();
		RunContext.Instance?.SetSelectedCharacter(_selectedCharacterDefinition);
		StartRun();
	}

	private static bool IsCharacterUnlocked(CharacterDefinition def)
	{
		if (def == null)
			return false;

		return MetaProgressionService.Instance.IsCharacterUnlocked(def.CharacterId);
	}

	private CharacterDefinition ResolveFirstUnlockedCharacterDefinition(CharacterDefinition preferred)
	{
		if (IsCharacterUnlocked(preferred))
			return preferred;
		if (IsCharacterUnlocked(_rangedCharacter))
			return _rangedCharacter;
		if (IsCharacterUnlocked(_meleeCharacter))
			return _meleeCharacter;
		if (IsCharacterUnlocked(_tankCharacter))
			return _tankCharacter;
		return preferred ?? _rangedCharacter ?? _meleeCharacter ?? _tankCharacter;
	}

	private static int GetCharacterUnlockCost(CharacterDefinition def)
	{
		if (def == null)
			return 0;
		if (!ProgressionDefs.TryGetCharacter(def.CharacterId, out CharacterDef defMeta))
			return 0;
		return Mathf.Max(0, defMeta.UnlockCost);
	}
}
