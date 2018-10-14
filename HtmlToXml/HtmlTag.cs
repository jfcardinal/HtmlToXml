using System;
using System.Diagnostics;
using System.Text;

using JohnCardinal.Text;

namespace HtmlToXml {
   public sealed partial class HtmlConverter {
      /// <summary>
      /// Nested class that represents the state of the current HTML tag.
      /// </summary>
      [DebuggerDisplay("Name={Name}")]
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
         /// <see cref="AttributesPart"/> properties. <see cref="ITextParser"/>
         /// <paramref name="tp"/> should be pointed at the '&lt;' character that
         /// starts an HTML tag.
         /// </summary>
         public bool Parse(ITextParser tp) {
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
         /// <see cref="ITextParser"/> <paramref name="tp"/>. The
         /// <see cref="ITextParser"/> position should be set to
         /// the first character of the tag following the "&lt;",
         /// or following the "&lt;/" for end tags.
         /// Returns true if a syntactically valid name is found.
         /// </summary>
         private bool ParseName(ITextParser tp) {
            const string nameCharacters = "abcdefghijklmnopqrstuvwxyz"
               + "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789:";

            var startPos = tp.Position;

            var offset = 0;
            char c;
            while ((c = tp.Peek(offset)) != TextParser.NullChar) {
               if (nameCharacters.IndexOf(c) == -1) break;
               offset++;
            }

            // Did we get any valid characters?
            if (offset < 1) return false;

            // Does the tag name end properly?
            if (c != '>' && c != '/' && !Char.IsWhiteSpace(c)) return false;
            if (tp.Peek(offset - 1) == ':') return false;

            // Our minimal validation has passed...
            tp.MoveAhead(offset);
            var length = tp.Position - startPos;
            Name = tp.Substring(startPos, length).ToLower();

            // Force void elements to be self-closing tags.
            IsSelfClosingTag = IsVoidElement(Name);

            return true;
         }

         /// <summary>
         /// Parses attributes from the tag using the given
         /// <see cref="ITextParser"/> <paramref name="tp"/>. The
         /// <see cref="ITextParser"/> position should be set to
         /// the first character of the tag following the element name.
         /// </summary>
         private bool ParseAttributes(ITextParser tp) {
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