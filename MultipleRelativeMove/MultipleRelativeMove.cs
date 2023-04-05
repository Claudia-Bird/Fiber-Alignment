using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using NewFocus.Picomotor;

namespace NewFocus.Picomotor
{
    class RelativeMove
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for device discovery...");
            string strDeviceKey = string.Empty;
            CmdLib8742 cmdLib = new CmdLib8742(false, 5000, ref strDeviceKey);

            // If no devices were discovered
            if (strDeviceKey == null)
            {
                Console.WriteLine("No devices discovered.");
            }
            else
            {
                // If the device was opened
                if (cmdLib.Open(strDeviceKey))
                {

                    int nMotorMax = 4;
                    int[] nSteps = new int[4];
                    int[] nPosition = new int[4];
                    bool[] bStatus = new bool[4]; 

                    // For each motor
                    for (int nMotor = 1; nMotor <= nMotorMax; nMotor++)
                    {
                        int index = nMotor - 1;

                        // Set the current position to zero
                        bStatus[index] = cmdLib.SetZeroPosition(strDeviceKey, nMotor);

                        if (!bStatus[index])
                        {
                            Console.WriteLine("I/O Error:  Could not set the current position.");
                        }

                        // Get the current position
                        bStatus[index] = cmdLib.GetPosition(strDeviceKey, nMotor, ref nPosition[index]);
                        
                        if (bStatus[index])
                        {
                            Console.WriteLine("Motor #{0}', Start Position = {1}", nMotor, nPosition[index]);
                            cmdLib.WriteLog("Motor #{0}', Start Position = {1}", nMotor, nPosition[index]);
                        }
                        else
                        {
                            Console.WriteLine("Motor #{0}', GetPosition Error.", nPosition[index]);
                            Console.WriteLine("Motor #{0}', GetPosition Error.", nPosition[index]);
                        }

                        Console.WriteLine("Enter the relative steps to move for Motor#{0} (0 or Ctrl-C for no movement): ", nMotor);
                        string strInput = Console.ReadLine();
                        nSteps[nMotor-1] = Convert.ToInt32(strInput);
                    }
                    for (int i = 0; i <= 3; i++)
                    {
                        Console.WriteLine("Motor#{0} told to move {1}", i, nSteps[i]);
                        int nMotor = i + 1;

                        if (nSteps[i] != 0)
                        {
                            // Perform a relative move
                            bStatus[i] = cmdLib.RelativeMove(strDeviceKey, nMotor, nSteps[i]);

                            if (!bStatus[i])
                            {
                                Console.WriteLine("I/O Error:  Could not perform relative move.");
                            }
                        }

                        bool bIsMotionDone = false;

                        while (bStatus[i] && !bIsMotionDone)
                        {
                            // Check for any device error messages
                            string strErrMsg = string.Empty;
                            bStatus[i] = cmdLib.GetErrorMsg(strDeviceKey, ref strErrMsg);

                            if (!bStatus[i])
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
                                bStatus[i] = cmdLib.GetMotionDone(strDeviceKey, nMotor, ref bIsMotionDone);

                                if (!bStatus[i])
                                {
                                    Console.WriteLine("I/O Error:  Could not get motion done status.");
                                }
                                else
                                {
                                    // Get the current position
                                    bStatus[i] = cmdLib.GetPosition(strDeviceKey, nMotor, ref nPosition[i]);

                                    if (!bStatus[i])
                                    {
                                        Console.WriteLine("I/O Error:  Could not get the current position.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Position = {0}", nPosition[i]);
                                    }
                                }
                            }
                        }
                    }
                }
                // Close the device
                cmdLib.Close(strDeviceKey);
            }
            // Shut down device communication
            Console.WriteLine("Shutting down.");
            cmdLib.Shutdown();
        }
    }
}
