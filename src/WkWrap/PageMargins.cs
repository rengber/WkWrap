namespace WkWrap
{
    /// <summary>
    /// Struct that represents PDF page margins.
    /// </summary>
    public struct PageMargins
    {
        /// <summary>
        /// Returns new instance of <see cref="PageMargins"/> with default margins (0mm).
        /// </summary>
        /// <returns>An instance of <see cref="PageMargins"/>.</returns>
        public static PageMargins CreateDefault() => new PageMargins();

        /// <summary>
        /// Returns PDF page left margin (in mm).
        /// </summary>
        public double? Left { get; set; }

        /// <summary>
        /// Returns PDF page top margin (in mm).
        /// </summary>
        public double? Top { get; set; }

        /// <summary>
        /// Returns PDF page right margin (in mm).
        /// </summary>
        public double? Right { get; set; }

        /// <summary>
        /// Returns PDF page bottom margin (in mm).
        /// </summary>
        public double? Bottom { get; set; }
    }
}
