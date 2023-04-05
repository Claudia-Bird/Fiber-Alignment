using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Thorlabs.TLPMX_64.Interop;
//added thru references, 64 bit ver

/// <summary>
/// GUI with a TLPMX sample
/// </summary>

namespace PM100USB_C_Sharp_Example
{
    internal class Program
    {
        private static TLPMX tlpm;
        //Create TLPMX object tlpm

        static void Main(string[] args)
            /* 
            Main method that is:
            static (called without object)
            void (doesn't return anything)
            can take command line args (String[] args) 
            */
        {
            HandleRef Instrument_Handle = new HandleRef();
            // variable handle to windows resource
            // I assume windows resource is the usb power meter
            TLPMX searchDevice = new TLPMX(Instrument_Handle.Handle);
            // searchDevice is a local variable within TLPMX class
            // set equal to a new instance of a type created by
            // function TLPMX(IntPtr Instrument_Handle) which establishes
            // IVI instrument driver session
            uint count = 0;

            string firstPowermeterFound = "";

            int pInvokeResult = searchDevice.findRsrc(out count);
            // findRsrc finds the actual usb device using previous established sesion?
            // and how many there are (should be value of 1 if using one)
            // stores in pInvokeResult

            if (count > 0)
            {
                StringBuilder descr = new StringBuilder(1024);

                searchDevice.getRsrcName(0, descr);
                // gets name of the usb power meter, which we need
                // to interface with it
                // name: "USB0::0x1313::0x8072::P2003372::INSTR"

                firstPowermeterFound = descr.ToString();
                // changes it to a string and saves in var
            }

            if (count == 0)
            {
                searchDevice.Dispose();
                Console.Write("No powermeter could be found");
                return;
            }
            // tells us if there was any power meters detected by
            // checking the # detected, if not then program ends.

            //always use true for ID Query
            tlpm = new TLPMX(firstPowermeterFound, true, true);  //  For valid Ressource_Name see NI-Visa documentation. Defines the tlpm object previously created with the name, says to perform ID QUERY and reset device
            double powerValue; // local var double powerValue to store value from meter
            tlpm.setWavelength(1310, TLPMConstants.Default_Channel); // set wavelength to 1310 nm
            while (true) 
            { 
            tlpm.measPower(out powerValue, TLPMConstants.Default_Channel); // TLPMX function measPower grabs powerValue from the default channel of the power meter (which I believe has only one channel)
            Console.Write("The power is {0} W \n", powerValue); // writes powerValue to console
            Thread.Sleep(1000);
            }
        }
    }
}

/*
deprecated code from using PM100D
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thorlabs.TLPMX_64.Interop;
// Using the Thorlabs TLPM namespace; first added as a reference

namespace PM100USB_C_Sharp_Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Create PM100D object
            //The Device resource name can be found by running Thorlabs Optical Power Meter and opening log file
            TLPMX pm = new TLPMX("USB0::0x1313::0x8072::P2003372::INSTR",false,false);
            //set wavelength
            pm.setWavelength(1064,1);
            //measure and display power
            double powerdens = 0;
            Console.Write("The power is {0} W", powerdens);
            //Wait until user presses key to exit
            Console.ReadKey();

        }
    }
}
*/