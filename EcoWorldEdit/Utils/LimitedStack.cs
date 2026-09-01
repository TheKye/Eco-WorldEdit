using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Eco.Mods.WorldEdit.Utils
{
	public class LimitedStack<T> : IEnumerable<T>
	{
		private readonly LinkedList<T> _stack;
		public int MaxSize { get; private set; }
		public int Count => this._stack.Count;

		public LimitedStack()
		{
			this._stack = new LinkedList<T>();
			this.MaxSize = Int32.MaxValue;
		}
		public LimitedStack(int maxSize)
		{
			if (maxSize < 1) throw new ArgumentOutOfRangeException(paramName: nameof(maxSize), message: "Accept only values greater than 1");
			this._stack = new LinkedList<T>();
			this.MaxSize = maxSize;
		}
		public LimitedStack(IEnumerable<T> collection)
		{
			this._stack = new LinkedList<T>(collection);
			this.MaxSize = this._stack.Count;
			if (this.MaxSize < 1)
			{ this.MaxSize = 1; }
		}
		public LimitedStack(IEnumerable<T> collection, int maxSize)
		{
			if (maxSize < 1) throw new ArgumentOutOfRangeException(paramName: nameof(maxSize), message: "Accept only values greater than 1");
			this._stack = new LinkedList<T>(collection.Take(maxSize));
			this.MaxSize = maxSize;
		}

		public void Clear() => this._stack.Clear();
		public bool Contains(T item) => this._stack.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => this._stack.CopyTo(array, arrayIndex);
		public override bool Equals(object? obj) => this._stack.Equals(obj);
		public override int GetHashCode() => this._stack.GetHashCode();
		public T Peek()
		{
			LinkedListNode<T> item = this._stack.First ?? throw new InvalidOperationException("Stack empty");
			return item.Value;
		}
		public T Pop()
		{
			LinkedListNode<T> item = this._stack.First ?? throw new InvalidOperationException("Stack empty");
			this._stack.RemoveFirst();
			return item.Value;
		}
		public void Push(T item)
		{
			if (this._stack.Count >= this.MaxSize)
			{
				this._stack.RemoveLast();
			}
			this._stack.AddFirst(item);
		}
		/// <summary>Adds a temporary top entry without evicting a regular bounded entry.</summary>
		public void PushTransient(T item) => this._stack.AddFirst(item);
		public T[] ToArray() => this._stack.ToArray();
		public bool TryPeek([MaybeNullWhen(false)] out T result)
		{
			result = default;
			LinkedListNode<T>? item = this._stack.First;
			if (item == null) return false;
			result = item.Value;
			return true;
		}
		public bool TryPop([MaybeNullWhen(false)] out T result)
		{
			result = default;
			LinkedListNode<T>? item = this._stack.First;
			if (item == null) return false;
			this._stack.RemoveFirst();
			result = item.Value;
			return true;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._stack.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._stack.GetEnumerator();
		}
	}
}
