using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Linq;


namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public List<int>
                Route;

            private City[] Cities;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(List<int> iroute, City[] cities)
            {
                Route = new List<int>(iroute);
                this.Cities = cities;
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                City there;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Cities[Route[x]];
                    there = Cities[Route[x + 1]];
                    cost += here.costToGetTo(there);
                }

                // go from the last city to the first. 
                here = Cities[Route.Last()];
                there = Cities[Route.First()];
                cost += here.costToGetTo(there);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private List<int> Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        private int statesStoredMax = 0;
        private int solutionsFound = 0;
        private int statesCreated = 0;
        private int statesPruned = 0;

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new List<int>(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (int c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+Cities[c].costToGetTo(Cities[bssf.Route[index+1]])+")", labelFont, cityBrushStartStyle, new PointF((float)Cities[c].X * width + 3F, (float)Cities[c].Y * height));
                    else 
                        g.DrawString(" " + index +"("+ Cities[c].costToGetTo(Cities[bssf.Route[0]])+")", labelFont, cityBrushStartStyle, new PointF((float)Cities[c].X * width + 3F, (float)Cities[c].Y * height));
                    ps[index++] = new Point((int)(Cities[c].X * width) + CITY_ICON_SIZE / 2, (int)(Cities[c].Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new List<int>();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(perm[i]);
                }
                bssf = new TSPSolution(Route, Cities);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        //gets BSSF using random cycle
        private void defaultGetBSSF()
        {
            int i, swap, temp, count = 0;
            int[] perm = new int[Cities.Length];
            Route = new List<int>();
            Random rnd = new Random();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(perm[i]);
                }
                bssf = new TSPSolution(Route, Cities);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];

            Stopwatch timer = new Stopwatch();
            timer.Start();

            double[,] graphMatrix = buildGraphMatrix();
            defaultGetBSSF(); //replace with greedy or however you want to initialize BSSF

            //build cityIndices list. Stupid but need it
            List<int> cityIndices = initCityIndices(); //O(n)

            //Build the q with the initial searchSpace having {0} as it's current route
            PriorityQ q = new PriorityQ();
            q.Makequeue(); //O(1)
            q.Insert(new SearchSpace(new List<int>(), cityIndices, 0, graphMatrix, 0, _size));  //O(log|V|)
            statesCreated++;

            //Branch and bound. 
            //O(n^2n!)
            while(q.NotEmpty() && timer.ElapsedMilliseconds < 60 * 1000) //could run O(n!) times
            {
                SearchSpace curr = q.Deletemin(); //O(log|V|)
                if (curr.Bound < costOfBssf())
                    explore(curr, q);
                else
                    statesPruned++;
            }

            timer.Stop();
            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = solutionsFound.ToString();


            statesStoredMax = q.MaxCount;
            statesPruned += q.Count;

            return results;
        }
        
        //returns number of updates to BSSF made during the exploration
        private void explore(SearchSpace current, PriorityQ q)
        {
            List<int> citiesRemaining = current.CitiesRemaining;
            bool leaf = true;
            //O()
            foreach (int city in citiesRemaining)
            {
                leaf = false;
                SearchSpace child = new SearchSpace(current, city);//O(n^2)
                statesCreated++;
                if (child.Bound < costOfBssf())
                    q.Insert(child);
                else
                    statesPruned++;
            }

            if(leaf)
            {
                TSPSolution possibleSoln = new TSPSolution(current.Route, Cities);
                if (possibleSoln.costOfRoute() < costOfBssf())
                {
                    bssf = possibleSoln;
                    solutionsFound++;
                }
            }
            
        }

        private double[,] buildGraphMatrix()
        {
            double[,] matrix = new double[_size,_size];
            //i is index of city of departure 
            for (int i = 0; i < _size; i++)
            {
                //j is index of destination city
                for (int j = 0; j < _size; j++)
                {
                    //i modified the city cost f(x) to put inifinity on the diagonals
                    matrix[i, j] = Cities[i].costToGetTo(Cities[j]);
                }
            }

            return matrix;
        }

        private List<int> initCityIndices()
        {
            List<int> indices = new List<int>(_size);
            for (int i = 0; i < _size; i++)
            {
                indices.Add(i);
            }

            return indices;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];
            int solutionsConsidered = 0;
            Stopwatch timer = new Stopwatch();

            timer.Start();
            solutionsConsidered = greedyCalcBSSF();
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = solutionsConsidered.ToString();

            return results;
        }

        //TODO - BROKEN/INCORRECT
        //returns number of solutions considered
        private int greedyCalcBSSF()
        {
            defaultGetBSSF();
            //for each starting city:
            //Route.add(startingCityIndex)
            //for cities remaining
            //get the smallest outgoing edge and add that to route
            //remove it from cities remaining
            //if this route's cost is < costBSSF
            //bssf = this route's soln

            int solutionsConsidered = 0;

            for (int i = 0; i < _size; i++)
            {
                Route.Add(i);
                List<int> citiesRemaining = initCityIndices();
                citiesRemaining.Remove(i);

                //build greedy route for starting city Cities[i]
                while (citiesRemaining.Count > 0)
                {
                    //get closest edge
                    int minCity = citiesRemaining[0];
                    double minCost = Cities[Route.Last()].costToGetTo(Cities[minCity]);
                    for (int c = 1; c < citiesRemaining.Count; c++)
                    {
                        double costToC = Cities[Route.Last()].costToGetTo(Cities[c]);
                        if (costToC < minCost)
                        {
                            minCity = c;
                            minCost = costToC;
                        }
                    }
                    //add closest edge to route and remove from remaining cities
                    Route.Add(minCity);
                    citiesRemaining.Remove(minCity);
                }

                //keep if it's better than BSSF
                TSPSolution candidate = new TSPSolution(Route, Cities);
                if (candidate.costOfRoute() < costOfBssf())
                {
                    bssf = candidate;
                    solutionsConsidered++;
                }
            }

            return solutionsConsidered;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
