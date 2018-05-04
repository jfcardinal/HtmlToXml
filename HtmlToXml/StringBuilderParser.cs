using System;
using System.Text;

namespace JohnCardinal.Text {
   /// <summary>
   /// Utility class for character-by-character parsing of text in a
   /// <see cref="StringBuilder"/> instance.
   /// </summary>
   public class StringBuilderParser : ITextParser {
      private const char kNullCharacter = '\0';

      private StringBuilder sb;

      /// <summary>
      /// True if the current position is at the end of the current text.
      /// </summary>
      public bool EndOfText {
         get { return (Position >= sb.Length); }
      }

      /// <summary>
      /// The length of the text being parsed.
      /// </summary>
      public int Length { get { return sb.Length; } }

      /// <summary>
      /// The 0-origin position of the current character within
      /// the text to be parsed.
      /// </summary>
      public int Position { get; private set; }

      /// <summary>
      /// The number of characters between Position and the
      /// end of the text being parsed.
      /// </summary>
      public int Remaining { get { return sb.Length - Position; } }

      /// <summary>
      /// Returns the character at the specified position in the string, or a null
      /// character if the specified position is at the end of the text.
      /// </summary>
      /// <param name="index">The index to the character of interest.</param>
      /// <returns>The character at the specified index.</returns>
      public char CharAt(int index) {
         if (index < sb.Length) return sb[index];
         return kNullCharacter;
      }

      /// <summary>
      /// Moves the current position ahead 1 character.
      /// </summary>
      public void MoveAhead() {
         if (Position < sb.Length) Position++;
      }

      /// <summary>
      /// Moves the current position ahead the specified number of characters.
      /// </summary>
      /// <param name="ahead">The number of characters to move ahead.</param>
      public void MoveAhead(int ahead) {
         Position = Math.Min(Position + ahead, sb.Length);
      }

      /// <summary>
      /// Moves to the next occurrence of any character that is not one
      /// of the specified characters.
      /// </summary>
      /// <param name="chars">Array of characters to move past.</param>
      public void MovePast(char[] chars) {
         while (IsInArray(Peek(), chars)) MoveAhead();
      }

      /// <summary>
      /// Determines if the specified character exists in the specified
      /// character array.
      /// </summary>
      /// <param name="c">Character to find.</param>
      /// <param name="chars">Character array to search.</param>
      /// <returns>Returns true if the character is found; otherwise false.</returns>
      private bool IsInArray(char c, char[] chars) {
         for (int index = 0; index < chars.Length; index++) {
            if (c == chars[index]) return true;
         }
         return false;
      }

      /// <summary>
      /// Moves to the next occurrence of the specified character or
      /// to the end of the string if the character is not found.
      /// </summary>
      /// <param name="c">Character to find.</param>
      public void MoveTo(char c) {
         Position = IndexOf(c, Position);
         if (Position < 0) Position = sb.Length;
      }

      private int IndexOf(char c, int startIndex) {
         for (int index = startIndex; index < sb.Length; index++) {
            if (sb[index] == c) {
               return index;
            }
         }
         return -1;
      }

      /// <summary>
      /// Moves the current <see cref="Position"/> to the specified index.
      /// </summary>
      /// <param name="index">The new Position value.</param>
      public void MoveTo(int index) {
         if (index > 0) {
            Position = Math.Min(index, sb.Length);
         }
         else {
            Position = sb.Length;
         }
      }

      /// <summary>
      /// Returns the character at the current position, 
      /// or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <returns>The character at the current position.</returns>
      public char Peek() {
         if (Position < sb.Length) return sb[Position];
         return kNullCharacter;
      }

      /// <summary>
      /// Returns the character at the specified number of characters beyond the current
      /// position, or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <param name="ahead">The number of characters beyond the current position. Defaults to zero.</param>
      /// <returns>The character at the specified position.</returns>
      public char Peek(int ahead) {
         int index = (Position + ahead);
         if (index < sb.Length) return sb[index];
         return kNullCharacter;
      }

      /// <summary>
      /// Extracts a substring from the specified position to the end of the text.
      /// </summary>
      /// <param name="start">Zero origin starting position of the substring.</param>
      /// <returns>The substring starting at the indicated position.</returns>
      public string Substring(int start) {
         if (start >= sb.Length) return String.Empty;
         return sb.ToString(start, sb.Length - start);
      }

      /// <summary>
      /// Extracts a substring from the specified range of the current text.
      /// </summary>
      /// <param name="start">Zero origin starting position with the text being parsed.</param>
      /// <param name="length">Number of characters to extract.</param>
      /// <returns>The substring starting at the indicated position for (at most) the given length.</returns>
      public string Substring(int start, int length) {
         if (start >= sb.Length) return String.Empty;
         length = Math.Min(length, sb.Length - start);
         return sb.ToString(start, length);
      }

      /// <summary>
      /// Creates an instance of <see cref="StringBuilderParser"/>.
      /// </summary>
      /// <param name="sb"></param>
      public StringBuilderParser(StringBuilder sb) {
         this.sb = sb;
      }
   }
}
