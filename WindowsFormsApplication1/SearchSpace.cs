using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class SearchSpace
    {
        private List<int> currRoute; //array of city indices
        private double lowerBound; //best possible solution from this space
        private int depthRemaining; //supposedly, this = total edges in graph - edges in currRoute
                                    //Suggested we make priority a combo of this and lowerBound
        private double[,] costMatrix;
        private int size;

        //Need this for the very first searchSpace created. 
        //costMatrix passed in to the first one will be the unreduced graph matrix
        public SearchSpace(List<int> prevRoute, double lb, double[,] prevMatrix, int cityVisiting, int size)
        {
            //deep copy of prevRoute
            currRoute = new List<int>(prevRoute.Count + 1);
            currRoute.AddRange(prevRoute);
            currRoute.Add(cityVisiting);
            this.size = size;

            reduceMatrix(lb, prevMatrix, cityVisiting);
        }

        public SearchSpace(SearchSpace prevSS, int cityVisiting) : this(prevSS.Route, prevSS.Bound, prevSS.Matrix, cityVisiting, prevSS.size)
        { }

        //Reduces the given matrix and sets the lowerbound
        private void reduceMatrix(double prevLB, double[,] prevMatrix, int cityVisiting)
        {
            //perform deep copy of prevMatrix before editing
            costMatrix = new double[prevMatrix.GetLength(0), prevMatrix.GetLength(1)]; //getLength(n) returns length of nth dimension
            Array.Copy(prevMatrix, costMatrix, prevMatrix.Length);
            
            double[] rowMin = new double[size];
            double[] colMin = new double[size];
            //reduce rows and find minimum in columns
            for (int i = 0; i < size; i++)
            {
                rowMin[i] = double.PositiveInfinity;
                //get rowMin[i]
                for (int j = 0; j < size; j++)
                {
                    if (costMatrix[i, j] < rowMin[i])
                        rowMin[i] = costMatrix[i, j];
                }
                //subtract rowMin from row (if rowMin isn't infinity or 0). Also get colMin[j]
                for (int j = 0; j < size; j++)
                {
                    if (!double.IsPositiveInfinity(rowMin[i]) && !(rowMin[i] == 0))
                    {
                        costMatrix[i, j] -= rowMin[i];
                        if (j == 0) //only do this on the first run, or else you add 5*rowMin[i] to the LB
                            lowerBound += rowMin[i];
                    }
                    if (i == 0) //initialize colMin[j] to infinity on first pass
                        colMin[j] = double.PositiveInfinity;
                    if (costMatrix[i, j] < colMin[j])
                        colMin[j] = costMatrix[i, j];
                }
            }

            //reduce columns if necessary
            for (int j = 0; j < size; j++)
            {
                if (!double.IsPositiveInfinity(colMin[j]) && !(colMin[j] == 0))
                {
                    lowerBound += colMin[j];
                    for (int i = 0; i < size; i++)
                    {
                        {
                            costMatrix[i, j] -= colMin[j];
                        }
                    }
                }
            }
            
            //stub
            depthRemaining = 0;
        }


        // These are C# accessor functions. Make get/set easier
        public List<int> Route
        {
            get { return currRoute;  }
        }

        public double[,] Matrix
        {
            get { return costMatrix; }
        }

        public double Bound
        {
            get { return lowerBound; }
        }
    }

}
