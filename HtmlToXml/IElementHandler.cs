using JohnCardinal.Utility;

namespace HtmlToXml {
   /// <summary>
   /// An <see cref="IElementHandler"/> implements behavior that is specific
   /// to a particular HTML element.
   /// </summary>
   internal interface IElementHandler {
      string Name { get; }
      ElementHandlerResult AddChild(StackArray<string> openElements, string childName);
   }
}
