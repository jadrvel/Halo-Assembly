using System;
using System.Collections.Generic;

#nullable enable
namespace Blamite.Patching
{
	public class PatchInfo
	{
		/// <summary>
		///     The ID of the .map file that the patch is meant for. -1 if this information is not present.
		/// </summary>
		public int MapID { get; set; } = -1;

		/// <summary>
		///     The internal name of the .map file that the patch is meant for. Can be null.
		/// </summary>
		public string? MapInternalName { get; set; }

		/// <summary>
		///     The desired name of the output .map file when the patch is applied. Can be null.
		/// </summary>
		public string? OutputName { get; set; }

		/// <summary>
		///     The name of the patch.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     A short description of the patch.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///     The patch's author.
		/// </summary>
		public string Author { get; set; }

		/// <summary>
		///     A screenshot of the patch in action.
		/// </summary>
		public byte[]? Screenshot { get; set; }

		/// <summary>
		///		The build string of the original cache file.
		/// </summary>
		public string? BuildString { get; set; }

		public bool PC { get; set; } = false;
	}

    public class Patch : PatchInfo
	{
		/// <summary>
		///     Changes which should be made to different parts of a map file.
		/// </summary>
		public List<SegmentChange> SegmentChanges { get; internal set; } = [];

		/// <summary>
		///     The base pointer to add to meta change offsets to get a pointer to poke to.
		/// </summary>
		public long MetaPokeBase { get; set; }

		/// <summary>
		///     The index in <see cref="SegmentChanges" /> of the file's meta area changes.
		///     Can be -1 if no meta change information is present in <see cref="SegmentChanges" />.
		/// </summary>
		public int MetaChangesIndex { get; set; } = -1;

		/// <summary>
		///     Embedded BLF and mapinfo files. Defaults to null.
		/// </summary>
		public BlfContent? CustomBlfContent { get; set; }

		#region Deprecated

		/// <summary>
		///     Changes which should be made to the map file's meta area.
		///     The offset of each change is the meta pointer to where the change should be made.
		/// </summary>
		[Obsolete]
		public List<DataChange> MetaChanges { get; internal set; } = [];

		/// <summary>
		///     Changes that should be made to a map's locales.
		/// </summary>
		[Obsolete]
		public List<LanguageChange> LanguageChanges { get; internal set; } = [];

		#endregion
	}

	public class TagPatch : PatchInfo 
	{
		/// <summary>
		///     Changes which should be made to different tags of map files.
		/// </summary>
		public List<TagChange> TagChanges { get; internal set; } = [];
    }
}