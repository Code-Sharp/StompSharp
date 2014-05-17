using StompSharp.Messages;

namespace StompSharp
{

    /// <summary>
    /// Represents a storage of <see cref="IDestination{TMessage}"/>s.
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
        /// <param name="subscriptionBehavior"></param>
        /// <returns></returns>
        IDestination<TMessage> Get<TMessage>(string destination, ISubscriptionBehavior<TMessage> subscriptionBehavior) 
            where TMessage : IMessage;


    }
}