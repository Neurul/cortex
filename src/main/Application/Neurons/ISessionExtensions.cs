using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace org.neurul.Cortex.Application.Neurons
{
    public static class ISessionExtensions
    {
        public static async Task<T> Get<T>(this ISession session, Guid id, string parameterName, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            T result = default(T);

            try
            {
                result = await session.Get<T>(id, expectedVersion, cancellationToken);
            }
            catch (InvalidOperationException ioe)
            {
                throw new ArgumentException($"Error occurred while retrieving '{parameterName}' with Id '{id.ToString()}'.", ioe);
            }
            catch (AggregateNotFoundException anfe)
            {
                throw new ArgumentException($"Required '{parameterName}' with id '{id.ToString()}' was not found.", anfe);
            }

            return result;
        }

        public static async Task<T> GetOrDefaultIfGuidEmpty<T>(this ISession session, Guid id, string parameterName, T defaultValue, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot
        {
            T result = defaultValue;

            if (id != Guid.Empty)
                result = await ISessionExtensions.Get<T>(session, id, parameterName, expectedVersion, cancellationToken);

            return result;
        }
    }
}
