using System;
using System.Threading.Tasks;

namespace StompSharp
{
    /// <summary>
    /// Represents a transaction with a STOMP compatible server.
    /// 
    /// Usage : Get the transaction via the <see cref="IStompClient"/>,
    /// You must call one of the methods <see cref="Commit"/> or <see cref="Rollback"/>,
    /// On dispose, <see cref="Rollback"/> will be called.
    /// 
    /// Note : The implementation of <see cref="IStompTransaction"/> in this client
    /// has no Finalizer, Meaning that you will leak transactions with the server if you will not call
    /// on of those methods.
    /// 
    /// Use within a dispose block (using ...) to ensure that rollback is called if commit haven't
    /// been called yet.
    /// </summary>
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
