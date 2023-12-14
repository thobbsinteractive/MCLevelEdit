using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.Infrastructure.Interfaces;

public interface IFilePort
{
    Task<Map> LoadMap(string fileName);
}
