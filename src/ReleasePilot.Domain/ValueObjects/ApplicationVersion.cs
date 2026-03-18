using ReleasePilot.Domain.Exceptions;

namespace ReleasePilot.Domain.ValueObjects
{
    public record ApplicationVersion
    {
        public string Value { get; init; }

        private ApplicationVersion(string value)
        {
            Value = value;
        }

        public static ApplicationVersion Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Application version cannot be empty.");

            return new ApplicationVersion(value);
        }

        public override string ToString() => Value;
    }
}