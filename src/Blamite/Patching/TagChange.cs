using System;
using System.Collections.Generic;
using Blamite.Blam;

namespace Blamite.Patching;

[Flags]
public enum BlockChangeFlag : byte
{
	None = 0,
	TopLevel = 1 << 0,
	DataRef = 1 << 1,
	TagBlock = 1 << 2
}

public class BlockChange(uint baseSize) 
{
	public uint BaseSize { get; internal set; } = baseSize;

	public List<DataChange> Changes { get; internal set; } = [];

	public Dictionary<uint, List<DataChange>> DataRefChanges { get; } = new();

    public Dictionary<uint, BlockChange> TagBlockChanges { get; } = new();

	public BlockChangeFlag ChangeFlag 
	{
		get 
		{
			var flag = BlockChangeFlag.None;
            if (Changes.Count > 0)
				flag |= BlockChangeFlag.TopLevel;
            if (DataRefChanges.Count > 0)
                flag |= BlockChangeFlag.DataRef;
            if (TagBlockChanges.Count > 0)
                flag |= BlockChangeFlag.TagBlock;
            return flag;
        }
	}
}

public class TagChange(string name, int group, uint baseSize) : BlockChange(baseSize)
{
	/// <summary>
	///     The path of the tag in <see cref="ICacheFile"/>.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///    The magic of the group of the tag in <see cref="ITagGroup"/>.
	/// </summary>
	public int Group { get; } = group;
}