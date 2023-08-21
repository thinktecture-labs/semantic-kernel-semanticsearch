using System.Net;

class LocalDebuggingProxy : IWebProxy
{
    public ICredentials? Credentials { get; set; }

    public Uri? GetProxy(Uri destination)
    {
        return new Uri("http://127.0.0.1:8081");
    }

    public bool IsBypassed(Uri host)
    {
        return false;
    }
}
