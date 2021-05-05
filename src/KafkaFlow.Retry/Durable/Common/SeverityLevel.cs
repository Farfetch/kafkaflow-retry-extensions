namespace KafkaFlow.Retry.Durable.Common
{
    /// <summary>
    /// Severity level indicates the relative impact of a message on our customer's system or
    /// business processes
    /// </summary>
    public enum SeverityLevel
    {
        /// <summary>
        /// A severity level was not defined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// No loss of service. The software should recover by itself.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Minor loss of service. The result is an inconvenience, it's unclear if the software can
        /// recover by itself.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Partial loss of service with severe impact on the business. Usually needs human
        /// intervention to be solved.
        /// </summary>
        High = 3
    }
}