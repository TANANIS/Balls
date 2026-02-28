using Godot;

public partial class PlayerHealth
{
	private async void TriggerDamageFeedback()
	{
		if (!EnableDamageFeedback || _isDead || GetTree() == null)
			return;

		Node2D anchor = GetParentOrNull<Node2D>();
		if (anchor == null)
			return;

		CanvasItem sprite = anchor.GetNodeOrNull<CanvasItem>("Sprite2D");
		float duration = Mathf.Clamp(DamageFeedbackDurationSeconds, 0.02f, 0.30f);
		if (duration <= 0f)
			return;

		_damageFeedbackToken++;
		int token = _damageFeedbackToken;

		if (sprite != null)
		{
			EnsureDamageFlashMaterial();
			if (_damageFlashMaterial != null)
			{
				if (sprite.Material != _damageFlashMaterial)
					_spriteMaterialBeforeFlash = sprite.Material;
				sprite.Material = _damageFlashMaterial;
				_damageFlashMaterial.SetShaderParameter("flash_amount", Mathf.Clamp(DamageFlashStrength, 0f, 1f));
			}
		}

		var timer = GetTree().CreateTimer(duration * 0.45f, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (token != _damageFeedbackToken)
			return;

		if (sprite != null && _damageFlashMaterial != null)
			_damageFlashMaterial.SetShaderParameter("flash_amount", Mathf.Clamp(DamageFlashStrength * 0.45f, 0f, 1f));

		timer = GetTree().CreateTimer(duration * 0.55f, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		if (token != _damageFeedbackToken)
			return;

		if (sprite != null && _damageFlashMaterial != null)
		{
			_damageFlashMaterial.SetShaderParameter("flash_amount", 0f);
			sprite.Material = _spriteMaterialBeforeFlash;
		}
	}

	private void EnsureDamageFlashMaterial()
	{
		if (_damageFlashMaterial != null)
			return;

		var shader = new Shader();
		shader.Code = @"
shader_type canvas_item;
uniform float flash_amount : hint_range(0.0, 1.0) = 0.0;

void fragment()
{
	vec4 tex = texture(TEXTURE, UV) * COLOR;
	tex.rgb = mix(tex.rgb, vec3(1.0), flash_amount);
	COLOR = tex;
}";

		_damageFlashMaterial = new ShaderMaterial();
		_damageFlashMaterial.Shader = shader;
		_damageFlashMaterial.SetShaderParameter("flash_amount", 0f);
	}

	private void ResetVisualRuntimeState()
	{
		_damageFeedbackToken++;
		_shieldFlashToken++;
		_shieldFlashActive = false;

		Node2D anchor = GetParentOrNull<Node2D>();
		CanvasItem sprite = anchor?.GetNodeOrNull<CanvasItem>("Sprite2D");
		if (sprite != null && _damageFlashMaterial != null)
		{
			_damageFlashMaterial.SetShaderParameter("flash_amount", 0f);
			sprite.Material = _spriteMaterialBeforeFlash;
		}

		if (_shieldSprite != null)
		{
			_shieldSprite.Visible = false;
			_shieldSprite.Modulate = ShieldReadyColor;
		}

		if (_shieldFallbackRing != null)
		{
			_shieldFallbackRing.Visible = false;
			_shieldFallbackRing.DefaultColor = ShieldFallbackRingColor;
		}
	}
}
