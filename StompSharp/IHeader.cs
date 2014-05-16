namespace StompSharp
{
    /// <summary>
    /// Represents a message header.
    /// </summary>
    public interface IHeader
    {
        /// <summary>
        /// Gets the key of the header
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the value of the header
        /// </summary>
        object Value { get; }

    }
}
