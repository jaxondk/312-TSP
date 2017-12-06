using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class PriorityQ
    {
        private List<SearchSpace> q;
        private int maxCount; //for analysis only, does not affect functionality

        //O(1)
        public void Makequeue()
        {
            q = new List<SearchSpace>();
            q.Add(null); //Have the first item in the array be garbage so that it's 1-indexed
            maxCount = 0; 
        }

        //O(log|V|) from BubbleUp. Other ops are O(1)
        public void Insert(SearchSpace v)
        {
            q.Add(v);
            BubbleUp(v);
            if (Count > maxCount)
                maxCount = Count;
        }

        //O(log|V|) - I use a look up array for the Q-index, so that's O(1). 
        //There can be at most log|V| swaps (height of tree), and each swap does only O(1) ops.
        private void BubbleUp(SearchSpace v)
        {
            int Qi = q.IndexOf(v);
            int parentQi = Qi / 2;

            //swap represents if the child has higher priority than the parent
            //This is based on depth remaining unless equal. If equal, it's based on the bounds
            bool swap = (Qi == 1) ? false 
                : (q[parentQi].DepthRemaining == v.DepthRemaining) ? q[parentQi].Bound > v.Bound 
                : q[parentQi].DepthRemaining > v.DepthRemaining;
            while (swap) //while not at root and while parent's key is lower priority than inserted node's key
            {
                q[Qi] = q[parentQi]; //put parent in child's place
                Qi = parentQi; //increment current to parent's position
                parentQi = Qi / 2; //find parent of current's new position
                swap = (Qi == 1) ? false 
                    : (q[parentQi].DepthRemaining == v.DepthRemaining) ? q[parentQi].Bound > v.Bound 
                    : q[parentQi].DepthRemaining > v.DepthRemaining;
            }
            q[Qi] = v; //put v in it's appropriate position
        }

        //O(log|V|) - O(1) ops except for siftdown function. 
        //Siftdown has same complexity as bubbleUp - O(log|V|) - for same reasons
        public SearchSpace Deletemin()
        {
            SearchSpace v = q[1]; //remember, q is 1-indexed

            //******** siftdown function **********//
            SearchSpace lastV = q.Last();
            q.Remove(lastV); //trim last leaf
            if (NotEmpty())
            {
                q[1] = lastV; //put last at root
                int currQi = 1;

                //sift the root down:
                int childQi = HighestPriorityChildQi(currQi);
                bool swap = (childQi == 0) ? false 
                    : (q[childQi].DepthRemaining == lastV.DepthRemaining) ? q[childQi].Bound < lastV.Bound 
                    : q[childQi].DepthRemaining < lastV.DepthRemaining;
                while (swap) //while current has children and 
                                             //the distance of the smallest child < the distance of the previously last node
                {
                    q[currQi] = q[childQi]; //put smallest child at current position
                    currQi = childQi; //set current = smallest child
                    childQi = HighestPriorityChildQi(currQi); //get new smallest child
                    swap = (childQi == 0) ? false
                        : (q[childQi].DepthRemaining == lastV.DepthRemaining) ? q[childQi].Bound < lastV.Bound
                        : q[childQi].DepthRemaining < lastV.DepthRemaining;
                }

                q[currQi] = lastV; //put lastV in it's appropriate position
            }
            //********** end siftdown ************//

            return v;
        }

        //O(1) - all O(1) lookups or arithmetic done one time.
        private int HighestPriorityChildQi(int parentQi)
        {
            //Left child = parent index * 2 
            //Right child = left child + 1
            int c1Qi = 2 * parentQi;
            int c2Qi = c1Qi + 1;
            if (c1Qi >= q.Count)
                return 0; //no children
            else if (c2Qi >= q.Count)
                return c1Qi; //only child
            else //return highest priority child
            {
                if (q[c1Qi].DepthRemaining == q[c2Qi].DepthRemaining)
                {
                    return (q[c1Qi].Bound < q[c2Qi].Bound) ? c1Qi : c2Qi;
                }
                else
                {
                    return (q[c1Qi].DepthRemaining < q[c2Qi].DepthRemaining) ? c1Qi : c2Qi; 
                }
            }
        }

        //O(log|V|) - just calls BubbleUp
        public void OnKeyDecreased(SearchSpace v)
        {
            BubbleUp(v);
        }

        //O(1)
        public bool NotEmpty()
        {
            return q.Count > 1; //q[0] is garbage, so q empty when 1 element in it
        }

        public int Count
        {
            get { return q.Count - 1; } //-1 for the gargabe at q[0]. Need count for calculating statesPruned in B&B
        }

        public int MaxCount
        {
            get { return maxCount; }
        }
    }
    
}
