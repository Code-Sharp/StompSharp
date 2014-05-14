namespace StompSharp.Messages
{
    public static class OutgoingMessageExtensions
    {


        public static IOutgoingMessage WithTransaction(this IOutgoingMessage message, IStompTransaction transaction)
        {
            return new TransactionHeaderDecorator(message, transaction.Id);
        }

        public static IOutgoingMessage WithPersistence(this IOutgoingMessage message)
        {
            return new PersistentHeaderDecorator(message);
        }


    }
}