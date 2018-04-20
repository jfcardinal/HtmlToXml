using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JohnCardinal.Text;

namespace HtmlToXmlTest {
   [TestClass]
   public class HtmlToXmlTests {
      private static HtmlToXml htmlToXml;

      [ClassInitialize()]
      public static void ClassInitializer(TestContext testContext) {
         htmlToXml = new HtmlToXml();
      }

      public static void Test(string input, string expectedResult) {
         var actualResult = htmlToXml.Convert(input);
         Assert.AreEqual(expectedResult, actualResult);
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TextOnly() {
         Test("Free text.",
            "Free text.");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TextWithGT() {
         Test("Free > text.",
            "Free &gt; text.");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphOnly() {
         Test("<p>A simple paragraph.</p>",
              "<p>A simple paragraph.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphUnclosed() {
         Test("<p>Unclosed paragraph.",
              "<p>Unclosed paragraph.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphUnclosedBeforeDiv() {
         Test("<p>Unclosed paragraph.<div>Something</div>",
              "<p>Unclosed paragraph.</p><div>Something</div>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphsUnclosed() {
         Test("<p>Paragraph one.<p>Paragraph two.",
              "<p>Paragraph one.</p><p>Paragraph two.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithBreakClosed() {
         Test("<p>A simple<br/>paragraph.</p>",
              "<p>A simple<br/>paragraph.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithBreakUnclosed() {
         Test("<p>A simple<br>paragraph.</p>",
              "<p>A simple<br/>paragraph.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithBoldUnclosed() {
         Test("<p>Paragraph <b>one.</p>",
              "<p>Paragraph <b>one.</b></p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphUnclosedWithBoldUnclosed() {
         Test("<p>Paragraph <b>one.",
              "<p>Paragraph <b>one.</b></p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void List() {
         Test("<ul><li>Item 1</li><li>Item 2</li></ul>",
              "<ul><li>Item 1</li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListUnclosed() {
         Test("<ul><li>Item 1</li><li>Item 2</li>",
              "<ul><li>Item 1</li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListItemsUnclosed() {
         Test("<ul><li>Item 1<li>Item 2</ul>",
              "<ul><li>Item 1</li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListUnclosedItemsUnclosed() {
         Test("<ul><li>Item 1<li>Item 2",
              "<ul><li>Item 1</li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListUnclosedItemsUnclosedWithDiv() {
         Test("<ul><li>Item 1<li>Item 2<div>Here",
              "<ul><li>Item 1</li><li>Item 2<div>Here</div></li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListNested() {
         Test("<ul><li>Item 1<ul><li>Item 1.1</li></ul><li>Item 2</li></ul>",
              "<ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListNestedUnclosedItem() {
         Test("<ul><li>Item 1<ul><li>Item 1.1</ul><li>Item 2</li></ul>",
              "<ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithMDash() {
         Test("<p>What will an entity&ndash;any entity&mdash;yield?</p>",
              "<p>What will an entity&#8211;any entity&#8212;yield?</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithAacute() {
         Test("<p>&Aacute; and &aacute;</p>",
              "<p>&#193; and &#225;</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithAacuteInvalid() {
         Test("<p>&Aacute and &aacute</p>",
              "<p>&amp;Aacute and &amp;aacute</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithNBSP() {
         Test("<p>Extra.&nbsp; Spacing.</p>",
              "<p>Extra.&#160; Spacing.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphWithGT() {
         Test("<p>File &gt; Save As</p>",
              "<p>File &gt; Save As</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphAndComment() {
         Test("<p>You <!--ain't-->got it.</p>",
              "<p>You got it.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ParagraphAndCommentWithNestedTags() {
         Test("<p>You <!--<strong>ain't</strong>-->got it.</p>",
              "<p>You got it.</p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void SpanWithAttributes() {
         Test("<span style=\"color:blue\">sad</span>",
              "<span style=\"color:blue\">sad</span>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ImgUnclosedWithAttributes() {
         Test("<img alt=\"\" src=\"image.jpg\">",
              "<img alt=\"\" src=\"image.jpg\"/>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ImgWithUnquotedAttributes() {
         Test("<img height=1 width=1 src=\"image.jpg\">",
              "<img height=\"1\" width=\"1\" src=\"image.jpg\"/>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void AnchorWithHRef() {
         Test("<a href=\"foo?doo&ret=no\">Text</a>",
              "<a href=\"foo?doo&amp;ret=no\">Text</a>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void AnchorMissingGT() {
         Test("<div><a href=\"foo\"text</a></div>",
              "<div></div>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void NonHtmlTag() {
         Test("<p><o>test</o></p>",
              "<p></p><o>test</o>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void NonHtmlTagWithNamespace() {
         Test("<p><o:p>test</o:p></p>",
              "<p><o:p>test</o:p></p>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void Attributes() {
         Test("<img src=\"image.jpg\" height='80' width=100>",
              "<img src=\"image.jpg\" height=\"80\" width=\"100\"/>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ManyOpenTags() {
         Test("<div><div><div><div><div><div><div><div><div><div>" +
              "<div><div><div><div><div><div><div><div><div><div>",
              "<div><div><div><div><div><div><div><div><div><div>" +
              "<div><div><div><div><div><div><div><div><div><div>" +
              "</div></div></div></div></div></div></div></div></div></div>" +
              "</div></div></div></div></div></div></div></div></div></div>");
      }
   }
}
