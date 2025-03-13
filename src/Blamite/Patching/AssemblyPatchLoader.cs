using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blamite.IO;

namespace Blamite.Patching
{
	public class AssemblyPatchLoader
	{
		private readonly IReader _reader;

        public AssemblyPatchLoader(IReader reader)
        {
            _reader = reader;
        }

		public static void LoadPatch(IReader reader, out Patch patch)
		{
			var inst = new AssemblyPatchLoader(reader);
			inst.LoadPatch(out patch);
		}

		public static void LoadPatch(IReader reader, out TagPatch patch)
		{
			var inst = new AssemblyPatchLoader(reader);
			inst.LoadPatch(out patch);
		}

        public void LoadPatch(out Patch patch)
		{
			var container = new ContainerReader(_reader);
			if (!container.NextBlock() || container.BlockName != "asmp")
				throw new InvalidOperationException("Invalid assembly patch");
			if (container.BlockVersion > 0)
				throw new InvalidOperationException("Unrecognized patch version");

			container.EnterBlock();
			ReadBlocks(container, out patch);
            container.LeaveBlock();
		}

		public void LoadPatch(out TagPatch patch)
		{
			var container = new ContainerReader(_reader);
			if (!container.NextBlock() || container.BlockName != "asmt")
				throw new InvalidOperationException("Invalid assembly patch");
			if (container.BlockVersion > 0)
				throw new InvalidOperationException("Unrecognized patch version");

			container.EnterBlock();
			ReadBlocks(container, out patch);
			container.LeaveBlock();
		}

        private void ReadBlocks(ContainerReader container, out Patch result)
		{
            result = new Patch();
            while (container.NextBlock())
			{
				var version = container.BlockVersion;
                switch (container.BlockName)
				{
					case "titl":
						ReadPatchInfo(version, result);
						break;

					case "segm":
						result.SegmentChanges = ReadSegmentChanges(version).ToList();
						break;

					case "blfc":
						result.CustomBlfContent = ReadBlfInfo(version);
						break;

					#region Deprecated

					case "meta":
						result.MetaChanges = ReadMetaChanges(version).ToList();
						break;

					case "locl":
						result.LanguageChanges = ReadLocaleChanges(version).ToList();
						break;

					#endregion Deprecated
				}
			}
		}

		private void ReadBlocks(ContainerReader container, out TagPatch result)
		{
			result = new TagPatch();
			while (container.NextBlock())
			{
				var version = container.BlockVersion;
				switch (container.BlockName)
				{
					case "titl":
						ReadPatchInfo(version, result);
						break;

					case "tags":
						result.TagChanges = ReadTagChanges(version).ToList();
						break;
				}
			}
		}

        private void ReadPatchInfo(byte version, PatchInfo output)
		{
			if (version > 3)
				throw new NotSupportedException("Unrecognized \"titl\" block version");

			// Version 0 (all versions)
			output.MapID = _reader.ReadInt32();
			output.MapInternalName = _reader.ReadAscii();
			output.Name = _reader.ReadUTF16();
			output.Description = _reader.ReadUTF16();
			output.Author = _reader.ReadUTF16();

			int screenshotLength = _reader.ReadInt32();
			if (screenshotLength > 0)
				output.Screenshot = _reader.ReadBlock(screenshotLength);

			// Version 1
			if (version >= 1 && output is Patch p) {
				p.MetaPokeBase = version >= 3 ? _reader.ReadInt64() : _reader.ReadUInt32();
				p.MetaChangesIndex = _reader.ReadSByte();
			}

			// Version 2
			output.OutputName = version >= 2 ? _reader.ReadAscii() : "";

			// Version 3
			if (version == 3)
			{
				output.PC = _reader.ReadByte() == 1;
				output.BuildString = _reader.ReadAscii();
			}
			else
				output.BuildString = "";
		}

		private IEnumerable<SegmentChange> ReadSegmentChanges(byte version)
		{
			if (version > 0)
				throw new NotSupportedException("Unrecognized \"segm\" block version");

			// Version 0 (all versions)
			byte numChanges = _reader.ReadByte();
			for (int i = 0; i < numChanges; i++)
			{
				uint oldOffset = _reader.ReadUInt32();
				uint oldSize = _reader.ReadUInt32();
				uint newOffset = _reader.ReadUInt32();
				uint newSize = _reader.ReadUInt32();
				bool resizeAtEnd = Convert.ToBoolean(_reader.ReadByte());
				var segmentChange = new SegmentChange(oldOffset, oldSize, newOffset, newSize, resizeAtEnd);
				segmentChange.DataChanges.AddRange(ReadDataChanges());

				yield return segmentChange;
			}
		}

		private IEnumerable<TagChange> ReadTagChanges(byte version)
		{
			if (version > 0)
				throw new NotSupportedException("Unrecognized \"tags\" block version");

			var numChanges = _reader.ReadUInt32();
			for (int i = 0; i < numChanges; i++)
			{
				var group = _reader.ReadInt32();
                var name = _reader.ReadUTF16();
				var tagChange = new TagChange(name, group, 0);
				ReadBlockChange(tagChange);

                yield return tagChange;
			}
		}

		private void ReadBlockChange(BlockChange block)
		{
			block.BaseSize = _reader.ReadUInt32();
			var flag = (BlockChangeFlag)_reader.ReadByte();

			if (flag.HasFlag(BlockChangeFlag.TopLevel))
				block.Changes = ReadDataChanges().ToList();

			if (flag.HasFlag(BlockChangeFlag.DataRef))
			{
				var count = _reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var offset = _reader.ReadUInt32();
                    block.DataRefChanges[offset] = ReadDataChanges().ToList();
                }
            }

			if (flag.HasFlag(BlockChangeFlag.TagBlock))
			{
				var count = _reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var offset = _reader.ReadUInt32();
                    var blockChange = new BlockChange(0);
                    ReadBlockChange(blockChange);
                    block.TagBlockChanges[offset] = blockChange;
                }
            }
        }

        private IEnumerable<DataChange> ReadDataChanges()
		{
			uint numFourByteChanges = _reader.ReadUInt32();
			for (int j = 0; j < numFourByteChanges; j++)
			{
				uint offset = _reader.ReadUInt32();
				byte[] data = _reader.ReadBlock(4);
				yield return new DataChange(offset, data);
			}

			uint numOtherChanges = _reader.ReadUInt32();
			for (int j = 0; j < numOtherChanges; j++)
			{
				uint offset = _reader.ReadUInt32();
				int dataSize = _reader.ReadInt32();
				byte[] data = _reader.ReadBlock(dataSize);
				yield return new DataChange(offset, data);
			}
		}

		private BlfContent ReadBlfInfo(byte version)
		{
			if (version > 0)
				throw new NotSupportedException("Unrecognized \"blfc\" block version");

			// Version 0 (all versions)
			var targetGame = (TargetGame) _reader.ReadByte();
			string mapInfoFileName = _reader.ReadAscii();
			uint mapInfoLength = _reader.ReadUInt32();
			byte[] mapInfo = _reader.ReadBlock((int) mapInfoLength);
			short blfContainerCount = _reader.ReadInt16();
			var blf = new BlfContent(mapInfoFileName, mapInfo, targetGame);
			for (int i = 0; i < blfContainerCount; i++)
			{
				string fileName = Path.GetFileName(_reader.ReadAscii());
				uint blfContainerLength = _reader.ReadUInt32();
				byte[] blfContainer = _reader.ReadBlock((int) blfContainerLength);

				blf.BlfContainerEntries.Add(new BlfContainerEntry(fileName, blfContainer));
			}

			return blf;
		}

		#region Deprecated

		private IEnumerable<DataChange> ReadMetaChanges(byte version)
		{
			if (version > 0)
				throw new NotSupportedException("Unrecognized \"meta\" block version");

			return ReadDataChanges();
		}

		private IEnumerable<LanguageChange> ReadLocaleChanges(byte version)
		{
			if (version > 0)
				throw new NotSupportedException("Unrecognized \"locl\" block version");

			// Read language changes
			byte numLanguageChanges = _reader.ReadByte();
			for (byte i = 0; i < numLanguageChanges; i++)
			{
				byte languageIndex = _reader.ReadByte();
				var languageChange = new LanguageChange(languageIndex);

				// Read string changes
				int numStringChanges = _reader.ReadInt32();
				for (int j = 0; j < numStringChanges; j++)
				{
					int index = _reader.ReadInt32();
					string newValue = _reader.ReadUTF8();
					languageChange.LocaleChanges.Add(new LocaleChange(index, newValue));
				}

				yield return languageChange;
			}
		}

		#endregion Deprecated
	}
}