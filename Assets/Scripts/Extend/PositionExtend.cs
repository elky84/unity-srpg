using Logic.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class PositionExtend
{
    public static PlayerPosition ToPlayerPosition(this Vector3 vector3)
    {
        return new PlayerPosition { X = (int)(vector3.x / 100), Y = (int)(Math.Abs(vector3.y / 100)) };
    }

    public static Vector3 PlayerPositionToVector3(this PlayerPosition playerPosition)
    {
        return new Vector3(playerPosition.X * 100 + 50, playerPosition.Y * -100 - 50, 1f);
    }

    public static Vector3 PlayerPositionToVector3(this Logic.Protocols.PlayerPosition playerPosition)
    {
        return new Vector3(playerPosition.X * 100 + 50, playerPosition.Y * -100 - 50, 1f);
    }

    public static PlayerPosition ToEntity(this Logic.Protocols.PlayerPosition playerPosition)
    {
        return new PlayerPosition { X = playerPosition.X, Y = playerPosition.Y };
    }
}
