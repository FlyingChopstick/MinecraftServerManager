using System;

namespace CommonFunctionality.Exceptions
{

    [Serializable]
    public class EulaNotAcceptedException : Exception
    {
        public EulaNotAcceptedException(string message = "EULA is not accepted. You have to modify the eula.txt file in the server directory.") : base(message) { }
        public EulaNotAcceptedException(string message, Exception inner) : base(message, inner) { }
        protected EulaNotAcceptedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
