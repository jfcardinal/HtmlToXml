using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using HtmlToXml;

namespace HtmlToXmlTest {
   [TestClass]
   public class HtmlToXmlTests {
      private static HtmlConverter htmlToXml;

      [ClassInitialize()]
      public static void ClassInitializer(TestContext testContext) {
         htmlToXml = new HtmlConverter();
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
      public void ListNestedMixed() {
         Test("<ul><li>Item 1<ol><li>Item 1.1</li></ol><li>Item 2</li></ul>",
              "<ul><li>Item 1<ol><li>Item 1.1</li></ol></li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ListNestedUnclosedItem() {
         Test("<ul><li>Item 1<ul><li>Item 1.1</ul><li>Item 2</li></ul>",
              "<ul><li>Item 1<ul><li>Item 1.1</li></ul></li><li>Item 2</li></ul>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void DictItemsUnclosed() {
         Test("<dl><dt>DT1<dd>DD1",
              "<dl><dt>DT1</dt><dd>DD1</dd></dl>");
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

      [TestMethod, TestCategory("HtmlToXml")]
      public void TableDataUnclosed() {
         Test("<table><tr><td>C1<td>C2",
              "<table><tr><td>C1</td><td>C2</td></tr></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TableHeadingUnclosed() {
         Test("<table><tr><th>H1<th>H2",
              "<table><tr><th>H1</th><th>H2</th></tr></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TableCellsUnclosed() {
         Test("<table><th>H1<th>H2<tbody><td>C1<td>C2",
              "<table><th>H1</th><th>H2</th><tbody><td>C1</td><td>C2</td></tbody></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TableCellsMixedUnclosed() {
         Test("<table><th>H1<td>C1<tbody><td>C2<th>H2",
              "<table><th>H1</th><td>C1</td><tbody><td>C2</td><th>H2</th></tbody></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TableRowUnclosed() {
         Test("<table><tr><td>R1C1<td>R1C2<tr><td>R2C1<td>R2C2</table>",
              "<table><tr><td>R1C1</td><td>R1C2</td></tr>"
               + "<tr><td>R2C1</td><td>R2C2</td></tr></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void TablesNested() {
         Test("<table><th>H1<th>H2<tbody><td>C1<table><tr><td>T2.C1</table><td>C2</table>",
              "<table><th>H1</th><th>H2</th><tbody><td>C1"
               + "<table><tr><td>T2.C1</td></tr></table></td><td>C2</td></tbody></table>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void HeadNotClosed() {
         Test("<html><head><meta charset=\"UTF-8\"><body><h1>Heading",
              "<html><head><meta charset=\"UTF-8\"/></head>"
               + "<body><h1>Heading</h1></body></html>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void IgnoreDocType() {
         Test("<!DOCTYPE html><html><body><h1>Heading",
              "<html><body><h1>Heading</h1></body></html>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ScriptNoCData() {
         Test("<script href=\"script.js\"></script>",
              "<script href=\"script.js\"></script>");
      }

      [TestMethod, TestCategory("HtmlToXml")]
      public void ScriptWithCData() {
         Test("<script>var x = 0;</script>",
              "<script><![CDATA[var x = 0;]]></script>");
      }
   }
}
