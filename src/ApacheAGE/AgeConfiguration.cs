namespace ApacheAGE
{
    internal class AgeConfiguration
    {
        public AgeLoggerConfiguration Logger { get; set; }

        public bool SuperUser { get; set; } = true;

        public AgeConfiguration(AgeLoggerConfiguration logger) => Logger = logger;
    }
}
