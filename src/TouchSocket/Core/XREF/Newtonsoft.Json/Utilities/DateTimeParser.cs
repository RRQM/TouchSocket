//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion License

using System;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Utilities
{
    internal enum ParserTimeZone
    {
        Unspecified = 0,
        Utc = 1,
        LocalWestOfUtc = 2,
        LocalEastOfUtc = 3
    }

    internal struct DateTimeParser
    {
        static DateTimeParser()
        {
            Power10 = new[] { -1, 10, 100, 1000, 10000, 100000, 1000000 };

            Lzyyyy = "yyyy".Length;
            Lzyyyy_ = "yyyy-".Length;
            Lzyyyy_MM = "yyyy-MM".Length;
            Lzyyyy_MM_ = "yyyy-MM-".Length;
            Lzyyyy_MM_dd = "yyyy-MM-dd".Length;
            Lzyyyy_MM_ddT = "yyyy-MM-ddT".Length;
            LzHH = "HH".Length;
            LzHH_ = "HH:".Length;
            LzHH_mm = "HH:mm".Length;
            LzHH_mm_ = "HH:mm:".Length;
            LzHH_mm_ss = "HH:mm:ss".Length;
            Lz_ = "-".Length;
            Lz_zz = "-zz".Length;
        }

        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public int Fraction;
        public int ZoneHour;
        public int ZoneMinute;
        public ParserTimeZone Zone;

        private char[] _text;
        private int _end;

        private static readonly int[] Power10;

        private static readonly int Lzyyyy;
        private static readonly int Lzyyyy_;
        private static readonly int Lzyyyy_MM;
        private static readonly int Lzyyyy_MM_;
        private static readonly int Lzyyyy_MM_dd;
        private static readonly int Lzyyyy_MM_ddT;
        private static readonly int LzHH;
        private static readonly int LzHH_;
        private static readonly int LzHH_mm;
        private static readonly int LzHH_mm_;
        private static readonly int LzHH_mm_ss;
        private static readonly int Lz_;
        private static readonly int Lz_zz;

        private const short MaxFractionDigits = 7;

        public bool Parse(char[] text, int startIndex, int length)
        {
            this._text = text;
            this._end = startIndex + length;

            if (this.ParseDate(startIndex) && this.ParseChar(Lzyyyy_MM_dd + startIndex, 'T') && this.ParseTimeAndZoneAndWhitespace(Lzyyyy_MM_ddT + startIndex))
            {
                return true;
            }

            return false;
        }

        private bool ParseDate(int start)
        {
            return (this.Parse4Digit(start, out this.Year)
                    && 1 <= this.Year
                    && this.ParseChar(start + Lzyyyy, '-')
                    && this.Parse2Digit(start + Lzyyyy_, out this.Month)
                    && 1 <= this.Month
                    && this.Month <= 12
                    && this.ParseChar(start + Lzyyyy_MM, '-')
                    && this.Parse2Digit(start + Lzyyyy_MM_, out this.Day)
                    && 1 <= this.Day
                    && this.Day <= DateTime.DaysInMonth(this.Year, this.Month));
        }

        private bool ParseTimeAndZoneAndWhitespace(int start)
        {
            return (this.ParseTime(ref start) && this.ParseZone(start));
        }

        private bool ParseTime(ref int start)
        {
            if (!(this.Parse2Digit(start, out this.Hour)
                  && this.Hour <= 24
                  && this.ParseChar(start + LzHH, ':')
                  && this.Parse2Digit(start + LzHH_, out this.Minute)
                  && this.Minute < 60
                  && this.ParseChar(start + LzHH_mm, ':')
                  && this.Parse2Digit(start + LzHH_mm_, out this.Second)
                  && this.Second < 60
                  && (this.Hour != 24 || (this.Minute == 0 && this.Second == 0)))) // hour can be 24 if minute/second is zero)
            {
                return false;
            }

            start += LzHH_mm_ss;
            if (this.ParseChar(start, '.'))
            {
                this.Fraction = 0;
                int numberOfDigits = 0;

                while (++start < this._end && numberOfDigits < MaxFractionDigits)
                {
                    int digit = this._text[start] - '0';
                    if (digit < 0 || digit > 9)
                    {
                        break;
                    }

                    this.Fraction = (this.Fraction * 10) + digit;

                    numberOfDigits++;
                }

                if (numberOfDigits < MaxFractionDigits)
                {
                    if (numberOfDigits == 0)
                    {
                        return false;
                    }

                    this.Fraction *= Power10[MaxFractionDigits - numberOfDigits];
                }

                if (this.Hour == 24 && this.Fraction != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ParseZone(int start)
        {
            if (start < this._end)
            {
                char ch = this._text[start];
                if (ch == 'Z' || ch == 'z')
                {
                    this.Zone = ParserTimeZone.Utc;
                    start++;
                }
                else
                {
                    if (start + 2 < this._end
                        && this.Parse2Digit(start + Lz_, out this.ZoneHour)
                        && this.ZoneHour <= 99)
                    {
                        switch (ch)
                        {
                            case '-':
                                this.Zone = ParserTimeZone.LocalWestOfUtc;
                                start += Lz_zz;
                                break;

                            case '+':
                                this.Zone = ParserTimeZone.LocalEastOfUtc;
                                start += Lz_zz;
                                break;
                        }
                    }

                    if (start < this._end)
                    {
                        if (this.ParseChar(start, ':'))
                        {
                            start += 1;

                            if (start + 1 < this._end
                                && this.Parse2Digit(start, out this.ZoneMinute)
                                && this.ZoneMinute <= 99)
                            {
                                start += 2;
                            }
                        }
                        else
                        {
                            if (start + 1 < this._end
                                && this.Parse2Digit(start, out this.ZoneMinute)
                                && this.ZoneMinute <= 99)
                            {
                                start += 2;
                            }
                        }
                    }
                }
            }

            return (start == this._end);
        }

        private bool Parse4Digit(int start, out int num)
        {
            if (start + 3 < this._end)
            {
                int digit1 = this._text[start] - '0';
                int digit2 = this._text[start + 1] - '0';
                int digit3 = this._text[start + 2] - '0';
                int digit4 = this._text[start + 3] - '0';
                if (0 <= digit1 && digit1 < 10
                    && 0 <= digit2 && digit2 < 10
                    && 0 <= digit3 && digit3 < 10
                    && 0 <= digit4 && digit4 < 10)
                {
                    num = (((((digit1 * 10) + digit2) * 10) + digit3) * 10) + digit4;
                    return true;
                }
            }
            num = 0;
            return false;
        }

        private bool Parse2Digit(int start, out int num)
        {
            if (start + 1 < this._end)
            {
                int digit1 = this._text[start] - '0';
                int digit2 = this._text[start + 1] - '0';
                if (0 <= digit1 && digit1 < 10
                    && 0 <= digit2 && digit2 < 10)
                {
                    num = (digit1 * 10) + digit2;
                    return true;
                }
            }
            num = 0;
            return false;
        }

        private bool ParseChar(int start, char ch)
        {
            return (start < this._end && this._text[start] == ch);
        }
    }
}