using System;
using System.Threading.Tasks;

namespace StompSharp
{
    public interface IStompTransaction : IDisposable
    {

        /// <summary>
        /// Gets the Id of the transaction.
        /// 
        /// Protocol wise, It should be string, But
        /// this stomp client creates transaction with 
        /// incrementing integers id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Commits the transaction,
        /// Returns a task which is completed when
        /// the message is sent to the server
        /// </summary>
        /// <returns></returns>
        Task Commit();

        /// <summary>
        /// Rollbacks the transaction,
        /// Returns a task which is completed
        /// when the message is sent to the server.
        /// </summary>
        /// <returns></returns>
        Task Rollback();

    }
}
