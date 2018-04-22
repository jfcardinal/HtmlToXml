﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JohnCardinal.Text;

namespace HtmlToXml {
   /// <summary>
   /// <see cref="HtmlConverter"/> provides a lightweight parser for converting
   /// HTML to XML including recovery from unclosed tags.
   /// </summary>
   public sealed class HtmlConverter {
      private static char[] restrictedCharacters = new char[] { '<', '>', '&' };

      /// <summary>
      /// voidElements should be self-closed.
      /// </summary>
      private static List<string> voidElements = new List<string> {
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
         return voidElements.Contains(elementName, StringComparer.OrdinalIgnoreCase);
      }

      /// <summary>
      /// A list of inline elements; all other elements are considered
      /// block elements except element names with a namespace.
      /// </summary>
      private static List<string> inlineElements = new List<string> {
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
         if (elementName.Contains(':')) return true;
         return inlineElements.Contains(elementName, StringComparer.OrdinalIgnoreCase);
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
      /// The <see cref="TextParser"/> instance used to parse the
      /// incoming HTML text.
      /// </summary>
      private TextParser tp;

      /// <summary>
      /// The <see cref="StringBuilder"/> instance where the XML output
      /// of <see cref="Convert"/> is assembled.
      /// </summary>
      private StringBuilder sb;

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
      public string Convert(string htmlText) {
         if (String.IsNullOrEmpty(htmlText)) return htmlText;
         if (htmlText.IndexOfAny(restrictedCharacters) == -1) return htmlText;

         tp = new TextParser(htmlText);
         sb = new StringBuilder(htmlText.Length);

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
                  sb.Append("&gt;");
                  tp.MoveAhead();
                  break;

               case '&':
                  EntityConverter.Convert(tp, sb);
                  break;

               default:
                  sb.Append(c);
                  tp.MoveAhead();
                  break;
            }
         }

         CloseOpenElements(openElements.Count);

         var result = sb.ToString();
         sb = null;
         return result;
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

         sb.Append('<');
         sb.Append(tag.Name);
         sb.Append(tag.AttributesPart);
         if (tag.IsSelfClosingTag) {
            if (sb[sb.Length - 1] == ' ') sb.Length = sb.Length - 1;
            sb.Append('/');
         }
         else {
            openElements.Push(tag.Name);
         }
         sb.Append('>');
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
         sb.Append("</");
         sb.Append(elementName);
         sb.Append('>');
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

         if (tp.Peek(1) == '!') {
            SkipToEndOfTag();
            return null;
         }

         var htmlTag = new HtmlTag();
         if (htmlTag.Parse(tp)) return htmlTag;

         return null;
      }

      /// <summary>
      /// Skips over all characters up to and including the next '>'.
      /// </summary>
      /// <remarks>
      /// Used to skip "&lt;! ... &gt;", including "&lt;!DOCTYPE ... &gt;".
      /// </remarks>
      private void SkipToEndOfTag() {
         while (!tp.EndOfText) {
            tp.MoveAhead();
            if (tp.Peek() == '>') break;
         }
         tp.MoveAhead();
         return;
      }

      /// <summary>
      /// Skips over all the characters in an HTML comment.
      /// </summary>
      /// <remarks>
      /// Uses simple rules: the comment ends at the next
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

      /// <summary>
      /// Nested class that represents the state of the current HTML tag.
      /// </summary>
      private sealed class HtmlTag {
         /// <summary>
         /// Gets the text between the element name and the end of the tag text. The
         /// class attempts to verify the attribute text, i.e., it quotes unquoted
         /// values, changes '&amp;' to '&amp;amp;', etc.
         /// </summary>
         public string AttributesPart { get; private set; }

         /// <summary>
         /// Gets true if the tag is a closing tag, i.e., "&lt;/foo&gt;".
         /// </summary>
         public bool IsEndTag { get; private set; }

         /// <summary>
         /// Gets true if the tag is self-closing, i.e., ends with " /&gt;".
         /// </summary>
         public bool IsSelfClosingTag { get; private set; }

         /// <summary>
         /// Gets the element name of the tag.
         /// </summary>
         public string Name { get; private set; }

         /// <summary>
         /// Creates an instance of <see cref="HtmlTag"/>.
         /// </summary>
         public HtmlTag() {
            AttributesPart = String.Empty;
            Name = String.Empty;
         }

         /// <summary>
         /// Parses the text of an HTML tag and updates the <see cref="Name"/> and
         /// <see cref="AttributesPart"/> properties. <see cref="TextParser"/>
         /// <paramref name="tp"/> should be pointed at the '&lt;' character that
         /// starts an HTML tag.
         /// </summary>
         public bool Parse(TextParser tp) {
            if (tp.Peek() != '<') return false;
            tp.MoveAhead();

            if (tp.Peek() == '/') {
               tp.MoveAhead();
               IsEndTag = true;
            }

            if (tp.EndOfText) return false;

            if (!ParseName(tp)) return false;

            if (!ParseAttributes(tp)) return false;

            return true;
         }

         /// <summary>
         /// Parses the element name from the tag using the given
         /// <see cref="TextParser"/> <paramref name="tp"/>. The
         /// <see cref="TextParser"/> position should be set to
         /// the first character of the tag following the "&lt;",
         /// or following the "&lt;/" for end tags.
         /// Returns true if a syntactically valid name is found.
         /// </summary>
         private bool ParseName(TextParser tp) {
            const string nameCharacters = "abcdefghijklmnopqrstuvwxyz"
               + "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789:";

            var startPos = tp.Position;
            while (!tp.EndOfText) {
               var c = tp.Peek();
               if (nameCharacters.IndexOf(c) == -1) break;
               tp.MoveAhead();
            }

            var length = tp.Position - startPos;
            if (length <= 0) return false;

            Name = tp.Substring(startPos, length).ToLower();

            // Force void elements to be self-closing tags.
            IsSelfClosingTag = IsVoidElement(Name);

            return true;
         }

         /// <summary>
         /// Parses attributes from the tag using the given
         /// <see cref="TextParser"/> <paramref name="tp"/>. The
         /// <see cref="TextParser"/> position should be set to
         /// the first character of the tag following the element name.
         /// </summary>
         private bool ParseAttributes(TextParser tp) {
            const char kDoubleQuote = '"';
            const char kSingleQuote = '\'';
            const string kDoubleQuoteEntity = "&#22;";

            if (tp.Peek() == '>') {
               tp.MoveAhead();
               return true;
            }

            var sb = new StringBuilder();

            // Copy current input character
            void Copy() {
               sb.Append(tp.Peek());
               tp.MoveAhead();
            }

            // Copy input characters until fence character or end of tag
            void CopyTo(char fence) {
               while (!tp.EndOfText) {
                  var c = tp.Peek();
                  if (c == fence || c == '>') break;
                  if (c != kDoubleQuote) {
                     sb.Append(c);
                  }
                  else {
                     sb.Append(kDoubleQuoteEntity);
                  }
                  tp.MoveAhead();
               }
            }

            // Copy attributes
            var startPos = tp.Position;
            while (!tp.EndOfText) {
               var c = tp.Peek();
               if (c == '>' || c == '<') break;
               switch (c) {
                  case '=':
                     Copy();
                     c = tp.Peek();
                     if (c == kDoubleQuote) {
                        // Copy double-quoted value
                        Copy();
                        CopyTo(kDoubleQuote);
                        sb.Append(kDoubleQuote);
                        tp.MoveAhead();
                     }
                     else if (c == kSingleQuote) {
                        // Copy single-quoted value, but with double-quotes
                        sb.Append(kDoubleQuote);
                        tp.MoveAhead();
                        CopyTo(kSingleQuote);
                        sb.Append(kDoubleQuote);
                        tp.MoveAhead();
                     }
                     else {
                        // Copy unqouted value adding double-quotes
                        sb.Append(kDoubleQuote);
                        CopyTo(' ');
                        sb.Append(kDoubleQuote);
                     }
                     break;

                  default:
                     Copy();
                     break;
               }
            }

            if (tp.Peek() != '>') return false;

            if (tp.CharAt(tp.Position - 1) == '/') {
               IsSelfClosingTag = true;
               sb.Length = sb.Length - 1;
            }

            AttributesPart = sb.ToString();
            if (AttributesPart.IndexOf('&') != -1) {
               AttributesPart = ResolveEntities(AttributesPart);
            }

            tp.MoveAhead();

            return true;
         }

         private string ResolveEntities(string text) {
            var tp = new TextParser(text);
            var sb = new StringBuilder(text.Length);

            while (!tp.EndOfText) {
               var c = tp.Peek();
               switch (c) {
                  case '&':
                     EntityConverter.Convert(tp, sb);
                     break;

                  default:
                     sb.Append(c);
                     tp.MoveAhead();
                     break;
               }
            }
            return sb.ToString();
         }
      }
   }
}