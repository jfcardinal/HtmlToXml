using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JohnCardinal.Text {
   /// <summary>
   /// <see cref="HtmlToXml"/> provides a lightweight parser for converting
   /// HTML to XML including recovery from unclosed tags.
   /// </summary>
   public sealed class HtmlToXml {
      private static char[] restrictedCharacters = new char[] { '<', '>', '&' };
      private const char kFenceChar = '\0';
      private const string digits = "0123456789;";
      private const string hex = "0123456789abcdefABCDEF;";

      private const string kElementListItem = "li";
      private const string kElementOrderedList = "ol";
      private const string kElementParagraph = "p";
      private const string kElementUnorderedList = "ul";

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
      /// A dictionary of HTML named entities and their decimal entity values.
      /// </summary>
      private static Dictionary<string, string> namedEnties = new Dictionary<string, string>() {
         { "amp", "&amp;" },
         { "apos", "&#27;" },
         { "gt", "&gt;" },
         { "lt", "&lt;" },
         { "nbsp", "&#160;" },
         { "mdash", "&#8212;" },
         { "ndash", "&#8211;" },
         { "quot", "&#22;" },

         { "Aacute", "&#193;" },
         { "aacute", "&#225;" },
         { "Acirc", "&#194;" },
         { "acirc", "&#226;" },
         { "acute", "&#180;" },
         { "AElig", "&#198;" },
         { "aelig", "&#230;" },
         { "Agrave", "&#192;" },
         { "agrave", "&#224;" },
         { "Alpha", "&#913;" },
         { "alpha", "&#945;" },
         { "and", "&#8743;" },
         { "ang", "&#8736;" },
         { "Aring", "&#197;" },
         { "aring", "&#229;" },
         { "asymp", "&#8776;" },
         { "Atilde", "&#195;" },
         { "atilde", "&#227;" },
         { "Auml", "&#196;" },
         { "auml", "&#228;" },
         { "bdquo", "&#8222;" },
         { "Beta", "&#914;" },
         { "beta", "&#946;" },
         { "brvbar", "&#166;" },
         { "bull", "&#8226;" },
         { "cap", "&#8745;" },
         { "Ccedil", "&#199;" },
         { "ccedil", "&#231;" },
         { "cedil", "&#184;" },
         { "cent", "&#162;" },
         { "Chi", "&#935;" },
         { "chi", "&#967;" },
         { "circ", "&#710;" },
         { "clubs", "&#9827;" },
         { "cong", "&#8773;" },
         { "copy", "&#169;" },
         { "crarr", "&#8629;" },
         { "cup", "&#8746;" },
         { "curren", "&#164;" },
         { "dagger", "&#8224;" },
         { "Dagger", "&#8225;" },
         { "darr", "&#8595;" },
         { "deg", "&#176;" },
         { "Delta", "&#916;" },
         { "delta", "&#948;" },
         { "diams", "&#9830;" },
         { "divide", "&#247;" },
         { "Eacute", "&#201;" },
         { "eacute", "&#233;" },
         { "Ecirc", "&#202;" },
         { "ecirc", "&#234;" },
         { "Egrave", "&#200;" },
         { "egrave", "&#232;" },
         { "empty", "&#8709;" },
         { "emsp", "&#8195;" },
         { "ensp", "&#8194;" },
         { "Epsilon", "&#917;" },
         { "epsilon", "&#949;" },
         { "equiv", "&#8801;" },
         { "Eta", "&#919;" },
         { "eta", "&#951;" },
         { "ETH", "&#208;" },
         { "eth", "&#240;" },
         { "Euml", "&#203;" },
         { "euml", "&#235;" },
         { "euro", "&#8364;" },
         { "exist", "&#8707;" },
         { "fnof", "&#402;" },
         { "forall", "&#8704;" },
         { "frac12", "&#189;" },
         { "frac14", "&#188;" },
         { "frac34", "&#190;" },
         { "Gamma", "&#915;" },
         { "gamma", "&#947;" },
         { "ge", "&#8805;" },
         { "harr", "&#8596;" },
         { "hearts", "&#9829;" },
         { "hellip", "&#8230;" },
         { "Iacute", "&#205;" },
         { "iacute", "&#237;" },
         { "Icirc", "&#206;" },
         { "icirc", "&#238;" },
         { "iexcl", "&#161;" },
         { "Igrave", "&#204;" },
         { "igrave", "&#236;" },
         { "infin", "&#8734;" },
         { "int", "&#8747;" },
         { "Iota", "&#921;" },
         { "iota", "&#953;" },
         { "iquest", "&#191;" },
         { "isin", "&#8712;" },
         { "Iuml", "&#207;" },
         { "iuml", "&#239;" },
         { "Kappa", "&#922;" },
         { "kappa", "&#954;" },
         { "Lambda", "&#923;" },
         { "lambda", "&#955;" },
         { "laquo", "&#171;" },
         { "larr", "&#8592;" },
         { "lceil", "&#8968;" },
         { "ldquo", "&#8220;" },
         { "le", "&#8804;" },
         { "lfloor", "&#8970;" },
         { "lowast", "&#8727;" },
         { "loz", "&#9674;" },
         { "lrm", "&#8206;" },
         { "lsaquo", "&#8249;" },
         { "lsquo", "&#8216;" },
         { "macr", "&#175;" },
         { "micro", "&#181;" },
         { "minus", "&#8722;" },
         { "Mu", "&#924;" },
         { "mu", "&#956;" },
         { "nabla", "&#8711;" },
         { "ne", "&#8800;" },
         { "ni", "&#8715;" },
         { "not", "&#172;" },
         { "notin", "&#8713;" },
         { "nsub", "&#8836;" },
         { "Ntilde", "&#209;" },
         { "ntilde", "&#241;" },
         { "Nu", "&#925;" },
         { "nu", "&#957;" },
         { "Oacute", "&#211;" },
         { "oacute", "&#243;" },
         { "Ocirc", "&#212;" },
         { "ocirc", "&#244;" },
         { "OElig", "&#338;" },
         { "oelig", "&#339;" },
         { "Ograve", "&#210;" },
         { "ograve", "&#242;" },
         { "oline", "&#8254;" },
         { "Omega", "&#937;" },
         { "omega", "&#969;" },
         { "Omicron", "&#927;" },
         { "omicron", "&#959;" },
         { "oplus", "&#8853;" },
         { "or", "&#8744;" },
         { "ordf", "&#170;" },
         { "ordm", "&#186;" },
         { "Oslash", "&#216;" },
         { "oslash", "&#248;" },
         { "Otilde", "&#213;" },
         { "otilde", "&#245;" },
         { "otimes", "&#8855;" },
         { "Ouml", "&#214;" },
         { "ouml", "&#246;" },
         { "para", "&#182;" },
         { "part", "&#8706;" },
         { "permil", "&#8240;" },
         { "perp", "&#8869;" },
         { "Phi", "&#934;" },
         { "phi", "&#966;" },
         { "Pi", "&#928;" },
         { "pi", "&#960;" },
         { "piv", "&#982;" },
         { "plusmn", "&#177;" },
         { "pound", "&#163;" },
         { "prime", "&#8242;" },
         { "Prime", "&#8243;" },
         { "prod", "&#8719;" },
         { "prop", "&#8733;" },
         { "Psi", "&#936;" },
         { "psi", "&#968;" },
         { "radic", "&#8730;" },
         { "raquo", "&#187;" },
         { "rarr", "&#8594;" },
         { "rceil", "&#8969;" },
         { "rdquo", "&#8221;" },
         { "reg", "&#174;" },
         { "rfloor", "&#8971;" },
         { "Rho", "&#929;" },
         { "rho", "&#961;" },
         { "rlm", "&#8207;" },
         { "rsaquo", "&#8250;" },
         { "rsquo", "&#8217;" },
         { "sbquo", "&#8218;" },
         { "Scaron", "&#352;" },
         { "scaron", "&#353;" },
         { "sdot", "&#8901;" },
         { "sect", "&#167;" },
         { "shy", "&#173;" },
         { "Sigma", "&#931;" },
         { "sigma", "&#963;" },
         { "sigmaf", "&#962;" },
         { "sim", "&#8764;" },
         { "spades", "&#9824;" },
         { "sub", "&#8834;" },
         { "sube", "&#8838;" },
         { "sum", "&#8721;" },
         { "sup", "&#8835;" },
         { "sup1", "&#185;" },
         { "sup2", "&#178;" },
         { "sup3", "&#179;" },
         { "supe", "&#8839;" },
         { "szlig", "&#223;" },
         { "Tau", "&#932;" },
         { "tau", "&#964;" },
         { "there4", "&#8756;" },
         { "Theta", "&#920;" },
         { "theta", "&#952;" },
         { "thetasym", "&#977;" },
         { "thinsp", "&#8201;" },
         { "THORN", "&#222;" },
         { "thorn", "&#254;" },
         { "tilde", "&#732;" },
         { "times", "&#215;" },
         { "trade", "&#8482;" },
         { "Uacute", "&#218;" },
         { "uacute", "&#250;" },
         { "uarr", "&#8593;" },
         { "Ucirc", "&#219;" },
         { "ucirc", "&#251;" },
         { "Ugrave", "&#217;" },
         { "ugrave", "&#249;" },
         { "uml", "&#168;" },
         { "upsih", "&#978;" },
         { "Upsilon", "&#933;" },
         { "upsilon", "&#965;" },
         { "Uuml", "&#220;" },
         { "uuml", "&#252;" },
         { "Xi", "&#926;" },
         { "xi", "&#958;" },
         { "Yacute", "&#221;" },
         { "yacute", "&#253;" },
         { "yen", "&#165;" },
         { "yuml", "&#255;" },
         { "Yuml", "&#376;" },
         { "Zeta", "&#918;" },
         { "zeta", "&#950;" },
         { "zwj", "&#8205;" },
         { "zwnj", "&#8204;" },
      };

      /// <summary>
      /// Tracks which elements are open.
      /// </summary>
      private OpenElements openElements;

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
      /// If true, a paragraph ("p") tag is open.
      /// </summary>
      private bool paragraphIsOpen;

      /// <summary>
      /// Creates a new instance of <see cref="HtmlToXml"/>.
      /// </summary>
      public HtmlToXml() {
         openElements = new OpenElements();
      }

      /// <summary>
      /// Converts the HTML in <paramref name="htmlText"/> to XML.
      /// </summary>
      public string Convert(string htmlText) {
         if (String.IsNullOrEmpty(htmlText)) return htmlText;
         if (htmlText.IndexOfAny(restrictedCharacters) == -1) return htmlText;

         tp = new TextParser(htmlText);
         sb = new StringBuilder(htmlText.Length);
         paragraphIsOpen = false;

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
                  HandleEntity();
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

         if (tag.Name.Equals(kElementListItem)) {
            CloseListItemsIfNecessary();
         }

         if (paragraphIsOpen) {
            if (!IsInlineElement(tag.Name)) {
               CloseBackToMatchingTag(kElementParagraph);
               paragraphIsOpen = false;
            }
         }

         if (tag.Name.Equals(kElementParagraph)) {
            paragraphIsOpen = true;
         }

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

      /// <summary>
      /// An LI start tag will close the prior LI sibling, if there
      /// is one and it is open. Any nested tags that are inside the
      /// LI are also closed.
      /// </summary>
      private void CloseListItemsIfNecessary() {
         // Find prior open LI, if any
         var index = openElements.Count - 1;
         while (index >= 0) {
            if (openElements[index].Equals(kElementListItem)
                  || openElements[index].Equals(kElementUnorderedList)
                  || openElements[index].Equals(kElementOrderedList)) break;
            index--;
         }

         // No prior LI, so do nothing
         if (index == -1) return;

         // Close open elements back to last-previous LI
         if (openElements[index].Equals(kElementListItem)) {
            CloseBackToMatchingTag(kElementListItem);
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
         if (elementName.Equals(kElementParagraph)) paragraphIsOpen = false;
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

         var htmlTag = new HtmlTag();
         if (htmlTag.Parse(tp)) return htmlTag;

         return null;
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
      /// Validates an HTML entity and appends it to the output.
      /// </summary>
      private void HandleEntity() {
         if (EntityIsHexOrDecimal()) {
            AppendEntityToOutput();
            return;
         }

         var entity = GetNamedEntity();
         if (entity != null) {
            AppendEntityValue(entity);
            tp.MoveAhead(entity.Length + 2);
            return;
         }

         // Not an entity; encode the & and ignore the rest.
         sb.Append("&amp;");
         tp.MoveAhead();
      }

      /// <summary>
      /// If the given named <paramref name="entity"/> is recognized, its
      /// decimal value is appended to the output. Otherwise, the '&amp;'
      /// is converted to '&amp;amp;' and the remaining text is appended
      /// as-is.
      /// </summary>
      private void AppendEntityValue(string entity) {
         if (namedEnties.TryGetValue(entity, out string value)) {
            sb.Append(value);
            return;
         }

         if (namedEnties.TryGetValue(entity.ToLower(), out value)) {
            sb.Append(value);
            return;
         }

         sb.Append("&amp;");
         sb.Append(entity);
         sb.Append(';');
      }

      /// <summary>
      /// Appends the entity substring that begins at
      /// the current location, which is assumed to
      /// be '&amp;', to the output.
      /// </summary>
      private void AppendEntityToOutput() {
         while (!tp.EndOfText) {
            var c = tp.Peek();
            sb.Append(c);
            tp.MoveAhead();
            if (c == ';') break;
         }
      }

      /// <summary>
      /// Gets a named entity starting at the currrent
      /// character ('&amp;') and proceeding until ';'.
      /// Returns null if the substring does not match
      /// the expected entity sequence.
      /// </summary>
      private string GetNamedEntity() {
         const string kValidCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

         var startPos = tp.Position + 1;
         var offset = 1;
         char c;
         while (true) {
            c = tp.Peek(offset);
            if (kValidCharacters.IndexOf(c) == -1) break;
            offset++;
         }

         if (c != ';') return null;

         var length = offset - 1;
         if (length < 1) return null;

         return tp.Substring(startPos, length);
      }

      /// <summary>
      /// Returns true if the substring starting at offset
      /// and continuing to the next ';' contains characters
      /// from "validChars" only.
      /// </summary>
      private bool EntityContainsValidCharacters(int offset, string validChars) {
         char c;
         while ((c = tp.Peek(offset++)) != kFenceChar) {
            if (validChars.IndexOf(c) == -1) return false;
            if (c == ';') return true;
         }
         return false;
      }

      /// <summary>
      /// Returns true if the substring starting at
      /// the current position and continuing to
      /// the next ';' contains a decimal or hex
      /// entity sequence. On entry, tp should be
      /// pointing at '&amp;'.
      /// </summary>
      private bool EntityIsHexOrDecimal() {
         // Handle hex and numeric
         if (tp.Peek(1) == '#') {
            var c = tp.Peek(2);
            if (c == 'x' || c == 'X') {
               return EntityContainsValidCharacters(3, hex);
            }
            else return EntityContainsValidCharacters(2, digits);
         }
         return false;
      }

      /// <summary>
      /// Mantains a list of open elements.
      /// </summary>
      private sealed class OpenElements {
         private const int kInitialCapacity = 32;
         private string[] openElements;

         public int Count { get; private set; }

         /// <summary>
         /// Creates a new instance of <see cref="OpenElements"/>.
         /// </summary>
         public OpenElements() {
            openElements = new string[kInitialCapacity];
            Count = 0;
         }

         /// <summary>
         /// Adds the given <paramref name="elementName"/> to the stack.
         /// </summary>
         public void Push(string elementName) {
            if (Count == openElements.Length) ResizeOpenElements();
            openElements[Count++] = elementName;
         }

         /// <summary>
         /// Expands the openElements array to accommodate additional items.
         /// </summary>
         private void ResizeOpenElements() {
            Array.Resize(ref openElements, openElements.Length + kInitialCapacity);
         }

         /// <summary>
         /// Removes the top element from the stack and returns it.
         /// </summary>
         /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to pop an
         /// element when the stack is empty.</exception>
         public string Pop() {
            return openElements[--Count];
         }

         /// <summary>
         /// Searches the open elements starting from the top for an element with the given
         /// <paramref name="elementName"/>. Returns the zero-based index of the first matching
         /// item. Returns -1 if no element with the given <paramref name="elementName"/> is found.
         /// </summary>
         public int IndexOf(string elementName) {
            var index = Count - 1;
            while (index >= 0 && !openElements[index].Equals(elementName)) --index;
            return index;
         }

         /// <summary>
         /// Gets the elementName at the specified index.
         /// </summary>
         /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/>
         /// is less than 0 or index is equal to or greater than the <see cref="Count"/>.
         /// </exception>
         public string this[int index] {
            get {
               return openElements[index];
            }
         }
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
            const string nameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ:";

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
               var htmlToXml = new HtmlToXml();
               AttributesPart = htmlToXml.Convert(AttributesPart);
            }

            tp.MoveAhead();

            return true;
         }
      }
   }
}