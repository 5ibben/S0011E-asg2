using UnityEngine;

enum messages
{
    Msg_Blank,
    Msg_PathReady,
    Msg_NoPathAvailable,
    Msg_GraphChanged,
    Msg_GraphItemChanged,
    Msg_Character_MoveToPosition,
    Msg_Character_FindItem,
}

public abstract class ExtraInfo
{
}

public class ExtraInfo_GraphChanged : ExtraInfo
{
    public ExtraInfo_GraphChanged(int node)
    {
        affectedNode = node;
    }
    public int affectedNode;
}
public class ExtraInfo_GraphItemChanged : ExtraInfo
{
    public ExtraInfo_GraphItemChanged(int node)
    {
        affectedNode = node;
    }
    public int affectedNode;
}

public class ExtraInfo_Character_MoveToPosition : ExtraInfo
{
    public ExtraInfo_Character_MoveToPosition(Vector2 pos)
    {
        this.position = pos;
    }
    public Vector2 position;
}
public class ExtraInfo_Character_FindItem : ExtraInfo
{
    public ExtraInfo_Character_FindItem(int item)
    {
        this.item = item;
    }
    public int item;
}


public class Telegram
{
    //the entity that sent this telegram
    public int Sender;

    //the entity that is to receive this telegram
    public int Receiver;

    //the message itself. These are all enumerated in the file
    //"MessageTypes.h"
    public int Msg;

    //messages can be dispatched immediately or delayed for a specified amount
    //of time. If a delay is necessary this field is stamped with the time 
    //the message should be dispatched.
    public double DispatchTime;

    //any additional information that may accompany the message
    public ExtraInfo extraInfo;

    public Telegram() 
    {
        DispatchTime = -1;
        Sender = -1;
        Receiver = -1;
        Msg = -1;
    }

    public Telegram(double time, int sender, int receiver, int msg, ExtraInfo extraInfo = null)
    {
        DispatchTime = time;
        Sender = sender;
        Receiver = receiver;
        Msg = msg;
        this.extraInfo = extraInfo;
    }

    const double SmallestDelay = 0.25;

    public static bool operator ==(Telegram a, Telegram b)
    {
        return (Mathf.Abs((float)(a.DispatchTime - b.DispatchTime)) < SmallestDelay) &&
        (a.Sender == b.Sender) &&
        (a.Receiver == b.Receiver) &&
        (a.Msg == b.Msg);
    }

    public static bool operator !=(Telegram a, Telegram b)
    {
        return !(a==b);
    }

    public override bool Equals(object other)
    {
        return this == (Telegram)other;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static bool operator >(Telegram a, Telegram b)
    {
        return a.DispatchTime > b.DispatchTime;
    }

    public static bool operator <(Telegram a, Telegram b)
    {
        return a.DispatchTime < b.DispatchTime;
    }


}
