using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    class MyMobile :Device
    {
        public override int batteryPercent
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void PlayVideo()
        {
            throw new NotImplementedException();
        }
    }

    public class Mango : IFruit
    {
        string IFruit.Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Color()
        {
            throw new NotImplementedException();
        }

        public string Name()
        {
            throw new NotImplementedException();
        }

        public string Season()
        {
            throw new NotImplementedException();
        }
    }
}
