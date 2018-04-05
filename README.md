# WkWrap
[wkhtmltopdf](http://wkhtmltopdf.org/) cross-platform C# wrapper for .NET Standard.
## Overview
WkWrap is a `wkhtmltopdf` wrapper for .NET Standard 1.4 and .NET Standard 2.0. WkWrap calls `wkhtmltopdf` in stream-based processing mode, so you don't need to provide any temp folders.

## Getting started

### Installation
WkWrap is [available on NuGet.](https://www.nuget.org/packages/WkWrap)
```
Install-Package WkWrap
```

### Usage
WkWrap is designed as easy to use wkhtmltopdf cross-platform wrapper. To start using it you need to [install wkhtmltopdf](http://wkhtmltopdf.org/downloads.html) or make sure the executable is available from your application. Create an instance of `HtmlToPdfConverter` by passing the path to the `wkhtmltopdf` executeable (may require chmod+x on \*nix based systems).
```csharp
var wkhtmltopdf = new FileInfo(@"path\to\wkhtmltopdf");
var converter = new HtmlToPdfConverter(wkhtmltopdf);
var pdf = await converter.ConvertToPdfAsync(html);
```

### Customize processing
Provide an instance of the `ConversionSettings` class to customize wkhtmltopdf processing.

```cs
var converter = new HtmlToPdfConverter(new FileInfo(@"path\to\wkhtmltopdf"));
var settings = new ConversionSettings
{
    PageSize = PageSize.A3,
    Orientation = PageOrientation.Landscape
    // ...
};
var pdf = await converter.ConvertToPdfAsync(html, settings);
```

#### Headers and footers
Use the `HeaderPath` and/or `FooterPath` properties of `ConversionSettings` to specify the HTML for the header/footer. The value must be either a path to a file on disk or a URL:

```cs
var converter = new HtmlToPdfConverter(new FileInfo(@"path\to\wkhtmltopdf"));
var settings = new ConversionSettings
{
    HeaderPath = "path/to/header.html",
    FooterPath = "path/to/footer.html"
};
var pdf = await converter.ConvertToPdfAsync(html, settings);
```

It's not possible to stream the HTML for headers and footers to wkhtmltopdf. You'll have to create temporary files if you want dynamic header/footer HTML.

#### Additional arguments
Use the `AdditionalSettings` property of `ConversionSettings` to specify arbitrary arguments for wkhtmltopdf.

```cs
var settings = new ConversionSettings
{
    AdditionalSettings = "-s A3 -L 0 -T 0 -B 0 -R 0 -g -q"
};
```

### Stream-based processing
You can specify a stream directly with HTML content rather than a string. When you do pass a string, an in-memory stream will be created for it.

```cs
var converter = new HtmlToPdfConverter(new FileInfo(@"path\to\wkhtmltopdf"));

Stream input; // Some input stream with HTML
using (var output = new MemoryStream())
{
    await converter.ConvertToPdfAsync(input, output);
    var pdf = output.ToArray();
}
```

### Sync and async
WkWrap provides both sync and async APIs.

```cs
// Sync
var pdf = converter.ConvertToPdf(html);

// Async equivalent
var pdf = await converter.ConvertToPdfAsync(html);
```