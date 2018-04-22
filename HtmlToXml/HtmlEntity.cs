using System;
using System.Collections.Generic;
using System.Text;

using JohnCardinal.Text;

namespace HtmlToXml {
   internal sealed class EntityConverter {
      private const string digits = "0123456789;";
      private const string hex = "0123456789abcdefABCDEF;";

      /// <summary>
      /// Validates an HTML entity and appends it to the output.
      /// </summary>
      public static void Convert(TextParser tp, StringBuilder sb) {
         if (EntityIsHexOrDecimal(tp)) {
            AppendEntityToOutput(tp, sb);
            return;
         }

         var entity = GetNamedEntity(tp);
         if (entity != null) {
            AppendEntityValue(sb, entity);
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
      private static void AppendEntityValue(StringBuilder sb, string entity) {
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
      private static void AppendEntityToOutput(TextParser tp, StringBuilder sb) {
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
      private static string GetNamedEntity(TextParser tp) {
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
      private static bool EntityContainsValidCharacters(TextParser tp, int offset, string validChars) {
         char c;
         while ((c = tp.Peek(offset++)) != TextParser.NullChar) {
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
      private static bool EntityIsHexOrDecimal(TextParser tp) {
         // Handle hex and numeric
         if (tp.Peek(1) == '#') {
            var c = tp.Peek(2);
            if (c == 'x' || c == 'X') {
               return EntityContainsValidCharacters(tp, 3, hex);
            }
            else return EntityContainsValidCharacters(tp, 2, digits);
         }
         return false;
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
   }
}
