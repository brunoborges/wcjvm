//############################################################
//# JVM launcher for Windows Containers
//############################################################
/*
    .NOTES
        Copyright (c) Microsoft Corporation.  All rights reserved.

        Use of this sample source code is subject to the terms of the Microsoft
        license agreement under which you licensed this sample source code. If
        you did not accept the terms of the license agreement, you are not
        authorized to use this sample source code. For the terms of the license,
        please see the license agreement between you and Microsoft or, if applicable,
        see the LICENSE.RTF on your install media or the root of your tools installation.
        THE SAMPLE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES.

    .SYNOPSIS
        Queries the job limits from inside a process-isolated container

    .DESCRIPTION
        Queries the job limits from inside a process-isolated container

    .PARAMETER Verbose 
        If passed, dump verbose output
        
*/

using System;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace WcJvmApp
{

    class Program
    {

        public enum JOBOBJECTINFOCLASS
        {
            JobObjectBasicAccountingInformation = 1,
            JobObjectBasicLimitInformation = 2,
            JobObjectBasicProcessIdList = 3,
            JobObjectBasicUIRestrictions = 4,
            JobObjectSecurityLimitInformation,
            JobObjectEndOfJobTimeInformation,
            JobObjectAssociateCompletionPortInformation,
            JobObjectBasicAndIoAccountingInformation,
            JobObjectExtendedLimitInformation,
            JobObjectCpuRateControlInformation = 15,
            JobObjectJobSetInformation,
            MaxJobObjectInfoClass,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool IsProcessInJob(IntPtr processHandle, IntPtr jobHandle, out bool result);

        [StructLayout(LayoutKind.Sequential)]
        public struct IO_COUNTERS
        {
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {

            // write a toString for this struct
            public override string ToString()
            {
                return string.Format("BasicLimitInformation: {0}, IoInfo: {1}, ProcessMemoryLimit: {2}, JobMemoryLimit: {3}, PeakProcessMemoryUsed: {4}, PeakJobMemoryUsed: {5}",
                    BasicLimitInformation.ToString(), IoInfo.ToString(), ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed);
            }
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        // define the struct for the JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        {

            // write a toString for this struct
            public override string ToString()
            {
                return string.Format("ControlFlags: {0}, CpuRate: {1}, Weight: {2}, MinRate: {3}, MaxRate: {4}",
                    ControlFlags, Union.CpuRate, Union.Weight, Union.DummyStructName.MinRate, Union.DummyStructName.MaxRate);
            }

            public UInt16 ControlFlags;

            public JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_UNION Union;

            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
            public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_UNION
            {
                [FieldOffset(4)]
                public UInt16 CpuRate;

                [FieldOffset(4)]
                public UInt16 Weight;

                [FieldOffset(4)]
                public JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_DUMMYSTRUCTNAME DummyStructName;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION_DUMMYSTRUCTNAME
            {
                public ushort MinRate;
                public ushort MaxRate;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public UInt64 PerProcessUserTimeLimit;
            public UInt64 PerJobUserTimeLimit;
            public UInt32 LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            public UIntPtr Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "QueryInformationJobObject", SetLastError = true)]
        public static extern bool QueryInformationJobObject(
           IntPtr handleJob,
           JOBOBJECTINFOCLASS jobObjectInfoClass,
           IntPtr lpJobObjectInfo,
           UInt32 jobObjectInfoLength,
           ref UInt32 returnLength
           );

        // Query for the JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        public static JOBOBJECT_CPU_RATE_CONTROL_INFORMATION QueryCpuRateControlInformation()
        {
            // Allocate an JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
            int inSize = Marshal.SizeOf(typeof(Program.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));
            IntPtr ptrData = IntPtr.Zero;
            try
            {
                // Marshal.AllocHGlobal will throw on failure, so we do not need to
                // check for allocation failure.
                ptrData = Marshal.AllocHGlobal(inSize);
                UInt32 outSize = 0;

                // Query the job object for its CPU rate control information
                bool result = Program.QueryInformationJobObject(IntPtr.Zero,
                    Program.JOBOBJECTINFOCLASS.JobObjectCpuRateControlInformation,
                    ptrData,
                    (UInt32)inSize,
                    ref outSize);
                if (result)
                {
                    // Marshal the result data into a .NET structure
                    // Return the CPU rate control information to the caller
                    return
                      (Program.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION)Marshal.PtrToStructure(ptrData, typeof(Program.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION));
                }
                else
                {
                    Console.WriteLine("QueryInformationJobObject failed with error {0}", Marshal.GetLastWin32Error());
                    return new Program.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION();
                }
            }
            finally
            {
                if (ptrData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptrData);
                }
            }
        }

        // Query the extended limit information for the running job
        // If this is run outside of an executing job, or if the memory limit is not set,
        // the script will return zero.
        public static JOBOBJECT_EXTENDED_LIMIT_INFORMATION QueryExtendedLimitInformation()
        {
            // Allocate an JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            int inSize = Marshal.SizeOf(typeof(Program.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr ptrData = IntPtr.Zero;
            try
            {
                // Marshal.AllocHGlobal will throw on failure, so we do not need to
                // check for allocation failure.
                ptrData = Marshal.AllocHGlobal(inSize);
                UInt32 outSize = 0;

                // Query the job object for its extended limits
                bool result = Program.QueryInformationJobObject(IntPtr.Zero,
                    Program.JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                    ptrData,
                    (UInt32)inSize,
                    ref outSize);
                if (result)
                {
                    // Marshal the result data into a .NET structure
                    // Return the extended limit information to the caller
                    return
                      (Program.JOBOBJECT_EXTENDED_LIMIT_INFORMATION)Marshal.PtrToStructure(ptrData, typeof(Program.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (ptrData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptrData);
                }
            }
        }

        // static method that takes an array of strings as input and returns a map of keyvalue pairs
        // the keys contain a dash, so you have to remove that. If the key contains XX:, then remove that as well
        public static System.Collections.Generic.Dictionary<string, string> ParseArgs(string[] args)
        {
            var result = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var arg in args)
            {
                var key = arg;
                var value = "";
                if (arg.StartsWith("-"))
                {
                    key = arg.Substring(1);
                }
                if (key.StartsWith("XX:"))
                {
                    key = key.Substring(3);
                }
                if (key.Contains("="))
                {
                    var parts = key.Split('=');
                    key = parts[0];
                    value = parts[1];
                }
                result.Add(key, value);
            }
            return result;
        }

        static void Main(string[] args)
        {
            // Convert args to a list
            var argList = ParseArgs(args);

            Console.WriteLine("Windows Container JVM Launcher v1.0");

            // Using System.Diagnostics to get processor count
            int processorCount = Environment.ProcessorCount;
            Console.WriteLine($"Number of Processors: {processorCount}");

            var memoryInfo = Program.QueryExtendedLimitInformation();
            // print result
            Console.WriteLine("JobMemoryLimit: {0}", memoryInfo.JobMemoryLimit);

            // Now prepare the java launcher arguments
            var javaArgs = new StringBuilder();

            // store JobMemoryLimit into a variable
            var jobMemoryLimit = memoryInfo.JobMemoryLimit;
            var originalJobMemoryLimit = jobMemoryLimit;

/*
            // From the args array, find if there is a -XX:MaxRAMPercentage argument. If there is, then use that to calculate the memory limit
            // If there is no -XX:MaxRAMPercentage argument, then use the value from the job object
            var maxRamPercentage = 75.0;

            if (argList.ContainsKey("MaxRAMPercentage"))
            {
                maxRamPercentage = Double.Parse(argList["MaxRAMPercentage"]);
            }

            // Calculate the memory limit based on the percentage
            jobMemoryLimit = (UIntPtr)(jobMemoryLimit.ToUInt64() * maxRamPercentage / 100.0);

            // Convert JobMemoryLimit from bytes to megabytes
            jobMemoryLimit = (UIntPtr)(jobMemoryLimit.ToUInt64() / 1024 / 1024);

            if (jobMemoryLimit > 0)
            {
                javaArgs.Append("-Xmx");
                javaArgs.Append(jobMemoryLimit.ToUInt64());
                javaArgs.Append("m");
            }

            */

            // append ActiveProcessorCount in case one was not specified
            if (processorCount > 0)
            {
                javaArgs.Append(" -XX:ActiveProcessorCount=");
                javaArgs.Append(processorCount);
            }

            javaArgs.Append($" -XX:MaxRAM={originalJobMemoryLimit}");

            // Append all other args
            foreach (var arg in args)
            {
                if (!arg.StartsWith("-Xmx") && !arg.StartsWith("-XX:MaxRAMPercentage") && !arg.StartsWith("-XX:ActiveProcessorCount"))
                {
                    javaArgs.Append(' ');
                    javaArgs.Append(arg);
                }
            }
            Console.WriteLine("Launching java with args: {0}", javaArgs.ToString());

            var startInfo = new ProcessStartInfo("java", javaArgs.ToString())
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Console.WriteLine("Java application started!");

            // Start the Java application
            using (Process process = Process.Start(startInfo))
            {
                // Set up asynchronous reading of standard output and error
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the Java application to exit (optional, depending on your requirements)
                process.WaitForExit();
            }
        }
    }

}
