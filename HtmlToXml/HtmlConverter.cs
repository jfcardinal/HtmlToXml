using System;
using System.Collections.Generic;
using System.Text;

using JohnCardinal.Text;
using JohnCardinal.Utility;

namespace HtmlToXml {
   /// <summary>
   /// <see cref="HtmlConverter"/> provides a lightweight parser for converting
   /// HTML to XML including recovery from unclosed tags.
   /// </summary>
   public sealed partial class HtmlConverter {
      private static char[] restrictedCharacters = new char[] { '<', '>', '&' };

      // These constants are used to start and end CDATA sections
      // when used inside SCRIPT and STYLE elements.
      private const string kScriptCDataStart = "/*<![CDATA[*/";
      private const string kScriptCDataEnd = "/*]]>*/";

      /// <summary>
      /// A list of inline elements; all other elements are considered
      /// block elements except element names with a namespace.
      /// </summary>
      private static HashSet<string> inlineElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
         "a",
         "abbr",
         "acronym",
         "b",
         "bdo",
         "big",
         "br",
         "button",
         "cite",
         "code",
         "dfn",
         "em",
         "i",
         "img",
         "input",
         "kbd",
         "label",
         "map",
         "object",
         "q",
         "samp",
         "script",
         "select",
         "small",
         "span",
         "strong",
         "sub",
         "sup",
         "textarea",
         "tt",
         "var",
      };

      /// <summary>
      /// Returns true if the given <paramref name="elementName"/> represents
      /// an inline element or includes a ':' character indicating a 
      /// namespace.
      /// </summary>
      /// <remarks>
      /// For compatability with systems that adorn HTML with custom,
      /// namespaced tags that are used inside paragraphs, all tags
      /// with an element name that includes ':' are considered inline
      /// elements.
      /// </remarks>
      public static bool IsInlineElement(string elementName) {
         if (elementName.IndexOf(':') != -1) return true;
         return inlineElements.Contains(elementName);
      }

      /// <summary>
      /// Returns true if the given <paramref name="elementName"/> represents
      /// an HTML element whose text should be wrapped in the CDATA start
      /// and end tags.
      /// </summary>
      public static bool IsCDataElement(string elementName) {
         return IsScriptCDataElement(elementName);
      }

      /// <summary>
      /// Returns true if the given <paramref name="elementName"/> represents
      /// an HTML element whose text should be wrapped in the CDATA start
      /// and end tags that are further wrapped by /* and */ to avoid issues
      /// if the XHTML is read in an HTML context.
      /// </summary>
      public static bool IsScriptCDataElement(string elementName) {
         return (elementName.Equals("script", StringComparison.OrdinalIgnoreCase) ||
            elementName.Equals("style", StringComparison.OrdinalIgnoreCase));
      }

      /// <summary>
      /// voidElements should be self-closed.
      /// </summary>
      private static HashSet<string> voidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
         "area",
         "base",
         "br",
         "col",
         "command",
         "embed",
         "hr",
         "img",
         "input",
         "keygen",
         "link",
         "meta",
         "param",
         "source",
         "track",
         "wbr"
      };

      /// <summary>
      /// Returns true if the given <paramref name="elementName"/> represents
      /// a void element that requires a self-closing tag.
      /// </summary>
      public static bool IsVoidElement(string elementName) {
         return voidElements.Contains(elementName);
      }

      /// <summary>
      /// Tracks which elements are open.
      /// </summary>
      private StackArray<string> openElements;

      /// <summary>
      /// Manages handlers for specific HTML elements.
      /// </summary>
      private ElementHandlerFactory handlers;

      /// <summary>
      /// The <see cref="ITextParser"/> instance used to parse the
      /// incoming HTML text.
      /// </summary>
      private ITextParser tp;

      /// <summary>
      /// The <see cref="StringBuilder"/> instance where the XML output
      /// of <see cref="Converter"/> is assembled.
      /// </summary>
      private StringBuilder xml;

      /// <summary>
      /// Creates a new instance of <see cref="HtmlConverter"/>.
      /// </summary>
      public HtmlConverter() {
         openElements = new StackArray<string>();
         handlers = new ElementHandlerFactory();
      }

      /// <summary>
      /// Converts the HTML in <paramref name="htmlText"/> to XML.
      /// </summary>
      /// <param name="htmlText">The HTML text to parse.</param>
      public string Convert(string htmlText) {
         if (String.IsNullOrEmpty(htmlText)) return htmlText;
         if (htmlText.IndexOfAny(restrictedCharacters) == -1) return htmlText;

         tp = new TextParser(htmlText);
         xml = new StringBuilder(htmlText.Length);
         Converter();
         var result = xml.ToString();
         xml = null;
         return result;
      }

      /// <summary>
      /// Converts the text read via the <paramref name="html"/>
      /// and appends the XML to <paramref name="xml"/>.
      /// </summary>
      /// <param name="html">An <see cref="ITextParser"/> that provides the HTML text to parse.</param>
      /// <param name="xml">A <see cref="StringBuilder"/> to which the XML is appended.</param>
      public void Convert(ITextParser html, StringBuilder xml) {
         tp = html;
         this.xml = xml;
         Converter();
      }

      private void Converter() {
         while (!tp.EndOfText) {
            var c = tp.Peek();
            switch (c) {
               case '<':
                  var tag = GetTag();
                  if (tag != null) {
                     HandleTag(tag);
                  }
                  break;

               case '>':
                  xml.Append("&gt;");
                  tp.MoveAhead();
                  break;

               case '&':
                  EntityConverter.Convert(tp, xml);
                  break;

               default:
                  xml.Append(c);
                  tp.MoveAhead();
                  break;
            }
         }

         CloseOpenElements(openElements.Count);
      }

      /// <summary>
      /// Writes the current tag to the output. Inserts closing
      /// tags as necessary.
      /// </summary>
      private void HandleTag(HtmlTag tag) {
         if (tag.IsSelfClosingTag && tag.IsEndTag) {
            return;
         }

         if (tag.IsEndTag) {
            CloseBackToMatchingTag(tag.Name);
            return;
         }

         ProcessHandlers(tag.Name);

         xml.Append('<');
         xml.Append(tag.Name);
         xml.Append(tag.AttributesPart);
         if (tag.IsSelfClosingTag) {
            if (xml[xml.Length - 1] == ' ') xml.Length = xml.Length - 1;
            xml.Append('/');
         }
         else {
            openElements.Push(tag.Name);
         }
         xml.Append('>');

         if (IsScriptCDataElement(tag.Name)) {
            SkipToEndOfScriptCData(tag.Name);
         }
      }

      private void ProcessHandlers(string currentElementName) {
         for (var index = openElements.Count; index > 0; index--) {
            var openElement = openElements[index - 1];
            var handler = handlers.GetHandler(openElement);
            if (handler != null) {
               var result = handler.AddChild(openElements, currentElementName);
               if (result == ElementHandlerResult.CloseElement || result == ElementHandlerResult.CloseElementAndBreak) {
                  CloseBackToMatchingTag(handler.Name);
               }
               if (result == ElementHandlerResult.CloseElementAndBreak) break;
            }
         }
      }

      /// <summary>
      /// Closes all elements that are open back to the last
      /// start tag for this element. Returns true if the
      /// matching tag is found and element(s) are closed. IF
      /// no matching tag is found, no tags are closed and the
      /// method returns false.
      /// </summary>
      private bool CloseBackToMatchingTag(string name) {
         // Find last-previous open
         var index = openElements.IndexOf(name);

         // No prior matching tag, so do nothing
         if (index == -1) return false;

         // Close open elements back to last-previous open
         CloseOpenElements(openElements.Count - index);

         return true;
      }

      /// <summary>
      /// Closes all open elements.
      /// </summary>
      private void CloseOpenElements(int count) {
         while (count > 0) {
            PopElementAndClose();
            count--;
         }
      }

      /// <summary>
      /// Pops the most recent element and closes it
      /// </summary>
      private void PopElementAndClose() {
         var elementName = openElements.Pop();
         xml.Append("</");
         xml.Append(elementName);
         xml.Append('>');
      }

      /// <summary>
      /// Parses an <see cref="HtmlTag"/> from the input and
      /// returns it.
      /// </summary>
      private HtmlTag GetTag() {
         // Handle comment
         if (tp.Peek(1) == '!' && tp.Peek(2) == '-' && tp.Peek(3) == '-') {
            SkipToEndOfComment();
            return null;
         }

         // Handle DOCTYPE or other <! ... >
         if (tp.Peek(1) == '!') {
            SkipToEndOfTag(true);
            return null;
         }

         var startPos = tp.Position;
         var htmlTag = new HtmlTag();
         if (htmlTag.Parse(tp)) {
            return htmlTag;
         }

         // Handle invalid tags
         tp.MoveTo(startPos + 1);
         xml.Append("&lt;");
         return null;
      }

      /// <summary>
      /// Skips over all characters up to and including the next '>'.
      /// </summary>
      /// <remarks>
      /// Used to skip "&lt;! ... &gt;", including "&lt;!DOCTYPE ... &gt;".
      /// </remarks>
      private void SkipToEndOfTag(bool skipTrailingNewLine) {
         while (!tp.EndOfText) {
            tp.MoveAhead();
            if (tp.Peek() == '>') break;
         }
         tp.MoveAhead();

         if (skipTrailingNewLine) {
            while (!tp.EndOfText) {
               var c = tp.Peek();
               if (c != '\r' && c != '\n') {
                  break;
               }
               tp.MoveAhead();
            }
         }
         return;
      }

      /// <summary>
      /// Skips all characters until the end tag for the given
      /// <paramref name="tagName"/>.
      /// </summary>
      /// <remarks>
      /// Uses a simple rule: the CData ends at the next
      /// instance of the end tag. An end that appears in 
      /// the text of the tag--not sure if that can
      /// happen, but maybe--will be treated as the end.
      /// </remarks>
      private void SkipToEndOfScriptCData(string tagName) {
         var endOffset = tagName.Length + 2;
         var haveCData = false;

         while (!tp.EndOfText) {
            if (tp.Peek() == '<' && tp.Peek(1) == '/' && tp.Peek(endOffset) == '>') {
               var endTag = tp.Substring(tp.Position + 2, tagName.Length);
               if (endTag.Equals(tagName, StringComparison.OrdinalIgnoreCase)) {
                  break;
               }
            }
            if (!haveCData) {
               haveCData = true;
               xml.Append(kScriptCDataStart);
            }
            xml.Append(tp.Peek());
            tp.MoveAhead();
         }

         if (haveCData) xml.Append(kScriptCDataEnd);
         return;
      }

      /// <summary>
      /// Skips over all the characters in an HTML comment.
      /// </summary>
      /// <remarks>
      /// Uses a simple rule: the comment ends at the next
      /// instance of "--&gt;".
      /// </remarks>
      private void SkipToEndOfComment() {
         tp.MoveAhead(4);
         while (!tp.EndOfText) {
            if (tp.Peek() == '-' && tp.Peek(1) == '-' && tp.Peek(2) == '>') {
               tp.MoveAhead(3);
               return;
            }
            tp.MoveAhead();
         }
         return;
      }
   }
}