using TouchSocket.Core;

namespace TouchSocket.Redis;

internal sealed class RedisAdapter : CustomDataHandlingAdapter<RedisValue>
{
    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref RedisValue request)
    {
        var position = reader.BytesRead;
        if (!RedisRespParser.TryParse(ref reader, out request))
        {
            reader.BytesRead = position;
            return FilterResult.Cache;
        }

        return FilterResult.Success;
    }
}
