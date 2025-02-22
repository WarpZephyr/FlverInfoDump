using System.Diagnostics;
using System.Text;

namespace FlverInfoDump
{
    public class ScopeWriter : IDisposable
    {
        private readonly TextWriter Writer;
        private readonly StringBuilder Buffer;
        private readonly StringBuilder ScopeBuffer;
        private readonly Stack<int> ScopeStack;
        private string Scope;
        private bool DoBuffering;
        private bool disposedValue;

        public ScopeWriter(TextWriter writer)
        {
            Writer = writer;
            Buffer = new StringBuilder();
            ScopeBuffer = new StringBuilder();
            ScopeStack = new Stack<int>();
            Scope = string.Empty;
        }

        #region IO

        public void Write(string value)
        {
            if (DoBuffering)
            {
                if (Scope != string.Empty)
                {
                    Buffer.Append(Scope);
                }

                Buffer.Append(value);
            }
            else
            {
                if (Scope != string.Empty)
                {
                    Writer.Write(Scope);
                }

                Writer.Write(value);
            }
        }

        public void WriteLine(string value)
        {
            if (DoBuffering)
            {
                if (Scope != string.Empty)
                {
                    Buffer.Append(Scope);
                }

                Buffer.AppendLine(value);
            }
            else
            {
                if (Scope != string.Empty)
                {
                    Writer.Write(Scope);
                }

                Writer.WriteLine(value);
            }
        }

        #endregion

        #region Buffering

        public void StartBuffering()
        {
            Debug.Assert(!DoBuffering, "Already buffering.");

            Buffer.Clear();
            DoBuffering = true;
        }

        public void EndBuffering()
        {
            Debug.Assert(DoBuffering, "Not buffering yet.");

            DoBuffering = false;
            if (Buffer.Length > 0)
            {
                Writer.Write(Buffer);
            }
        }

        #endregion

        #region Scope

        public void PushScope(string value)
        {
            ScopeStack.Push(ScopeBuffer.Length);
            ScopeBuffer.Append(value);
            Scope = ScopeBuffer.ToString();
        }

        public void PopScope()
        {
            Debug.Assert(ScopeStack.Count > 0, "There are no scopes to pop.");

            ScopeBuffer.Length = ScopeStack.Pop();
            Scope = ScopeBuffer.ToString();
        }

        public void ScopeTitleLine(string value, string scope)
        {
            WriteLine(value);
            PushScope(scope);
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Writer.Dispose();
                    Debug.Assert(DoBuffering == false, "Never stopped buffering.");
                    Buffer.Clear();
                    ScopeBuffer.Clear();
                    Scope = string.Empty;
                    Debug.Assert(ScopeStack.Count == 0, "Scope stack is not cleared.");
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
