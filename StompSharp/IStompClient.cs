using System;
using System.Threading.Tasks;
using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    /// <summary>
    /// Represents a client for a STOMP compatible server.
    /// </summary>
    public interface IStompClient : IDisposable
    {

        /// <summary>
        /// Gets the transport that used to 
        /// make communication with the other side.
        /// </summary>
        ITransport Transport { get; }

        /// <summary>
        /// Gets the <paramref name="destination"/> destination.
        /// </summary>
        /// <param name="destination">The destination name (See stomp protocol for more information)</param>
        /// <param name="subscriptionBehavior">A subscription behavior from <see cref="SubscriptionBehaviors"/></param>
        /// <returns></returns>
        IDestination<TMessage> GetDestination<TMessage>(string destination, ISubscriptionBehavior<TMessage> subscriptionBehavior) 
            where TMessage : IMessage;

        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        /// <returns></returns>
        Task<IStompTransaction> BeginTransaction();

        /// <summary>
        /// Gets the default available subscription behaviors
        /// </summary>
        ISubscriptionBehaviorProvider SubscriptionBehaviors { get; }

    }

    /// <summary>
    /// Provides default available <see cref="ISubscriptionBehavior{TMessage}"/>s
    /// </summary>
    public interface ISubscriptionBehaviorProvider

    {

        /// <summary>
        /// Gets a subscription behavior with auto acknowledges
        /// </summary>
        ISubscriptionBehavior<IMessage> AutoAcknowledge { get; }

        /// <summary>
        /// Gets a subscription behavior with client acknowledges,
        /// This means that you (as the user of this library) should call
        /// <see cref="IAcknowledgableMessage.Ack"/> or <see cref="IAcknowledgableMessage.Nack"/>
        /// on (some) received messages.
        /// 
        /// <see cref="http://stomp.github.io/stomp-specification-1.2.html#SUBSCRIBE_ack_Header"/>
        /// </summary>
        ISubscriptionBehavior<IAcknowledgableMessage> ClientAcknowledge { get; }

        /// <summary>
        /// Gets a subscription behavior with client acknowledges,
        /// This means that you (as the user of this library) should call
        /// <see cref="IAcknowledgableMessage.Ack"/> or <see cref="IAcknowledgableMessage.Nack"/>
        /// on all of the received messages.
        /// 
        /// <see cref="http://stomp.github.io/stomp-specification-1.2.html#SUBSCRIBE_ack_Header"/>
        /// </summary>
        ISubscriptionBehavior<IAcknowledgableMessage> ClientIndividualAcknowledge { get; } 

    }

    class SubscriptionBehaviorProvider : ISubscriptionBehaviorProvider
    {
        private readonly ISubscriptionBehavior<IAcknowledgableMessage> _clientAcknowledge;
        private readonly ISubscriptionBehavior<IAcknowledgableMessage> _clientIndividualAcknowledge;

        public SubscriptionBehaviorProvider(ITransport transport)
        {
            _clientAcknowledge = new AcknowledgeSubscriptionBehavior(transport, AcknowledgeMode.Client);
            _clientIndividualAcknowledge = new AcknowledgeSubscriptionBehavior(transport, AcknowledgeMode.ClientIndividual);       
        }

        public ISubscriptionBehavior<IMessage> AutoAcknowledge
        {
            get { return NormalSubscriptionBehavior.Default; }
        }

        public ISubscriptionBehavior<IAcknowledgableMessage> ClientAcknowledge
        {
            get { return _clientAcknowledge; }
        }

        public ISubscriptionBehavior<IAcknowledgableMessage> ClientIndividualAcknowledge
        {
            get { return _clientIndividualAcknowledge; }
        }
    }
}