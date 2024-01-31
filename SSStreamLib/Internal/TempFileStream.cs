

namespace SStreamLib.Internal
{
    internal class TempFileStream : FileStream
    {
        private readonly Action disposeAction;
        private readonly string name;
        private readonly FileMode mode;
        private readonly FileAccess access;
        private readonly Func<Stream, Action> creator;

        public TempFileStream(string name, FileMode mode, FileAccess access, Func<Stream, Action> disposeActionCreator) : base(name, mode, access)
        {
            this.name = name;
            this.mode = mode;
            this.access = access;
            this.creator = disposeActionCreator;
            disposeAction = disposeActionCreator(this);
        }
        public new void Dispose()
        {
            base.Dispose();
            disposeAction();
        }
        public override void Close()
        {
            base.Close();
            disposeAction();
        }
        public TempFileStream Reopen()
        {
            return new TempFileStream(name, mode, access, creator);
        }
    }

}
