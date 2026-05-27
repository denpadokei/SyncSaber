namespace SyncSaber.Utilities
{
    public class Stack<T> : System.Collections.Stack
    {
        public new T Pop()
        {
            return (T)base.Pop();
        }

        public void Push(T value)
        {
            base.Push(value);
        }

        public new T Peek()
        {
            return (T)base.Peek();
        }
    }
}
