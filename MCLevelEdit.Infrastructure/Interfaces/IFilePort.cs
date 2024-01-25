using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Infrastructure.Interfaces;

public interface IFilePort
{
    Task<Map> LoadMapAsync(string fileName);
    Map LoadMap(string fileName);
}
