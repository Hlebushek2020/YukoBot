namespace YukoClientBase.Args
{
    /// <summary>
    /// Class containing data about changing the progress of the task
    /// </summary>
    public class ProgressReportArgs
    {
        /// <summary>
        /// <see langword="false" /> if the shows actual value; <see langword="true" /> if the shows generic progress;
        /// <see langword="null" /> if the value does not need to be changed. 
        /// </summary>
        public bool? IsIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Controls.Primitives.RangeBase.Minimum" /> possible
        /// <see cref="System.Windows.Controls.Primitives.RangeBase.Value" /> of the range element;
        /// <see langword="null" /> if the value does not need to be changed.
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the highest possible <see cref="System.Windows.Controls.Primitives.RangeBase.Value" /> of the
        /// range element; <see langword="null" /> if the value does not need to be changed.
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the current magnitude of the range control; <see langword="null" /> if the value does not
        /// need to be changed.
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Text value of the operation progress; <see langword="null" /> if the value does not need to be changed.
        /// </summary>
        public string Text { get; set; }

        public static implicit operator ProgressReportArgs(double value) => new ProgressReportArgs { Value = value };
        public static implicit operator ProgressReportArgs(string text) => new ProgressReportArgs { Text = text };
    }
}