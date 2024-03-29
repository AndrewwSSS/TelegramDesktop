﻿using System;
using System.Text;

namespace CommonLibrary
{
    public static class PasswordGenerator
    {
        private static Random random = new Random();

        public static string GeneratePassword(int length = 6)
        {
            if (length <= 0)
                throw new InvalidOperationException("attribute lenght invalid");

  
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                //isLetter
                if (random.Next(0, 2) == 0)
                {
                    var isBig = random.Next(0, 2);

                    if (isBig == 0)
                        sb.Append((char)random.Next('\u0041', '\u005A'));
                    else
                        sb.Append((char)random.Next('\u0061', '\u007A'));
                }
                else
                    sb.Append(random.Next(0, 10).ToString());

            }

            return sb.ToString();
        }
    }
}
