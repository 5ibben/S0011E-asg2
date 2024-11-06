
public class CharacterStateGlobal : State<Character>
{
    CharacterStateGlobal() { }

    //this is a singleton
    private static CharacterStateGlobal instance = null;
    public static CharacterStateGlobal Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateGlobal();
            }
            return instance;
        }
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        switch (msg.Msg)
        {
            case (int)messages.Msg_PathReady:
                {
                    character.OnPathReady();
                    return true;
                }
            case (int)messages.Msg_Character_MoveToPosition:
                {
                    character.moveDestination = ((ExtraInfo_Character_MoveToPosition)msg.extraInfo).position;
                    character.stateMachine.ChangeState(CharacterStateFindDestination.Instance);
                    return true;
                }
            case (int)messages.Msg_Character_FindItem:
                {
                    character.currentItemSearch = ((ExtraInfo_Character_FindItem)msg.extraInfo).item;
                    character.stateMachine.ChangeState(CharacterStateFindItem.Instance);
                    return true;
                }
            default:
                break;
        }
        return false;
    }
}

public class CharacterStateIdle : State<Character>
{
    CharacterStateIdle() { }

    //this is a singleton
    private static CharacterStateIdle instance = null;
    public static CharacterStateIdle Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateIdle();
            }
            return instance;
        }
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        if (msg.Msg == (int)messages.Msg_GraphChanged)
        {
            return true;
        }
        return false;
    }
}

public class CharacterStateFindDestination : State<Character>
{
    CharacterStateFindDestination() { }

    //this is a singleton
    private static CharacterStateFindDestination instance = null;
    public static CharacterStateFindDestination Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateFindDestination();
            }
            return instance;
        }
    }
    public override void Enter(Character character)
    {
        character.RequestMove();
    }

    public override void Execute(Character character)
    {
        if (!character.is_moving)
        {
            character.stateMachine.ChangeState(CharacterStateIdle.Instance);
        }
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        if (msg.Msg == (int)messages.Msg_GraphChanged)
        {
            character.RequestMove();
            return true;
        }
        return false;
    }
}

public class CharacterStateFindItem : State<Character>
{
    CharacterStateFindItem() { }

    //this is a singleton
    private static CharacterStateFindItem instance = null;
    public static CharacterStateFindItem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CharacterStateFindItem();
            }
            return instance;
        }
    }
    public override void Enter(Character character)
    {
        character.FindItem();
    }

    public override void Execute(Character character)
    {
        if (!character.is_moving)
        {
            character.stateMachine.ChangeState(CharacterStateIdle.Instance);
        }
    }

    public override bool OnMessage(Character character, Telegram msg)
    {
        if (msg.Msg == (int)messages.Msg_GraphChanged)
        {
            character.FindItem();
            return true;
        }
        else if (msg.Msg == (int)messages.Msg_GraphItemChanged)
        {
            character.FindItem();
            return true;
        }
        return false;
    }
}