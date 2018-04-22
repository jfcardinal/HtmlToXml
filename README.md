# HtmlToXml
Utility to convert HTML to XML, including recovery from unclosed tags and other adjustments.

## OverView

I wrote HtmlToXml because I was unable to find an existing .NET component for converting HTML to XML. Several facilities will consume HTML and provide a DOM or XML version of the input, but my need was somewhat different: I had to convert user-supplied HTML to XHTML for inclusion in an EPUB book. (Unlike browsers, EPUB readers will reject an XHTML document that is not a valid XML document.)

## Goals

The primary goal is to avoid errors when reading the text after it has been included in an XML document.

The secondary goal is to produce XHTML that mimics what a web browser would render after interpreting the original HTML.

## Features

- Converts HTML named entities to XML decimal character references, i.e., "`&mdash;`" is converted to "`&#8212;`".

- Converts tags for HTML "[void elements](https://html.spec.whatwg.org/multipage/syntax.html#void-elements "WHATWG specification")", such as IMG, BR, and META, to self-closed XML tags.

- Converts unquoted attribute values to quoted values.

- Closes unclosed tags, with special handling for elements that HTML specifies are closed by subsequent elements, such as P, LI, and TD.

- Closes unclosed elements that would otherwise produce an improperly nested result.

- Ignores end tags with no matching start tag. Such tags may occur due to the processing associated with other features.

- Removes HTML comments.

- Removes the DOCTYPE statement.

- Treats elements with unrecognized names as block elements, except for   names that include a namespace, which are treated as inline elements. This affects unclosed paragraph processing: `<o>` will close an open paragraph, but `<o:p>` will not.

## Usage

```csharp
using HtmlToXml;

var html = new HtmlConverter();
var xmlText = html.Convert("<p>This is a test.");
```

`xmlText` would equal "`<p>This is a test.</p>`" on output.

## Examples

```html
I: <p>File > Save As</p>
O: <p>File &gt; Save As</p>
```

A `'>'` character that is not used to end a tag
is encoded.

```html
I: <p>Unclosed paragraph.
O: <p>Unclosed paragraph.</p>
```

Any open elements are closed when the end of the
input is reached.

```html
I: <p>Paragraph one.<p>Paragraph two.
O: <p>Paragraph one.</p><p>Paragraph two.</p>
```

Open paragraph elements are closed when the next
block element is encountered.

```html
I: <p>A simple<br>paragraph.</p>
O: <p>A simple<br/>paragraph.</p>
```

BR elements and other void elements become
self-closing XML elements.
</p>

```html
I: <p>A simple<br />paragraph.</p>
O: <p>A simple<br/>paragraph.</p>
```

HTML that is already valid XML is left unchanged, though
minor variations may be introduced.

```html
I: <p>Paragraph <b>one.</p>
O: <p>Paragraph <b>one.</b></p>
```

Inline elements inside a block element are closed
when the block ends, but not because they are inline
elements. They are closed to honor XML's nesting rules.

```html
I: <ul><li>Item 1<li>Item 2</ul>
O: <ul><li>Item 1</li><li>Item 2</li></ul>
```

Open LI elements are closed when its next sibling
is opened.

```html
I: <ul><li>Item 1<li>Item 2<div>Here
O: <ul><li>Item 1</li><li>Item 2<div>Here</div></li></ul>
```

An open LI element is not closed by any block element,
only by a sibling LI. The DIV above is inside the prior
LI.

```html
I: <ul><li>Item 1<ul><li>Item 1.1</li></ul><li>Item 2</li></ul>
O: <ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>

I: <ul><li>Item 1<ul><li>Item 1.1</ul><li>Item 2</li></ul>
O: <ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>
```

Nested lists will be handled properly as long as the
start and end tags for the lists (UL or OL) are present.

```html
I: <p>Extra.&nbsp; Spacing.</p>
O: <p>Extra.&#160; Spacing.</p>
```

Named HTML entities are converted to decimal entities.

```html
I: <p>You <!--ain't-->got it.</p>
O: <p>You got it.</p>
```

HTML comments are removed.

```html
I: <img alt="" height=1 width=1 src="image.jpg">
O: <img alt="" height="1" width="1" src="image.jpg"/>
```
Unquoted parameter values are quoted.

```html
I: <a href="foo?doo&ret=no">Text</a>
O: <a href="foo?doo&amp;ret=no">Text</a>
```

Unencoded ampersands in parameter values are encoded.

```html
I: <p><o>test</o></p>
O: <p></p><o>test</o>
```

The `<o>` tag closes the P element because an
unknown element is treated as a block element.

```html
I: <p><o:p>test</o:p></p>
O: <p><o:p>test</o:p></p>
```

The `<o:p>` tag does not close the P element
because an unknown element with a namespace is
treated as an inline element.

## Credits

Includes a modified version of [TextParser](http://www.blackbeltcoder.com/Articles/strings/a-text-parsing-helper-class), a class originally written by Jonathan Wood.
