using System.Collections.Generic;

public class PathManager
{
    //a container of all the active search requests
    List<PathPlanner> m_SearchRequests = new List<PathPlanner>();

    public PathManager()
    {
    }

    //every time this is called the total amount of search cycles available will
    //be shared out equally between all the active path requests. If a search
    //completes successfully or fails the method will notify the relevant bot
    public void UpdateSearches()
    {
        uint NumCyclesRemaining = Config.timeSliceUpdates;

        //iterate through the search requests until either all requests have been
        //fulfilled or there are no search cycles remaining for this update-step.
        int curPath = m_SearchRequests.Count-1;
        while (0 < NumCyclesRemaining-- && curPath >= 0)
        {
            //make one search cycle of this path request
            int result = m_SearchRequests[curPath].CycleOnce();

            //if the search has terminated remove it from the list
            if ((result == (int)search_status.target_found) || result == (int)search_status.target_not_found)
            {
                m_SearchRequests.RemoveAt(curPath);
            }
            //if there's more updates in the tank we loop it
            if (--curPath < 0)
            {
                curPath = m_SearchRequests.Count - 1;
            }
        }
    }

    //a path planner should call this method to register a search with the 
    //manager. (The method checks to ensure the path planner is only registered
    //once)
    public void Register(PathPlanner pPathPlanner)
    {
        //make sure the bot does not already have a current search in the queue
        if (!m_SearchRequests.Contains(pPathPlanner))
        {
            //add to the list
            m_SearchRequests.Add(pPathPlanner);
            //Debug.Log("current search requests: " + m_SearchRequests.Count);
        }
    }

    public void UnRegister(PathPlanner pPathPlanner)
    {
        m_SearchRequests.Remove(pPathPlanner);
    }

    //returns the amount of path requests currently active.
    public int GetNumActiveSearches(){return m_SearchRequests.Count;}
}
