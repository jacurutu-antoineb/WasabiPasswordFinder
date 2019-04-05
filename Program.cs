using NBitcoin;
using NDesk.Options;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Collections;
using System.Collections.Generic;

namespace WasabiPasswordFinder
{
    internal class Program
    {
        // Function to write to STDOUT and to an output file
        //   @parm: str
        //            string to write to the output
        static void output(string str)
        {
           if (parameters.ofile != null)
           {
             File.AppendAllText(parameters.ofile, str+"\n");
           }
           Console.WriteLine(str);
        }

        // Function to convert a #seconds to something more legible
        //    @parm: seconds
        //           Total number of seconds 
        static string getTime(int seconds)
        {
           int minutes = seconds / 60;
           int hours = minutes / 60;
           int days = hours / 24;
           if (days > 0)
           {
              int hours_adj = hours % 24;
              return (days.ToString()+" days "+hours_adj.ToString()+" hours");
           }
           else if (hours > 0)
           {
              int minutes_adj = minutes % 60;
              return (hours.ToString()+" hours "+minutes_adj.ToString()+" minutes");
           }
           else if (minutes > 0)
           {
              int seconds_adj = seconds & 60;
              return (minutes.ToString()+" minutes "+seconds_adj.ToString()+" seconds");
           }
           return "";
        }

        // Function to convert a #seconds to something more legible
        //    @parm: strToTest
        //              This is the string that is currently under test
        //    @parm: enc
        //              Reference to the encryption object which contains
        //              the EncryptedSecret
        //    @parm: debug
        //              If this is enabled, print the result of the test to
        //              the STDOUT as well as to ofile
     
       static int TestPass(string strToTest, 
                           ref BitcoinEncryptedSecretNoEC enc, 
                           bool debug = true)
       {
         var sw = new Stopwatch();
         if (debug)
         {
            sw.Start();
            output("TEST ["+strToTest+"]");
         }
         try
           {
              enc.GetKey(strToTest);
              output("--KEY FOUND "+strToTest);
              return 0;
           }
         catch
           {
              if (debug)
              {
                sw.Stop();

                output("--FAIL ("+sw.Elapsed.Seconds.ToString()+"."+sw.Elapsed.Milliseconds.ToString()+")");
              }
              return -1;
           }
       }

       
       // Function to test all possible shift combinations of a password
       //   @parm: password
       //            This is the password as you think it was typed..
       //   @parm: enc
       //            Reference to the encryption object which contains
       //            the EncryptedSecret
       //    @parm: debug
       //            If this is enabled, print the result of the test to
       //            the STDOUT as well as to ofile
       static void TestShiftError(string password, 
                                  ref BitcoinEncryptedSecretNoEC enc,
                                  bool debug = true)
       {
          int n = password.Length;

          // Number of permutations is 2^n 
          int max = 1 << n;

          // We will track the number of combinations we've actually
          // performed for time to complete estimate
          int count = 0;
          
          // Track the number of seconds left to completion (Estimate)
          // We initially estimate 0.5s per test, this will get updated later once
          // we've begun running tests
          int seconds = max / 2;

          output("Max combinations "+max.ToString());
          
          output("-Estimated Time to Completion: "+getTime(seconds));
                    
          // The number of iterations that we'll use to average out how many
          // attempts per second (for time to complete estimate)
          int iter_average = 5;

          // Converting string 
          // to lower case 
          password = password.ToLower();

          // We use a stopwatch to estimate how long this test will take to run
          var sw = new Stopwatch();

          sw.Start();

          // Using all subsequences  
          // and permuting them  
          for(int i = 0;i < max; i++)
          {
             char []combination = password.ToCharArray();

             // If j-th bit is set, we  
             // convert it to upper case 
             for(int j = 0; j < n; j++)
             {
                if(((i >> j) & 1) == 1)
                {
                    switch(combination[j])
                    {
                        case '1':
                           combination[j] = '!';
                           break;
                        case '2':
                           combination[j] = '@';
                           break;
                        case '3':
                           combination[j] = '#';
                           break;
                        case '4':
                           combination[j] = '$';
                           break;
                        case '5':
                           combination[j] = '%';
                           break;
                        case '6':
                           combination[j] = '^';
                           break;
                        case '7':
                           combination[j] = '&';
                           break;
                        case '8':
                           combination[j] = '*';
                           break;
                        case '9':
                           combination[j] = '(';
                           break;
                        case '0':
                           combination[j] = ')';
                           break;
                        case '[':
                           combination[j] = '{';
                           break;
                        case ']':
                           combination[j] = '}';
                           break;
                        case ';':
                           combination[j] = ':';
                           break;
                        case '\'':
                           combination[j] = '"';
                           break;
                        // handles all a-z characters
                        default:
                           combination[j] = (char) (combination[j] - 32);
                           break;
                    }

                    // Printing current combination 
                    string s = new string(combination);
                    if (TestPass(s, ref enc, debug) == 0) 
                    {
                       output("Key Found "+s); 
                       return; 
                    }
                    count ++;


                    if (count % iter_average == 0)
                    {
                       TimeSpan time = sw.Elapsed;
                       decimal rate = (decimal)time.Seconds / (decimal)iter_average;
                       decimal timeleft = (decimal)(max - count) * rate;
                       seconds = (int)timeleft;
                       sw.Restart();

                       if (debug)
                       {
                          output("-Estimated Time to completion: "+getTime(seconds));
                       }
                    }

                 }
            }
          }
        }
 
       struct parms_t
       {
          public string password;
          public string secret;
          public string ofile;
          public List<string> tests;
          public bool debug;
          public bool help;
       };

       static parms_t parameters; 
       static List<string> myva = new List<string> ();
 

       static OptionSet  parms = new OptionSet () {
            { "x|password=", "The password you thought you typed",
                v => parameters.password = v},
            { "s|secret=", "The secret from your .json file (EncryptedSecret).",
                v => parameters.secret = v },
            { "o|ofile=", "Output file",
                v => parameters.ofile = v},
            { "t|tc=", "Test to perform \n\t-t shift_test\n\t-t single_char_test",
                v => parameters.tests.Add(v)},
            { "H", "Show Help",
                v => parameters.help = true},
            { "d", "Debug logging",
                v => parameters.debug = true}};

       static void ShowHelp (OptionSet p)
       {
          Console.WriteLine ("Usage: dotnet run [OPTIONS]+");
          Console.WriteLine ();
          Console.WriteLine ("Options:");
          p.WriteOptionDescriptions (Console.Out);
       }

       // Function to test all possible single character mistypes
       //   @parm: password
       //            This is the password as you think it was typed..
       //   @parm: enc
       //            Reference to the encryption object which contains
       //            the EncryptedSecret
       //    @parm: debug
       //            If this is enabled, print the result of the test to
       //            the STDOUT as well as to ofile
       private static int TestSingleCharError(string password, ref BitcoinEncryptedSecretNoEC enc, bool debug)
       {
          char[] chars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_- \"#$%&()=".ToArray(); 

          int max = password.Length * chars.Length;
          output("Total tests to perform: "+max);
          int count = 0;
          int seconds = max / 2;
          output("Estimated time to completion: "+getTime(seconds));

          // The number of iterations that we'll use to average out how many
          // attempts per second (for time to complete estimate)
          int iter_average = 5;

          var sw = new Stopwatch();
          sw.Start();
          var pwChar = password.ToCharArray();
          for (int i=0; i < password.Length; i++)
          {
              var original = pwChar[i]; 
              if (debug)
              {
                 output("Testing position "+i);
              }
              foreach(var c in chars)
              {
                  pwChar[i] = c;
                  string s = new string(pwChar); 
                  if (TestPass(s, ref enc, debug) == 0) 
                  {
                     output("Key Found "+s); 
                     return 0; 
                  }
                  count++;
                  if (count % iter_average == 0)
                  {
                     TimeSpan time = sw.Elapsed;
                     decimal rate = (decimal)time.Seconds / (decimal)iter_average;
                     decimal timeleft = (decimal)(max - count) * rate;
                     seconds = (int)timeleft;
                     sw.Restart();

                     if (debug)
                     {
                        output("-Estimated Time to completion: "+getTime(seconds));
                     }
                  }
              }
              pwChar[i] = original; 
          }
          return -1;
       }

       private static void Main(string[] args)
        {
            parameters.tests = new List<string> ();
            try 
            {
               parms.Parse(args);
               if (parameters.help)
               {
                  ShowHelp(parms);
                  return;
               }
               BitcoinEncryptedSecretNoEC encryptedSecret = new BitcoinEncryptedSecretNoEC(parameters.secret);
            
               for (int i = 0; i < parameters.tests.Count; ++i)
               {
                   string t = parameters.tests[i];
                   switch (t)
                   {
                        case "shift_test":
                           TestShiftError(parameters.password, ref encryptedSecret, parameters.debug);
                           break;
                        case "single_char_test":
                           TestSingleCharError(parameters.password, ref encryptedSecret, parameters.debug);
                           break;
                        default:
                           Console.WriteLine("Unknown test type ("+t+")");
                           break;
                   }
               }
            }
            catch (OptionException e) {
               Console.WriteLine ("Exception Thrown");
               Console.WriteLine (e.Message);
               ShowHelp(parms);
               return;
            }
        }
    }
}
