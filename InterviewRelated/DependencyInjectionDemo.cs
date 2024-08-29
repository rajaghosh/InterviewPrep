using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewRelated
{
    //DEPENDENCY INVERSION PRINCIPLE (DIP)
    //Between 2 classes One will be calling other class. That is CALLER CLASS and CALLED CLASS.
    //Now as such there will be coupling between these two classes. That is huge dependency.
    //If the CALLED CLASS gets changed then we might have to modify the CALLING CLASS or
    //If some blocking happens in CALLED CLASS this will impact the CALLING CLASS too. 
    //So to remove this dependency we need to decouple (with very limited dependency).
    //This is called the DEPENDENCY INVERSION PRINCIPLE. To be precise:
    //1. High-level modules should not depend on low-level modules.Both should depend on the abstraction.
    //2. Abstractions should not depend on details. Details should depend on abstractions.


    //INVERSION OF CONTROL (IOC)
    //Once the decoupling happens then how we are going to connect these classes making them loosely coupled.
    //So to reduce the dependency we would try to invert the dependency flow or control of the flow
    //from "CALLER -> CALLED" to "CALLED -> CALLER". So now the CALLED CLASS will determine how the flow should be.
    //And the CALLER CLASS do not need to know about the flow.

    //DEPENDENCY INJECTION
    //As we will be inverting the control we somehow need to hook the CALLED CLASS to the CALLER CLASS. This hooking
    //will be done by DEPENDENCY INJECTION. That is creating an abstract class point in the CALLER CLASS which will be invoked
    //only when needed. As it is an abstract class so the CALLER CLASS do not need to have any knowledge of the CALLED CLASS.
    //But this is forms a dependency so we call it DEPENDENCY INJECTION or a Dependency is hooked.

    //Dependency injection is the most popular method of implementing IOC. There are mainly three types of dependency injection:
    //Property Injection
    //Constructor Injection
    //Method Injection


    interface IEvent
    {
        public void LoadEventDetails();
    }
    class TechEvents : IEvent
    {
        public void LoadEventDetails()
        {
            Console.WriteLine("Tech Events");
        }
    }
    class SportsEvents : IEvent
    {
        public void LoadEventDetails()
        {
            Console.WriteLine("Sports Events");
        }
    }

    class PartyEvents: IEvent
    {
        public void LoadEventDetails()
        {
            Console.WriteLine("Party Events");
        }
    }

    //Dependency Injection Using Constructors - As needed College1 Class will be invoked and used
    class College1
    {
        private IEvent _iEvent;
        public College1(IEvent ie)
        {
            _iEvent = ie;
        }

        public void GetEvents()
        {
            this._iEvent.LoadEventDetails();
        }
    }

    //Dependency Injection Using Property - As needed College2 Class will be invoked and used
    class College2
    {
        private IEvent _iEvent;
        public IEvent MyEvent
        {
            //get => _iEvent;
            set => _iEvent = value;
        }
        public void GetEvents()
        {
            this._iEvent.LoadEventDetails();
        }
    }


    class College3
    {
        private IEvent _iEvent;
        public void GetEvent(IEvent ie)
        {
            _iEvent = ie;
            _iEvent.LoadEventDetails();
        }
    }


    class DependencyInjectionDemo
    {
        public void DependencyInjectionTest() 
        {
            //Constructor Injection - In the constructor we will be sending the class instance informtion
            College1 clg1 = new College1(new TechEvents());
            clg1.GetEvents();

            //Property Injection - In the property we will be sending the class instance information
            College2 clg2 = new College2();
            clg2.MyEvent = new SportsEvents();
            clg2.GetEvents();

            //Method Injection - In the methd we will be sending the class instance information
            College3 clg3 = new College3();
            clg3.GetEvent(new PartyEvents());

        }

    }
}
