using System;
using System.Collections;
using System.Collections.Generic;

namespace Blamite.Blam
{
    /// <summary>
    ///     Provides a list of tag filenames which can be used to retrieve names for tags.
    /// </summary>
    public abstract class FileNameSource : IEnumerable<string>
    {
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public abstract IEnumerator<string> GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string this[int tagIndex]
        {
            get => GetTagName(tagIndex);
            set => SetTagName(tagIndex, value);
        }

        public string this[DatumIndex tagIndex]
        {
            get => GetTagName(tagIndex);
            set => SetTagName(tagIndex, value);
        }

        public string this[ITag tag]
        {
            get => GetTagName(tag);
            set => SetTagName(tag, value);
        }

        public abstract int Count { get; }

        /// <summary>
        ///     Given an index of a tag in the tag table, retrieves a tag's corresponding filename if it exists.
        /// </summary>
        /// <param name="tagIndex">The index of the tag to get the filename of.</param>
        /// <returns>The tag's name if available, or <see langword="null" /> otherwise.</returns>
        public abstract string GetTagName(int tagIndex);

        /// <param name="tagIndex">The datum index of the tag to set the name of.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <inheritdoc cref="GetTagName(int)"/>
        public string GetTagName(DatumIndex tagIndex)
        {
            if (!tagIndex.IsValid)
                throw new ArgumentNullException(nameof(tagIndex), "Invalid tag datum index");
            return GetTagName(tagIndex.Index);
        }

        /// <param name="tag">The tag to get the filename of.</param>
        /// <inheritdoc cref="GetTagName(DatumIndex)"/>
        public string GetTagName(ITag tag) => GetTagName(tag.Index);

        /// <summary>
        ///     Sets the name of a tag based upon its index in the tag table.
        /// </summary>
        /// <param name="tagIndex">The index of the tag to set the name of.</param>
        /// <param name="name">The new name.</param>
        public abstract void SetTagName(int tagIndex, string name);

        /// <param name="tagIndex">The datum index of the tag to set the name of.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <inheritdoc cref="SetTagName(int, string)"/>
        public void SetTagName(DatumIndex tagIndex, string name)
        {
            if (!tagIndex.IsValid)
                throw new ArgumentNullException(nameof(tagIndex), "Invalid tag datum index");
            SetTagName(tagIndex.Index, name);
        }

        /// <param name="tag">The tag to set the name of.</param>
		/// <inheritdoc cref="SetTagName(DatumIndex, string)"/>
        public void SetTagName(ITag tag, string name) => SetTagName(tag.Index, name);

        /// <summary>
        ///     Finds the index of the first tag which has a given name.
        /// </summary>
        /// <param name="name">The tag name to search for.</param>
        /// <returns>The index in the tag table of the first tag with the given name, or -1 if not found.</returns>
        public abstract int FindName(string name);
    }
}