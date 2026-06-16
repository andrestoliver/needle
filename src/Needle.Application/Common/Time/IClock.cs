namespace Needle.Application.Common.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}