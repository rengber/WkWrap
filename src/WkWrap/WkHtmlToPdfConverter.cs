using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WkWrap
{
    /// <summary>
    /// Html to PDF converter (.NET WkHtmlToPdf process wrapper).
    /// </summary>
    public class WkHtmlToPdfConverter
    {
        /// <summary>
        /// Gets wkhtmltopdf executable file.
        /// </summary>
        private readonly FileInfo _wkHtmlToPdfExecutableFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="WkHtmlToPdfConverter"/> class.
        /// </summary>
        /// <param name="wkHtmlToPdfExecutableFile">wkhtmltopdf executable file.</param>
        public WkHtmlToPdfConverter(FileInfo wkHtmlToPdfExecutableFile)
        {
            if (wkHtmlToPdfExecutableFile == null)
            {
                throw new ArgumentNullException(nameof(wkHtmlToPdfExecutableFile));
            }
            if (!wkHtmlToPdfExecutableFile.Exists)
            {
                throw new FileNotFoundException($"wkhtmltopdf executable file not found at path '{wkHtmlToPdfExecutableFile.FullName}'.");
            }

            _wkHtmlToPdfExecutableFile = wkHtmlToPdfExecutableFile;
        }

        /// <summary>
        /// Instance of wkhtmltopdf working process.
        /// </summary>
        private Process _wkHtmlToPdfProcess;

        /// <summary>
        /// Occurs when log line is received from WkHtmlToPdf process.
        /// </summary>
        /// <remarks>
        /// Quiet mode should be disabled if you want to get wkhtmltopdf info/debug messages.
        /// </remarks>
        public event EventHandler<DataReceivedEventArgs> LogReceived;

        /// <summary>
        /// Generates a PDF using the specified HTML content with <see cref="ConversionSettings.CreateDefault"/>.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        public Task<byte[]> ConvertToPdfAsync(string html) => ConvertToPdfAsync(html, Encoding.UTF8, ConversionSettings.CreateDefault());

        /// <summary>
        /// Generates a PDF using the specified HTML content and settings.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        /// <param name="settings">A <see cref="ConversionSettings"/> instance.</param>
        public Task<byte[]> ConvertToPdfAsync(string html, ConversionSettings settings) => ConvertToPdfAsync(html, Encoding.UTF8, settings);

        /// <summary>
        /// Generates a PDF using the specified HTML content and settings.
        /// </summary>
        /// <param name="html">The HTML content.</param>
        /// <param name="htmlEncoding">The encoding of the HTML content.</param>
        /// <param name="settings">A <see cref="ConversionSettings"/> instance.</param>
        public async Task<byte[]> ConvertToPdfAsync(string html, Encoding htmlEncoding, ConversionSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrEmpty(html))
            {
                return Array.Empty<byte>();
            }

            using (var input = new MemoryStream(htmlEncoding.GetBytes(html)))
            using (var output = new MemoryStream())
            {
                await ConvertToPdfInternalAsync(input, output, settings.ToString(), settings.ExecutionTimeout);
                return output.ToArray();
            }
        }

        /// <summary>
        /// Generates a PDF into specified output <see cref="Stream" />.
        /// </summary>
        /// <param name="input">HTML content input stream.</param>
        /// <param name="output">PDF file output stream.</param>
        public Task ConvertToPdfAsync(Stream input, Stream output) => ConvertToPdfAsync(input, output, ConversionSettings.CreateDefault());

        /// <summary>
        /// Generates a PDF into specified output <see cref="Stream" />.
        /// </summary>
        /// <param name="input">HTML content input stream.</param>
        /// <param name="output">PDF file output stream.</param>
        /// <param name="settings">wkhtmltopdf command line arguments.</param>
        public Task ConvertToPdfAsync(Stream input, Stream output, ConversionSettings settings)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return ConvertToPdfInternalAsync(input, output, settings.ToString(), settings.ExecutionTimeout);
        }

        /// <summary>
        /// Generate PDF into specified output <see cref="Stream" />.
        /// </summary>
        /// <param name="input">HTML content input stream.</param>
        /// <param name="output">PDF file output stream.</param>
        /// <param name="settings">wkhtmltopdf command line arguments.</param>
        /// <param name="executionTimeout">Maximum execution time for PDF generation process (null means that no timeout).</param>
        private Task ConvertToPdfInternalAsync(Stream input, Stream output, string settings, TimeSpan? executionTimeout)
        {
            try
            {
                CheckWkHtmlProcess();
                return InvokeWkHtmlToPdfAsync(input, output, settings, executionTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate PDF: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Invokes wkhtmltopdf programm.
        /// </summary>
        /// <param name="input">HTML content input stream.</param>
        /// <param name="output">PDF file output stream.</param>
        /// <param name="settings">Conversion settings.</param>
        /// <param name="executionTimeout">Maximum execution time for PDF generation process (null means that no timeout).</param>
        private async Task InvokeWkHtmlToPdfAsync(Stream input, Stream output, string settings, TimeSpan? executionTimeout)
        {
            var arguments = settings + " - -";
            try
            {
                _wkHtmlToPdfProcess =
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _wkHtmlToPdfExecutableFile.FullName,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = _wkHtmlToPdfExecutableFile.Directory.FullName,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });

                _wkHtmlToPdfProcess.ErrorDataReceived += ErrorDataHandler;
                _wkHtmlToPdfProcess.BeginErrorReadLine();

                var stdin = _wkHtmlToPdfProcess.StandardInput;
                var stdout = _wkHtmlToPdfProcess.StandardOutput;

                // Write the html content to the standard input stream.
                await input.CopyToAsync(stdin.BaseStream);
                await stdin.BaseStream.FlushAsync();
                stdin.Dispose();

                // Read the bytes representing the PDF from the standard output stream.
                await stdout.BaseStream.CopyToAsync(output);

                // Exit wkhtmltopdf process.
                WaitWkHtmlProcessForExit(executionTimeout);
                CheckExitCode(_wkHtmlToPdfProcess.ExitCode, _lastLogLine);
            }
            finally
            {
                EnsureWkHtmlProcessStopped();
                _lastLogLine = null;
            }
        }

        /// <summary>
        /// Stores last log line.
        /// </summary>
        private string _lastLogLine;

        /// <summary>
        /// Error handler to rethrow wkhtmltopdf log events.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of <see cref="DataReceivedEventArgs"/>.</param>
        private void ErrorDataHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e?.Data))
            {
                _lastLogLine = e.Data;
                LogReceived?.Invoke(this, e);
            }
        }

        /// <summary>
        /// Checks whether the wkhtmltopdf process is not running.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when wkhtmltopdf process is runnning.</exception>
        private void CheckWkHtmlProcess()
        {
            if (_wkHtmlToPdfProcess != null)
            {
                throw new InvalidOperationException("WkHtmlToPdf process has already started");
            }
        }

        /// <summary>
        /// Waits for wkhtmltopdf process ending.
        /// </summary>
        /// <param name="executionTimeout">wkhtmltopdf execution timeout.</param>
        private void WaitWkHtmlProcessForExit(TimeSpan? executionTimeout)
        {
            if (executionTimeout.HasValue)
            {
                if (!_wkHtmlToPdfProcess.WaitForExit((int)executionTimeout.Value.TotalMilliseconds))
                {
                    EnsureWkHtmlProcessStopped();
                    throw new WkException(-2, string.Format("WkHtmlToPdf process exceeded execution timeout ({0}) and was aborted", executionTimeout));
                }
            }
            else
            {
                _wkHtmlToPdfProcess.WaitForExit();
            }
        }

        /// <summary>
        /// Stops wkhtmltopdf process if it is not stopped.
        /// </summary>
        private void EnsureWkHtmlProcessStopped()
        {
            if (_wkHtmlToPdfProcess == null)
            {
                return;
            }
            if (!_wkHtmlToPdfProcess.HasExited)
            {
                try
                {
                    _wkHtmlToPdfProcess.Kill();
                    _wkHtmlToPdfProcess = null;
                }
                catch
                {
                    // Ignore erros when stopping the process.
                }
            }
            else
            {
                _wkHtmlToPdfProcess = null;
            }
        }

        /// <summary>
        /// Checks wkhtmltopdf's exit code.
        /// </summary>
        /// <param name="exitCode">Exit code.</param>
        /// <param name="lastErrorLine">Last error line.</param>
        private void CheckExitCode(int exitCode, string lastErrorLine)
        {
            if (exitCode != 0)
            {
                if (exitCode == 1 && Array.IndexOf(IgnoredWkHtmlToPdfErrors, lastErrorLine) > -1)
                {
                    return;
                }

                throw new WkException(exitCode, lastErrorLine);
            }
        }

        /// <summary>
        /// Ignored WkHtmlToPdf errors.
        /// </summary>
        private static readonly string[] IgnoredWkHtmlToPdfErrors =
        {
            "Exit with code 1 due to network error: HostNotFoundError",
            "Exit with code 1 due to network error: ContentNotFoundError",
            "Exit with code 1 due to network error: ContentOperationNotPermittedError",
            "Exit with code 1 due to network error: ProtocolUnknownError",
            "Exit with code 1 due to network error: UnknownContentError",
            "QFont::setPixelSize: Pixel size <= 0"
        };
    }
}
