using System;
using System.Collections.Generic;
using System.Linq;
using Blamite.IO;

namespace Blamite.Patching
{
	public static class SegmentComparer
	{
		/// <summary>
		///     Compares two sets of file segments and the segment contents.
		/// </summary>
		/// <param name="originalSegments">The original set of file segments.</param>
		/// <param name="originalReader">The stream to use to read from the original file.</param>
		/// <param name="newSegments">The modified set of file segments.</param>
		/// <param name="newReader">The stream to use to read from the modified file.</param>
		/// <returns>The differences that were found.</returns>
		public static IEnumerable<SegmentChange> CompareSegments(
			IList<FileSegment> originalSegments, 
			IReader originalReader,
			IList<FileSegment> newSegments, 
			IReader newReader
		) {
			if (originalSegments.Count != newSegments.Count)
				throw new InvalidOperationException("The files have different segment counts");

			for (int i = 0; i < originalSegments.Count; i++)
			{
				var originalSegment = originalSegments[i];
				var newSegment = newSegments[i];
				var changes = DataComparer.CompareData(
					originalReader,
					originalSegment.Offset,
					originalSegment.ActualSize,
					newReader,
					newSegment.Offset,
					newSegment.ActualSize,
					originalSegment.ResizeOrigin != SegmentResizeOrigin.Beginning
				).ToList();
				if (changes.Count > 0 || originalSegment.Size != newSegment.Size)
				{
					var change = new SegmentChange(
						originalSegment.Offset, 
						originalSegment.ActualSize, 
						newSegment.Offset,
						newSegment.ActualSize, 
						originalSegment.ResizeOrigin != SegmentResizeOrigin.Beginning
					);
					change.DataChanges.AddRange(changes);
					yield return change;
				}
			}
		}
	}
}