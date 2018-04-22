namespace HtmlToXml {
   /// <summary>
   /// <see cref="ParagraphHandler"/> closes an open paragraph element
   /// upon the addition of any non-inline element.
   /// </summary>
   internal sealed class ParagraphHandler : IElementHandler {
      private const string kElementName = "p";

      public string Name { get { return kElementName; } }

      public ElementHandlerResult AddChild(StackArray<string> openElements, string childName) {
         if (!HtmlConverter.IsInlineElement(childName)) {
            return ElementHandlerResult.CloseElement;
         }
         return ElementHandlerResult.Continue;
      }
   }
}
