
namespace pdf_generator_service.Exceptions
{
    public class ServerErrorException : Exception
    {
        public ServerErrorException()
        {
        }

        public ServerErrorException(string? message) : base(message)
        {
        }

        public ServerErrorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}