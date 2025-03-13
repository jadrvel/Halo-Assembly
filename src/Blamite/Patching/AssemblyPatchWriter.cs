using System;
using System.Collections.Generic;
using System.Linq;
using Blamite.IO;

namespace Blamite.Patching
{
	public class AssemblyPatchWriter
	{
		private readonly IWriter _writer;

		private readonly ContainerWriter _container;

        public AssemblyPatchWriter(IWriter writer)
        {
            _writer = writer;
			_container = new ContainerWriter(_writer);
        }

		public static void WritePatch(IWriter writer, Patch patch)
		{
			var inst = new AssemblyPatchWriter(writer);
			inst.WritePatch(patch);
		}

		public static void WritePatch(IWriter writer, TagPatch patch)
		{
			var inst = new AssemblyPatchWriter(writer);
			inst.WritePatch(patch);
		}

        public void WritePatch(Patch patch)
		{
			_container.StartBlock("asmp", 0);

			WritePatchInfo(patch);
			WriteSegmentChanges(patch.SegmentChanges);
			WriteBlfInfo(patch.CustomBlfContent);

			#region Deprecated

			WriteMetaChanges(patch.MetaChanges);
			WriteLocaleChanges(patch.LanguageChanges);

			#endregion Deprecated

            _container.EndBlock();
		}

		public void WritePatch(TagPatch patch)
		{
			_container.StartBlock("asmt", 0);

			WritePatchInfo(patch);
			WriteTagChanges(patch.TagChanges);

            _container.EndBlock();
		}

		private void WritePatchInfo(PatchInfo patch)
		{
			_container.StartBlock("titl", 3); // Version 2

			// Write target map info
			_writer.WriteInt32(patch.MapID);
			if (patch.MapInternalName != null)
				_writer.WriteAscii(patch.MapInternalName);
			else
				_writer.WriteByte(0);

			// Write patch info
			_writer.WriteUTF16(patch.Name);
			_writer.WriteUTF16(patch.Description);
			_writer.WriteUTF16(patch.Author);

			// Write screenshot
			if (patch.Screenshot != null)
			{
				_writer.WriteInt32(patch.Screenshot.Length);
				_writer.WriteBlock(patch.Screenshot);
			}
			else
			{
				_writer.WriteInt32(0);
			}

			// Write meta info
			if (patch is Patch p)
			{
				_writer.WriteInt64(p.MetaPokeBase);
				_writer.WriteSByte((sbyte)p.MetaChangesIndex);
            }

			// Write output name
			if (patch.OutputName != null)
				_writer.WriteAscii(patch.OutputName);
			else
				_writer.WriteByte(0);

			// PC?
			_writer.WriteByte(patch.PC ? (byte)1 : (byte)0);

			// Write the build string
			if (patch.BuildString != null)
				_writer.WriteAscii(patch.BuildString);
			else
				_writer.WriteByte(0);

			_container.EndBlock();
		}

		private void WriteSegmentChanges(ICollection<SegmentChange> changes)
		{
			if (changes.Count == 0)
				return;

			_container.StartBlock("segm", 0); // Version 0

			_writer.WriteByte((byte) changes.Count);
			foreach (var segment in changes)
			{
				_writer.WriteUInt32(segment.OldOffset);
				_writer.WriteUInt32(segment.OldSize);
				_writer.WriteUInt32(segment.NewOffset);
				_writer.WriteUInt32(segment.NewSize);
				_writer.WriteByte(Convert.ToByte(segment.ResizeAtEnd));

				WriteDataChanges(segment.DataChanges);
			}

			_container.EndBlock();
		}

		private void WriteTagChanges(ICollection<TagChange> changes)
		{
			if (changes.Count == 0)
				return;

			_container.StartBlock("tags", 0);

			_writer.WriteUInt32((uint)changes.Count);
			foreach (var tag in changes)
			{
				_writer.WriteInt32(tag.Group);
				_writer.WriteUTF16(tag.Name);
				WriteBlockChange(tag);
			}

			_container.EndBlock();
		}

		private void WriteBlockChange(BlockChange block)
		{
			_writer.WriteUInt32(block.BaseSize);

			var flag = block.ChangeFlag;
            _writer.WriteByte((byte)flag);

			if (flag.HasFlag(BlockChangeFlag.TopLevel))
				WriteDataChanges(block.Changes);

			if (flag.HasFlag(BlockChangeFlag.DataRef))
			{
				_writer.WriteUInt32((uint)block.DataRefChanges.Count);
				foreach (var pair in block.DataRefChanges)
				{
					_writer.WriteUInt32(pair.Key);
					WriteDataChanges(pair.Value);
				}
            }

			if (flag.HasFlag(BlockChangeFlag.TagBlock)) 
			{
				_writer.WriteUInt32((uint)block.TagBlockChanges.Count);
				foreach (var pair in block.TagBlockChanges)
				{
					_writer.WriteUInt32(pair.Key);
					WriteBlockChange(pair.Value);
				}
            }
        }

        private void WriteDataChanges(ICollection<DataChange> changes)
		{
			var fourByteChanges = changes.Where(c => c.Data.Length == 4).ToList();
			var otherChanges = changes.Where(c => c.Data.Length != 4).ToList();

			// Write 4-byte changes
			_writer.WriteUInt32((uint) fourByteChanges.Count);
			foreach (var change in fourByteChanges)
			{
				_writer.WriteUInt32(change.Offset);
				_writer.WriteBlock(change.Data);
			}

			// Write other changes
			_writer.WriteUInt32((uint) otherChanges.Count);
			foreach (var change in otherChanges)
			{
				_writer.WriteUInt32(change.Offset);
				_writer.WriteInt32(change.Data.Length);
				_writer.WriteBlock(change.Data);
			}
		}

		private void WriteBlfInfo(BlfContent blf)
		{
			if (blf == null)
				return;

			_container.StartBlock("blfc", 0); // Version 0

			_writer.WriteByte((byte) blf.TargetGame);

			// Write mapinfo filename
			if (blf.MapInfoFileName != null)
				_writer.WriteAscii(blf.MapInfoFileName);
			else
				_writer.WriteByte(0);

			// Write mapinfo data
			if (blf.MapInfo != null)
			{
				_writer.WriteUInt32((uint) blf.MapInfo.Length);
				_writer.WriteBlock(blf.MapInfo);
			}
			else
			{
				_writer.WriteUInt32(0);
			}

			// Write BLF containers
			_writer.WriteInt16((short) blf.BlfContainerEntries.Count);
			foreach (var blfContainerEntry in blf.BlfContainerEntries)
			{
				_writer.WriteAscii(blfContainerEntry.FileName);
				_writer.WriteUInt32((uint) blfContainerEntry.BlfContainer.Length);
				_writer.WriteBlock(blfContainerEntry.BlfContainer);
			}

			_container.EndBlock();
		}

		#region Deprecated

		private void WriteMetaChanges(ICollection<DataChange> metaChanges)
		{
			if (metaChanges.Count == 0)
				return;

			_container.StartBlock("meta", 0); // Version 0

			var fourByteChanges = metaChanges.Where(c => c.Data.Length == 4).ToList();
			var otherChanges = metaChanges.Where(c => c.Data.Length != 4).ToList();

			// Write 4-byte changes
			_writer.WriteUInt32((uint) fourByteChanges.Count);
			foreach (var change in fourByteChanges)
			{
				_writer.WriteUInt32(change.Offset);
				_writer.WriteBlock(change.Data);
			}

			// Write other changes
			_writer.WriteUInt32((uint) otherChanges.Count);
			foreach (var change in otherChanges)
			{
				_writer.WriteUInt32(change.Offset);
				_writer.WriteInt32(change.Data.Length);
				_writer.WriteBlock(change.Data);
			}

			_container.EndBlock();
		}

		private void WriteLocaleChanges(ICollection<LanguageChange> langChanges)
		{
			if (langChanges.Count == 0)
				return;

			_container.StartBlock("locl", 0); // Version 0

			// Write change data for each language
			_writer.WriteByte((byte) langChanges.Count);
			foreach (var language in langChanges)
			{
				_writer.WriteByte(language.LanguageIndex);

				// Write the change data for each string in the language
				_writer.WriteInt32(language.LocaleChanges.Count);
				foreach (var change in language.LocaleChanges)
				{
					_writer.WriteInt32(change.Index);
					_writer.WriteUTF8(change.NewValue);
				}
			}

			_container.EndBlock();
		}

		#endregion Deprecated
	}
}