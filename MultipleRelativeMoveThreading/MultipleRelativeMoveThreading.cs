////////////////////////////////////////////////////////////////////////////////////////////////////////
//FileName: MultipleRelativeMoveThreading.cs
//FileType: Visual C# Source file
//Author : Andrew Duong
//Created On : 4/26/23
//Last Modified On : 4/27/23
//Description : Program to move multiple Newport Picomotors at the same time
////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using NewFocus.Picomotor;

namespace NewFocus.Picomotor
{
    // MOTOR CLASS FOR IND. MOTOR PROP.
    class MotorProperties
    {
        public int nSteps; // number of steps this motor should move when prompted by relativeMove
        public bool bStatus; // status of position
        public int nPosition; // actual step value position
        public int nMotor; // motor # that identifies which is being modified
    }
    // MOTOR CLASS END

    public static class Global
    {
        public static string strDeviceKey = string.Empty; //creating a variable for the motor controller's device key, used to communicate with the controller
        public static int index;
        public const int nMotorMax = 4; // !!!update!!! with how many motors are being used
    }
    
    // MAIN CLASS
    class MultipleRelativeMoveThreading
    {
        public static List<MotorProperties> motor = new List<MotorProperties>(); // List for housing all the motors
        public static CmdLib8742 cmdLib = new CmdLib8742(false, 1000, ref Global.strDeviceKey); // Acquiring the strDeviceKey, params: no logging, 5 second delay for discovery, string var to save device key to

        //MAIN METHOD
        static void Main(string[] args)
        {
            // VARIABLES
            // VARIABLES END

            // MOTOR COMMUNICATION SETUP
            if (Global.strDeviceKey == null) // if device key is not found, no devices have been discovered, nothing happens
            {
                Console.WriteLine("No devices discovered");
            }
            else
            {
                if (cmdLib.Open(Global.strDeviceKey)) // If communication with motor controller successfully opened
                {
                    for (int i = 0; i <= Global.nMotorMax-1; i++) // loop thru number of motors
                    {
                        int n = i + 1; // temporary variable that indicates which motor is currently being used 

                        motor.Add(new MotorProperties() { bStatus = cmdLib.SetZeroPosition(Global.strDeviceKey,n), nMotor = n }); // Add a new instance to the list "motor" with bStatus set to the zero position and nMotor set to n
                        
                        if (!motor[i].bStatus) // check to see if zero position was set successfully
                        {
                            Console.WriteLine("I/O Error:  Could not set the current position.");
                        }

                        motor[i].bStatus = cmdLib.GetPosition(Global.strDeviceKey, n, ref motor[i].nPosition); // Sets the current position in nPosition variable

                        if (motor[i].bStatus) // Another check to see if the position was set correctly, if it wasn't, bStatus would be FALSE
                        {
                            Console.WriteLine("Motor #{0}', Start Position = {1}", n, motor[i].nPosition);
                            cmdLib.WriteLog("Motor #{0}', Start Position = {1}", n, motor[i].nPosition);
                        }
                        else
                        {
                            Console.WriteLine("Motor #{0}', GetPosition Error.", motor[i].nPosition);
                            cmdLib.WriteLog("Motor #{0}', GetPosition Error.", motor[i].nPosition);
                        }
                    }
                }
            }
            Console.WriteLine("MOTOR COMMUNICATION SETUP END");
            // MOTOR COMMUNICATION SETUP END

            /*
            // RANDOM TEST FOR MOTOR MOVEMENT
            motor[1].bStatus = cmdLib.RelativeMove(Global.strDeviceKey, 1, 100);
            Thread.Sleep(1000);
            motor[1].bStatus = cmdLib.GetPosition(Global.strDeviceKey, 1, ref motor[1].nPosition);
            Console.WriteLine(motor[1].nPosition);
            // TEST END
            */

            // MAIN MENU 
            bool showMenu = true; // Variable that controls whether a menu is displayed
            while (showMenu)
            {
                showMenu = Menu();
            }
            // MAIN MENU END
            return;
        }
        //MAIN METHOD END

        // MENU METHOD
        static bool Menu()
        {
            Console.WriteLine("\n \n \n");
            Console.WriteLine("Choose a motor to move:");
            Console.WriteLine("1) Motor #1");
            Console.WriteLine("2) Motor #2");
            Console.WriteLine("3) Motor #3");
            Console.WriteLine("4) Motor #4");
            Console.WriteLine("5) All Motors");
            Console.WriteLine("6) STOP program");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Global.index = 0; // we are editing motor 1, which is in motor[0]
                    ///Console.Clear();
                    Console.WriteLine("How many steps to move?");
                    motor[Global.index].nSteps = Convert.ToInt32(Console.ReadLine());
                    move();
                    return true;
                case "2":
                    Global.index = 1; // we are editing motor 2, which is in motor[1]
                    //onsole.Clear();
                    Console.WriteLine("How many steps to move?");
                    motor[Global.index].nSteps = Convert.ToInt32(Console.ReadLine());
                    move();
                    return true;
                case "3":
                    Global.index = 2; // we are editing motor 3, which is in motor[2]
                    //Console.Clear();
                    Console.WriteLine("How many steps to move?");
                    motor[Global.index].nSteps = Convert.ToInt32(Console.ReadLine());
                    move();
                    return true;
                case "4":
                    Global.index = 3; // we are editing motor 4, which is in motor[3]
                    //Console.Clear();
                    Console.WriteLine("How many steps to move?");
                    motor[Global.index].nSteps = Convert.ToInt32(Console.ReadLine());
                    move();
                    return true;
                case "5":
                    //Console.Clear();

                    for (int i = 0; i <= Global.nMotorMax-1; i++)
                    {
                        int motornumber = i + 1;
                        Console.WriteLine("How many steps to move motor {0}?", motornumber);
                        motor[i].nSteps = Convert.ToInt32(Console.ReadLine());
                    }
                    Console.WriteLine("\n \n \n");
                    for (int i = 0; i <= Global.nMotorMax-1; i++)
                    {
                        Global.index = i;
                        move();
                    }
                    return true;
                case "6":
                    return false;
                default:
                    return true;
            }
        }
        // MENU METHOD END

        // RELATIVE MOVE METHOD
        static void move()
        {
            int localindex = Global.index;

            if (motor[localindex].nSteps > 0)
            {
                motor[localindex].bStatus = cmdLib.RelativeMove(Global.strDeviceKey, motor[localindex].nMotor, motor[localindex].nSteps);
            }
            else
            {
                Console.WriteLine("You entered a negative or a zero.");
            }

            bool bIsMotionDone = false;

            while (motor[localindex].bStatus && !bIsMotionDone)
            {
                // Check for any device error messages
                string strErrMsg = string.Empty;
                motor[localindex].bStatus = cmdLib.GetErrorMsg(Global.strDeviceKey, ref strErrMsg);

                if (!motor[localindex].bStatus)
                {
                    Console.WriteLine("I/O Error:  Could not get error status.");
                }
                else
                {
                    string[] strTokens = strErrMsg.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    // If the error message number is not zero
                    if (strTokens[0] != "0")
                    {
                        Console.WriteLine("Device Error:  {0}", strErrMsg);
                        break;
                    }

                    // Get the motion done status
                    motor[localindex].bStatus = cmdLib.GetMotionDone(Global.strDeviceKey, motor[localindex].nMotor, ref bIsMotionDone);

                    if (!motor[localindex].bStatus)
                    {
                        Console.WriteLine("I/O Error:  Could not get motion done status.");
                    }
                    else
                    {
                        // Get the current position
                        motor[localindex].bStatus = cmdLib.GetPosition(Global.strDeviceKey, motor[localindex].nMotor, ref motor[localindex].nPosition);

                        if (!motor[localindex].bStatus)
                        {
                            Console.WriteLine("I/O Error:  Could not get the current position.");
                        }
                        /* else
                        {
                            Console.WriteLine("Position of Motor #{0} = {1}", localindex+1, motor[localindex].nPosition);
                        }
                        */
                    }
                }
            }
            if (bIsMotionDone)
            {
                Console.WriteLine("Position of Motor #{0} = {1}", localindex + 1, motor[localindex].nPosition);
            }
        }
        // RELATIVE MOVE METHOD END
    }
    // MAIN CLASS END
}

/*
if (i == 0) //RELATIVE MOVEMENT if motor#1 is being selected
{
motor[i].bStatus = cmdLib.RelativeMove(Global.strDeviceKey, n, 100);
}

Thread.Sleep(1000);
motor[i].bStatus = cmdLib.GetPosition(Global.strDeviceKey, n, ref motor[i].nPosition); // Sets the current position in nPosition variable

Console.WriteLine(motor[i].nPosition);
Console.ReadLine();
*/