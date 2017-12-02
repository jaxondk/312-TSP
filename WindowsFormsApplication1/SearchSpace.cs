﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP
{
    class SearchSpace
    {
        private List<int> currRoute = new List<int>(); //array of city indices
        private double lowerBound; //best possible solution from this space
        private int depthRemaining; //supposedly, this = total edges in graph - edges in currRoute
                                    //Suggested we make priority a combo of this and lowerBound
        private double[,] costMatrix;

        //Need this for the very first searchSpace created. 
        //costMatrix passed in to the first one will be the unreduced graph matrix
        public SearchSpace(List<int> prevRoute, double lb, double[,] prevMatrix, int cityVisiting)
        {
            currRoute = prevRoute;
            currRoute.Add(cityVisiting);

            //perform deep copy of prevMatrix before editing
            costMatrix = new double[prevMatrix.GetLength(0), prevMatrix.GetLength(1)]; //getLength(n) returns length of nth dimension
            Array.Copy(prevMatrix, costMatrix, prevMatrix.Length);

            costMatrix = reduceMatrix(lb);
        }

        public SearchSpace(SearchSpace prevSS, int cityVisiting) : this(prevSS.Route, prevSS.Bound, prevSS.Matrix, cityVisiting)
        { }

        //Reduces the given matrix and sets the lowerbound
        private double[,] reduceMatrix(double prevLB) 
        {

            //stub
            lowerBound = 0;
            depthRemaining = 0;
            return null;
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
