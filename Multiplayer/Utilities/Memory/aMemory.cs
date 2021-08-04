using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Multiplayer.Utilities.Memory
{
    public class aMemory
    {
        /// <summary>
        /// The process to read and write data on.
        /// Authored by Alizer for Adafcaefc's TASBot.
        /// </summary>
        public Process BaseProcess;
        private IntPtr pHandle;

        /// <summary>
        /// Initialize an object instance of aMemory
        /// </summary>
        public aMemory(Process pRoc)
        {
            InitProc(pRoc);
        }

        public void InitProc(Process proc)
        {
            if (BaseProcess != proc)
            {
            
                isAlreadyInjected = false;
                BaseProcess = proc;
                pHandle = NATIVE_FUNCTIONS.OpenProcess(0x1F0FFF, true, proc.Id);
            }
        }
        /// <summary>
        /// Initialize an object instance of aMemory
        /// </summary>
        /// <param name="ProcessName">The process filename, this is not the window title. Will automatically get the first process of the same name.</param>
        public aMemory(string ProcessName)
        {
            BaseProcess = Process.GetProcessesByName(ProcessName)[0];
        }
        public int GetModuleAddress(string Module) {

            foreach (ProcessModule _Module in BaseProcess.Modules)
            {
                if (!string.IsNullOrEmpty(_Module.ModuleName))
                    if (_Module.ModuleName.ToLower().Trim() == Module.ToLower().Trim())
                        return (int)_Module.BaseAddress;
            }
            return -1;
        }
        /// <summary>
        /// Reads an address and converts it to type.
        /// </summary>
        /// <typeparam name="DType">Convert the read address to this type.</typeparam>
        /// <param name="Address">The address to read one.</param>
        /// <returns>Converted data.</returns>
        public DType ReadMemory<DType>(IntPtr Address)
        {
            var Type_ = typeof(DType);
            int SizeOfDataType = DataTypeSizes.GetSize(Type_);
            byte[] buffer = new byte[SizeOfDataType];
            // read memory
            NATIVE_FUNCTIONS.ReadProcessMemory(pHandle, Address, buffer, SizeOfDataType, out _);
            if (Type_ == typeof(byte))
                return (DType)Convert.ChangeType(buffer[0], Type_);
            else if (Type_ == typeof(float))
                return (DType)Convert.ChangeType(BitConverter.ToSingle(buffer, 0), Type_);
            else if (Type_ == typeof(int))
                return (DType)Convert.ChangeType(BitConverter.ToInt32(buffer, 0), Type_);
            else if (Type_ == typeof(uint))
                return (DType)Convert.ChangeType(BitConverter.ToUInt32(buffer, 0), Type_);
            else if (Type_ == typeof(bool))
                return (DType)Convert.ChangeType(BitConverter.ToBoolean(buffer, 0), Type_);
            else if (Type_ == typeof(long))
                return (DType)Convert.ChangeType(BitConverter.ToInt64(buffer, 0), Type_);

            return (DType)Convert.ChangeType(0, 0);
        }
        public byte[] ReadXBytes(IntPtr Address, int length)
        {
            byte[] bytes = new byte[length];
            NATIVE_FUNCTIONS.ReadProcessMemory(pHandle, Address, bytes, bytes.Length, out _);
            return bytes;
        }
        /// <summary>
        /// Writes to an address.
        /// </summary>
        /// <typeparam name="DType">Convert the read address to this type.</typeparam>
        /// <param name="Address">The address to read one.</param>
        /// <returns>Converted data.</returns>
        /// 
        public bool WriteMemory(IntPtr Address, bool Value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(Value));
        }

        public bool WriteMemory(IntPtr Address, int Value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(Value));
        }

        public bool WriteMemory(IntPtr Address, float Value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(Value));
        }

        public bool WriteMemory(IntPtr Address, byte[] Value)
        {
            return NATIVE_FUNCTIONS.WriteProcessMemory(pHandle, Address, Value, Value.Length, out _);
        }

        /// <summary>
        /// Array of Byte scan.
        /// https://github.com/erfg12/memory.dll/blob/master/Memory/memory.cs
        /// </summary>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(string search, bool writable = false, bool executable = true)
        {
            return AoBScan(0, long.MaxValue, search, writable, executable);
        }

        /// <summary>
        /// Array of Byte scan.
        /// https://github.com/erfg12/memory.dll/blob/master/Memory/memory.cs
        /// </summary>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="readable">Include readable addresses in scan</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(string search, bool readable, bool writable, bool executable)
        {
            return AoBScan(0, long.MaxValue, search, readable, writable, executable);
        }


        /// <summary>
        /// Array of Byte scan.
        /// https://github.com/erfg12/memory.dll/blob/master/Memory/memory.cs
        /// </summary>
        /// <param name="start">Your starting address.</param>
        /// <param name="end">ending address</param>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool writable = false, bool executable = true)
        {
            // Not including read only memory was scan behavior prior.
            return AoBScan(start, end, search, false, writable, executable);
        }

        /// <summary>
        /// Array of Byte scan.
        /// https://github.com/erfg12/memory.dll/blob/master/Memory/memory.cs
        /// </summary>
        /// <param name="start">Your starting address.</param>
        /// <param name="end">ending address</param>
        /// <param name="search">array of bytes to search for, OR your ini code label.</param>
        /// <param name="readable">Include readable addresses in scan</param>
        /// <param name="writable">Include writable addresses in scan</param>
        /// <param name="executable">Include executable addresses in scan</param>
        /// <returns>IEnumerable of all addresses found.</returns>
        public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable, bool executable)
        {
            return Task.Run(() =>
            {
                var memRegionList = new List<MEMORY_REGION_RESULT>();

                string memCode = search;

                string[] stringByteArray = memCode.Split(' ');

                byte[] aobPattern = new byte[stringByteArray.Length];
                byte[] mask = new byte[stringByteArray.Length];

                for (var i = 0; i < stringByteArray.Length; i++)
                {
                    string ba = stringByteArray[i];

                    if (ba == "??" || (ba.Length == 1 && ba == "?"))
                    {
                        mask[i] = 0x00;
                        stringByteArray[i] = "0x00";
                    }
                    else if (Char.IsLetterOrDigit(ba[0]) && ba[1] == '?')
                    {
                        mask[i] = 0xF0;
                        stringByteArray[i] = ba[0] + "0";
                    }
                    else if (Char.IsLetterOrDigit(ba[1]) && ba[0] == '?')
                    {
                        mask[i] = 0x0F;
                        stringByteArray[i] = "0" + ba[1];
                    }
                    else
                        mask[i] = 0xFF;
                }


                for (int i = 0; i < stringByteArray.Length; i++)
                    aobPattern[i] = (byte)(Convert.ToByte(stringByteArray[i], 16) & mask[i]);

                SYSTEM_INFO sys_info = new SYSTEM_INFO();
                NATIVE_FUNCTIONS.GetSystemInfo(out sys_info);

                UIntPtr proc_min_address = sys_info.minimumApplicationAddress;
                UIntPtr proc_max_address = sys_info.maximumApplicationAddress;

                if (start < (long)proc_min_address.ToUInt64())
                    start = (long)proc_min_address.ToUInt64();

                if (end > (long)proc_max_address.ToUInt64())
                    end = (long)proc_max_address.ToUInt64();

                UIntPtr currentBaseAddress = new UIntPtr((ulong)start);

                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();

                //Debug.WriteLine("[DEBUG] start:0x" + start.ToString("X8") + " curBase:0x" + currentBaseAddress.ToUInt64().ToString("X8") + " end:0x" + end.ToString("X8") + " size:0x" + memInfo.RegionSize.ToString("X8") + " vAloc:" + VirtualQueryEx(pHandle, currentBaseAddress, out memInfo).ToUInt64().ToString());

                while (NATIVE_FUNCTIONS.VirtualQueryEx(pHandle, currentBaseAddress, out memInfo).ToUInt64() != 0 &&
                       currentBaseAddress.ToUInt64() < (ulong)end &&
                       currentBaseAddress.ToUInt64() + (ulong)memInfo.RegionSize >
                       currentBaseAddress.ToUInt64())
                {
                    bool isValid = memInfo.State == NATIVE_FUNCTIONS.MEM_COMMIT;
                    isValid &= memInfo.BaseAddress.ToUInt64() < (ulong)proc_max_address.ToUInt64();
                    isValid &= ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_GUARD) == 0);
                    isValid &= ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_NOACCESS) == 0);
                    isValid &= (memInfo.Type == NATIVE_FUNCTIONS.MEM_PRIVATE) || (memInfo.Type == NATIVE_FUNCTIONS.MEM_IMAGE);

                    if (isValid)
                    {
                        bool isReadable = (memInfo.Protect & NATIVE_FUNCTIONS.PAGE_READONLY) > 0;

                        bool isWritable = ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_READWRITE) > 0) ||
                                          ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_WRITECOPY) > 0) ||
                                          ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE_READWRITE) > 0) ||
                                          ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE_WRITECOPY) > 0);

                        bool isExecutable = ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE) > 0) ||
                                            ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE_READ) > 0) ||
                                            ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE_READWRITE) > 0) ||
                                            ((memInfo.Protect & NATIVE_FUNCTIONS.PAGE_EXECUTE_WRITECOPY) > 0);

                        isReadable &= readable;
                        isWritable &= writable;
                        isExecutable &= executable;

                        isValid &= isReadable || isWritable || isExecutable;
                    }

                    if (!isValid)
                    {
                        currentBaseAddress = new UIntPtr(memInfo.BaseAddress.ToUInt64() + (ulong)memInfo.RegionSize);
                        continue;
                    }

                    MEMORY_REGION_RESULT memRegion = new MEMORY_REGION_RESULT
                    {
                        CurrentBaseAddress = currentBaseAddress,
                        RegionSize = memInfo.RegionSize,
                        RegionBase = memInfo.BaseAddress
                    };

                    currentBaseAddress = new UIntPtr(memInfo.BaseAddress.ToUInt64() + (ulong)memInfo.RegionSize);

                    //Console.WriteLine("SCAN start:" + memRegion.RegionBase.ToString() + " end:" + currentBaseAddress.ToString());

                    if (memRegionList.Count > 0)
                    {
                        var previousRegion = memRegionList[memRegionList.Count - 1];

                        if ((long)previousRegion.RegionBase + previousRegion.RegionSize == (long)memInfo.BaseAddress)
                        {
                            memRegionList[memRegionList.Count - 1] = new MEMORY_REGION_RESULT
                            {
                                CurrentBaseAddress = previousRegion.CurrentBaseAddress,
                                RegionBase = previousRegion.RegionBase,
                                RegionSize = previousRegion.RegionSize + memInfo.RegionSize
                            };

                            continue;
                        }
                    }

                    memRegionList.Add(memRegion);
                }

                ConcurrentBag<long> bagResult = new ConcurrentBag<long>();

                Parallel.ForEach(memRegionList,
                                 (item, parallelLoopState, index) =>
                                 {
                                     long[] compareResults = CompareScan(item, aobPattern, mask);

                                     foreach (long result in compareResults)
                                         bagResult.Add(result);
                                 });

                Debug.WriteLine("[DEBUG] memory scan completed. (time:" + DateTime.Now.ToString("h:mm:ss tt") + ")");

                return bagResult.ToList().OrderBy(c => c).AsEnumerable();
            });
        }

        private long[] CompareScan(MEMORY_REGION_RESULT item, byte[] aobPattern, byte[] mask)
        {
            if (mask.Length != aobPattern.Length)
                throw new ArgumentException($"{nameof(aobPattern)}.Length != {nameof(mask)}.Length");

            IntPtr buffer = Marshal.AllocHGlobal((int)item.RegionSize);

            NATIVE_FUNCTIONS.ReadProcessMemory(pHandle, item.CurrentBaseAddress, buffer, (UIntPtr)item.RegionSize, out ulong bytesRead);

            int result = 0 - aobPattern.Length;
            List<long> ret = new List<long>();
            unsafe
            {
                do
                {

                    result = FindPattern((byte*)buffer.ToPointer(), (int)bytesRead, aobPattern, mask, result + aobPattern.Length);

                    if (result >= 0)
                        ret.Add((long)item.CurrentBaseAddress + result);

                } while (result != -1);
            }
            Marshal.FreeHGlobal(buffer);

            return ret.ToArray();
        }

        private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
        {
            int foundIndex = -1;

            if (body.Length <= 0 || pattern.Length <= 0 || start > body.Length - pattern.Length ||
                pattern.Length > body.Length) return foundIndex;

            for (int index = start; index <= body.Length - pattern.Length; index++)
            {
                if (((body[index] & masks[0]) == (pattern[0] & masks[0])))
                {
                    var match = true;
                    for (int index2 = 1; index2 <= pattern.Length - 1; index2++)
                    {
                        if ((body[index + index2] & masks[index2]) == (pattern[index2] & masks[index2])) continue;
                        match = false;
                        break;

                    }

                    if (!match) continue;

                    foundIndex = index;
                    break;
                }
            }

            return foundIndex;
        }

        private unsafe int FindPattern(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
        {
            int foundIndex = -1;

            if (bodyLength <= 0 || pattern.Length <= 0 || start > bodyLength - pattern.Length ||
                pattern.Length > bodyLength) return foundIndex;

            for (int index = start; index <= bodyLength - pattern.Length; index++)
            {
                if (((body[index] & masks[0]) == (pattern[0] & masks[0])))
                {
                    var match = true;
                    for (int index2 = 1; index2 <= pattern.Length - 1; index2++)
                    {
                        if ((body[index + index2] & masks[index2]) == (pattern[index2] & masks[index2])) continue;
                        match = false;
                        break;

                    }

                    if (!match) continue;

                    foundIndex = index;
                    break;
                }
            }

            return foundIndex;
        }
        public bool isAlreadyInjected = false;
        public bool dllInject(string sDllPath)
        {
            if (!isAlreadyInjected)
            {
                // inject it twice using adaf's injector
                try
                {
                    Process proc = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = GDM.Globals.Global_Data.InjectorName;
                    psi.RedirectStandardInput = true;
                    psi.RedirectStandardOutput = false;
                    psi.UseShellExecute = false;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    // psi.Arguments = "\"" + GDM.Globals.Global_Data.Main.UserPref.WindowName + "\" Multiplayer.dll";
                    proc.StartInfo = psi;
                    proc.Start();
                }
                catch (Exception ex){
                    Utilities.Utils.HandleException(ex);
                }

                bool DoneOnce = false;
                Thread.Sleep(1000);
                for (int i = 0; i < BaseProcess.Modules.Count; i++) {
                    var j = BaseProcess.Modules[i];
                    if (j.ModuleName.ToLower().Contains("multiplayer")) {
                        DoneOnce = true;
                        Debug.WriteLine("First inject success.");
                        break;
                    }
                }

                if (!DoneOnce)
                {
                    // inject it again
                    // dangerous inject for non unicode charas and paths with more than 255 charas in length
                    sDllPath = sDllPath.Trim();
                    if (sDllPath.Length <= 255)
                    {
                        IntPtr lpLLAddress = NATIVE_FUNCTIONS.GetProcAddress(NATIVE_FUNCTIONS.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                        IntPtr lpAddress = NATIVE_FUNCTIONS.VirtualAllocEx(pHandle, (IntPtr)null, sDllPath.Length,
                            (NATIVE_FUNCTIONS.AllocationType)(0x1000 | 0x2000), (NATIVE_FUNCTIONS.MemoryProtection)0X40);

                        byte[] bytes = Encoding.Default.GetBytes(sDllPath);

                        var Kl = (UIntPtr)(long)lpAddress;

                        NATIVE_FUNCTIONS.WriteProcessMemory(pHandle, Kl, bytes, (UIntPtr)bytes.Length, out _);

                        NATIVE_FUNCTIONS.CreateRemoteThread(pHandle, (IntPtr)null, 0, lpLLAddress, lpAddress, 0, (IntPtr)null);
                    }
                }
                isAlreadyInjected = true;
            }
            return true;
        }
    }
}
