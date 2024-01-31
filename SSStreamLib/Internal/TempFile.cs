namespace SStreamLib.Internal
{
    internal class TempFile : IDisposable
    {
        private List<Stream> openHandles = new List<Stream>();
        private string name;
        public TempFile()
        {
            name = Path.GetTempFileName();
        }
        private Action CreateDisposeAction(Stream toDispose)
        {
            return () =>
            {
                if (openHandles.Contains(toDispose))
                {
                    openHandles.Remove(toDispose);
                }

            };
        }
        private Func<Stream, Action> CreateCreateDisposeAction()
        {
            return (stream) =>
            {
                return CreateDisposeAction(stream);
            };
        }
        public TempFileStream OpenForReading()
        {
            var o = new TempFileStream(name, FileMode.Open, FileAccess.Read, CreateCreateDisposeAction());
            openHandles.Add(o);
            return o;

        }
        public TempFileStream OpenForWriting()
        {
            var o = new TempFileStream(name, FileMode.Open, FileAccess.Write, CreateCreateDisposeAction());
            openHandles.Add(o);
            return o;
        }
        public TempFileStream OpenForRandomAccess()
        {
            var o = new TempFileStream(name, FileMode.Open, FileAccess.ReadWrite, CreateCreateDisposeAction());
            openHandles.Add(o);
            return o;
        }

        public void Dispose()
        {
            var openHandlesSafe = new List<Stream>();
            openHandlesSafe.AddRange(openHandles);
            var results = openHandlesSafe.Aggregate((ReturnCode: 0, ExceptionList: new List<Exception>()), (prev, current) =>
            {
                var return_code = prev.ReturnCode;
                var exception_list = prev.ExceptionList;
                if (return_code == 0)
                {
                    try
                    {
                        current.Close();
                    }
                    catch (Exception e)
                    {
                        exception_list.Add(e);
                    }
                }
                return (ReturnCode: return_code, ExceptionList: exception_list);
            });
            var return_code = results.ReturnCode;
            var exception_list = results.ExceptionList;
            try
            {
                File.Delete(name);
            }
            catch (Exception e)
            {
                exception_list.Add(e);
                return_code = 1;
            }
            if (return_code != 0)
            {

                throw new IOException("And Error has occurred trying to close this TempFile.", new AggregateException(exception_list));
            }
        }

    }

}
