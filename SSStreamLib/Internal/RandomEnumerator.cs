using System.Collections;

namespace SStreamLib.Internal
{
    class RandomEnumerator : IEnumerator<byte>
    {
        private byte[]? buffer = null;
        private int bufferIndex = 0;
        private System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        private byte @byte;
        public byte Current => @byte;

        object IEnumerator.Current => @byte;

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            if (buffer == null || bufferIndex >= buffer.Length)
            {
                if (buffer == null)
                {
                    buffer = new byte[4096];
                }
                rng.GetBytes(buffer);
                bufferIndex = 0;
            }
            @byte = buffer[bufferIndex++];
            return true;
        }

        public void Reset()
        {
            
        }
    }
}
