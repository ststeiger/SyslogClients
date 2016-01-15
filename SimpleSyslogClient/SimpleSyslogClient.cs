
// http://blogs.msdn.com/b/dotnetinterop/archive/2004/11/05/net-and-syslog.aspx

// This code works great with just one problem... the enums for the Facility are wrong, 
// especially if you are trying to hit the LocalX facilities.
// http://tools.ietf.org/html/rfc5424#section-6.2.1



// syslog.cs
// ------------------------------------------------------------------
//
// small class for sending log messages to Syslog from .NET
// 
// Author: Admin
// built on host: DINOCH-2
// Created Fri Jul 24 01:10:38 2009
//
// last saved: 
// Time-stamp: <2009-July-24 01:23:12>
// ------------------------------------------------------------------
//
// Copyright (c) 2009 by Dino Chiesa
// All rights reserved!
//
// This code is licensed under the Ms-PL license:
//
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license,
// do not use the software.
//
// 1. Definitions
//
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have
// the same meaning here as under U.S. copyright law.  A "contribution" is the original
// software, or any additions or changes to the software.  A "contributor" is any person
// that distributes its contribution under this license.  "Licensed patents" are a
// contributor's patent claims that read directly on its contribution.
//
// 
// 2. Grant of Rights
//
// (A) Copyright Grant- Subject to the terms of this license, including the license
//     conditions and limitations in section 3, each contributor grants you a
//     non-exclusive, worldwide, royalty-free copyright license to reproduce its
//     contribution, prepare derivative works of its contribution, and distribute its
//     contribution or any derivative works that you create.
//
// (B) Patent Grant- Subject to the terms of this license, including the license
//     conditions and limitations in section 3, each contributor grants you a
//     non-exclusive, worldwide, royalty-free license under its licensed patents to
//     make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
//     its contribution in the software or derivative works of the contribution in the
//     software.
//
// 3. Conditions and Limitations
//
// (A) No Trademark License- This license does not grant you rights to use any
//     contributors' name, logo, or trademarks.
//
// (B) If you bring a patent claim against any contributor over patents that you claim
//     are infringed by the software, your patent license from such contributor to the
//     software ends automatically.  (C) If you distribute any portion of the software,
//     you must retain all copyright, patent, trademark, and attribution notices that
//     are present in the software.
//
// (D) If you distribute any portion of the software in source code form, you may do so
//     only under this license by including a complete copy of this license with your
//     distribution. If you distribute any portion of the software in compiled or object
//     code form, you may only do so under a license that complies with this license.
//
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors
//     give no express warranties, guarantees or conditions. You may have additional
//     consumer rights under your local laws which this license cannot change. To the
//     extent permitted under your local laws, the contributors exclude the implied
//     warranties of merchantability, fitness for a particular purpose and
//     non-infringement.
//
// ------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;



namespace SimpleSyslogClient
{


    public enum Level
    {
        Emergency = 0,
        Alert = 1,
        Critical = 2,
        Error = 3,
        Warning = 4,
        Notice = 5,
        Information = 6,
        Debug = 7,
    }


    // http://tools.ietf.org/html/rfc5424#section-6.2.1
    public enum Facility
    {
        Kernel = 0,
        User = 1,
        Mail = 2,
        Daemon = 3,
        Auth = 4,
        Syslog = 5,
        Lpr = 6,
        News = 7,
        UUCP = 8,
        Cron = 9, // clock daemon
        
        Security=10, // security/authorization messages
        FTP=11, // FTP daemon
        NTP = 12,
        LogAudit =13,
        LogAlert=14,
        ClockDaemon=15,


        Local0 = 16,
        Local1 = 17,
        Local2 = 18,
        Local3 = 19,
        Local4 = 20,
        Local5 = 21,
        Local6 = 22,
        Local7 = 23,
    }


    public class Message
    {
        private int _facility;
        public int Facility
        {
            get { return _facility; }
            set { _facility = value; }
        }
        private int _level;
        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }
        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        public Message() { }
        public Message(int facility, int level, string text)
        {
            _facility = facility;
            _level = level;
            _text = text;
        }
    }


    /// need this helper class to expose the Active propery of UdpClient
    /// (why is it protected, anyway?) 
    public class UdpClientEx : System.Net.Sockets.UdpClient
    {
        public UdpClientEx() : base() { }
        public UdpClientEx(IPEndPoint ipe) : base(ipe) { }
        ~UdpClientEx()
        {
            if (this.Active) this.Close();
        }

        public bool IsActive
        {
            get { return this.Active; }
        }
    }


    public class Client
    {
        private IPHostEntry ipHostInfo;
        private IPAddress ipAddress;
        private IPEndPoint ipLocalEndPoint;
        private UdpClientEx udpClient;
        private string _sysLogServerIp = null;
        private int _port = 514;

        public Client()
        {
            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            ipLocalEndPoint = new IPEndPoint(ipAddress, 0);
            udpClient = new UdpClientEx(ipLocalEndPoint);
        }

        public bool IsActive
        {
            get { return udpClient.IsActive; }
        }

        public void Close()
        {
            if (udpClient.IsActive) udpClient.Close();
        }

        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        public string SysLogServerIp
        {
            get { return _sysLogServerIp; }
            set
            {
                if ((_sysLogServerIp == null) && (!IsActive))
                {
                    _sysLogServerIp = value;
                    //udpClient.Connect(_hostIp, _port);
                }
            }
        }

        public void Send(Message message)
        {
            if (!udpClient.IsActive)
                udpClient.Connect(_sysLogServerIp, _port);
            if (udpClient.IsActive)
            {
                int priority = message.Facility * 8 + message.Level;
                string msg = System.String.Format("<{0}>{1} {2} {3}",
                                                  priority,
                                                  DateTime.Now.ToString("MMM dd HH:mm:ss"),
                                                  ipLocalEndPoint.Address,
                                                  message.Text);
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(msg);
                udpClient.Send(bytes, bytes.Length);
            }
            else throw new Exception("Syslog client Socket is not connected. Please set the SysLogServerIp property");
        }


        // I've modified your Send() method to include client IP and timestamp in the message header, so I thought I'd share:
        public void Send_WithIpAndTimestamp(Message message)
        {
            if (!udpClient.IsActive)
                udpClient.Connect(_sysLogServerIp, _port);

            if (udpClient.IsActive)
            {
                int priority = message.Facility * 8 + message.Level;

                string msg = System.String.Format("<{0}>{1} {2} {3}",
                                                 priority,
                                                 DateTime.Now.ToString("MMM dd HH:mm:ss"),
                                                 ipLocalEndPoint.Address,
                                                 message.Text);

                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(msg);
                udpClient.Send(bytes, bytes.Length);
            }
            else throw new Exception("Syslog client Socket is not connected. Please set the host IP");
        } // End Sub Send_WithIpAndTimestamp


    }


}
