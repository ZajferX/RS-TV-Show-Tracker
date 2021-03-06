﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides various little utility functions.
    /// </summary>
    public static partial class Utils
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Provides access to Win32 API functions.
        /// </summary>
        public static class Interop
        {
            #region Icons
            /// <summary>
            /// Creates an array of handles to large or small icons extracted from the specified executable file, DLL, or icon file.
            /// </summary>
            /// <param name="lpszFile">The name of an executable file, DLL, or icon file from which icons will be extracted.</param>
            /// <param name="nIconIndex">The zero-based index of the first icon to extract. For example, if this value is zero, the function extracts the first icon in the specified file.</param>
            /// <param name="phiconLarge">An array of icon handles that receives handles to the large icons extracted from the file. If this parameter is NULL, no large icons are extracted from the file.</param>
            /// <param name="phiconSmall">An array of icon handles that receives handles to the small icons extracted from the file. If this parameter is NULL, no small icons are extracted from the file.</param>
            /// <param name="nIcons">The number of icons to be extracted from the file.</param>
            /// <returns>If the nIconIndex parameter is -1, the phiconLarge parameter is NULL, and the phiconSmall parameter is NULL, then the return value is the number of icons contained in the specified file. Otherwise, the return value is the number of icons successfully extracted from the file.</returns>
            [DllImport("shell32.dll", EntryPoint = "ExtractIconEx")]
            public static extern int ExtractIconExW(string lpszFile, int nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

            /// <summary>
            /// Destroys an icon and frees any memory the icon occupied.
            /// </summary>
            /// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImport("user32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
            #endregion

            #region Symbolic links
            /// <summary>
            /// Creates a symbolic link.
            /// </summary>
            /// <param name="lpSymlinkFileName">The symbolic link to be created.</param>
            /// <param name="lpTargetFileName">The name of the target for the symbolic link to be created.</param>
            /// <param name="dwFlags">Indicates whether the link target, lpTargetFileName, is a directory.</param>
            /// <returns></returns>
            [DllImport("kernel32.dll")]
            public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlags dwFlags);

            public enum SymbolicLinkFlags : uint
            {
                SYMBLOC_LINK_FLAG_FILE      = 0x0,
                SYMBLOC_LINK_FLAG_DIRECTORY = 0x1
            }
            #endregion

            #region File copy/move with progress
            /// <summary>
            /// Copies an existing file to a new file, notifying the application of its progress through a callback function.
            /// </summary>
            /// <param name="lpExistingFileName">The name of an existing file.</param>
            /// <param name="lpNewFileName">The name of the new file.</param>
            /// <param name="lpProgressRoutine">The address of a callback function of type LPPROGRESS_ROUTINE that is called each time another portion of the file has been copied. This parameter can be NULL. For more information on the progress callback function, see the CopyProgressRoutine function.</param>
            /// <param name="lpData">The argument to be passed to the callback function. This parameter can be NULL.</param>
            /// <param name="pbCancel">If this flag is set to TRUE during the copy operation, the operation is canceled. Otherwise, the copy operation will continue to completion.</param>
            /// <param name="dwCopyFlags">Flags that specify how the file is to be copied. This parameter can be a combination of the following values.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To get extended error information call GetLastError.
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

            /// <summary>
            /// Moves a file or directory, including its children. You can provide a callback function that receives progress notifications.
            /// </summary>
            /// <param name="lpExistingFileName">The name of the existing file or directory on the local computer.</param>
            /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
            /// <param name="lpProgressRoutine">A pointer to a CopyProgressRoutine callback function that is called each time another portion of the file has been moved. The callback function can be useful if you provide a user interface that displays the progress of the operation. This parameter can be NULL.</param>
            /// <param name="lpData">An argument to be passed to the CopyProgressRoutine callback function. This parameter can be NULL.</param>
            /// <param name="dwFlags">The move options. This parameter can be one or more of the following values.</param>
            /// <returns>
            /// If the function succeeds, the return value is nonzero.
            /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
            /// </returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MoveFileWithProgress(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, MoveFileFlags dwFlags);

            /// <summary>
            /// An application-defined callback function used with the CopyFileEx, MoveFileTransacted, and MoveFileWithProgress functions. It is called when a portion of a copy or move operation is completed. The LPPROGRESS_ROUTINE type defines a pointer to this callback function. CopyProgressRoutine is a placeholder for the application-defined function name.
            /// </summary>
            public delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

            public enum CopyProgressResult : uint
            {
                PROGRESS_CONTINUE = 0,
                PROGRESS_CANCEL   = 1,
                PROGRESS_STOP     = 2,
                PROGRESS_QUIET    = 3
            }

            public enum CopyProgressCallbackReason : uint
            {
                CALLBACK_CHUNK_FINISHED = 0x00000000,
                CALLBACK_STREAM_SWITCH  = 0x00000001
            }

            [Flags]
            public enum CopyFileFlags : uint
            {
                COPY_FILE_FAIL_IF_EXISTS              = 0x00000001,
                COPY_FILE_RESTARTABLE                 = 0x00000002,
                COPY_FILE_OPEN_SOURCE_FOR_WRITE       = 0x00000004,
                COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
            }

            [Flags]
            public enum MoveFileFlags : uint
            {
                MOVE_FILE_REPLACE_EXISTSING     = 0x00000001,
                MOVE_FILE_COPY_ALLOWED          = 0x00000002,
                MOVE_FILE_DELAY_UNTIL_REBOOT    = 0x00000004,
                MOVE_FILE_WRITE_THROUGH         = 0x00000008,
                MOVE_FILE_CREATE_HARDLINK       = 0x00000010,
                MOVE_FILE_FAIL_IF_NOT_TRACKABLE = 0x00000020
            }
            #endregion

            #region Aero
            /// <summary>
            /// Extends the window frame into the client area.
            /// </summary>
            /// <param name="hWnd">The handle to the window in which the frame will be extended into the client area.</param>
            /// <param name="pMargins">A pointer to a <c>MARGINS</c> structure that describes the margins to use when extending the frame into the client area.</param>
            /// <returns>
            /// If this function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.
            /// </returns>
            [DllImport("dwmapi.dll")]
            public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);

            /// <summary>
            /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled.
            /// Applications can listen for composition state changes by handling the <c>WM_DWMCOMPOSITIONCHANGED</c> notification.
            /// </summary>
            /// <returns>
            /// A pointer to a value that, when this function returns successfully, receives <c>TRUE</c> if DWM composition is enabled; otherwise, <c>FALSE</c>.
            /// </returns>
            [DllImport("dwmapi.dll", PreserveSig = false)]
            public static extern bool DwmIsCompositionEnabled();

            /// <summary>
            /// Returned by the <c>GetThemeMargins</c> function to define the margins of windows that have visual styles applied.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct MARGINS
            {
                /// <summary>
                /// Width of the left border that retains its size.
                /// </summary>
                public int cxLeftWidth;

                /// <summary>
                /// Width of the right border that retains its size.
                /// </summary>
                public int cxRightWidth;

                /// <summary>
                /// Width of the top border that retains its size.
                /// </summary>
                public int cyTopHeight;

                /// <summary>
                /// Width of the bottom border that retains its size.
                /// </summary>
                public int cyBottomHeight;
            }
            #endregion

            #region IP Helper
            /// <summary>
            /// The <c>GetExtendedTcpTable</c> function retrieves a table that contains a list of TCP endpoints available to the application.
            /// </summary>
            /// <param name="pTcpTable">A pointer to the table structure that contains the filtered TCP endpoints available to the application. For information about how to determine the type of table returned based on specific input parameter combinations, see the Remarks section later in this document.</param>
            /// <param name="pdwSize">The estimated size of the structure returned in pTcpTable, in bytes. If this value is set too small, ERROR_INSUFFICIENT_BUFFER is returned by this function, and this field will contain the correct size of the structure.</param>
            /// <param name="bOrder">A value that specifies whether the TCP connection table should be sorted. If this parameter is set to TRUE, the TCP endpoints in the table are sorted in ascending order, starting with the lowest local IP address. If this parameter is set to FALSE, the TCP endpoints in the table appear in the order in which they were retrieved.</param>
            /// <param name="ulAf">The version of IP used by the TCP endpoints.</param>
            /// <param name="TableClass">The type of the TCP table structure to retrieve. This parameter can be one of the values from the TCP_TABLE_CLASS enumeration.</param>
            /// <param name="Reserved">Reserved. This value must be zero.</param>
            /// <returns>
            /// If the call is successful, the value NO_ERROR is returned.
            /// </returns>
            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, int ulAf, TcpTableType TableClass, int Reserved);

            /// <summary>
            /// The <c>TCP_TABLE_CLASS</c> enumeration defines the set of values used to indicate the type of table returned by calls to <c>GetExtendedTcpTable</c>.
            /// </summary>
            public enum TcpTableType
            {
                BasicListener,
                BasicConnections,
                BasicAll,
                OwnerPidListener,
                OwnerPidConnections,
                OwnerPidAll,
                OwnerModuleListener,
                OwnerModuleConnections,
                OwnerModuleAll,
            }

            /// <summary>
            /// The <c>MIB_TCPTABLE_OWNER_PID</c> structure contains a table of process IDs (PIDs) and the IPv4 TCP links that are context bound to these PIDs.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TcpTable
            {
                public uint length;
                public TcpRow row;
            }

            /// <summary>
            /// The <c>MIB_TCPROW_OWNER_PID</c> structure contains information that describes an IPv4 TCP connection with IPv4 addresses, ports used by the TCP connection, and the specific process ID (PID) associated with connection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TcpRow
            {
                public TcpState state;
                public uint localAddr;
                public byte localPort1;
                public byte localPort2;
                public byte localPort3;
                public byte localPort4;
                public uint remoteAddr;
                public byte remotePort1;
                public byte remotePort2;
                public byte remotePort3;
                public byte remotePort4;
                public int owningPid;
            }
            #endregion

            #region Single instance
            [DllImport("user32")]
            public static extern int RegisterWindowMessage(string message);

            public static int RegisterWindowMessage(string format, params object[] args)
            {
                string message = String.Format(format, args);
                return RegisterWindowMessage(message);
            }

            public const int HWND_BROADCAST = 0xffff;
            public const int SW_SHOWNORMAL = 1;

            [DllImport("user32")]
            public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

            [DllImportAttribute("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImportAttribute("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            public static void ShowToFront(IntPtr window)
            {
                ShowWindow(window, SW_SHOWNORMAL);
                SetForegroundWindow(window);
            }
            #endregion

            #region Signature verification
            [DllImport("mscoree.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool StrongNameSignatureVerificationEx(
                 [MarshalAs(UnmanagedType.LPWStr)]string wszFilePath,
                 [MarshalAs(UnmanagedType.U1)]bool fForceVerification,
                 [MarshalAs(UnmanagedType.U1)]ref bool pfWasVerified);

            [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
            public static extern WinVerifyTrustResult WinVerifyTrust(
                 [In] IntPtr hwnd,
                 [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
                 [In] WinTrustData pWVTData);

            #region WinTrustData struct field enums
            enum WinTrustDataUIChoice : uint
            {
                All = 1,
                None = 2,
                NoBad = 3,
                NoGood = 4
            }

            enum WinTrustDataRevocationChecks : uint
            {
                None = 0x00000000,
                WholeChain = 0x00000001
            }

            enum WinTrustDataChoice : uint
            {
                File = 1,
                Catalog = 2,
                Blob = 3,
                Signer = 4,
                Certificate = 5
            }

            enum WinTrustDataStateAction : uint
            {
                Ignore = 0x00000000,
                Verify = 0x00000001,
                Close = 0x00000002,
                AutoCache = 0x00000003,
                AutoCacheFlush = 0x00000004
            }

            [FlagsAttribute]
            enum WinTrustDataProvFlags : uint
            {
                UseIe4TrustFlag = 0x00000001,
                NoIe4ChainFlag = 0x00000002,
                NoPolicyUsageFlag = 0x00000004,
                RevocationCheckNone = 0x00000010,
                RevocationCheckEndCert = 0x00000020,
                RevocationCheckChain = 0x00000040,
                RevocationCheckChainExcludeRoot = 0x00000080,
                SaferFlag = 0x00000100,        // Used by software restriction policies. Should not be used.
                HashOnlyFlag = 0x00000200,
                UseDefaultOsverCheck = 0x00000400,
                LifetimeSigningFlag = 0x00000800,
                CacheOnlyUrlRetrieval = 0x00001000,      // affects CRL retrieval and AIA retrieval
                DisableMD2andMD4 = 0x00002000      // Win7 SP1+: Disallows use of MD2 or MD4 in the chain except for the root 
            }

            enum WinTrustDataUIContext : uint
            {
                Execute = 0,
                Install = 1
            }
            #endregion

            #region WinTrust structures
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public class WinTrustFileInfo
            {
                UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));
                IntPtr pszFilePath;                     // required, file name to be verified
                IntPtr hFile = IntPtr.Zero;             // optional, open handle to FilePath
                IntPtr pgKnownSubject = IntPtr.Zero;    // optional, subject type if it is known

                public WinTrustFileInfo(String _filePath)
                {
                    pszFilePath = Marshal.StringToCoTaskMemAuto(_filePath);
                }
                ~WinTrustFileInfo()
                {
                    Marshal.FreeCoTaskMem(pszFilePath);
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public class WinTrustData
            {
                UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
                IntPtr PolicyCallbackData = IntPtr.Zero;
                IntPtr SIPClientData = IntPtr.Zero;
                // required: UI choice
                WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;
                // required: certificate revocation check options
                WinTrustDataRevocationChecks RevocationChecks = WinTrustDataRevocationChecks.None;
                // required: which structure is being passed in?
                WinTrustDataChoice UnionChoice = WinTrustDataChoice.File;
                // individual file
                IntPtr FileInfoPtr;
                WinTrustDataStateAction StateAction = WinTrustDataStateAction.Ignore;
                IntPtr StateData = IntPtr.Zero;
                String URLReference = null;
                WinTrustDataProvFlags ProvFlags = WinTrustDataProvFlags.RevocationCheckChainExcludeRoot;
                WinTrustDataUIContext UIContext = WinTrustDataUIContext.Execute;

                // constructor for silent WinTrustDataChoice.File check
                public WinTrustData(String _fileName)
                {
                    // On Win7SP1+, don't allow MD2 or MD4 signatures
                    if ((Environment.OSVersion.Version.Major > 6) ||
                        ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor > 1)) ||
                        ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1) && !String.IsNullOrEmpty(Environment.OSVersion.ServicePack)))
                    {
                        ProvFlags |= WinTrustDataProvFlags.DisableMD2andMD4;
                    }

                    WinTrustFileInfo wtfiData = new WinTrustFileInfo(_fileName);
                    FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
                    Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
                }
                ~WinTrustData()
                {
                    Marshal.FreeCoTaskMem(FileInfoPtr);
                }
            }
            #endregion

            public enum WinVerifyTrustResult : uint
            {
                Success = 0,
                ProviderUnknown = 0x800b0001,           // Trust provider is not recognized on this system
                ActionUnknown = 0x800b0002,         // Trust provider does not support the specified action
                SubjectFormUnknown = 0x800b0003,        // Trust provider does not support the form specified for the subject
                SubjectNotTrusted = 0x800b0004,         // Subject failed the specified verification action
                FileNotSigned = 0x800B0100,         // TRUST_E_NOSIGNATURE - File was not signed
                SubjectExplicitlyDistrusted = 0x800B0111,   // Signer's certificate is in the Untrusted Publishers store
                SignatureOrFileCorrupt = 0x80096010,    // TRUST_E_BAD_DIGEST - file was probably corrupt
                SubjectCertExpired = 0x800B0101,        // CERT_E_EXPIRED - Signer's certificate was expired
                SubjectCertificateRevoked = 0x800B010       // CERT_E_REVOKED Subject's certificate was revoked
            }

            sealed class WinTrust
            {
                private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
                // GUID of the action to perform
                private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

                [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
                static extern WinVerifyTrustResult WinVerifyTrust(
                    [In] IntPtr hwnd,
                    [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
                    [In] WinTrustData pWVTData
                );

                // call WinTrust.WinVerifyTrust() to check embedded file signature
                public static bool VerifyEmbeddedSignature(string fileName)
                {
                    WinTrustData wtd = new WinTrustData(fileName);
                    Guid guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
                    WinVerifyTrustResult result = WinVerifyTrust(INVALID_HANDLE_VALUE, guidAction, wtd);
                    bool ret = (result == WinVerifyTrustResult.Success);
                    return ret;
                }
                private WinTrust() { }
            }
            #endregion
        }
        // ReSharper restore InconsistentNaming
    }
}
