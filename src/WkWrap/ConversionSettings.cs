using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace WkWrap
{
    /// <summary>
    /// Settings for converting HTML to PDF's.
    /// </summary>
    public class ConversionSettings
    {
        /// <summary>
        /// Returns new instance of <see cref="ConversionSettings"/> with default settings.
        /// </summary>
        public static ConversionSettings CreateDefault() => new ConversionSettings();

        /// <summary>
        /// Returns PDF page size.
        /// </summary>
        public PageSize PageSize { get; set; } = PageSize.Default;

        /// <summary>
        /// Returns PDF page orientation.
        /// </summary>
        public PageOrientation Orientation { get; set; } = PageOrientation.Default;

        /// <summary>
        /// Returns PDF page margins.
        /// </summary>
        public PageMargins Margins { get; set; }

        /// <summary>
        /// Returns option to generate grayscale PDF.
        /// </summary>
        public bool Grayscale { get; set; }

        /// <summary>
        /// Returns option to generate low quality PDF (to reduce the result document size).
        /// </summary>
        public bool LowQuality { get; set; }

        /// <summary>
        /// Returns option to suppress wkhtmltopdf debug/info log messages.
        /// </summary>
        public bool Quiet { get; set; } = true;

        /// <summary>
        /// Returns option that allows web pages to run JavaScript.
        /// </summary>
        public bool EnableJavaScript { get; set; } = true;

        /// <summary>
        /// Returns delay for JavaScript finish (will applies only if JavaScript enabled).
        /// </summary>
        public TimeSpan? JavaScriptDelay { get; set; }

        /// <summary>
        /// Returns option that allows make links to remote web pages.
        /// </summary>
        public bool EnableExternalLinks { get; set; }

        /// <summary>
        /// Returns option that allows to load or print images.
        /// </summary>
        public bool EnableImages { get; set; } = true;

        /// <summary>
        /// Returns maximum execution time for PDF generation process (by default is null that means no timeout).
        /// </summary>
        public TimeSpan? ExecutionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the path or URL to the header HTML.
        /// </summary>
        public string HeaderPath { get; set; }

        /// <summary>
        /// Gets or sets the path or URL to the footer HTML.
        /// </summary>
        public string FooterPath { get; set; }

        /// <summary>
        /// Gets or sets an arbitrary settings string to be passed to wkhtmltopdf.
        /// </summary>
        public string AdditionalSettings { get; set; }

        /// <summary>
        /// Compose all settings to single wkhtmltopdf command line arguments string.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (PageSize != PageSize.Default)
            {
                builder.AppendFormat(" -s {0}", PageSize.ToString("G"));
            }

            if (Orientation != PageOrientation.Default)
            {
                builder.AppendFormat(" -O {0}", Orientation.ToString("G"));
            }

            if (Margins.Left.HasValue)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, " -L {0}", Margins.Left.Value);
            }

            if (Margins.Top.HasValue)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, " -T {0}", Margins.Top.Value);
            }

            if (Margins.Right.HasValue)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, " -R {0}", Margins.Right.Value);
            }

            if (Margins.Bottom.HasValue)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, " -B {0}", Margins.Bottom.Value);
            }

            if (Grayscale)
            {
                builder.Append(" -g");
            }

            if (LowQuality)
            {
                builder.Append(" -l");
            }

            if (Quiet)
            {
                builder.Append(" -q");
            }

            if (EnableJavaScript)
            {
                builder.Append(" --enable-javascript");
                if (JavaScriptDelay.HasValue)
                {
                    var jsDelayString = JavaScriptDelay.Value.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture);
                    builder.Append($" --javascript-delay {jsDelayString}");
                }
            }
            else
            {
                builder.Append(" --disable-javascript");
            }

            if (EnableExternalLinks)
            {
                builder.Append(" --enable-external-links");
            }
            else
            {
                builder.Append(" --disable-external-links");
            }

            if (EnableImages)
            {
                builder.Append(" --images");
            }
            else
            {
                builder.Append(" --no-images");
            }

            if (!string.IsNullOrEmpty(HeaderPath))
            {
                if (!ValidPath(HeaderPath))
                {
                    throw new InvalidOperationException($"The specified header path '{HeaderPath}' is not a valid path or URL.");
                }

                builder.AppendFormat(" --header-html \"{0}\"", HeaderPath);
            }

            if (!string.IsNullOrEmpty(FooterPath))
            {
                if (!ValidPath(FooterPath))
                {
                    throw new InvalidOperationException($"The specified footer path '{FooterPath}' is not a valid path or URL.");
                }

                builder.AppendFormat(" --footer-html \"{0}\"", FooterPath);
            }

            if (!string.IsNullOrEmpty(AdditionalSettings))
            {
                builder.Append(" ").Append(AdditionalSettings.Trim());
            }

            return builder.ToString().Trim();

            bool ValidPath(string path) => File.Exists(path) || Uri.TryCreate(path, UriKind.Absolute, out _);
        }
    }
}
