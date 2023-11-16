using System;

namespace Localization.WPF
{
    /// <summary>
    /// Represents an error that occurred as a result of a programmer-provided data binding within a <see cref="TrExtension"/> instance.
    /// </summary>
    public sealed class TrExtensionBindingException : Exception
    {
        #region Constructors
        internal TrExtensionBindingException(TrExtension trExtension, Exception? innerException) 
            : base($"An exception was thrown by a provided binding in \"{trExtension.GetType()}\" with {nameof(TrExtension.Key)} \"{trExtension.Key}\", and {nameof(TrExtension)}.{nameof(TrExtension.CatchBindingExceptions)} was false! (See {nameof(InnerException)} for the exception)", innerException)
            => Instance = trExtension;
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the <see cref="TrExtension"/> instance that the exception occurred in.
        /// </summary>
        public TrExtension Instance { get; }
        #endregion Properties
    }
}
