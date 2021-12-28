using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightlessAbyss.Dev
{
    public interface IRoomDetector
    {
        Task<List<Room>> FindRooms(Structure structure);
    }
}