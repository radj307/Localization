using Localization.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Localization
{
    /// <summary>
    /// Represents an error that occurs when no <see cref="ITranslationLoader"/> instances were added prior to attempting to load a file.
    /// </summary>
    public sealed class NoTranslationLoadersException : Exception
    {
        internal NoTranslationLoadersException(){}
        internal NoTranslationLoadersException(string message) : base(message){}
        internal NoTranslationLoadersException(SerializationInfo info, StreamingContext context) : base(info, context){}
        internal NoTranslationLoadersException(string message, Exception innerException) : base(message, innerException){}
    }
}
