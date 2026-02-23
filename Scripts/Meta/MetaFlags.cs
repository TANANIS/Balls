using System.Collections.Generic;

public sealed class MetaFlags
{
	private readonly HashSet<string> _flags = new();

	public IReadOnlyCollection<string> Values => _flags;

	public bool Has(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag))
			return false;

		return _flags.Contains(flag);
	}

	public bool Add(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag))
			return false;

		return _flags.Add(flag);
	}

	public bool Remove(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag))
			return false;

		return _flags.Remove(flag);
	}

	public void ReplaceAll(IEnumerable<string> flags)
	{
		_flags.Clear();
		if (flags == null)
			return;

		foreach (string flag in flags)
		{
			if (!string.IsNullOrWhiteSpace(flag))
				_flags.Add(flag);
		}
	}
}
