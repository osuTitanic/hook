// Licensed under the MIT License
// Copyright (c) 2017 NeoSmart Technologies
// https://github.com/neosmart/Stream.CopyTo/blob/master/Stream.CopyTo/StreamCopyExtension.cs
using System;

#if NET20
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    internal sealed class ExtensionAttribute : Attribute { }
}

namespace System.IO
{
    public static class StreamCopyExtension
    {
        public static void CopyTo(this Stream srcStream, Stream dstStream, int bufferSize = 4096)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = srcStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                dstStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}
#endif