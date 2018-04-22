using System;
using System.Collections.Generic;

namespace HtmlToXml {
   /// <summary>
   /// Creates and manages a cache of <see cref="IElementHandler"/> 
   /// instances.
   /// </summary>
   internal class ElementHandlerFactory {
      private Dictionary<string, IElementHandler> handlerCache;

      public ElementHandlerFactory() {
         handlerCache = new Dictionary<string, IElementHandler>();
      }

      public IElementHandler GetHandler(string elementName) {
         if (handlerCache.TryGetValue(elementName, out IElementHandler handler)) {
            return handler;
         }

         switch (elementName) {
            case "li":
               return AddHandlerToCache(elementName, GetListItemHandler);

            case "p":
               return AddHandlerToCache(elementName, GetParagraphHandler);

            case "td":
            case "th":
               return AddHandlerToCache(elementName, GetTableCellHandler);

            case "tr":
               return AddHandlerToCache(elementName, GetTableRowHandler);

            case "thead":
            case "tbody":
            case "tfoot":
            case "caption":
               return AddHandlerToCache(elementName, GetTableBlockHandler);

            case "head":
            case "body":
               return AddHandlerToCache(elementName, GetHtmlChildHandler);

            default:
               return null;
         }
      }

      private IElementHandler AddHandlerToCache(string elementName, Func<string, IElementHandler> getHandlerMethod) {
         var elementHandler = getHandlerMethod(elementName);
         handlerCache.Add(elementName, elementHandler);
         return elementHandler;
      }

      #region P

      private IElementHandler GetParagraphHandler(string elementName) {
         return new ParagraphHandler();
      }

      #endregion

      #region OL, UL, LI

      private static string[] litItemParents = new[] { "ol", "ul" };

      private static string[] listItemPeers = new[] { "li" };

      private IElementHandler GetListItemHandler(string elementName) {
         return new NestedElementHandler(elementName, listItemPeers,
            litItemParents, listItemPeers);
      }

      #endregion

      #region TABLE, THEAD, etc.

      private static string[] tableParent = new[] { "table" };

      private static string[] tableCellPeers = new[] {
         "td", "th"
      };

      private static string[] tableCellClosers = new[] {
         "td", "th", "tr", "thead", "tbody", "tfoot", "caption"
      };

      private IElementHandler GetTableCellHandler(string elementName) {
         return new NestedElementHandler(elementName, tableCellPeers,
            tableParent, tableCellClosers);
      }

      private static string[] tableRowPeers = new[] {
         "tr",
      };

      private static string[] tableRowClosers = new[] {
         "tr", "thead", "tbody", "tfoot", "caption"
      };

      private IElementHandler GetTableRowHandler(string elementName) {
         return new NestedElementHandler(elementName, tableRowPeers,
            tableParent, tableRowClosers);
      }

      private static string[] tableBlockPeers = new[] {
         "thead", "tbody", "tfoot", "caption"
      };

      private IElementHandler GetTableBlockHandler(string elementName) {
         return new NestedElementHandler(elementName, tableBlockPeers,
            tableParent, tableBlockPeers);
      }

      #endregion

      #region HTML/HEAD/BODY

      private static string[] htmlParent = new[] { "html" };

      private static string[] htmlPeers = new[] {
         "head", "body"
      };

      private IElementHandler GetHtmlChildHandler(string elementName) {
         return new NestedElementHandler(elementName, htmlPeers,
            htmlParent, htmlPeers);
      }

      #endregion
   }
}
