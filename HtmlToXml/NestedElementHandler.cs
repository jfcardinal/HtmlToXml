using System.Linq;

namespace HtmlToXml {
   /// <summary>
   /// <see cref="NestedElementHandler"/> closes an element based
   /// on the addition of other elements as long as the closing
   /// elements are not children of particular nested elements.
   /// </summary>
   internal class NestedElementHandler : IElementHandler {
      private string[] peers;
      private string[] parents;
      private string[] closers;

      /// <summary>
      /// The name of the HTML element associated with this
      /// <see cref="NestedElementHandler"/>.
      /// </summary>
      public string Name { get; }

      public ElementHandlerResult AddChild(StackArray<string> openElements, string childName) {
         if (!closers.Contains(childName)) return ElementHandlerResult.Continue;

         for (var index = openElements.Count; index > 0; index--) {
            var openElement = openElements[index - 1];
            if (parents.Contains(openElement)) {
               return ElementHandlerResult.Continue;
            }
            if (openElement.Equals(Name)) break;
         }

         if (closers.Contains(childName)) {
            if (peers.Contains(childName)) {
               return ElementHandlerResult.CloseElementAndBreak;
            }
            return ElementHandlerResult.CloseElement;
         }

         return ElementHandlerResult.Continue;
      }

      /// <summary>
      /// Creates an instance of <see cref="NestedElementHandler"/>.
      /// </summary>
      /// <param name="elementName">The name of the element.</param>
      /// <param name="peers">
      /// An array of peers of this element, including, at minimum,
      /// the element itself. For example, a peer of TD is TH. The
      /// addition of a peer closes the current element.
      /// </param>
      /// <param name="parents">
      /// An array of parent elements. For example, OL and UL are
      /// possible parents of LI. A parent element nested inside this
      /// element prevents any of the closer elements from closing
      /// this element.
      /// </param>
      /// <param name="closers">
      /// An array of elements that force the closure of this element,
      /// including, at minimum, the element itself.
      /// </param>
      public NestedElementHandler(
            string elementName,
            string[] peers,
            string[] parents,
            string[] closers) {
         Name = elementName;
         this.peers = peers;
         this.parents = parents;
         this.closers = closers;
      }
   }
}