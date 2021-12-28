using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightlessAbyss.AbyssEngine.Dev
{
    public interface IRoomDetector
    {
        Task<List<Room>> FindRooms(Structure structure);
    }
}