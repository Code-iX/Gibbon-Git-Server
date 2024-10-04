﻿/* Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
 * Microsoft Open Technologies would like to thank its contributors, a list
 * of whom are at http://aspnetwebstack.codeplex.com/wikipage?title=Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you
 * may not use this file except in compliance with the License. You may
 * obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
 * implied. See the License for the specific language governing permissions
 * and limitations under the License. */

using System.Net.Mime;

namespace Gibbon.Git.Server.Helpers;

public static class ContentDispositionUtil
{
    public static string GetHeaderValue(string fileName)
    {
        // If fileName contains any Unicode characters, encode according
        // to RFC 2231 (with clarifications from RFC 5987)
        if (fileName.Any(c => c > 127))
        {
            return CreateRfc2231HeaderValue(fileName);
        }

        // Knowing there are no Unicode characters in this fileName, rely on
        // ContentDisposition.ToString() to encode properly.
        // In .Net 4.0, ContentDisposition.ToString() throws FormatException if
        // the file name contains Unicode characters.
        // In .Net 4.5, ContentDisposition.ToString() no longer throws FormatException
        // if it contains Unicode, and it will not encode Unicode as we require here.
        // The Unicode test above is identical to the 4.0 FormatException test,
        // allowing this helper to give the same results in 4.0 and 4.5.         
        return new ContentDisposition()
        {
            FileName = fileName
        }.ToString();
    }

    private const string HexDigits = "0123456789ABCDEF";

    private static void AddByteToStringBuilder(byte b, StringBuilder builder)
    {
        builder.Append('%');
        var num = (int)b;
        AddHexDigitToStringBuilder(num >> 4, builder);
        AddHexDigitToStringBuilder(num % 16, builder);
    }

    private static void AddHexDigitToStringBuilder(int digit, StringBuilder builder)
    {
        builder.Append(HexDigits[digit]);
    }

    private static string CreateRfc2231HeaderValue(string filename)
    {
        var builder = new StringBuilder("attachment; filename*=UTF-8''");
        foreach (var b in Encoding.UTF8.GetBytes(filename))
        {
            if (IsByteValidHeaderValueCharacter(b))
                builder.Append((char)b);
            else
                AddByteToStringBuilder(b, builder);
        }
        return builder.ToString();
    }

    // Application of RFC 2231 Encoding to Hypertext Transfer Protocol (HTTP) Header Fields, sec. 3.2
    // http://greenbytes.de/tech/webdav/draft-reschke-rfc2231-in-http-latest.html
    private static bool IsByteValidHeaderValueCharacter(byte b)
    {
        switch (b)
        {
            case >= (byte)'0' and <= (byte)'9':
            case >= (byte)'a' and <= (byte)'z':
            case >= (byte)'A' and <= (byte)'Z':
                return true; 
            case (byte)'-':
            case (byte)'.':
            case (byte)'_':
            case (byte)'~':
            case (byte)':':
            case (byte)'!':
            case (byte)'$':
            case (byte)'&':
            case (byte)'+':
                return true;
            default:
                return false;
        }
    }
}