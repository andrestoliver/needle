namespace Needle.Domain.Reviews;

public readonly record struct Rating
{
    public Rating(decimal value)
    {
        if (value < 0.5m || value > 5.0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Rating must be between 0.5 and 5.0.");
        }

        if (value % 0.5m != 0)
        {
            throw new ArgumentException(
                "Rating must be in 0.5 increments.",
                nameof(value));
        }
        
        Value = value;
    }
    
    public decimal Value { get; }
}