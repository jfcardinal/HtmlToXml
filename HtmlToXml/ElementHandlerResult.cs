namespace HtmlToXml {
   /// <summary>
   /// Describes return codes from <see cref="IElementHandler"/> methods.
   /// </summary>
   internal enum ElementHandlerResult {
      /// <summary>
      /// Continue processing HTML element handlers.
      /// </summary>
      Continue,

      /// <summary>
      /// Close elements back to the matching element.
      /// Continue processing subsequent HTML element handlers.
      /// </summary>
      CloseElement,

      /// <summary>
      /// Close elements back to the matching element. Do not
      /// continue processing subsequent HTML element handlers.
      /// </summary>
      CloseElementAndBreak,
   }
}
