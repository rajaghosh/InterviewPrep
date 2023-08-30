using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    /*
    Difference between virtual and non-virtual methods
        We have two classes; one is a "Vehicle" class and another is a "Cart" class. 
    The "Vehicle" class is the base class that has two methods; one is a virtual method "Speed()" and another 
    is a non-virtual method "Average()". So the base class virtual method "Speed()" is overriden in the sub class. 
    We have one more class "Virtual2" (the execution class) that has an entry point where we create 
    an instance of sub class "Cart" and that instance is assigned to the base class "Vehicle" type. 
    When we call virtual and non-virtual methods by both class's instance then according to the run 
    type the instance virtual method implementation is invoked; in other words both class's instances 
    invoke the subclass override method and the non-virtual method invoked is determined based on the instance of the class.
    */
    class Virtual2
    {
        public Virtual2()
        {
            double distance, hour, fuel = 0.0;

            Console.WriteLine("Enter the Distance");
            distance = Double.Parse(Console.ReadLine());
            
            Console.WriteLine("Enter the Hours");
            hour = Double.Parse(Console.ReadLine());

            Console.WriteLine("Enter the Fuel");
            fuel = Double.Parse(Console.ReadLine());
            
            Car objCar = new Car(distance, hour, fuel);
            Vehicle objVeh = objCar;
            
            objCar.Average();
            objVeh.Average();

            objCar.Speed();
            objVeh.Speed();
            Console.Read();
        }
    }

    class Vehicle
    {
        public double distance = 0.0;
        public double hour = 0.0;
        public double fuel = 0.0;

        public Vehicle(double distance, double hour, double fuel)
        {
            this.distance = distance;
            this.hour = hour;
            this.fuel = fuel;
        }

        public void Average()
        {
            double average = 0.0;
            average = distance / fuel;
            Console.WriteLine("Vehicle Average is {0:0.00}", average);
        }

        public virtual void Speed()
        {
            double speed = 0.0;
            speed = distance / hour;
            Console.WriteLine("Vehicle Speed is {0:0.00}", speed);
        }
    }

    class Car : Vehicle
    {
        public Car(double distance, double hour, double fuel)
            : base(distance, hour, fuel)
        {
        }
        public new void Average()
        {
            double average = 0.0;
            average = distance / fuel;
            Console.WriteLine("Car Average is {0:0.00}", average);
        }

        public override void Speed()
        {
            double speed = 0.0;
            speed = distance / hour;
            Console.WriteLine("Car Speed is {0:0.00}", speed);
        }
    }


}
