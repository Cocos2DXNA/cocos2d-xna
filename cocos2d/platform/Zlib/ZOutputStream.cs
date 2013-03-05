// Copyright (c) 2006, ComponentAce
// http://www.componentace.com
// All rights reserved.

// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
// Neither the name of ComponentAce nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


/*
Copyright (c) 2001 Lapo Luchini.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright 
notice, this list of conditions and the following disclaimer in 
the documentation and/or other materials provided with the distribution.

3. The names of the authors may not be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS
OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
/*
* This program is based on zlib-1.1.3, so all credit should go authors
* Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
* and contributors of zlib.
*/

using System;
using System.IO;

namespace cocos2d.Compression.Zlib
{
    public class ZOutputStream : Stream
    {
        protected internal byte[] buf, buf1 = new byte[1];
        protected internal int bufsize = 4096;
        protected internal bool compress;
        protected internal int flush_Renamed_Field;

        private Stream out_Renamed;
        protected internal ZStream z = new ZStream();

        public ZOutputStream(Stream out_Renamed)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.InflateInit();
            compress = false;
        }

        public ZOutputStream(Stream out_Renamed, int level)
        {
            InitBlock();
            this.out_Renamed = out_Renamed;
            z.DeflateInit(level);
            compress = true;
        }

        public virtual int FlushMode
        {
            get { return (flush_Renamed_Field); }

            set { flush_Renamed_Field = value; }
        }

        /// <summary> Returns the total number of bytes input so far.</summary>
        public virtual long TotalIn
        {
            get { return z.total_in; }
        }

        /// <summary> Returns the total number of bytes output so far.</summary>
        public virtual long TotalOut
        {
            get { return z.total_out; }
        }

        public override Boolean CanRead
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Boolean CanSeek
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Boolean CanWrite
        {
            get { return false; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Length
        {
            get { return 0; }
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Position
        {
            get { return 0; }

            set { }
        }

        private void InitBlock()
        {
            flush_Renamed_Field = zlibConst.Z_NO_FLUSH;
            buf = new byte[bufsize];
        }

        public void WriteByte(int b)
        {
            buf1[0] = (byte) b;
            Write(buf1, 0, 1);
        }

        //UPGRADE_TODO: The differences in the Expected value  of parameters for method 'WriteByte'  may cause compilation errors.  'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092_3"'
        public override void WriteByte(byte b)
        {
            WriteByte(b);
        }

        public override void Write(Byte[] b1, int off, int len)
        {
            if (len == 0)
                return;
            int err;
            var b = new byte[b1.Length];
            Array.Copy(b1, 0, b, 0, b1.Length);
            z.next_in = b;
            z.next_in_index = off;
            z.avail_in = len;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                    err = z.Deflate(flush_Renamed_Field);
                else
                    err = z.Inflate(flush_Renamed_Field);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                out_Renamed.Write(buf, 0, bufsize - z.avail_out);
            } while (z.avail_in > 0 || z.avail_out == 0);
        }

        public virtual void Finish()
        {
            int err;
            do
            {
                z.next_out = buf;
                z.next_out_index = 0;
                z.avail_out = bufsize;
                if (compress)
                {
                    err = z.Deflate(zlibConst.Z_FINISH);
                }
                else
                {
                    err = z.Inflate(zlibConst.Z_FINISH);
                }
                if (err != zlibConst.Z_STREAM_END && err != zlibConst.Z_OK)
                    throw new ZStreamException((compress ? "de" : "in") + "flating: " + z.msg);
                if (bufsize - z.avail_out > 0)
                {
                    out_Renamed.Write(buf, 0, bufsize - z.avail_out);
                }
            } while (z.avail_in > 0 || z.avail_out == 0);
            try
            {
                Flush();
            }
            catch
            {
            }
        }

        public virtual void End()
        {
            if (compress)
            {
                z.DeflateEnd();
            }
            else
            {
                z.InflateEnd();
            }
            z.Free();
            z = null;
        }
#if NETFX_CORE
        public void Close()
#else
        public override void Close()
#endif
        {
            try
            {
                try
                {
                    Finish();
                }
                catch
                {
                }
            }
            finally
            {
                End();
                out_Renamed.Close();
                out_Renamed = null;
            }
        }

        public override void Flush()
        {
            out_Renamed.Flush();
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
        {
            return 0;
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override void SetLength(Int64 value)
        {
        }

        //UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            return 0;
        }

        //UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
    }
}