# HtmlToXml
Utility class to convert HTML to XML, including recovery from unclosed tags and other adjustments.

## OverView

I wrote HtmlToXml because I was unable to find an existing .NET component for converting HTML to XML. Several facilities will consume HTML and provide a DOM or XML version of the input, but my need was somewhat different: I had to convert user-supplied HTML to XHTML for inclusion in an EPUB book. (Unlike browsers, EPUB readers will reject an XHTML document that is not a valid XML document.)

## Goals

The primary goal is to avoid an error when reading the text after it has been included in an XML document.

The secondary goal is to produce XHTML that mimics what a web browser would render after interpreting the original HTML.

## Features

- Converts HTML named entities to XML decimal character references, i.e., "`&mdash;`" is converted to "`&#8212;`".

- Converts tags for HTML "[void elements](https://html.spec.whatwg.org/multipage/syntax.html#void-elements "WHATWG specification")", such as IMG, BR, and META, to self-closed XML tags.

- Converts unquoted attribute values to quoted values.

- Closes unclosed tags, with special handling for P and LI elements:
  - An open P element is closed if a start tag for a block element is encountered.
  - An open LI element is closed if a subsequent sibling LI element is encountered.

- Closes unclosed elements that would otherwise produce an improperly nested result.

- Ignores end tags with no matching start tag. Such tags may occur due to the processing associated with other features.

- Removes HTML comments.

- Treats elements with unrecognized names as block elements, except for   names that include a namespace which are treated as inline elements. This affects unclosed paragraph processing: `<o>` will close an open paragraph, but `<o:p>` will not.

## Usage

```csharp
var htmlToXml = new HtmlToXml();
var xmlText = htmlToXml.Convert("<p>This is a test.");
```

`xmlText` would equal "`<p>This is a test.</p>`" on output.

## Examples

| Input | Output |
| ----- | ------ |
| `<p>File > Save As</p>` | `<p>File &gt; Save As</p>` |
| `<p>Unclosed paragraph.` | `<p>Unclosed paragraph.</p>` |
| `<p>Paragraph one.<p>Paragraph two.` | `<p>Paragraph one.</p><p>Paragraph two.</p>` |
| `<p>Unclosed paragraph.<div>Something</div>` | `<p>Unclosed paragraph.</p><div>Something</div>` |
| `<p>A simple<br/>paragraph.</p>` | `<p>A simple<br/>paragraph.</p>` |
| `<p>A simple<br>paragraph.</p>` | `<p>A simple<br/>paragraph.</p>` |
| `<p>Paragraph <b>one.</p>` | `<p>Paragraph <b>one.</b></p>` |
| `<ul><li>Item 1<li>Item 2</ul>` | `<ul><li>Item 1</li><li>Item 2</li></ul>` |
| `<ul><li>Item 1<li>Item 2` | `<ul><li>Item 1</li><li>Item 2</li></ul>` |
| `<ul><li>Item 1<li>Item 2<div>Here` | `<ul><li>Item 1</li><li>Item 2<div>Here</div></li></ul>` |
| `<ul><li>Item 1<ul><li>Item 1.1</li></ul><li>Item 2</li></ul>` | `<ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>` |
| `<ul><li>Item 1<ul><li>Item 1.1</ul><li>Item 2</li></ul>` | `<ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>` |
| `<p>Extra.&nbsp; Spacing.</p>` | `<p>Extra.&#160; Spacing.</p>` |
| `<p>You <!--ain't-->got it.</p>` | `<p>You got it.</p>` |
| `<img alt="" height=1 width=1 src="image.jpg">` | `<img height="1" width="1" src="image.jpg"/>` |
| `<a href="foo?doo&ret=no">Text</a>` | `<a href="foo?doo&amp;ret=no">Text</a>` |
| `<p><o>test</o></p>` | `<p></p><o>test</o>` |
| `<p><o:p>test</o:p></p>` | `<p><o:p>test</o:p></p>` |

## Credits

Includes a modified version of [TextParser](http://www.blackbeltcoder.com/Articles/strings/a-text-parsing-helper-class), a class originally written by Jonathan Wood.
