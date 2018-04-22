using System;

namespace HtmlToXml {
   /// <summary>
   /// Mantains a simple LIFO stack that provides a count and
   /// where items in the stack can be accessed via their index.
   /// </summary>
   internal sealed class StackArray<T> {
      private const int kDefaultAllocationUnit = 32;
      private T[] items;

      /// <summary>
      /// Gets the number of items that will be added to the capacity
      /// of the stack when the current stack is full.
      /// </summary>
      public int AllocationUnit { get; }

      /// <summary>
      /// Gets the number of items currently in the stack.
      /// </summary>
      public int Count { get; private set; }

      /// <summary>
      /// Creates a new instance of <see cref="StackArray{T}"/>.
      /// </summary>
      public StackArray(int allocationUnit = kDefaultAllocationUnit) {
         AllocationUnit = allocationUnit;
         items = new T[allocationUnit];
         Count = 0;
      }

      /// <summary>
      /// Adds the given <paramref name="item"/> to the stack.
      /// </summary>
      public void Push(T item) {
         if (Count == items.Length) ResizeStack();
         items[Count++] = item;
      }

      /// <summary>
      /// Expands the  stack to accommodate additional items.
      /// </summary>
      private void ResizeStack() {
         Array.Resize(ref items, items.Length + AllocationUnit);
      }

      /// <summary>
      /// Removes the top item from the stack and returns it.
      /// </summary>
      /// <exception cref="ArgumentOutOfRangeException">Thrown when attempting to pop an
      /// item when the stack is empty.</exception>
      public T Pop() {
         if (Count <= 0) throw new ArgumentOutOfRangeException();
         return items[--Count];
      }

      /// <summary>
      /// Searches the stack starting from the top for the given
      /// <paramref name="item"/>. Returns the zero-based index of the first matching
      /// item. Returns -1 if <paramref name="item"/> is not found.
      /// </summary>
      public int IndexOf(T item) {
         var index = Count - 1;
         while (index >= 0 && !items[index].Equals(item)) --index;
         return index;
      }

      /// <summary>
      /// Gets the item at the specified <paramref name="index"/>.
      /// </summary>
      /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/>
      /// is less than 0 or index is equal to or greater than the <see cref="Count"/>.
      /// </exception>
      public T this[int index] {
         get {
            return items[index];
         }
      }
   }
}
