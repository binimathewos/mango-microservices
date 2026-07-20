namespace Mango.Service.OrderApi.Utility;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}