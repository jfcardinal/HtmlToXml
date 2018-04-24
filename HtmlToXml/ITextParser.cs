namespace JohnCardinal.Text {
   /// <summary>
   /// Represents a class that provides character-by-character text inspection.
   /// </summary>
   public interface ITextParser {
      /// <summary>
      /// True if the current position is at the end of the current text.
      /// </summary>
      bool EndOfText { get; }

      /// <summary>
      /// The length of the text being parsed.
      /// </summary>
      int Length { get; }

      /// <summary>
      /// The 0-origin position of the current character within
      /// the text to be parsed.
      /// </summary>
      int Position { get; }

      /// <summary>
      /// The number of characters between Position and the
      /// end of the text being parsed.
      /// </summary>
      int Remaining { get; }

      /// <summary>
      /// Returns the character at the specified position in the string, or a null
      /// character if the specified position is at the end of the text.
      /// </summary>
      /// <param name="index">The index to the character of interest.</param>
      /// <returns>The character at the specified index.</returns>
      char CharAt(int index);

      /// <summary>
      /// Moves the current position ahead 1 character.
      /// </summary>
      void MoveAhead();

      /// <summary>
      /// Moves the current position ahead the specified number of characters.
      /// </summary>
      /// <param name="ahead">The number of characters to move ahead.</param>
      void MoveAhead(int ahead);

      /// <summary>
      /// Returns the character at the current position, 
      /// or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <returns>The character at the current position.</returns>
      char Peek();

      /// <summary>
      /// Returns the character at the specified number of characters beyond the current
      /// position, or a null character if the specified position is at the end of the
      /// text.
      /// </summary>
      /// <param name="ahead">The number of characters beyond the current position. Defaults to zero.</param>
      /// <returns>The character at the specified position.</returns>
      char Peek(int ahead);

      /// <summary>
      /// Extracts a substring from the specified position to the end of the text.
      /// </summary>
      /// <param name="start">Zero origin starting position of the substring.</param>
      /// <returns>The substring starting at the indicated position.</returns>
      string Substring(int start);

      /// <summary>
      /// Extracts a substring from the specified range of the current text.
      /// </summary>
      /// <param name="start">Zero origin starting position with the text being parsed.</param>
      /// <param name="length">Number of characters to extract.</param>
      /// <returns>The substring starting at the indicated position for (at most) the given length.</returns>
      string Substring(int start, int length);
   }
}