﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ClassicBotter
{
    public static class Memory
    {
        public static Process process;
        public static IntPtr handle = new IntPtr();
        
        public static bool GetHandle() //TODO göra om mot classname(?) tibiaclient grejen
        {
            List<Process> pr = new List<Process>();
            List<IntPtr> ha = new List<IntPtr>();

            Process[] pro = Process.GetProcesses();
            try
            {
                foreach (Process p in pro)
                {
                    Debug.WriteLine(p.ProcessName);
                    if (p.ProcessName.Contains("Tibia"))
                    {
                        pr.Add(p);
                        ha.Add(p.Handle);
                        //process = p;
                        //handle = p.Handle;
                        //return;
                    }
                }
                if (pr.Count == 1)
                {
                    process = pr[0];
                    handle = ha[0];
                    return true;
                }
                else
                {
                    Random r = new Random();
                    int i = r.Next(0, pr.Count);
                    process = pr[i];
                    handle = ha[i];
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                System.Windows.Forms.MessageBox.Show("Could not find Tibia client.");
                Debug.WriteLine(e.StackTrace);
            }
            return false;
        }

        public static byte[] ReadBytes(long address, uint bytesToRead)
        {
            try
            {
                IntPtr ptrBytesRead;
                byte[] buffer = new byte[bytesToRead];
                WinApi.ReadProcessMemory(handle, (IntPtr)address, buffer, bytesToRead, out ptrBytesRead);
                return buffer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                return new byte[bytesToRead];
            }
        }

        public static byte ReadByte(long address)
        {
            return ReadBytes(address, 1)[0];
        }

        public static int ReadInt(long address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        public static string ReadString(long address)
        {
            return ReadString(address, 0);
        }

        public static string ReadString(long address, uint length)
        {
            if (length > 0)
            {
                byte[] buffer;
                buffer = ReadBytes(address, length);
                return System.Text.ASCIIEncoding.Default.GetString(buffer).Split(new Char())[0];
            }
            else
            {
                string s = "";
                byte temp = ReadByte(address++);
                while (temp != 0)
                {
                    s += (char)temp;
                    temp = ReadByte(address++);
                }
                return s;
            }
        }
        
        public static bool WriteBytes(long address, byte[] bytes, uint length)
        {
            try
            {
                IntPtr bytesWritten;
                int result = WinApi.WriteProcessMemory(handle, new IntPtr(address), bytes, length, out bytesWritten);
                return result != 0;
            }
            catch { return false; }
        }

        internal static bool WriteByte(long address, byte value)
        {
            return WriteBytes(address, new byte[] { value }, 1);
        }

        internal static void WriteNops(long address, int nops)
        {
            byte nop = 0x90;
            int j = 0;
            for (int i = 0; i < nops; i++)
            {
                WriteBytes(address + j, new byte[] { nop }, 1);
                j++;
            }
        }

        internal static bool WriteInt(long address, int value)
        {
            return WriteBytes(address, BitConverter.GetBytes(value), 4);
        }

        public static bool WriteString(long address, string str)
        {
            str += '\0';
            byte[] bytes = System.Text.ASCIIEncoding.Default.GetBytes(str);
            return WriteBytes(address, bytes, (uint)bytes.Length);
        }
    }
}
