using System;
using System.Collections.Generic;
using System.Text;

namespace DesignPatterns
{
    class BridgeDesignPattern
    {
        public BridgeDesignPattern()
        {
            //Selecting Card Payment
            Payment order = new CardPayment();

            //Selecting Merchant for Payment
            order._IPaymentSystem = new CitiPaymentSystem();
            order.MakePayment();

            //Selecting Merchant for Payment
            order._IPaymentSystem = new IdbiPaymentSystem();
            order.MakePayment();

            //Selecting Net Banking
            order = new NetBankingPayment();
            order._IPaymentSystem = new CitiPaymentSystem();
            order.MakePayment();

            order._IPaymentSystem = new IdbiPaymentSystem();
            order.MakePayment();

        }
    }


    //Here this is the abstraction. Generally this portion governs the direct client interaction
    abstract class Payment
    {
        //We need to have hook to a would be implementation
        public IPaymentSystem _IPaymentSystem;
        public abstract void MakePayment();
    }

    //Wrapper class to make bridge to possible implementation and let the implementation know about the Payment Process
    class CardPayment : Payment
    {
        public override void MakePayment()
        {
            //throw new NotImplementedException();
            _IPaymentSystem.ProcessPayment("Card Payment");
        }
    }

    class NetBankingPayment : Payment
    {
        public override void MakePayment()
        {
            //throw new NotImplementedException();
            _IPaymentSystem.ProcessPayment("NetBaning Payment");
        }
    }

    //Here this is the implementation. To be determined at run time
    public interface IPaymentSystem
    {
        void ProcessPayment(string paymentSystem);
    }

    public class CitiPaymentSystem : IPaymentSystem
    {
        //The PaymentSystem info is received from the Abstract Class 
        public void ProcessPayment(string paymentSystem)
        {
            Console.WriteLine("Using CitiBank gateway for " + paymentSystem);
        }
    }

    public class IdbiPaymentSystem : IPaymentSystem
    {
        public void ProcessPayment(string paymentSystem)
        {
            Console.WriteLine("Using IdbiBank gateway for " + paymentSystem);
        }
    }

    
}
/*
 * Bridge Design Pattern - We have a structural design pattern. Here we need to decouple "abstraction" from its "implementation".
 * Abstraction (also called interface) is a high-level control layer for some entity. 
 * This layer isn’t supposed to do any real work on its own. 
 * 
 * It should delegate the work to the implementation layer (also called platform).
 * 
 * Abstraction can be thought as the process of show which is needed to be shown to the client.
 * Implementation can be thought as how things are to be coded/ implemented.
 * 
 * Interface will be implemented
 * 
 * We have 2 examples:
 * Example 1 : You can have 2 PaymentMode. 
 * But the underlying payment gateway services are also important.
 * These payment gateway can only be availed only when a particular payment mode is selected.
 * So Payment Mode Selection is the "Abstraction" and payment gateway selection is the "Implemention".
 * 
 * Note - This could be other way aroud too depending upon which is given priority.
 * 
 * Example 2 : There is company where people in Associate can have a hugh range of salaries sometimes it may touch Senior Associate Salary.
 * So we cannot link Salary to roles. As roles (as per experience + tech stack) can have range of salaries.
 * So here roles needed to be the determining factor and salary is a major aspect for the role.
 * So role is the "abstraction" and salary is the "implementation".
 * 
 * 
 */