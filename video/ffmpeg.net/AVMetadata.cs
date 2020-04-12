//      libavformat/metadata.h
//00033 struct AVMetadata{
//00034     int count;
//00035     AVMetadataTag *elems;
//00036 };
//00037 
//00038 struct AVMetadataConv{
//00039     const char *native;
//00040     const char *generic;
//00041 };
//
//      libavformat/avformat.h
//00075 typedef struct {
//00076     char* key;
//00077     char* value;
//00078 }AVMetadataTag;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ffmpeg.net
{
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct AVMetadata
    {
        public int count;
        public /*AVMetadataTag* */IntPtr elems;
    }
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct AVMetadataTag
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public /*char* */string key;
        [MarshalAs(UnmanagedType.LPStr)]
        public /*char* */string value;
    }
}
