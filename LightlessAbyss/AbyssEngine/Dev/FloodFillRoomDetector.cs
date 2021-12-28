using System.Collections.Generic;
using System.Threading.Tasks;
using LightlessAbyss.AbyssEngine.CustomMath;

namespace LightlessAbyss.AbyssEngine.Dev
{
    public sealed class FloodFillRoomDetector : IRoomDetector
    {
        public async Task<List<Room>> FindRooms(Structure sData)
        {
            List<CVector2Int> interiorPositions = sData.Bounds.GetAllPositionsWithin();
            
            // inverses flood fill by only using modified positionsToCheck list
            await FloodFill(sData.Bounds.Min, sData, interiorPositions, false);
            
            List<Room> rooms = new List<Room>();

            while (interiorPositions.Count > 0)
            {
                CVector2Int origin = interiorPositions.GetRandomElementOfList();

                List<CVector2Int> roomPositions = 
                    await FloodFill(origin, sData, interiorPositions, false);
                
                rooms.Add(new Room(roomPositions));

                await Task.Yield();
            }

            return rooms;
        }

        private static async Task<List<CVector2Int>> FloodFill(CVector2Int origin, 
            Structure sData, List<CVector2Int> positionsToCheck, bool includeWallsInFoundPositions)
        {
            List<CVector2Int> foundPositions = new List<CVector2Int>();
            Stack<CVector2Int> positionStack = new Stack<CVector2Int>();
            
            positionStack.Push(origin);

            int iterations = 0;
            
            while (positionStack.Count > 0)
            {
                CVector2Int tilePos = positionStack.Pop();

                if (!sData.Bounds.Contains(tilePos))
                    continue;

                positionsToCheck.Remove(tilePos);

                if (sData.HasCollisionWallAtTilePos(tilePos))
                {
                    if (includeWallsInFoundPositions)
                        foundPositions.Add(tilePos);
                    continue;
                }
                
                foundPositions.Add(tilePos);

                CVector2Int up = tilePos + CVector2Int.Up;
                CVector2Int down = tilePos + CVector2Int.Down;
                CVector2Int right = tilePos + CVector2Int.Right;
                CVector2Int left = tilePos + CVector2Int.Left;

                if (positionsToCheck.Contains(up)
                    && !foundPositions.Contains(up) && !positionStack.Contains(up)) 
                    positionStack.Push(up);
                
                if (positionsToCheck.Contains(down)
                    && !foundPositions.Contains(down) && !positionStack.Contains(down)) 
                    positionStack.Push(down);
                
                if (positionsToCheck.Contains(right) 
                    && !foundPositions.Contains(right) && !positionStack.Contains(right)) 
                    positionStack.Push(right);
                
                if (positionsToCheck.Contains(left)
                    && !foundPositions.Contains(left) && !positionStack.Contains(left)) 
                    positionStack.Push(left);

                iterations++;
                
                if (positionsToCheck.Count > 256 && iterations % 10 == 0)
                    await Task.Yield();
            }

            return foundPositions;
        }
    }
}