﻿// Shane.Macaulay@IOActive.com Copyright (C) 2013-2015

//Copyright(C) 2015 Shane Macaulay

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.


using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace inVtero.net
{
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class DetectedProc
    {
        public DetectedProc()
        {
            TopPageTablePage = new Dictionary<int, long>();
        }
        public int Group;       // linux only 
        public VMCS vmcs;       // vmcs if available
        public PageTable PT;    // extracted page table
        public long CR3Value;
        public long FileOffset;
        public long Diff;
        public int Mode; // 1 or 2
        public PTType PageTableType;
        public CODEVIEW_HEADER DebugData;
        public Dictionary<int, long> TopPageTablePage;

        // the high bit signals if we collected a kernel address space for this AS group
        public int AddressSpaceID;

        public byte[] VGetBlock(long VA)
        {
            bool GotData = false;
            long[] rv = new long[512];
            byte[] buffer = new byte[4096];

            var _va = VA & ~0xfff;

            HARDWARE_ADDRESS_ENTRY hw;
            if (vmcs == null)
                hw = MemAccess.VirtualToPhysical(CR3Value, _va);
            else
                hw = MemAccess.VirtualToPhysical(vmcs.EPTP, CR3Value, _va);
            
            unsafe
            {
                fixed (void* lp = rv, bp = buffer)
                {
                    MemAccess.GetPageForPhysAddr(hw, ref rv, ref GotData, true);
                    Buffer.MemoryCopy((byte*)lp, (byte*)bp, 4096, 4096);
                }
            }
            return buffer;
        }

        [ProtoIgnore]
        public Mem MemAccess { get; set; }

        public override string ToString() => $"Process CR3 [{CR3Value:X12}] File Offset [{FileOffset:X12}] Diff [{Diff:X12}] Type [{PageTableType}] VMCS [{vmcs}]";

    }


    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class VMCS
    {
        public VMCS()
        {
        }


        [ProtoIgnore]
        public DetectedProc dp; // which proc this came from
        public long gCR3_off;
        public long gCR3;
        public long hCR3_off;
        public long hCR3;
        public long EPTP;
        public long EPTP_off;

        public override string ToString() => $"EPTP:[{new EPTP(this.EPTP):X12}]";
    }

    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class CODEVIEW_HEADER
    {
        public uint Sig;
        public Guid aGuid;
        public uint Age;
        public string PdbName;
    }

}