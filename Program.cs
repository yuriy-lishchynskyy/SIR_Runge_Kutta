using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Assignment3
{
    public delegate Vector ODESystem(Vector inputs); // delegate with signature for passing systems of equations into Runge-Kutta class

    class Program
    {
        static void Main(string[] args)
        {
            Vector init_values = new Vector(new double[] { 0.99, 0.01, 0.00 }); // vector of starting values

            RungeKutta r = new RungeKutta(init_values);
            r.Stepsize = 1;
            r.T_final = 500;

            r.Solve4(SystemEquationStore.SIR);

            string filename = "...//output.csv";
            r.WriteCSV(filename);
        }
    }

    class SystemEquationStore
    {
        public static Vector SIR(Vector inputs) // take in vector of current values of S, I, R and output dS/dt, dI/dt and dR/dt
        {
            double r0 = 2.4;
            double gamma = 1.0 / 14.0; // define constants
            double beta = r0 * gamma;

            double Sj = inputs[0]; // store current values of state equations
            double Ij = inputs[1];
            double Rj = inputs[2];

            Vector dSIR = new Vector(3);

            dSIR[0] = -(beta * Ij * Sj);
            dSIR[1] = (beta * Ij * Sj) - (gamma * Ij);
            dSIR[2] = gamma * Ij;

            return dSIR;
        }
    }

    class RungeKutta
    {
        // data
        private int n_steps;
        private double stepsize = 0.1;
        private Vector initial_values;
        private double t_final = 100;
        private int calc_e = 0;

        private double[] xvals; // store x = t
        private Vector[] yvals; // store y's

        public double Stepsize
        {
            get
            {
                return stepsize;
            }
            set
            {
                if (value > 0 & value <= t_final)
                {
                    stepsize = value;
                    n_steps = Convert.ToInt32(t_final / stepsize);
                    this.xvals = new double[n_steps + 1];
                    this.yvals = new Vector[n_steps + 1];
                }
                else
                {
                    string e = String.Format("ERROR: Invalid Step Size selected ({0}) - must be positive and less than Final Time ({1})", value, t_final);
                    throw new Exception(e);
                }
            }
        }

        public Vector Initial_values { get => initial_values; set => initial_values = value; }
        public double T_final
        {
            get
            {
                return t_final;
            }
            set
            {
                if (value > 0 & value > stepsize)
                {
                    t_final = value;
                    n_steps = Convert.ToInt32(t_final / stepsize);
                    this.xvals = new double[n_steps + 1];
                    this.yvals = new Vector[n_steps + 1];
                }
                else
                {
                    string e = String.Format("ERROR: Invalid Final Time selected ({0}) - must be positive and greater than Step Size ({1})", value, stepsize);
                    throw new Exception(e);
                }
            }
        }

        // constructors
        public RungeKutta()
        {

        }

        public RungeKutta(Vector values0) // initialise object with vector of starting values (length of which infers no. system equations)
        {
            this.n_steps = Convert.ToInt32(this.t_final / this.stepsize);

            this.xvals = new double[n_steps + 1];
            this.yvals = new Vector[n_steps + 1];

            if (values0.sum() != 1) // must be valid starting values for S/I/R (proportions add up to 1)
            {
                string e = String.Format("Initial values of S, I, R do not add up to 1.0");
                throw new Exception(e);
            }
            else
            {
                this.initial_values = values0;
            }
        }

        // methods
        public void Solve4(ODESystem system)
        {
            calc_e = 0;

            try
            {
                int m = this.initial_values.Length; // size of system (no. of equations)

                double xj = 0; // value of current t (starting value = 0)
                double xj1; // value of next t

                Vector yj = this.initial_values; // vector of current state of system (starting value = input)
                Vector yj1 = new Vector(m); // vector of next state of system

                xvals[0] = xj;
                yvals[0] = yj;

                Vector k1 = new Vector(m); // vector of k1/2/3/4 values for each system variable
                Vector k2 = new Vector(m);
                Vector k3 = new Vector(m);
                Vector k4 = new Vector(m);

                for (int i = 1; i < n_steps + 1; i++)
                {
                    k1 = system(yj);
                    k2 = system(yj + ((this.stepsize / 2) * k1));
                    k3 = system(yj + ((this.stepsize / 2) * k2));
                    k4 = system(yj + (this.stepsize * k3));

                    xj1 = xj + this.stepsize;
                    yj1 = yj + (this.stepsize / 6) * (k1 + (2 * k2) + (2 * k3) + k4);

                    xvals[i] = xj1;
                    yvals[i] = yj1;

                    xj = xj1; // set "next" state of system as "current" state for next loop
                    yj = yj1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message); // print error message
                calc_e = 1;
            }
        }

        public void WriteCSV(string location)
        {
            StreamWriter sw = null;
            int n_data = xvals.Length; // how much data to write

            Console.WriteLine("\nWRITING DATA TO CSV");
            Console.WriteLine("-------------------");

            try // enclose problematic code in try block to throw exception if any part fails
            {
                sw = new StreamWriter(location);

                string content = "t,"; // string to store content to be written to csv row
                Console.Write("t    ");

                for (int j = 0; j < yvals[0].Length; j++) // create header row (t and m no. y's - depending on size of system)
                {
                    content += "y" + (j + 1).ToString() + ",";
                    Console.Write("y{0}   ", j + 1);
                }

                sw.WriteLine(content); // write header to csv

                for (int i = 0; i < n_data; i++)
                {
                    Vector ydata = yvals[i];
                    content = "";

                    Console.Write("\n{0:0.00}", xvals[i]);

                    content = xvals[i].ToString() + ","; // add x-value (t)

                    for (int j = 0; j < ydata.Length; j++) 
                    {
                        content += ydata[j].ToString() + ",";
                        Console.Write(" {0:0.00}", ydata[j]); // add y-values
                    }

                    sw.WriteLine(content); // write data row to csv
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message); // print error message
            }
            finally
            {
                if (sw != null) // only close file if no errors after writing file
                {
                    sw.Close();
                    Console.WriteLine("\n-------------------------------");

                    if (calc_e != 1)
                    {
                        Console.WriteLine("DATA EXPORT TO CSV: SUCCESSFUL");
                    }
                    else
                    {
                        Console.WriteLine("DATA EXPORT TO CSV: UNSUCCESSFUL");
                    }
                }
            }
        }
    }

    public class Vector
    {
        // data
        private double[] data;
        private int length;

        public int Length { get => this.length; }

        // constructors
        public Vector()
        {

        }

        public Vector(int l) // create zero vector of length l
        {
            if (l > 0)
            {
                this.length = l;
                this.data = new double[this.length];

                for (int i = 0; i < this.length; i++)
                {
                    this.data[i] = 0;
                }
            }
            else
            {
                Console.WriteLine("Invalid input for Vector length");
            }
        }

        public Vector(double[] a) // create vector from an array
        {
            this.length = a.GetLength(0);
            this.data = new double[this.length];

            for (int i = 0; i < this.length; i++)
            {
                this.data[i] = a[i];
            }
        }

        public Vector(Vector a) // create vector from another vector
        {
            this.length = a.Length;
            this.data = new double[this.length];

            for (int i = 0; i < this.length; i++)
            {
                this.data[i] = a[i];
            }
        }

        // overloaded operators
        public double this[int i] // indexing operator
        {
            get
            {
                return this.data[i];
            }
            set
            {
                this.data[i] = value;
            }
        }

        public static Vector operator +(Vector a, Vector b) // addition of 2 vectors
        {
            int i;

            int a_l = a.Length;
            int b_l = b.Length;

            if (a_l != b_l) // check if vectors same size
            {
                Console.WriteLine("Cannot add vectors: Lengths not equal!");
                return null;
            }

            Vector temp = new Vector(a_l);

            for (i = 0; i < a_l; i++) // add corresponding elements
            {
                temp[i] = a[i] + b[i];
            }

            return temp;
        }

        public static Vector operator -(Vector a, Vector b) // subtraction of 2 vectors
        {
            int i;

            int a_l = a.Length;
            int b_l = b.Length;

            if (a_l != b_l) // check if vectors same size
            {
                Console.WriteLine("Cannot subtract vectors: Lengths not equal!");
                return null;
            }

            Vector temp = new Vector(a_l);

            for (i = 0; i < a_l; i++) // subtract corresponding elements
            {
                temp[i] = a[i] - b[i];
            }

            return temp;
        }

        public static double operator *(Vector a, Vector b) // dot product multiplication of 2 vectors
        {
            int i;

            int a_l = a.Length;
            int b_l = b.Length;

            if (a_l != b_l) // check if vectors same size
            {
                Console.WriteLine("Cannot dot multiply vectors: Lengths not equal!");
                return 0;
            }

            double sum = 0;

            for (i = 0; i < a_l; i++) // running sum of multiplied terms
            {
                sum = sum + (a[i] * b[i]);
            }

            return sum;
        }

        public static Vector operator *(Vector a, double x) // multiplication of vector by constant
        {
            int i;
            int a_l = a.Length;

            Vector temp = new Vector(a_l);

            for (i = 0; i < a_l; i++) // scale each term by constant
            {
                temp[i] = a[i] * x;
            }

            return temp;
        }

        public static Vector operator *(double x, Vector a) // multiplication of constant by vector
        {
            int i;
            int a_l = a.Length;

            Vector temp = new Vector(a_l);

            for (i = 0; i < a_l; i++) // scale each term by constant
            {
                temp[i] = a[i] * x;
            }

            return temp;
        }

        // methods
        public double sum()
        {
            double sum = 0;

            for (int i = 0; i < this.length; i++)
            {
                sum += this.data[i];
            }

            return sum;
        }
    }
}
