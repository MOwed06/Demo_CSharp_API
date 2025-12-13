using System.Runtime.Serialization;

namespace BigBooks.API.Core
{
    [Serializable]
    public class BigBooksException : Exception
    {
        public BigBooksException()
        {
        }

        public BigBooksException(string message)
            : base(message)
        {
        }

        public BigBooksException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BigBooksException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
