using System;

namespace WkWrap
{
    /// <summary>
    /// The exception that thrown when WkHtmlToPdf process retruns non-zero exit code.
    /// </summary>
    public class WkException : Exception
    {
        /// <summary>
        /// Returns new instance of <see cref="WkException"/>.
        /// </summary>
        /// <param name="errorCode">WkHtmlToPdf process error code.</param>
        /// <param name="message">WkHtmlToPdf error text.</param>
        public WkException(int errorCode, string message) : base($"{message} ({errorCode:D})")
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets WkHtmlToPdf process error code.
        /// </summary>
        public int ErrorCode { get; }
    }
}
