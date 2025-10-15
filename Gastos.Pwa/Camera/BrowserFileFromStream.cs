using Microsoft.AspNetCore.Components.Forms;

namespace Gastos.Pwa.Camera;

public class BrowserFileFromStream : IBrowserFile
{
    private readonly byte[] _data;
    
    public BrowserFileFromStream(byte[] data, string name, string contentType)
    {
        _data = data;
        Name = name;
        ContentType = contentType;
        Size = data.Length;
    }
    
    public string Name { get; }
    public DateTimeOffset LastModified { get; } = DateTimeOffset.Now;
    public long Size { get; }
    public string ContentType { get; }

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        return new MemoryStream(_data);
    }
}
