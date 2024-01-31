using System.Collections;

namespace SStreamLib.Internal
{
    internal class RandomEnumerable : IEnumerable<byte>
    {
        
        public IEnumerator<byte> GetEnumerator()
        {
            return new RandomEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RandomEnumerator();
        }
    }
}
