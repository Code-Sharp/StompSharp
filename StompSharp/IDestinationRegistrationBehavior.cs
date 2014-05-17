using StompSharp.Messages;
using StompSharp.Transport;

namespace StompSharp
{
    /// <summary>
    /// Defines a registration behavior.
    /// </summary>
    public interface ISubscriptionBehavior<out TMessage> 
        where TMessage : IMessage
    {

        /// <summary>
        /// Transforms the incomming message 
        /// to the wanted message type.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        TMessage Transform(IMessage message);

        /// <summary>
        /// Gets the message that need to be sent to the server
        /// to register to the given destination.
        /// </summary>
        /// <param name="destinationName">The destination to subscribe to</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns></returns>
        IMessage GetSubscriptionMessage(string destinationName, int subscriptionId);

    }

    /// <summary>
    /// Provides Acknowledge Types.
    /// </summary>
    public class AcknowledgeMode
    {

        /// <summary>
        /// This acknowledge type force you to acknowledge/nack for every message
        /// </summary>
        public static readonly AcknowledgeMode ClientIndividual = new AcknowledgeMode("client-individual");

        /// <summary>
        /// This acknowledge mode allows you to send acknowledge/nack for every message,
        /// But every acknowledge/nack will be cumulative for every message that haven't 
        /// acknowledged/nack'd before.
        /// </summary>
        public static readonly AcknowledgeMode Client = new AcknowledgeMode("client");

        private readonly string _stompName;

        /// <summary>
        /// Gets the name of the ack to use within stomp protocol.
        /// </summary>
        public string StompName
        {
            get { return _stompName; }
        }

        private AcknowledgeMode(string stompName)
        {
            _stompName = stompName;
        }
    }

    class NormalSubscriptionBehavior : ISubscriptionBehavior<IMessage>
    {
        public static readonly ISubscriptionBehavior<IMessage> Default = new NormalSubscriptionBehavior();

        private NormalSubscriptionBehavior()
        {
            
        }

        public IMessage Transform(IMessage message)
        {
            return message;
        }

        public IMessage GetSubscriptionMessage(string destinationName, int subscriptionId)
        {
            return
                new MessageBuilder("SUBSCRIBE")
                    .Header("destination", destinationName)
                    .Header("id", subscriptionId)
                    .WithoutBody();
        }
    }

    class AcknowledgeSubscriptionBehavior : ISubscriptionBehavior<IAcknowledgableMessage>
    {
        private readonly ITransport _transport;
        private readonly string _acknowledgeMode;

        public AcknowledgeSubscriptionBehavior(ITransport transport, AcknowledgeMode acknowledgeMode)
        {
            _transport = transport;
            _acknowledgeMode = acknowledgeMode.StompName;
        }

        public IAcknowledgableMessage Transform(IMessage message)
        {
            return new AcknowledgableMessage(message, _transport);
        }

        public IMessage GetSubscriptionMessage(string destinationName, int subscriptionId)
        {
            return
                new MessageBuilder("SUBSCRIBE")
                    .Header("destination", destinationName)
                    .Header("id", subscriptionId)
                    .Header("ack", _acknowledgeMode)
                    .WithoutBody();
        }
    }
}
