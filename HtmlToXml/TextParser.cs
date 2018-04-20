using System;

namespace JohnCardinal.Text {
   /// <summary>
   /// Utility class for character-by-character text parsing.
   /// </summary>
   /// <remarks>
   /// Derived from TextParser by Jonathan Wood and published under the
   /// Code Project Open License (CPOL).
   /// 
   /// Article:
   /// http://www.blackbeltcoder.com/Articles/strings/a-text-parsing-helper-class
   /// 
   /// CPOL:
   /// https://www.codeproject.com/info/cpol10.aspx
   /// 
   /// There have been numerous changes, additions, and deletions. Some methods
   /// have been renamed. Documentation has been edited.
   /// 
   /// For the version included with HtmlToXml, any methods not used by HtmlToXml
   /// were removed.
   /// </remarks>
   public sealed class TextParser {
      /// <summary>
      /// '\0', a fence character while parsing, and the result when
      /// reading past the end of the text.
      /// </summary>
      public const char NullChar = '\0';

      private string text;
      private int pos;

      /// <summary>
      /// The text being parsed.
      /// </summary>
      public string Text { get { return text; } }

      /// <summary>
      /// The 0-origin position of the current character within
      /// the text to be parsed.
      /// </summary>
      public int Position { get { return pos; } }

      /// <summary>
      /// The length of the text being parsed.
      /// </summary>
      public int Length { get { return text.Length; } }

      /// <summary>
      /// The number of characters between Position and the
      /// end of the text being parsed.
      /// </summary>
      public int Remaining { get { return text.Length - pos; } }

      /// <summary>
      /// Construct a TextParser instance without text to parse.
      /// Supply the text later with Reset(string).
      /// </summary>
      public TextParser() {
         Reset(null);
      }

      /// <summary>
      /// Construct a TextParser instance and specify the text to parse.
      /// </summary>
      public TextParser(string text) {
         Reset(text);
      }

      /// <summary>
      /// Resets the current position to the start of the current text.
      /// </summary>
      public void Reset() {
         pos = 0;
      }

      /// <summary>
      /// Sets the current text and resets the current position to the start of it.
      /// </summary>
      /// <param name="text">The text to be parsed.</param>
      public void Reset(string text) {
         this.text = text ?? String.Empty;
         pos = 0;
      }

      /// <summary>
      /// True if the current position is at the end of the current text.
      /// </summary>
      public bool EndOfText {
         get { return (pos >= text.Length); }
      }

      /// <summary>
      /// Returns the character at the current position, 
      /// or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <returns>The character at the specified position.</returns>
      public char Peek() {
         if (pos < text.Length) return text[pos];
         return NullChar;
      }

      /// <summary>
      /// Returns the character at the specified number of characters beyond the current
      /// position, or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <param name="ahead">The number of characters beyond the current position. Defaults to zero.</param>
      /// <returns>The character at the specified position.</returns>
      public char Peek(int ahead) {
         int index = (pos + ahead);
         if (index < text.Length) return text[index];
         return NullChar;
      }

      /// <summary>
      /// Returns the character at the specified position in the string, or a null
      /// character if the specified position is at the end of the text.
      /// </summary>
      /// <param name="index">The index to the character of interest.</param>
      /// <returns>The character at the specified index.</returns>
      public char CharAt(int index) {
         if (index < text.Length) return text[index];
         return NullChar;
      }

      /// <summary>
      /// Extracts a substring from the specified position to the end of the text.
      /// </summary>
      /// <param name="start">Zero origin starting position of the substring.</param>
      /// <returns>The substring starting at the indicated position.</returns>
      public string Substring(int start) {
         return text.Substring(start);
      }

      /// <summary>
      /// Extracts a substring from the specified range of the current text.
      /// </summary>
      /// <param name="start">Zero origin starting position with the text being parsed.</param>
      /// <param name="length">Number of characters to extract.</param>
      /// <returns></returns>
      public string Substring(int start, int length) {
         return text.Substring(start, length);
      }

      /// <summary>
      /// Moves the current position ahead 1 character.
      /// </summary>
      public void MoveAhead() {
         if (pos < text.Length) pos++;
      }

      /// <summary>
      /// Moves the current position ahead the specified number of characters.
      /// </summary>
      /// <param name="ahead">The number of characters to move ahead.</param>
      public void MoveAhead(int ahead) {
         pos = Math.Min(pos + ahead, text.Length);
      }
   }
}
