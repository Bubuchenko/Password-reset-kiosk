using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CoreClassLib
{
    public static class Core
    {
        public static string LDAP_URL { get; set; }
        public static string BasePassword { get; set; }
        public static bool RandomPassword { get; set; }
        public static int FinalScreenShowTime { get; set; }
        public static bool MaxResetLimit { get; set; }
        public static int MaxResetCount { get; set; }
        public static int UserIDLength { get; set; }
        public static int RFIDLength { get; set; }
        public static bool UsePrinter { get; set; }
        //Screen refresh in seconds
        public static int ScreenProtectionRefreshInterval = 3600;

        //Text files for storing configurations and usage activities
        public const string CONFIG_FILE = "config.ini";
        public const string ERROR_FILE = "ErrorLog.txt";
        public const string ACTIVITY_FILE = "ActivityLog.txt";

        //Password error scheme
        //Code 1 = User not found
        //Code 2 = Reset limit exceeded
        //Code 3 = Success
        //Code 4 = Unknown error
        //Code 5 = Account disabled

        public static async Task<int> ResetPassword(string studentNummer, string password)
        {
            //Stopwatch is used to force the task to not be completed before atleast 2 seconds have passed, so the animation gets played regardless
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                string leerlingnaam = "";
                int passwordResetCount = 0;

                //If no specific password is given, use the password from the config
                if (password == null)
                {
                    password = GetPassword();
                }

                using (DirectoryEntry dir = new DirectoryEntry(LDAP_URL)) //Instantiate dir entry and pass the domain
                {
                    using (DirectorySearcher search = new DirectorySearcher(dir)) //Search query instance
                    {
                        search.Filter = "(&(objectClass=user)(pager=" + studentNummer + "))"; //Filter by pager (Student number)
                        search.PropertiesToLoad.Add("telephoneNumber"); //So we can use the "pager" property to search by student ID
                        SearchResult searchresult = search.FindOne();

                        //User not found or disabled
                        if (searchresult == null)
                        {
                            if (sw.Elapsed.Seconds < 2)
                                await Task.Delay(2000);
                            return 1;
                        }

                        using (DirectoryEntry uEntry = searchresult.GetDirectoryEntry())
                        {
                            leerlingnaam = uEntry.Properties["givenName"].Value.ToString() + " " + uEntry.Properties["sn"].Value.ToString(); //Store full student name in string

                            //If user doesn't exist
                            if (leerlingnaam == "")
                            {
                                if (sw.Elapsed.Seconds < 2)
                                    await Task.Delay(2000);
                                return 1;
                            }

                            //Check if account is disabled
                            if (!IsActive(uEntry))
                            {
                                if (sw.Elapsed.Seconds < 2)
                                    await Task.Delay(2000);

                                return 5;
                            }

                            try
                            {
                                passwordResetCount = int.Parse(uEntry.Properties["Description"].Value.ToString()); //Get users pass reset count, which is stored in description
                            }
                            catch //If it appears to be empty, set it to 0
                            {
                                uEntry.Properties["Description"].Value = "0";
                            }

                            //If password count field is empty, (never reset before) set it to 0 at least
                            if (!IsDigitsOnly(passwordResetCount.ToString()))
                            {
                                passwordResetCount = 0;
                            }

                            //Check if user exceeded their amount of tries
                            if (passwordResetCount >= MaxResetCount)
                            {
                                writeActivityLog(String.Format("Leerling {0} met leerling nummer {1}, heeft wachtwoord niet kunnen wijzigen. Reset limiet bereikt.", leerlingnaam, studentNummer));
                                if (sw.Elapsed.Seconds < 2)
                                    await Task.Delay(2000);
                                return 2;
                            }

                            //If password never expires is checked, uncheck it so the user can change their password on their first login
                            int NON_EXPIRE_FLAG = 0x10000;
                            var val = (int)uEntry.Properties["userAccountControl"].Value;
                            if (val.ToString() == "66048") //If password is set to never expire, undo it.
                            {
                                uEntry.Properties["userAccountControl"].Value = val ^ NON_EXPIRE_FLAG; //Set the flag
                            }


                            uEntry.Properties["LockOutTime"].Value = 0; //Unlock account
                            uEntry.Invoke("SetPassword", new object[] { password }); //Change the actual password
                            uEntry.Properties["pwdLastSet"].Value = 0; // Force user to change password at first logon
                            uEntry.Properties["Description"].Value = passwordResetCount + 1; //Increment users password reset count
                            uEntry.CommitChanges();
                            uEntry.Close();

                            if (UsePrinter)
                                Printer.Print(Texts.PrintedMessage(studentNummer, password));


                            writeActivityLog(String.Format("Leerling {0} met leerling nummer {1}, heeft wachtwoord gewijzigd (milliseconds: {2})", leerlingnaam, studentNummer, sw.Elapsed.Milliseconds));

                            if (sw.Elapsed.Seconds < 2)
                                await Task.Delay(2000);

                            return 3;
                        }
                    }
                }
            }
            catch // Unknown error
            {
                writeActivityLog("----------------NOTIFICATION-------------------");
                writeActivityLog("Unknown error occured student number: " + studentNummer);
                return 4;
            }
        }

        public static bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }

        //Function that makes sure the read barcode is correct
        public static bool ValidateInput(string input)
        {
            //Remove any new lines if there are any, which COULD be caused by the enter.
            input = input.Replace(Environment.NewLine, "");

            //Something has to be entered
            if (input.Length == 0)
                return false;

            //Only accept numeric values
            if (!IsDigitsOnly(input))
                return false;

            return true;
        }

        private static string GetPassword()
        {
            if (RandomPassword)
            {
                Random r = new Random();
                string randomDigits = r.Next(100, 999).ToString();
                return BasePassword + randomDigits;
            }
            else
            {
                return BasePassword;
            }
        }
        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }

        //Functions to write activity and error text files
        private static void writeActivityLog(string msg)
        {
            try
            {
                File.AppendAllText(ACTIVITY_FILE, getDateTime() + " >> " + msg + Environment.NewLine);
            }
            catch
            {

            }
        }
        public static void writeErrorLog(Exception msg)
        {
            try
            {
                File.AppendAllText(ERROR_FILE, getDateTime() + " >> " + msg.ToString() + Environment.NewLine);
            }
            catch
            {

            }
        }
        private static string getDateTime()
        {
            return DateTime.Now.ToString("G");
        }

        public static bool ReadConfigFile()
        {
            try
            {
                string[] settings = File.ReadAllLines(CONFIG_FILE);
                foreach (string setting in settings)
                {
                    switch (setting.Split('=')[0])
                    {
                        case "LDAP":
                            LDAP_URL = setting.Split('=')[1];
                            break;
                        case "BasePassword":
                            BasePassword = setting.Split('=')[1];
                            break;
                        case "FinalScreenShowTime":
                            FinalScreenShowTime = int.Parse(setting.Split('=')[1]);
                            break;
                        case "MaxResetLimit":
                            MaxResetLimit = bool.Parse(setting.Split('=')[1]);
                            break;
                        case "MaxResetCount":
                            MaxResetCount = int.Parse(setting.Split('=')[1]);
                            break;
                        case "ScreenProtectionRefreshInterval":
                            ScreenProtectionRefreshInterval = int.Parse(setting.Split('=')[1]);
                            break;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class Texts
    {
        public static string SuccessMessage = "Je wachtwoord is succesvol hersteld, pak een briefje.";
        public static string ExceededLimitFailMessage = "Je hebt je wachtwoord te vaak hersteld, meld je bij systeembeheer.";
        public static string UserNotFoundMessage = "De opgegeven gebruiker komt niet in het systeem voor.";
        public static string NoConnectErrorMessage = "Kon geen verbinding maken met de server.";
        public static string AccountDisabledMessage = "Deze account is uitgeschakeld. Neem contact op met systeembeheer.";
        public static string DisappearMessage = "Dit scherm verdwijnt in % seconden.";

        public static string PrintedMessage(string id, string password)
        {
            string newString = null;
            newString += Environment.NewLine;
            newString += Environment.NewLine;
            newString += String.Format("Je inlognaam is: {0}", id) + Environment.NewLine;
            newString += String.Format("Je nieuwe wachtwoord is: {0}", password) + Environment.NewLine;
            newString += "Bij het inloggen wordt je direct gevraagd om je wachtwoord te wijzigen." + Environment.NewLine;
            newString += Environment.NewLine;
            newString +=     "Het wachtwoord moet:" + Environment.NewLine;
            newString += "-   Minimaal 7 tekens lang zijn." + Environment.NewLine;
            newString += "-   Tenminste een hoofdletter, een kleine letter en een cijfer bevatten." + Environment.NewLine;
            newString += "-   Mag niet (een deel van) je eigen naam bevatten" + Environment.NewLine;
            return newString;
        }
    }

    public static class Printer
    {

        public static void Print(string text)
        {
            PrintDocument p = new PrintDocument();
            //Print a header in bold
            p.PrintPage += delegate(object sender1, PrintPageEventArgs e1)
            {
                e1.Graphics.DrawString("Je wachtwoord is gereset!", new Font("Calibri", 14, FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(0, 0, p.DefaultPageSettings.PrintableArea.Width, p.DefaultPageSettings.PrintableArea.Height));

            };

            //Print the rest of the body
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                {
                    e1.Graphics.DrawString(text, new Font("Calibri", 12), new SolidBrush(Color.Black), new RectangleF(0, 0, p.DefaultPageSettings.PrintableArea.Width, p.DefaultPageSettings.PrintableArea.Height));

                };
            try
            {
                p.Print();
            }
            catch (Exception ex)
            {
                Core.writeErrorLog(ex);
            }
        }
    }

    public static class Misc
    {
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);

        public static void KillExplorer()
        {
            try
            {
                int hwnd;
                hwnd = FindWindow("Progman", null);
                PostMessage(hwnd, /*WM_QUIT*/ 0x12, 0, 0);
            }
            catch
            {

            }
        }
    }
}
