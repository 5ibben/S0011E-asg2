using System.Collections.Generic;
using UnityEngine;

public class IndexedPriorityQueueLow
{
    List<double>  m_vecKeys;
    List<int> m_Heap;
    List<int> m_invHeap;

    int m_iSize;
    int m_iMaxSize;

    void Swap(int a, int b)
    {
        int temp = m_Heap[a]; m_Heap[a] = m_Heap[b]; m_Heap[b] = temp;

        //change the handles too
        m_invHeap[m_Heap[a]] = a; m_invHeap[m_Heap[b]] = b;
    }

    void ReorderUpwards(int nd)
    {
        //move up the heap swapping the elements until the heap is ordered
        while ((nd > 1) && (m_vecKeys[m_Heap[nd / 2]] > m_vecKeys[m_Heap[nd]]))
        {
            Swap(nd / 2, nd);

            nd /= 2;
        }
    }

    void ReorderDownwards(int nd, int HeapSize)
    {
        //move down the heap from node nd swapping the elements until
        //the heap is reordered
        while (2 * nd <= HeapSize)
        {
            int child = 2 * nd;

            //set child to smaller of nd's two children
            if ((child < HeapSize) && (m_vecKeys[m_Heap[child]] > m_vecKeys[m_Heap[child + 1]]))
            {
                ++child;
            }

            //if this nd is larger than its child, swap
            if (m_vecKeys[m_Heap[nd]] > m_vecKeys[m_Heap[child]])
            {
                Swap(child, nd);

                //move the current node down the tree
                nd = child;
            }

            else
            {
                break;
            }
        }
    }

    public List<double> GetKeys()
    {
        return m_vecKeys;
    }

    //you must pass the constructor a reference to the std::vector the PQ
    //will be indexing into and the maximum size of the queue.
    public IndexedPriorityQueueLow(List<double> keys, int MaxSize)
    {
        m_vecKeys = keys;
        m_iMaxSize = MaxSize;
        m_iSize = 0;
        m_Heap = new List<int>(MaxSize + 1);
        for (int i = 0; i < m_Heap.Capacity; i++)
        {
            m_Heap.Add(0);
        }
        m_invHeap = new List<int>(MaxSize + 1);
        for (int i = 0; i < m_invHeap.Capacity; i++)
        {
            m_invHeap.Add(0);
        }
    }

    public bool empty(){return (m_iSize==0);}

    //to insert an item into the queue it gets added to the end of the heap
    //and then the heap is reordered from the bottom up.
    public void insert(int idx)
    {
        Debug.Assert(m_iSize + 1 <= m_iMaxSize, "m_iSize + 1 <= m_iMaxSize");

        ++m_iSize;

        m_Heap[m_iSize] = idx;

        m_invHeap[idx] = m_iSize;

        ReorderUpwards(m_iSize);
    }

    //to get the min item the first element is exchanged with the lowest
    //in the heap and then the heap is reordered from the top down. 
    public int Pop()
    {
        Swap(1, m_iSize);

        ReorderDownwards(1, m_iSize - 1);

        return m_Heap[m_iSize--];
    }

    //if the value of one of the client key's changes then call this with 
    //the key's index to adjust the queue accordingly
    public void ChangePriority(int idx)
    {
        ReorderUpwards(m_invHeap[idx]);
    }
}
