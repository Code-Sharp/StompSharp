namespace StompSharp
{

    /// <summary>
    /// Represents a storage of <see cref="IDestination"/>s.
    /// </summary>
    public interface IDestinationStorage
    {
        /// <summary>
        /// Gets the destination by its name.
        /// 
        /// See stomp specification along with your STOMP server to
        /// understand the naming scheme of stomp destinations.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IDestination Get(string destination);

    }
}