
namespace MetaRPC.CSharpMT5;

[Serializable]
internal class ConnectExceptionMT5 : Exception
{
    public ConnectExceptionMT5()
    {
    }

    public ConnectExceptionMT5(string? message) : base(message)
    {
    }

    public ConnectExceptionMT5(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}