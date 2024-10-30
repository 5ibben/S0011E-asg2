using System.Collections.Generic;
using UnityEngine;

class MessageDispatcher
{
    public const double SEND_MSG_IMMEDIATELY = 0.0;
    public const int NO_ADDITIONAL_INFO = 0;
    public const int SENDER_ID_IRRELEVANT = -1;

    static SortedSet<Telegram> PriorityQ = new SortedSet<Telegram>();

    //This method calls the message handling member function of the receiving entity, pReceiver, with the newly created telegram
    static void Discharge(BaseGameEntity pReceiver, Telegram msg) 
    {
        if (!pReceiver.HandleMessage(msg))
        {
            //telegram could not be handled
            Debug.Log("Message not handled");
        }
    }

    public static void Flush()
    {
        PriorityQ = new SortedSet<Telegram>();
    }

    MessageDispatcher() 
    {
        
    }

    //this class is a singleton
    private static MessageDispatcher instance = null;
    public static MessageDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MessageDispatcher();
            }
            return instance;
        }
    }

    //send a message to another agent. Receiving agent is referenced by ID.
    public static void DispatchMessage(double delay,int sender,int receiver,int msg, ExtraInfo extraInfo = null)
    {
        //get pointers to the sender and receiver
        BaseGameEntity pSender = EntityManager.Instance.GetEntityFromID(sender);
        BaseGameEntity pReceiver = EntityManager.Instance.GetEntityFromID(receiver);

        //make sure the receiver is valid
        if (pReceiver == null)
        {
            Debug.Log("Warning! No Receiver with ID of " + receiver + " found");
            return;
        }

        //create the telegram
        Telegram telegram = new Telegram(0, sender, receiver, msg, extraInfo);

        //if there is no delay, route telegram immediately                       
        if (delay <= 0.0f)
        {
            //send the telegram to the recipient
            Discharge(pReceiver, telegram);
        }

        //else calculate the time when the telegram should be dispatched
        else
        {
            double CurrentTime = Time.time;

            telegram.DispatchTime = CurrentTime + delay;

            //and put it in the queue
            PriorityQ.Add(telegram);
        }

    }

    //send out any delayed messages. This method is called each time through the main game loop.
    public void DispatchDelayedMessages() 
    {
        //get current time
        double CurrentTime = Time.time;

        //now peek at the queue to see if any telegrams need dispatching. remove all telegrams from the front of the queue that have gone past their sell by date
        while (PriorityQ.Count != 0 && (PriorityQ.Min.DispatchTime < CurrentTime) && (PriorityQ.Min.DispatchTime > 0))
        {
            //read the telegram from the front of the queue
            Telegram telegram = PriorityQ.Min;

            //find the recipient
            BaseGameEntity pReceiver = EntityManager.Instance.GetEntityFromID(telegram.Receiver);

            //send the telegram to the recipient
            Discharge(pReceiver, telegram);

            //remove it from the queue
            PriorityQ.Remove(PriorityQ.Min);
        }
    }
};
