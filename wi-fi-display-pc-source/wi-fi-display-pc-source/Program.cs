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
            public Guid InterfaceGuid;
            public char[] strInterfaceDescription;
            public WLAN_INTERFACE_STATE isState;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WLAN_INTERFACE_INFO_LIST {
            public int dwNumberOfItems;
            public int dwIndex;
            public WLAN_INTERFACE_INFO[] InterfaceInfo;
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

            WLAN_INTERFACE_INFO_LIST infoList = new WLAN_INTERFACE_INFO_LIST();

            infoList.dwNumberOfItems = Marshal.ReadInt32(ppInterfaceList, 0);
            Console.WriteLine("WLAN_INTERFACE_INFO_LIST.dwNumberOfItems: {0}", infoList.dwNumberOfItems);
            infoList.dwIndex = Marshal.ReadInt32(ppInterfaceList, 4);
            Console.WriteLine("WLAN_INTERFACE_INFO_LIST.dwIndex: {0}", infoList.dwIndex);

            int ifListStartOffset = 8;
            int ifListItemSize = 16 + 512 + 4; // Int32*4 + WCHAR[256] + Int32
            infoList.InterfaceInfo = new WLAN_INTERFACE_INFO[infoList.dwNumberOfItems];
            for (int i = 0; i < infoList.dwNumberOfItems; i++)
            {
                infoList.InterfaceInfo[i] = new WLAN_INTERFACE_INFO();

                //Guid
                int guid1 = (int)Marshal.ReadInt32(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 0);
                short guid2 = (short)Marshal.ReadInt16(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 4);
                short guid3 = (short)Marshal.ReadInt16(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 8);
                byte[] guid4 = new byte[8];

                for (int j = 0; j < 8; j++)
                {
                    guid4[j] = (byte)Marshal.ReadByte(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 12 + j);
                }

                infoList.InterfaceInfo[i].InterfaceGuid = new Guid(guid1, guid2, guid3, guid4);
                Console.WriteLine("GUID: {0}", infoList.InterfaceInfo[i].InterfaceGuid.ToString());
                
                // description
                byte[] p = new byte[512];
                for (int j = 0; j < 512; j++)
                {
                    p[j] = Marshal.ReadByte(ppInterfaceList, ifListStartOffset + 16 + (i * ifListItemSize) + j);
                }
                infoList.InterfaceInfo[i].strInterfaceDescription = Encoding.Unicode.GetChars(p);
                Console.WriteLine("Interface description: " + new String(infoList.InterfaceInfo[i].strInterfaceDescription));
                
                // state
                infoList.InterfaceInfo[i].isState = (WLAN_INTERFACE_STATE)Marshal.ReadInt32(ppInterfaceList, ifListStartOffset + (i * ifListItemSize) + 16 + 512);
                Console.WriteLine("Interface state: {0}", infoList.InterfaceInfo[i].isState.ToString());
            }




            Console.WriteLine(Environment.NewLine + "Success Status: " + iSucces.ToString());

            Console.Write("Beginning WFD device discovery... ");

            Console.WriteLine("Press any key to end the program.");
            Console.ReadKey(true);
        }
    }
}
