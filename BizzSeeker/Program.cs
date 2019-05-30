using System;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace BizzSeeker
{
    public class Program
    {
        static void Main(string[] args)
        {
            TermServicesManager.TerminalSessionInfo SessionInfo = TermServicesManager.GetSessionInfo(Dns.GetHostName(), Process.GetCurrentProcess().SessionId);

            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("VIRTUAL IP.");
            string LocalIPAddress = "Failed.";
            if (SessionInfo.ClientAddress != null)
            {
                LocalIPAddress = SessionInfo.ClientAddress;
            }
            Console.WriteLine(LocalIPAddress + Environment.NewLine);
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }

    public class TermServicesManager
    {

        [DllImport("wtsapi32.dll")]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_ADDRESS
        {
            public uint AddressFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;
        }

        public enum WTS_INFO_CLASS
        {
            InitialProgram = 0,
            ApplicationName = 1,
            WorkingDirectory = 2,
            OEMId = 3,
            SessionId = 4,
            UserName = 5,
            WinStationName = 6,
            DomainName = 7,
            ConnectState = 8,
            ClientBuildNumber = 9,
            ClientName = 10,
            ClientDirectory = 11,
            ClientProductId = 12,
            ClientHardwareId = 13,
            ClientAddress = 14,
            ClientDisplay = 15,
            ClientProtocolType = 16,
            WTSIdleTime = 17,
            WTSLogonTime = 18,
            WTSIncomingBytes = 19,
            WTSOutgoingBytes = 20,
            WTSIncomingFrames = 21,
            WTSOutgoingFrames = 22,
            WTSClientInfo = 23,
            WTSSessionInfo = 24,
            WTSSessionInfoEx = 25,
            WTSConfigInfo = 26,
            WTSValidationInfo = 27,
            WTSSessionAddressV4 = 28,
            WTSIsRemoteSession = 29
        }

        public static TerminalSessionInfo GetSessionInfo(string ServerName, int SessionId)
        {
            IntPtr server = IntPtr.Zero;
            server = OpenServer(ServerName);
            System.IntPtr buffer = IntPtr.Zero;
            uint bytesReturned;
            TerminalSessionInfo data = new TerminalSessionInfo();

            try
            {
                bool worked = WTSQuerySessionInformation(server, SessionId,
                    WTS_INFO_CLASS.WTSSessionAddressV4, out buffer, out bytesReturned);

                if (!worked)
                    return data;

                WTS_SESSION_ADDRESS si = (WTS_SESSION_ADDRESS)Marshal.PtrToStructure((System.IntPtr)buffer, typeof(WTS_SESSION_ADDRESS));
                data.ClientAddress = si.Address[2] + "." + si.Address[3] + "." + si.Address[4] + "." + si.Address[5];

            }
            finally
            {
                WTSFreeMemory(buffer);
                buffer = IntPtr.Zero;
                CloseServer(server);
            }

            return data;
        }
        private static IntPtr OpenServer(string Name)
        {
            IntPtr server = WTSOpenServer(Name);
            return server;
        }

        private static void CloseServer(IntPtr ServerHandle)
        {
            WTSCloseServer(ServerHandle);
        }

        public class TerminalSessionInfo
        {
            public string ClientAddress;
        }

    }
}


