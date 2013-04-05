using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WiFiDisplayPc.Source
{
    class Program
    {
        private enum WLAN_INTERFACE_STATE
        {
            wlan_interface_state_not_ready = 0,
            wlan_interface_state_connected = 1,
            wlan_interface_state_ad_hoc_network_formed = 2,
            wlan_interface_state_disconnecting = 3,
            wlan_interface_state_disconnected = 4,
            wlan_interface_state_associating = 5,
            wlan_interface_state_discovering = 6,
            wlan_interface_state_authenticating = 7
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WLAN_INTERFACE_INFO {
            Guid InterfaceGuid;
            char[] strInterfaceDescription;
            WLAN_INTERFACE_STATE isState;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WLAN_INTERFACE_INFO_LIST {
            int dwNumberOfItems;
            int dwIndex;
            WLAN_INTERFACE_INFO[] InterfaceInfo;
        }
        
        [DllImport("Wlanapi.dll")]
        private static extern int WlanOpenHandle(
            int dwClientVersion,
            IntPtr pReserved,
            out IntPtr pdwNegotiatedVersion,
            out IntPtr phClientHandle);

        [DllImport("Wlanapi.dll")]
        private static extern int WlanEnumInterfaces(
            IntPtr hClientHandle,
            IntPtr pReserved,
            out IntPtr ppInterfaceList
            );

        static void Main(string[] args)
        {
            Console.Write("Searching for valid Wi-Fi devices... ");
            IntPtr pdwNegotiatedVersion = IntPtr.Zero;
            IntPtr phClientHandle = IntPtr.Zero;
            int iSucces = WlanOpenHandle(2, IntPtr.Zero, out pdwNegotiatedVersion, out phClientHandle);
            
            IntPtr ppInterfaceList = IntPtr.Zero;
            iSucces = WlanEnumInterfaces(phClientHandle, IntPtr.Zero, out ppInterfaceList);

            int iNumberOfItems = Marshal.ReadInt32(ppInterfaceList, 0);
            Console.WriteLine("WLAN_INTERFACE_INFO_LIST.dwNumberOfItems: {0}", iNumberOfItems);
            int dwIndex = Marshal.ReadInt32(ppInterfaceList, 4);
            Console.WriteLine("WLAN_INTERFACE_INFO_LIST.dwIndex: {0}", dwIndex);

            int ifListStartOffset = 8;
            int ifListItemSize = 16 + 512 + 4; // Int32*4 + WCHAR[256] + Int32
            for (int i = 0; i < iNumberOfItems; i++)
            {
                //Guid
                UInt32 guid1 = (UInt32)Marshal.ReadInt32(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 0);
                UInt16 guid2 = (UInt16)Marshal.ReadInt16(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 4);
                UInt16 guid3 = (UInt16)Marshal.ReadInt16(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 8);
                UInt64 guid4 = (UInt64)Marshal.ReadInt64(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 12);
                Console.WriteLine("GUID: {0}-{1}-{2}-{3}", guid1, guid2, guid3, guid4);
                // description
                byte[] p = new byte[512];
                for (int j = 0; j < 512; j++)
                {
                    p[j] = Marshal.ReadByte(ppInterfaceList, ifListStartOffset + 16 + (i * ifListItemSize) + j);
                }
                String sInterfaceDescription = Encoding.Unicode.GetString(p);
                Console.WriteLine("Interface description: " + sInterfaceDescription);
                // state
                Int32 iIsState = Marshal.ReadInt32(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 16 + 512);
                Console.WriteLine("Interface state: {0}", iIsState);
            }




            Console.WriteLine(Environment.NewLine + "Success Status: " + iSucces.ToString());

            Console.Write("Beginning WFD device discovery... ");

            Console.WriteLine("Press any key to end the program.");
            Console.ReadKey(true);
        }
    }
}
