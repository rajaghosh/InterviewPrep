using System;
using System.Collections.Generic;
using System.Text;

namespace Basic
{
    public abstract class Device
    {
        public int IMEI = 0;
        public abstract int batteryPercent {get; set;} // Need implementation in derived class

        private  void PowerOn() {  } //Blank method
        public abstract void PlayVideo();
        public virtual void PlayAudio() { }

    }
    interface IFruit
    {
        string Description { get; set; }
        string Name();
        string Season();
        string Color();
    }
}
