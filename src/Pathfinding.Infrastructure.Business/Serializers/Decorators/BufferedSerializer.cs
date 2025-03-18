using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Serializers.Decorators
{
    public sealed class BufferedSerializer<T>(ISerializer<T> serializer) : ISerializer<T>
    {
        public async Task<T> DeserializeFromAsync(Stream stream, CancellationToken token = default)
        {
            try
            {
                await using var buffer = new BufferedStream(stream);
                return await serializer.DeserializeFromAsync(buffer, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SerializationException(ex.Message, ex);
            }
        }

        public async Task SerializeToAsync(T graph, Stream stream, CancellationToken token = default)
        {
            try
            {
                await using var buffer = new BufferedStream(stream);
                await serializer.SerializeToAsync(graph, buffer, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new SerializationException(ex.Message, ex);
            }
        }
    }
}
