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
        private List<int> citiesRemaining;
        private double lowerBound; //best possible solution from this space
        private double[,] costMatrix;
        private int size;

        //Need this for the very first searchSpace created. 
        //costMatrix passed in to the first one will be the unreduced graph matrix
        public SearchSpace(List<int> prevRoute, List<int> prevCitiesRemaining, double prevLB, double[,] prevMatrix, int cityVisiting, int size)
        {
            //deep copy of prevRoute. Note that cityVisiting is added in visitCity f(x)
            currRoute = new List<int>(prevRoute.Count + 1);
            currRoute.AddRange(prevRoute);

            //deep copy of prevCitiesRemaining. Note that cityVisiting is removed in visitCity f(x)
            citiesRemaining = new List<int>(prevCitiesRemaining.Count);
            citiesRemaining.AddRange(prevCitiesRemaining);

            this.size = size;
            this.lowerBound = prevLB;

            reduceMatrix(prevMatrix, cityVisiting);
        }

        public SearchSpace(SearchSpace prevSS, int cityVisiting) : this(prevSS.Route, prevSS.citiesRemaining, prevSS.Bound, prevSS.Matrix, cityVisiting, prevSS.size)
        { }

        //Reduces the given matrix and sets the lowerbound
        //O(n^2)
        private void reduceMatrix(double[,] prevMatrix, int cityVisiting)
        {
            //perform deep copy of prevMatrix before editing
            costMatrix = new double[prevMatrix.GetLength(0), prevMatrix.GetLength(1)]; //getLength(n) returns length of nth dimension
            Array.Copy(prevMatrix, costMatrix, prevMatrix.Length);
            
            visitCity(cityVisiting);

            double[] rowMin = new double[size];
            double[] colMin = new double[size];
            //reduce rows and find minimum in columns
            for (int r = 0; r < size; r++)
            {
                rowMin[r] = double.PositiveInfinity;
                //get rowMin[i]
                for (int c = 0; c < size; c++)
                {
                    if (costMatrix[r, c] < rowMin[r])
                        rowMin[r] = costMatrix[r, c];
                }
                //subtract rowMin from row (if rowMin isn't infinity or 0). Also get colMin[j]
                for (int c = 0; c < size; c++)
                {
                    if (!double.IsPositiveInfinity(rowMin[r]) && !(rowMin[r] == 0))
                    {
                        costMatrix[r, c] -= rowMin[r];
                        if (c == 0) //only do this on the first run, or else you add 5*rowMin[i] to the LB
                            lowerBound += rowMin[r];
                    }
                    if (r == 0) //initialize colMin[j] to infinity on first pass
                        colMin[c] = double.PositiveInfinity;
                    if (costMatrix[r, c] < colMin[c])
                        colMin[c] = costMatrix[r, c];
                }
            }

            //reduce columns if necessary
            for (int c = 0; c < size; c++)
            {
                if (!double.IsPositiveInfinity(colMin[c]) && !(colMin[c] == 0))
                {
                    lowerBound += colMin[c];
                    for (int r = 0; r < size; r++)
                    {
                        {
                            costMatrix[r, c] -= colMin[c];
                        }
                    }
                }
            }
        }
        
        private void visitCity(int cityVisiting)
        {
            int cityFrom;
            citiesRemaining.Remove(cityVisiting);

            try
            {
                cityFrom = currRoute.Last();
            }
            catch (InvalidOperationException) //currRoute had nothing in it, you can skip this f(x) but still need cityVisiting in currRoute
            {
                return;
            }
            finally
            {
                currRoute.Add(cityVisiting);
            }

            lowerBound += costMatrix[cityFrom, cityVisiting];

            //infinity out the row for the city you're coming from
            for (int c = 0; c < size; c++)
            {
                costMatrix[cityFrom, c] = double.PositiveInfinity;
            }

            //infinity out the column for the city you're visiting
            for (int r = 0; r < size; r++)
            {
                costMatrix[r, cityVisiting] = double.PositiveInfinity;
            }

            //infinity out the symmetrical cell (as if visiting "cityFrom" from "cityVisiting")
            costMatrix[cityVisiting, cityFrom] = double.PositiveInfinity;
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

        public List<int> CitiesRemaining
        {
            get { return citiesRemaining; }
        }

        public int DepthRemaining
        {
            get { return citiesRemaining.Count; }
        }
    }

}
